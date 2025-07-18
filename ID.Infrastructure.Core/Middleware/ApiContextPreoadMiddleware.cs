using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace ID.Infrastructure.Middleware
{
    public class ApiContextPreoadMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _preloadActionPath;

        public ApiContextPreoadMiddleware(RequestDelegate next, string preloadActionPath)
        {
            _next = next;
            _preloadActionPath = preloadActionPath;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            if (httpContext.Request.Path.Value.StartsWith("/api") && httpContext.Request.Headers.ContainsKey("Api-Request-Long"))
            {
                if (_preloadActionPath.Equals(httpContext.Request.Path.Value, StringComparison.InvariantCultureIgnoreCase))
                {
                    var _path = httpContext.Request.Path;
                    
                    httpContext.Request.Path = new PathString(_preloadActionPath);
                    await _next(httpContext);

                    httpContext.Request.Path = new PathString(_path);
                    await _next(httpContext);
                    ////await GeneralContext.ServiceScope.ServiceProvider.PreloadConsistentData();

                    //foreach (var preloadActionPath in _preloadActionPaths)
                    //{
                    //    httpContext.Request.Path = new PathString(preloadActionPath);
                    //    await _next(httpContext);
                    //}

                    //httpContext.Request.Path = _path;
                }
            }

            await _next(httpContext);

            if (httpContext.Request.Path.Value.StartsWith("/api"))
            {
            }
        }
    }
}
