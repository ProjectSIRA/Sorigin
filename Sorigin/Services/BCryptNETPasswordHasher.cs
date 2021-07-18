using Microsoft.Extensions.Logging;
using Sorigin.Authorization;

namespace Sorigin.Services
{
    public class BCryptNETPasswordHasher : IPasswordHasher
    {
        private readonly ILogger _logger;

        public BCryptNETPasswordHasher(ILogger logger)
        {
            _logger = logger;
        }

        public string Hash(string rawPassword)
        {
            _logger.LogInformation("Hashing password using BCrypt and SHA384.");
            string passwordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(rawPassword);
            return passwordHash;
        }

        public bool Verify(string rawPassword, string hash)
        {
            _logger.LogInformation("Verifying password with BCrypt and SHA384.");
            bool valid = BCrypt.Net.BCrypt.EnhancedVerify(rawPassword, hash);
            return valid;
        }
    }
}