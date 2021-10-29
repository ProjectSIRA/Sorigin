using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NodaTime;
using Sorigin.Models;
using Sorigin.Settings;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Sorigin.Services
{
    public interface ITokenService
    {
        Task<TokenSignContract> Sign(Guid id, params string[] scopes);
        Task<TokenSignContract?> Refresh(string refreshToken);
        Task<bool> Revoke(string refreshToken);
        string GenerateRefreshToken();
    }

    public class SoriginTokenService : ITokenService
    {

        private readonly IClock _clock;
        private readonly ILogger _logger;
        private readonly JWTSettings _jwtSettings;
        private readonly SoriginContext _soriginContext;
        private readonly RNGCryptoServiceProvider _rngCryptoServiceProvider;

        public SoriginTokenService(IClock clock, ILogger<SoriginTokenService> logger, JWTSettings jwtSettings, SoriginContext SoriginContext, RNGCryptoServiceProvider rngCryptoServiceProvider)
        {
            _clock = clock;
            _logger = logger;
            _jwtSettings = jwtSettings;
            _soriginContext = SoriginContext;
            _rngCryptoServiceProvider = rngCryptoServiceProvider;
        }

        public string GenerateRefreshToken()
        {
            byte[] bytes = new byte[32];
            _rngCryptoServiceProvider.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        public async Task<TokenSignContract?> Refresh(string refreshToken)
        {
            RefreshToken? tokenModel = await _soriginContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.Value == refreshToken);
            if (tokenModel is null)
                return null;

            // Check if the refresh token has expired.
            // We don't allow the revokation if it's already been expired.
            // We don't remove it, we let the token cleaner worker handle that,
            // since it'll be able to remove in batches. (TODO)
            Instant now = _clock.GetCurrentInstant();
            if (now > tokenModel.Expiration)
                return null;

            TokenSignContract contract = await Sign(tokenModel.UserID);
            _soriginContext.RefreshTokens.Remove(tokenModel);
            await _soriginContext.SaveChangesAsync();
            return contract;
        }

        public async Task<bool> Revoke(string refreshToken)
        {
            RefreshToken? tokenModel = await _soriginContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.Value == refreshToken);
            if (tokenModel is null)
                return false;

            // Check if the refresh token has expired.
            // We don't allow the revokation if it's already been expired.
            // We don't remove it, we let the token cleaner worker handle that,
            // since it'll be able to remove in batches.
            Instant now = _clock.GetCurrentInstant();
            if (now > tokenModel.Expiration)
                return false;

            _logger.LogInformation("Revoking a refresh token signed for {User}", tokenModel.UserID);
            _soriginContext.RefreshTokens.Remove(tokenModel);
            await _soriginContext.SaveChangesAsync();
            return true;
        }

        public async Task<TokenSignContract> Sign(Guid id, params string[] scopes)
        {
            _logger.LogInformation("Signing HMACSHA256 Symmetric Security Key for {id}", id);
            SymmetricSecurityKey securityKey = new(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            SigningCredentials credentials = new(securityKey, SecurityAlgorithms.HmacSha256);
            List<Claim> claims = new() { new Claim("sub", id.ToString()) };
            if (scopes.Length != 0)
                claims.Add(new Claim("scope", scopes.Aggregate((a, b) => a + " " + b)));
            JwtSecurityToken token = new(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims,
                expires: DateTime.UtcNow.AddHours(_jwtSettings.TokenLifetimeInHours),
                signingCredentials: credentials);

            TokenSignContract contract = new(new JwtSecurityTokenHandler().WriteToken(token), GenerateRefreshToken());

            User user = await _soriginContext.Users.Include(u => u.Tokens).FirstAsync(u => u.ID == id);
            Instant now = _clock.GetCurrentInstant();
            RefreshToken refreshToken = new()
            {
                Created = now,
                UserID = user.ID, // I think EF might get angry that I'm setting this manually.
                Value = contract.refreshToken,
                Expiration = now.Plus(Duration.FromHours(_jwtSettings.RefreshTokenLifetimeInHours))
            };
            user.Tokens.Add(refreshToken);
            await _soriginContext.SaveChangesAsync();

            return contract;
        }
    }
}
