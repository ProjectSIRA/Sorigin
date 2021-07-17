using Microsoft.EntityFrameworkCore;
using Sorigin.Models;

namespace Sorigin
{
    public class SoriginContext : DbContext
    {
        public DbSet<User> Users => Set<User>();

        public SoriginContext(DbContextOptions<SoriginContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.LogTo((string _) => { });
        }
    }
}