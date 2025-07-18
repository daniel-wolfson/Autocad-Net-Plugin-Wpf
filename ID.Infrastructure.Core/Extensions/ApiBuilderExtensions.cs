using Microsoft.AspNetCore.Builder;
using ID.Infrastructure.Middleware;
using System.Reflection;
using ID.Infrastructure.Filters;
using System.Linq;

namespace ID.Infrastructure.Extensions
{
    public static class ApiBuilderExtensions
    {
        public static IApplicationBuilder UseApiApiLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ApiLoggingMiddleware>();
        }

        public static IApplicationBuilder UseApiAuthentication(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ApiAuthenticationMiddleware>();
        }
        public static IApplicationBuilder UseApiContext(this IApplicationBuilder builder)
        {
            builder.UseMiddleware<ApiContextMiddleware>();
            
            //var preloadActionPaths = Assembly.GetEntryAssembly().GetPreloadActions<ApiCacheAttribute>("DalApi").ToArray();
            //foreach (var preloadActionPath in preloadActionPaths)
            //{
            //    builder.UseMiddleware<ApiContextPreoadMiddleware>(preloadActionPath);
            //}

            return builder; //.UseMiddleware<ApiContextMiddleware>();
        }

        public static IApplicationBuilder UseApiErrorHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ApiErrorHandlingMiddleware>();
        }
    }
}
