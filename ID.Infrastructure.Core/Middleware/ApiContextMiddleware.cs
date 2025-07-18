using ID.Infrastructure.Core;
using ID.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.Infrastructure.Middleware
{
    public class ApiContextMiddleware
    {
        private readonly RequestDelegate _next;

        public ApiContextMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            if (httpContext.Request.Path.Value.StartsWith("/api"))
            {
                if (httpContext.Request.Headers.ContainsKey("ApiTransactionToken"))
                {
                    var transactionToken = Encoding.UTF8.GetString(Convert.FromBase64String(httpContext.Request.Headers["ApiTransactionToken"].FirstOrDefault()));
                    httpContext.Items.Add("ApiTransactionToken", transactionToken);
                }

                GeneralContext.ServiceScope = httpContext.RequestServices.CreateScope();

                string language = httpContext.Request.Headers["Accept-Language"];
                GeneralContext.Language = !string.IsNullOrEmpty(language) && language.StartsWith("he-IL") ? "he" : "en";

                if (httpContext.Request.Headers.ContainsKey("ApiTransactionToken") || httpContext.Request.Headers.ContainsKey("ApiCachePreload"))
                {
                    ICacheService cacheService = httpContext.RequestServices.GetService<ICacheService>();
                    if (!cacheService.CacheLoading && !cacheService.CacheLoaded)
                    {
                        await cacheService.PreloadAsync(httpContext.RequestServices);
                    }
                }

                if (httpContext.Request.Headers.ContainsKey("ApiCacheReload"))
                {
                    //cacheService.ReloadAsync(serviceProvider).GetAwaiter().GetResult();
                }
            }

            await _next(httpContext);

            if (httpContext.Request.Path.Value.StartsWith("/api"))
            {
                GeneralContext.ServiceScope?.Dispose();
            }
        }
    }
}
