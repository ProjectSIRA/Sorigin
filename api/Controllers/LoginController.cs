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
    private readonly IUserService _userService;
    private readonly ISteamService _steamService;
    private readonly ITokenService _tokenService;
    private readonly IAdminSettings _adminSettings;
    private readonly ILocationService _locationService;

    public LoginController(ILogger<LoginController> logger, IUserService userService, ISteamService steamService, ITokenService tokenService, IAdminSettings adminSettings, ILocationService locationService)
    {
        _logger = logger;
        _userService = userService;
        _steamService = steamService;
        _tokenService = tokenService;
        _adminSettings = adminSettings;
        _locationService = locationService;
    }

    [HttpPost("{platform}")]
    public async Task<ActionResult<Token>> Login(string platform, [FromBody] GameLoginBody loginBody)
    {
        _logger.LogInformation("A {Platform} login to Sorigin has been requested.", platform);

        bool canOverride = loginBody.OverrideID.HasValue && loginBody.Token == _adminSettings.OverrideKey;

        ulong? id = null;
        string? username = null;
        Uri? fullProfilePictureURL = null;
        string? profilePictureContract = null;

        if (platform.ToLower() == "steam")
        {
            SteamUser? steamUser = canOverride ? (await _steamService.GetProfileFromID(loginBody.OverrideID.GetValueOrDefault())) : (await _steamService.GetProfile(loginBody.Token));
            if (steamUser is null)
            {
                _logger.LogWarning("Could not verify ownership through a Steam ticket. The user could be pirating the game, Steam could be down, or ");
                return Unauthorized(new Error("unverifiable-game-ownership", "Unable to verify ownership of the game. Was the steam ticket invalid? Is Steam offline? Is the account pirated?"));
            }
            username = steamUser.Username;
            id = ulong.Parse(steamUser.Id);
            profilePictureContract = steamUser.Avatar;
            fullProfilePictureURL = new Uri($"https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/{steamUser.Avatar[..2]}/{steamUser.Avatar}_full.jpg");
        }

        // TODO: Oculus

        if (!id.HasValue || username is null || fullProfilePictureURL is null || profilePictureContract is null)
            return NotFound(new Error("no-platform", $"Unable to find a platform with the name '{platform}'"));

        string? country = null;
        var ip = HttpContext.Connection.RemoteIpAddress;
        if (ip is not null)
            country = await _locationService.GetLocationAsync(ip.ToString());

        User? user = await _userService.GetUser(id.Value);
        if (user is null)
        {
            user = await _userService.CreateUser(id.Value, username, profilePictureContract, fullProfilePictureURL, country);
        }
        else
        {
            await _userService.Login(user);
            if (country is not null && user.Country is not null && country != user.Country)
            {
                _logger.LogWarning("The user {Username} ({ID}) tried to login. However, their country seem to have changed. For security, we will refuse to log them in.", user.Username, user.ID);
                return BadRequest(new Error("invalid-country", "Unusual account activity detected. Seek assistance from an administrator."));
            }
            await _userService.UpdateUser(user, username, profilePictureContract, fullProfilePictureURL, country);
        }

        string token = await _tokenService.Sign(id.Value);
        return Ok(new Token(token));
    }

    public class GameLoginBody
    {
        public string Token { get; init; } = null!;
        public ulong? OverrideID { get; set; } = null;
    }
}