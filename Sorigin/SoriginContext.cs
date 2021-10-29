using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sorigin.Models;

namespace Sorigin
{
    public class SoriginContext : DbContext
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<Transfer> Transfers => Set<Transfer>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        private readonly ILogger _logger;

        public SoriginContext(ILogger<SoriginContext> logger, DbContextOptions<SoriginContext> options) : base(options)
        {
            _logger = logger;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.LogTo((_, level) => level != LogLevel.Debug && level != LogLevel.Trace, eventData =>
            {
                string command = eventData.ToString();
                if (command.StartsWith("Executed DbCommand"))
                {
                    int queryTime = int.Parse(Between(command, '(', 'm'));
                    string query = command[(command.LastIndexOf("]") + 1)..].Replace(@"
", " ");
                    _logger.Log(eventData.LogLevel, eventData.EventId, "[{Time}ms] |{Query}", queryTime, query);
                }
            });
        }

        private static string Between(string input, char start, char end)
        {
            int iStart = input.IndexOf(start) + 1;
            int iEnd = input.IndexOf(end, iStart);
            return input[iStart..iEnd];
        }
    }
}