using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NodaTime;
using Sorigin.Models;
using Sorigin.Settings;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


namespace Sorigin.Workers
{
    public class DatabaseCleaner : IHostedService
    {
        private bool _enabled;
        private readonly IClock _clock;
        private readonly ILogger _logger;
        private readonly SoriginSettings _soriginSettings;
        private readonly IServiceProvider _serviceProvider;
        private readonly float _databaseCleanupCycleLength = 24f;
        private Instant _nextUpdateTime;

        public DatabaseCleaner(IClock clock, ILogger<DatabaseCleaner> logger, SoriginSettings soriginSettings, IServiceProvider serviceProvider)
        {
            _clock = clock;
            _logger = logger;
            _soriginSettings = soriginSettings;
            _serviceProvider = serviceProvider;
            _databaseCleanupCycleLength = _soriginSettings.DatabaseCleanupCycleInHours;

            _nextUpdateTime = clock.GetCurrentInstant() + Duration.FromHours(_databaseCleanupCycleLength);

            Task.Run(() =>
            {
                while (true)
                {
                    if (_enabled && _clock.GetCurrentInstant() > _nextUpdateTime)
                    {
                        _nextUpdateTime = clock.GetCurrentInstant() + Duration.FromHours(_databaseCleanupCycleLength);
                        Task.Run(Cleanup);
                    }
                }
            });
        }

        private async Task Cleanup()
        {
            await Task.Delay(10000);

            int count = 0;
            bool needsToCleanup = false;
            Instant nowWhenInit = _clock.GetCurrentInstant();
            // IHostedService needs to be global, database is scoped.
            _logger.LogInformation("Cleaning up old refresh tokens...");
            using SoriginContext soriginContext = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<SoriginContext>();

            IAsyncEnumerable<RefreshToken> tokens = soriginContext.RefreshTokens.AsAsyncEnumerable();
            await foreach (RefreshToken token in tokens)
            {
                // If the expiration date has been reached...
                if (nowWhenInit > token.Expiration)
                {
                    if (!needsToCleanup)
                        needsToCleanup = true;

                    count++;
                    // Delete it!
                    soriginContext.Remove(token);
                }
            }
            _logger.LogInformation("Deleted {Count} old refresh tokens from the database.", count);

            if (needsToCleanup)
                await soriginContext.SaveChangesAsync();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _enabled = true;
            _ = Task.Run(Cleanup, default);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _enabled = false;
            return Task.CompletedTask;
        }
    }
}
