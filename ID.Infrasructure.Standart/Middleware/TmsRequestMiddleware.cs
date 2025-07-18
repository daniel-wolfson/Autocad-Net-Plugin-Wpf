using ID.Infrastructure.Enums;
using ID.Infrastructure.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Linq;
using System.Threading.Tasks;

namespace ID.Infrastructure.Middleware
{
    public class TmsRequestMiddleware
    {
        private readonly RequestDelegate _next;

        public TmsRequestMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var httpContext = (context as DefaultHttpContext).Request.HttpContext;
            var userIsAuthenticated = httpContext.User.Identities.Any(x => x.IsAuthenticated);

            if (httpContext.Session.IsAvailable && httpContext.Session.Contains(SessionKeys.AppUser))
            {
                var routeData = httpContext.GetRouteData();
                if (routeData != null)
                {
                    var routeDataValues = routeData.Values;
                    var isAreaAdmin = httpContext.Request.Path.StartsWithSegments("/admin");
                    if (isAreaAdmin)
                    {
                        routeData.DataTokens.Add("Admin", "");
                    }
                }
            }
            // Call the next delegate/middleware in the pipeline
            await _next(context);
        }
    }
}
