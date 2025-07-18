using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ID.Infrastructure.Permissions
{
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            Claim claim = context.User.FindFirst(c => c.Type == "permission-foo");
            if (requirement.Permissions.Any() && claim != null) //TODO: your code
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
}
