using ID.Infrastructure.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Threading.Tasks;

namespace ID.Infrastructure.Permissions
{
    public class AuthorizePermissionAttribute : TypeFilterAttribute
    {
        public AuthorizePermissionAttribute(params PermissionTypes[] permissions)
            : base(typeof(PermissionFilterV1))
        {
            Arguments = new[] { new PermissionRequirement(permissions) };
            Order = int.MinValue;
        }
    }

    public class PermissionFilterV1 : Attribute, IAsyncAuthorizationFilter
    {
        private readonly IAuthorizationService _authService;
        private readonly PermissionTypes[] _permissions;

        public PermissionFilterV1(IAuthorizationService authService, PermissionTypes[] permissions)
        {
            _authService = authService;
            _permissions = permissions;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var result = await _authService.AuthorizeAsync(context.HttpContext.User, null, new PermissionRequirement(_permissions));

            if (result == AuthorizationResult.Failed())
                context.Result = new ChallengeResult();
        }
    }
}
