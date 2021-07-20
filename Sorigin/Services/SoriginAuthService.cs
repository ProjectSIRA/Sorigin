using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Sorigin.Authorization;
using Sorigin.Models;
using Sorigin.Settings;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Sorigin.Services
{
    public class SoriginAuthService : IAuthService
    {
        private readonly ILogger _logger;
        private readonly JWTSettings _jwtSettings;
        private readonly SoriginContext _soriginContext;

        public SoriginAuthService(ILogger<SoriginAuthService> logger, JWTSettings jwtSettings, SoriginContext soriginContext)
        {
            _logger = logger;
            _jwtSettings = jwtSettings;
            _soriginContext = soriginContext;
        }

        public async Task<User?> GetUser(Guid? id)
        {
            return await _soriginContext.Users.Include(u => u.Discord).Include(u => u.Steam).FirstOrDefaultAsync(u => u.ID == id);
        }

        public string Sign(Guid id, float lengthInHours = 1344, Role role = Role.None)
        {
            string[] scopes = Enum.GetValues<Role>().Where(v => !Equals((int)(object)v, 0) && role.HasFlag(v)).Select(r => r.ToString()).ToArray();

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
                expires: DateTime.UtcNow.AddHours(lengthInHours),
                signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}