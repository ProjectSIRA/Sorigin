using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sorigin.Models;

namespace Sorigin
{
    public class SoriginContext : DbContext
    {
        public DbSet<User> Users => Set<User>();
        private static ILoggerFactory ContextLoggerFactory => LoggerFactory.Create(b => b.AddConsole().AddFilter("", LogLevel.Information));

        public SoriginContext(DbContextOptions<SoriginContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLoggerFactory(ContextLoggerFactory);
            optionsBuilder.LogTo((string _) => { });
        }
    }
}