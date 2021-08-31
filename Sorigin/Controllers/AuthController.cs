using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sorigin.Authorization;
using Sorigin.Models;
using Sorigin.Models.Platforms;
using Sorigin.Services;
using Sorigin.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Sorigin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IAuthService _authService;
        private readonly SoriginContext _soriginContext;
        private readonly IUserStateCache _userStateCache;

        private readonly SteamService _steamService;
        private readonly DiscordService _discordService;
        private readonly DiscordSettings _discordSettings;
    
        public AuthController(ILogger<AuthController> logger, IAuthService authService, SoriginContext soriginContext, IUserStateCache userStateCache,
                                SteamService steamService, DiscordService discordService, DiscordSettings discordSettings)
        {
            _logger = logger;
            _authService = authService;
            _soriginContext = soriginContext;
            _userStateCache = userStateCache;

            _steamService = steamService;
            _discordService = discordService;
            _discordSettings = discordSettings;
        }

        [Authorize]
        [HttpGet("@me")]
        public async Task<ActionResult> GetSelf()
        {
            return Ok(await _authService.GetUser(User.GetID()));
        }

        [HttpGet("discord/auth")]
        public IActionResult DiscordAuthenticate([FromQuery] string redirect_url)
        {
            Guid requestID = _userStateCache.Add(redirect_url);
            return Redirect($"{_discordSettings.URL}/oauth2/authorize?response_type=code&client_id={_discordSettings.ID}&scope=identify&redirect_uri={_discordSettings.RedirectURL}&state={requestID}");
        }

        [HttpGet("discord/callback")]
        public IActionResult DiscordCallback([FromQuery] Guid state, [FromQuery] string code)
        {
            string? redirect_url = _userStateCache.Pull(state);
            if (redirect_url is not null)
            {
                return Redirect($"{redirect_url}?grant={code}");
            }
            return Unauthorized(Error.Create("Could not lock onto authorization flow state."));
        }

        [HttpPost("login")]
        public async Task<ActionResult<TokenResponse>> Login([FromQuery] string grant, [FromQuery] string platform)
        {
            var badGrant = BadRequest(Error.Create("Could not authenticate from the provided grant."));

            User? user = null;
            platform = platform.ToLower();
            if (platform == Platform.Discord.ToStringFast(true))
            {
                string? token = null;
                // If there is no period, it's a code and we need to get an access token from it.
                if (!grant.Contains('.'))
                    token = await _discordService.GetAccessToken(grant);

                if (token is null)
                    return badGrant;

                DiscordUser? discordUserFromAccessToken = await _discordService.GetProfile(token);
                if (discordUserFromAccessToken is null)
                    return badGrant;

                user = await _soriginContext.Users.FirstOrDefaultAsync(u => u.Discord != null && u.Discord.Id == discordUserFromAccessToken.Id);
                if (user is null)
                {
                    user = await CreateUser(discordUserFromAccessToken.Username);
                    user.Discord = discordUserFromAccessToken;
                    await _soriginContext.SaveChangesAsync();
                }
                if (!user.Role.HasFlag(Role.Owner) && _discordSettings.Roots.Contains(discordUserFromAccessToken.Id))
                {
                    user.Role = Role.Owner | Role.Admin | Role.Verified;
                    await _soriginContext.SaveChangesAsync();
                }
            }
            else if (platform == Platform.Steam.ToStringFast(true))
            {
                // Check in with steam servers so we know who their token belongs to.
                SteamUser? steamUserFromTicket = await _steamService.GetProfile(grant);
                if (steamUserFromTicket is null)
                    return badGrant;

                user = await _soriginContext.Users.FirstOrDefaultAsync(u => u.Steam != null && u.Steam.Id == steamUserFromTicket.Id);
                if (user is null)
                {
                    user = await CreateUser(steamUserFromTicket.Username);
                    user.Steam = steamUserFromTicket;
                    await _soriginContext.SaveChangesAsync();
                }
            }

            if (user is null)
                return badGrant;

            _logger.LogInformation("Logging in {Useranme} ({ID}) from {Platform}.", user.Username, user.ID, platform);
            string userToken = _authService.Sign(user.ID, 4, user.Role);
            return Ok(new TokenResponse { Token = userToken });
        }

        /// <summary>
        /// Creates a new user and adds them to the database if successful.
        /// </summary>
        /// <param name="id">The original specific ID for the user.</param>
        private async Task<User> CreateUser(string username)
        {
            // Repeat Validation
            string lUsername = username.ToLower();
            User? activeUser = await _soriginContext.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == lUsername);
            if (activeUser is not null)
            {
                _logger.LogWarning("A user attempted to create the username with {username}.", username);
                // If there's already a user with that username, assign them a default
                username += "_" + Guid.NewGuid().ToString();
            }

            // Create and add the user
            activeUser = new User
            {
                ID = Guid.NewGuid(),
                Username = username,
            };

            _soriginContext.Add(activeUser);
            await _soriginContext.SaveChangesAsync();
            return activeUser;
        }

        public class TokenResponse
        {
            [JsonPropertyName("token")]
            public string Token { get; set; } = null!;
        }
    }
}