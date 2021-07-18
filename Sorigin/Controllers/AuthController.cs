using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sorigin.Authorization;
using Sorigin.Models;
using System;
using System.Threading.Tasks;

namespace Sorigin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly SoriginContext _soriginContext;
        private readonly IPasswordHasher _passwordHasher;
    
        public AuthController(ILogger<AuthController> logger, SoriginContext soriginContext, IPasswordHasher passwordHasher)
        {
            _logger = logger;
            _soriginContext = soriginContext;
            _passwordHasher = passwordHasher;
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
            string lUsername = username.ToLowerInvariant();
            User? activeUser = await _soriginContext.Users.FirstOrDefaultAsync(u => u.Username.ToLowerInvariant() == lUsername);
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
    }
}
