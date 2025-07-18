using ID.Infrastructure.Core;
using ID.Infrastructure.Helpers;
using ID.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;

namespace ID.Infrastructure.Auth
{
    public class ApiAuthorizationHandler : AuthorizationHandler<TokenRequirement>
    {
        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            TokenRequirement requirement)
        {
            IAuthOptions authOptions = requirement.AuthOptions; //GeneralContext.GetService<IAuthOptions>();
            var httpContext = GeneralContext.HttpContext;

            string authHeader = httpContext.Request.Headers["Authorization"];
            if (!string.IsNullOrEmpty(authHeader))
            {
                authHeader = authHeader.Replace("Bearer ", "");
                if (Util.VerifyToken(authHeader, authOptions.KEY))
                {
                    context.Succeed(requirement);
                }
            }
            else
            {
                context.Fail();
                throw new UnauthorizedAccessException();
            }

            await Task.CompletedTask;
        }

        public override Task HandleAsync(AuthorizationHandlerContext context)
        {
            return base.HandleAsync(context);
        }
    }
}