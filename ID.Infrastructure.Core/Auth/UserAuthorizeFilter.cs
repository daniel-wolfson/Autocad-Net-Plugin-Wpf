using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Authorization;

namespace ID.Infrastructure.Auth
{
    public class UserAuthorizeFilter : IAsyncAuthorizationFilter
    {
        public AuthorizationPolicy Policy { get; }

        public UserAuthorizeFilter(AuthorizationPolicy policy)
        {
            Policy = policy ?? throw new ArgumentNullException(nameof(policy));
        }

        public Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // Allow Anonymous skips all authorization
            if (context.Filters.Any(item => item is IAllowAnonymousFilter))
            {
                return null;
            }

            //var policyEvaluator = context.HttpContext.RequestServices.GetRequiredService<IPolicyEvaluator>();
            //var authenticateResult = await policyEvaluator.AuthenticateAsync(Policy, context.HttpContext);
            //var authorizeResult = await policyEvaluator.AuthorizeAsync(Policy, authenticateResult, context.HttpContext, context);

            //if (authorizeResult.Challenged)
            //{
            //    // Return custom 401 result
            //    context.Result = new CustomUnauthorizedResult("Authorization failed.");
            //}
            //else if (authorizeResult.Forbidden)
            //{
            //    // Return default 403 result
            //    context.Result = new ForbidResult(Policy.AuthenticationSchemes.ToArray());
            //}
            context.Result = new UserUnauthorizedResult("Authorization failed.");
            return null;
        }
    }
}
