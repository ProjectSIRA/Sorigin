using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sorigin.Authorization;
using Sorigin.Models;
using Sorigin.Models.Platforms;
using Sorigin.Services;
using System;
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
        private readonly IPasswordHasher _passwordHasher;

        private readonly SteamService _steamService;
        private readonly DiscordService _discordService;
    
        public AuthController(ILogger<AuthController> logger, IAuthService authService, SoriginContext soriginContext, IPasswordHasher passwordHasher,
                                SteamService steamService, DiscordService discordService)
        {
            _logger = logger;
            _authService = authService;
            _soriginContext = soriginContext;
            _passwordHasher = passwordHasher;

            _steamService = steamService;
            _discordService = discordService;
        }

        [Authorize]
        [HttpGet("@me")]
        public async Task<ActionResult> GetSelf()
        {
            return Ok(await _authService.GetUser(User.GetID()));
        }

        [HttpPost]
        public async Task<ActionResult<TokenResponse>> Create([FromBody] CreateBody body)
        {
            var validationResponse = await ValidateUser(body.Platform, body.PlatformToken);
            if (!validationResponse.Item1)
            {
                return Unauthorized(Error.Create(validationResponse.Item3!));
            }
            var userCreate = await CreateUser(body.Username, body.Password);
            if (userCreate.Item1 is null)
            {
                return BadRequest(Error.Create(userCreate.Item2!));
            }
            User user = userCreate.Item1;
            if (body.Platform == Platform.Discord)
            {
                DiscordUser discordUser = (validationResponse.Item2 as DiscordUser)!;
                userCreate.Item1.Discord = discordUser;
                await _soriginContext.SaveChangesAsync();
            }
            else if (body.Platform == Platform.Steam)
            {
                SteamUser steamUser = (validationResponse.Item2 as SteamUser)!;
                userCreate.Item1.GamePlatform = GamePlatform.Steam;
                userCreate.Item1.Steam = steamUser;
                await _soriginContext.SaveChangesAsync();
            }
            string userToken = _authService.Sign(user.ID, 4, user.Role);
            return Ok(new TokenResponse { Token = userToken });
        }

        [HttpPost("login")]
        public async Task<ActionResult<TokenResponse>> Login([FromBody] LoginBody body)
        {
            string lUsername = body.Username.ToLower();
            User? user = await _soriginContext.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == lUsername);
            if (user is null)
            {
                _logger.LogWarning("A user tried to log into the account {Username}, but that account does not exist!", body.Username);
                return BadRequest(Error.Create("User does not exist."));
            }

            // (っ＾▿＾)💨
            bool passwordOK = await Task.Run(() => _passwordHasher.Verify(body.Password, user.Hash));

            if (!passwordOK)
            {
                _logger.LogWarning("A user tried to log into the account {Username}, but had an incorrect password!", body.Username);
                return Unauthorized(Error.Create("Incorrect password."));
            }
            string userToken = _authService.Sign(user.ID, 168, user.Role);
            return Ok(new TokenResponse { Token = userToken });
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
            }
            else if (body.Platform == Platform.Steam)
            {
                SteamUser steamUser = (validationResponse.Item2 as SteamUser)!;
                user = await _soriginContext.Users.FirstOrDefaultAsync(u => u.Steam != null && u.Steam.Id == steamUser.Id);
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

        private async Task<ValueTuple<bool, object?, string?>> ValidateUser(Platform platform, string platformToken)
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
                        return (false, null, "Could not acquire access token.");
                    }
                }
                DiscordUser? discordUser = await _discordService.GetProfile(platformToken);
                if (discordUser is null)
                {
                    return (false, null, "Unable to get user profile.");
                }
                bool alreadyExists = await _soriginContext.Users.AnyAsync(u => u.Discord != null && u.Discord.Id == discordUser.Id);
                return (!alreadyExists, discordUser, null);
            }
            else if (platform == Platform.Steam)
            {
                SteamUser? steamUser = await _steamService.GetProfile(platformToken);
                if (steamUser is null)
                {
                    return (false, null, "Unable to get user profile.");
                }
                bool alreadyExists = await _soriginContext.Users.AnyAsync(u => u.Steam != null && u.Steam.Id == steamUser.Id);
                return (!alreadyExists, steamUser, null);
            }
            return (false, null, "Could not find authentication method.");
        }

        /// <summary>
        /// Creates a new user and adds them to the database if successful.
        /// </summary>
        /// <param name="username">The username for the user.</param>
        /// <param name="password">The password for the user.</param>
        /// <returns>A value tuple containing the user (if successfully created) and an error (if creation failed).</returns>
        private async Task<ValueTuple<User?, string?>> CreateUser(string username, string password)
        {
            // Repeat Validation
            string lUsername = username.ToLower();
            User? activeUser = await _soriginContext.Users.FirstOrDefaultAsync(u => u.Username == lUsername);
            if (activeUser is not null)
            {
                _logger.LogWarning("A user attempted to create the username with {username}.", username);
                // If there's already a user with that username, don't create it.
                return (null, $"User with username '{username}' already exists.");
            }

            // Password Validation
            Zxcvbn.Result result = Zxcvbn.Core.EvaluatePassword(password);
            if (result.Score <= 1)
            {
                _logger.LogWarning("While creating an account, the user {username} set a password which received a score of {result.Scoe} and doesn't meet the requirement.", username, result.Score);
                return (null, $"Password: {result.Feedback.Warning}");
            }
            string hash = _passwordHasher.Hash(password);
    
            // Create and add the user
            activeUser = new User
            {
                ID = Guid.NewGuid(),
                Username = username,
                Hash = hash,
            };
            _soriginContext.Add(activeUser);
            await _soriginContext.SaveChangesAsync();
            return (activeUser, null);
        }

        public class CreateBody
        {
            public Platform Platform { get; set; }
            public string PlatformToken { get; set; } = null!;
            public string Username { get; set; } = null!;
            public string Password { get; set; } = null!;
        }

        public class LoginBody
        {
            public string Username { get; set; } = null!;
            public string Password { get; set; } = null!;
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