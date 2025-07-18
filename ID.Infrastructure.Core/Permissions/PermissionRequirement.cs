using ID.Infrastructure.Enums;
using Microsoft.AspNetCore.Authorization;

namespace ID.Infrastructure.Permissions
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public PermissionRequirement(PermissionTypes[] permissions)
        {
            Permissions = permissions;
        }
        public PermissionTypes[] Permissions { get; set; }
    }
}
