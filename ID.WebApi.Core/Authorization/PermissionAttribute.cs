using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Security.Claims;

namespace ID.Api.Authorization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class PermissionAttribute : AuthorizeAttribute
    {
        public PermissionAttribute(string roles) : base("Permission")
        {
            Roles = roles;
        }
    }

    public class AuthorizeAttribute1 : TypeFilterAttribute
    {
        public AuthorizeAttribute1(PermissionRoles item, PermissionActions action)
        : base(typeof(AuthorizeActionFilter))
        {
            Arguments = new object[] { item, action };
        }
    }

    public class AuthorizeActionFilter : IAuthorizationFilter
    {
        private readonly PermissionRoles _item;
        private readonly PermissionActions _action;
        private readonly AuthorizationFilterContext _context;
        public AuthorizeActionFilter(PermissionRoles item, PermissionActions action)
        {
            _item = item;
            _action = action;
        }
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            bool isAuthorized = Authorize(context.HttpContext.User, _item, _action);

            if (!isAuthorized)
            {
                context.Result = new ForbidResult();
            }
        }

        private bool Authorize(ClaimsPrincipal user, PermissionRoles item, PermissionActions action)
        {
            return user.HasClaim(c => c.Type == ClaimTypes.Role.ToString() && c.Value == "Admin");
        }
    }
}