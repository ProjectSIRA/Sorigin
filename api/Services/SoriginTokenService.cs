using Microsoft.IdentityModel.Tokens;
using Sorigin.Settings;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Sorigin.Services;

public interface ITokenService
{
    Task<string> Sign(ulong id);
}

internal class SoriginTokenService : ITokenService
{
    private readonly ILogger _logger;
    private readonly IJWTSettings _jwtSettings;

    public SoriginTokenService(ILogger logger, IJWTSettings jwtSettings)
    {
        _logger = logger;
        _jwtSettings = jwtSettings;
    }

    public Task<string> Sign(ulong id)
    {
        _logger.LogInformation("Signing HMACSHA256 Symmetric Security Key for {ID}", id);

        List<Claim> claims = new() { new Claim("sub", id.ToString()) };
        SymmetricSecurityKey securityKey = new(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        SigningCredentials credentials = new(securityKey, SecurityAlgorithms.HmacSha256);
        JwtSecurityToken token = new(_jwtSettings.Issuer, _jwtSettings.Audience, claims, null, DateTime.UtcNow.AddHours(_jwtSettings.TokenLifetime), credentials);

        string tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return Task.FromResult(tokenString);
    }
}