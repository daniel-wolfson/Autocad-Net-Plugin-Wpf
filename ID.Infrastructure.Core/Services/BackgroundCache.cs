using ID.Infrastructure.Core;
using ID.Infrastructure.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ID.Infrastructure.Services
{
    public class BackgroundCache : BackgroundService //IHostedService
    {
        private readonly ILogger<BackgroundCache> _logger;
        private readonly ICacheService _cacheService;

        public BackgroundCache(ILogger<BackgroundCache> logger, ICacheService cacheService)
        {
            _logger = logger;
            _cacheService = cacheService;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting {jobName}", nameof(BackgroundCache));
            base.StartAsync(cancellationToken);
            //RefreshCacheAsync(cancellationToken).ConfigureAwait(false);
            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping {jobName}", nameof(BackgroundCache));

            base.StopAsync(cancellationToken);

            // Perform any cleanup here
            _cacheService.RemoveAll();

            return Task.CompletedTask;
        }

        protected override async Task ExecuteAsync(CancellationToken stopCancellationToken)
        {
            while (await _cacheService.WaitToReadAsync(stopCancellationToken))
            {
                var cacheItem = await _cacheService.ReadAsync(stopCancellationToken);

                if (GeneralContext.ServiceScope != null && !_cacheService.IsCancellationRequested)
                {
                    await RefreshCacheAsync(stopCancellationToken).ConfigureAwait(false);
                }
            }
        }

        private async Task RefreshCacheAsync(CancellationToken cancellationToken)
        {
            while (!_cacheService.IsCancellationRequested)
            {
                await _cacheService.RefreshCacheAsync();
                
                await Task.Delay(5000, cancellationToken);
            }
        }
    }
}