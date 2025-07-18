using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;

namespace ID.Infrastructure.Permissions
{
    public class TypeIdHandler : AuthorizationHandler<TypeIdRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, TypeIdRequirement requirement)
        {
            bool hasClaim = context.User.HasClaim(c => c.Type == "age");
            bool hasIdentity = context.User.Identities.Any(i => i.AuthenticationType == "MyCookieMiddlewareInstance");
            string claimValue = context.User.FindFirst(c => c.Type == "age")?.Value;

            if (!string.IsNullOrEmpty(claimValue) && int.Parse(claimValue) > requirement.TypeId)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }
            return Task.CompletedTask;
        }
    }
}
