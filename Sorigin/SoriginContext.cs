using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sorigin.Models;

namespace Sorigin
{
    public class SoriginContext : DbContext
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<Transfer> Transfers => Set<Transfer>();
        private readonly ILogger _logger;

        public SoriginContext(ILogger<SoriginContext> logger, DbContextOptions<SoriginContext> options) : base(options)
        {
            _logger = logger;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.LogTo((string _) =>
            {
                
            });
        }
    }
}