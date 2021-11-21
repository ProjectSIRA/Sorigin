using Microsoft.AspNetCore.Mvc;
using Sorigin.Models;
using Sorigin.Models.Platforms;
using Sorigin.Services;
using Sorigin.Settings;

namespace Sorigin.Controllers;

[Route("api/login")]
[ApiController]
public class LoginController : ControllerBase
{
    private readonly ILogger _logger;
    private readonly ISteamService _steamService;
    private readonly ITokenService _tokenService;
    private readonly IAdminSettings _adminSettings;
    private readonly ILocationService _locationService;

    public LoginController(ILogger<LoginController> logger, ISteamService steamService, ITokenService tokenService, IAdminSettings adminSettings, ILocationService locationService)
    {
        _logger = logger;
        _steamService = steamService;
        _tokenService = tokenService;
        _adminSettings = adminSettings;
        _locationService = locationService;
    }

    [HttpPost("{platform}")]
    public async Task<IActionResult> Login(string platform, [FromBody] GameLoginBody loginBody)
    {
        _logger.LogInformation("A {Platform} login to Sorigin has been requested.", platform);

        bool canOverride = loginBody.OverrideID.HasValue && loginBody.Token == _adminSettings.OverrideKey;

        ulong? id = null;
        string? username = null;
        string? fullProfilePictureURL = null;

        if (platform.ToLower() == "steam")
        {
            SteamUser? steamUser = canOverride ? (await _steamService.GetProfileFromID(loginBody.OverrideID.GetValueOrDefault())) : (await _steamService.GetProfile(loginBody.Token));
            if (steamUser is null)
            {
                _logger.LogWarning("Could not verify ownership through a Steam ticket. The user could be pirating the game, Steam could be down, or ");
                return Unauthorized(new Error(nameof(Unauthorized), "Unable to verify ownership of the game. Was the steam ticket invalid? Is Steam offline? Is the account pirated?"));
            }
            username = steamUser.Username;
            id = ulong.Parse(steamUser.Id);
            fullProfilePictureURL = $"https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/{steamUser.Avatar[..2]}/{steamUser.Avatar}_full.jpg";
        }

        // TODO: Oculus

        if (!id.HasValue || username is null || fullProfilePictureURL is null)
            return NotFound(new Error(nameof(NotFound), $"Unable to find a platform with the name '{platform}'"));

        _logger.LogInformation("{ID}", id.Value);
        _logger.LogInformation("{Username}", username);
        _logger.LogInformation("{PFPURL}", fullProfilePictureURL);

        string token = await _tokenService.Sign(id.Value);
        return Ok(new Token(token));
    }

    [HttpGet("test")]
    public async Task<IActionResult> Test()
    {
        var ip = HttpContext.Connection.RemoteIpAddress;
        if (ip is not null)
        {
            _logger.LogInformation("Location: {Location}", await _locationService.GetLocationAsync(ip.ToString()));
        }
        return Ok();
    }

    public class GameLoginBody
    {
        public string Token { get; init; } = null!;
        public ulong? OverrideID { get; set; } = null;
    }
}