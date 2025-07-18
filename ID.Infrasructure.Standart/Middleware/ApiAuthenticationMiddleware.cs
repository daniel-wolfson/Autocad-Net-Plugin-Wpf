using ID.Infrastructure.Core;
using ID.Infrastructure.Extensions;
using ID.Infrastructure.Helpers;
using ID.Infrastructure.Interfaces;
using ID.Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace ID.Infrastructure.Middleware
{
    public class ApiAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;

        public ApiAuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext, IAuthOptions authOptions)
        {
            if (authOptions.AuthenticationType == "Bearer")
            {
                var token = httpContext.GetRequestToken();
                if (!string.IsNullOrEmpty(token))
                {
                    AppUser loggedUser = Util.ReadToken<AppUser>(token, authOptions.KEY);
                    if (loggedUser != null && !Convert.ToBoolean(httpContext?.User?.Identity?.IsAuthenticated))
                    {
                        httpContext.Items.TryAdd("loggedUser", loggedUser);
                        await GeneralContext.HttpContext.SignInAsync(loggedUser);
                    }
                }
            }

            // Call the next delegate/middleware in the pipeline
            await _next(httpContext);
        }
    }
}
