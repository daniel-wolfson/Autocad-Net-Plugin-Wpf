using ID.Infrastructure.Core;
using ID.Infrastructure.Interfaces;
using ID.Infrastructure.Models;
using ID.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static void AddCacheServices(this IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddDistributedMemoryCache();
            services.AddHostedService<BackgroundCache>()
                .AddSingleton(_ => Channel.CreateUnbounded<ApiCacheItem>());
            services.AddSingleton<ICacheService, CacheService>();
            services.AddLogging(loggerConfig =>
            {
                loggerConfig.AddConsole(consoleConfig => consoleConfig.TimestampFormat = "[HH:mm:ss]");
            });

            //services.AddStackExchangeRedisCache(options => options.Configuration = "localhost:6379");
        }

        public static void AddBackgroundServices(this IServiceCollection services)
        {
            services
                .AddHostedService<BackgroundWorker>()
                .AddSingleton<IBackgroundQueue<ApiReportItem>, BackgroundQueue<ApiReportItem>>();
        }
    }
}