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

        private readonly SteamService _steamService;
        private readonly DiscordService _discordService;
        private readonly DiscordSettings _discordSettings;

        private static readonly Dictionary<Guid, string> _discordStateCache = new();
    
        public AuthController(ILogger<AuthController> logger, IAuthService authService, SoriginContext soriginContext,
                                SteamService steamService, DiscordService discordService, DiscordSettings discordSettings)
        {
            _logger = logger;
            _authService = authService;
            _soriginContext = soriginContext;

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
            Guid requestID = Guid.NewGuid();
            _discordStateCache.Add(requestID, redirect_url);
            return Redirect($"{_discordSettings.URL}/oauth2/authorize?response_type=code&client_id={_discordSettings.ID}&scope=identify&redirect_uri={_discordSettings.RedirectURL}&state={requestID}");
        }

        [HttpGet("discord/callback")]
        public IActionResult DiscordCallback([FromQuery] Guid state, [FromQuery] string code)
        {
            if (_discordStateCache.TryGetValue(state, out string? redirect_url))
            {
                _discordStateCache.Remove(state);
                return Redirect($"{redirect_url}?grant={code}");
            }
            return Unauthorized(Error.Create("Could not lock onto authorization flow state."));
        }

        [HttpPost("token")]
        public async Task<ActionResult<TokenResponse>> TokenLogin([FromBody] TokenLoginBody body)
        {
            User? user = null;
            var validationResponse = await ValidateUser(body.Platform, body.Token);
            if (!string.IsNullOrEmpty(validationResponse.Item3) || validationResponse.Item2 is null)
            {
                return Unauthorized(Error.Create(validationResponse.Item3!));
            }

            if (body.Platform == Platform.Discord)
            {
                DiscordUser discordUser = (validationResponse.Item2 as DiscordUser)!;
                user = await _soriginContext.Users.FirstOrDefaultAsync(u => u.Discord != null && u.Discord.Id == discordUser.Id);
                if (user is not null)
                {
                    if (user.Discord is null)
                    {
                        user.Discord = discordUser;
                        await _soriginContext.SaveChangesAsync();
                    }
                    else if (user.Discord != discordUser)
                    {
                        user.Discord!.Avatar = discordUser.Avatar;
                        user.Discord.Username = discordUser.Username;
                        user.Discord.Discriminator = discordUser.Discriminator;
                        await _soriginContext.SaveChangesAsync();
                    }
                }
            }
            else if (body.Platform == Platform.Steam)
            {
                SteamUser steamUser = (validationResponse.Item2 as SteamUser)!;
                user = await _soriginContext.Users.FirstOrDefaultAsync(u => u.Steam != null && u.Steam.Id == steamUser.Id);
                if (user is not null)
                {
                    if (user.Steam is null)
                    {
                        user.Steam = steamUser;
                        user.GamePlatform = GamePlatform.Steam;
                        await _soriginContext.SaveChangesAsync();
                    }
                    else if (user.Steam != steamUser)
                    {
                        user.Steam!.Avatar = steamUser.Avatar;
                        user.Steam.Username = steamUser.Username;
                        await _soriginContext.SaveChangesAsync();
                    }
                }
            }

            if (user is null)
            {
                _logger.LogCritical("A user has been validated but does not exist.");
                return BadRequest(Error.Create("The user has been validated but does not exist! (You should NOT be seeing this, please report it.)"));
            }
            else
            {
                _logger.LogInformation("Logging in {Useranme} ({ID}) from {Platform}.", user.Username, user.ID, body.Platform);
                string userToken = _authService.Sign(user.ID, 4, user.Role);
                return Ok(new TokenResponse { Token = userToken });
            }
        }

        [Authorize]
        [HttpPost("add")]
        public async Task<ActionResult> AddPlatform([FromBody] TokenLoginBody body)
        {
            var validationResponse = await ValidateUser(body.Platform, body.Token);
            if (!validationResponse.Item1)
            {
                if (validationResponse.Item2 is not null)
                    return BadRequest(Error.Create("Platform already added!"));
                return BadRequest(validationResponse.Item3);
            }
            User user = (await _authService.GetUser(User.GetID()))!;
            if (body.Platform == Platform.Discord)
            {
                DiscordUser discordUser = (validationResponse.Item2 as DiscordUser)!;
                user.Discord = discordUser;
                await _soriginContext.SaveChangesAsync();
            }
            else if (body.Platform == Platform.Steam)
            {
                SteamUser steamUser = (validationResponse.Item2 as SteamUser)!;
                user.GamePlatform = GamePlatform.Steam;
                user.Steam = steamUser;
                await _soriginContext.SaveChangesAsync();
            }
            return NoContent();
        }

        // bool: Successful
        // object?: User
        // string?: Error
        // string?: ID
        private async Task<ValueTuple<bool, object?, string?, string?>> ValidateUser(Platform platform, string platformToken)
        {
            _logger.LogInformation("Validating a user through {platform}", platform);
            if (platform == Platform.Discord)
            {
                // This is the code, we need the access token.
                if (!platformToken.Contains('.'))
                {
                    platformToken = await _discordService.GetAccessToken(platformToken) ?? string.Empty;
                    if (string.IsNullOrEmpty(platformToken))
                    {
                        // Could not acquire access token.
                        return (false, null, "Could not acquire access token.", null);
                    }
                }
                DiscordUser? discordUser = await _discordService.GetProfile(platformToken);
                if (discordUser is null)
                {
                    return (false, null, "Unable to get user profile.", null);
                }
                bool alreadyExists = await _soriginContext.Users.AnyAsync(u => u.Discord != null && u.Discord.Id == discordUser.Id);
                if (!alreadyExists)
                {
                    var userCreate = await CreateUser(discordUser.Username);
                    if (userCreate.Item1 is not null)
                    {
                        userCreate.Item1.Discord = discordUser;
                        await _soriginContext.SaveChangesAsync();
                    }
                }
                return (true, discordUser, null, discordUser.Id);
            }
            else if (platform == Platform.Steam)
            {
                SteamUser? steamUser = await _steamService.GetProfile(platformToken);
                if (steamUser is null)
                {
                    return (false, null, "Unable to get user profile.", null);
                }
                bool alreadyExists = await _soriginContext.Users.AnyAsync(u => u.Steam != null && u.Steam.Id == steamUser.Id);
                if (!alreadyExists)
                {
                    var userCreate = await CreateUser(steamUser.Username);
                    if (userCreate.Item1 is not null)
                    {
                        userCreate.Item1.Steam = steamUser;
                        await _soriginContext.SaveChangesAsync();
                    }
                }
                return (true, steamUser, null, steamUser.Id);
            }
            return (false, null, "Could not find authentication method.", null);
        }

        /// <summary>
        /// Creates a new user and adds them to the database if successful.
        /// </summary>
        /// <param name="id">The original specific ID for the user.</param>
        /// <returns>A value tuple containing the user (if successfully created) and an error (if creation failed).</returns>
        private async Task<ValueTuple<User?, string?>> CreateUser(string username)
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
            return (activeUser, null);
        }
        
        public class CreateBody
        {
            public Platform Platform { get; set; }
            public string PlatformToken { get; set; } = null!;
        }

        public class TokenLoginBody
        {
            public Platform Platform { get; set; }
            public string Token { get; set; } = null!;
        }

        public class TokenResponse
        {
            [JsonPropertyName("token")]
            public string Token { get; set; } = null!;
        }
    }
}