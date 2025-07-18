using Microsoft.AspNetCore.Authorization;

namespace ID.Api.Authorization
{
    public class PermissionAuthorizationRequirement : IAuthorizationRequirement
    {
        public string Role { get; private set; }

        public PermissionAuthorizationRequirement(string role) { Role = role; }
    }
}
