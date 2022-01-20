using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Sorigin.Models;

namespace Sorigin;

public class SoriginContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Media> Media => Set<Media>();

    private readonly ILogger _logger;

    public SoriginContext(ILogger<SoriginContext> logger, DbContextOptions<SoriginContext> options) : base(options)
    {
        _logger = logger;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.LogTo(Log, LogLevel.Information, DbContextLoggerOptions.SingleLine);
    }

    private void Log(string command)
    {

        if (command.StartsWith("Executed DbCommand"))
        {
            int queryTime = int.Parse(Between(command, '(', 'm').Replace(",", string.Empty));
            string query = command[(command.LastIndexOf("]") + 1)..];
            _logger.LogInformation("[{Time}ms] | {Query}", queryTime, query);
        }
    }

    private static string Between(string input, char start, char end)
    {
        int iStart = input.IndexOf(start) + 1;
        int iEnd = input.IndexOf(end, iStart);
        return input[iStart..iEnd];
    }
}