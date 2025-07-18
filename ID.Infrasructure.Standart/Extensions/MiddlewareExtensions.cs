using ID.Infrastructure.Middleware;
using Microsoft.AspNetCore.Builder;

namespace ID.Api.Extensions
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseApiGeneralRequest(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ApiRequestMiddleware>();
        }

        public static IApplicationBuilder UseTmsGeneralRequest(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TmsRequestMiddleware>();
        }
    }
}
