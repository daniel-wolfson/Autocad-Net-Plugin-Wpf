using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using ID.Infrastructure.Core;

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
                GeneralContext.ServiceScope = httpContext.RequestServices.CreateScope();
            }

            await _next(httpContext);

            if (httpContext.Request.Path.Value.StartsWith("/api"))
            {
                GeneralContext.ServiceScope?.Dispose();
            }
        }
    }
}
