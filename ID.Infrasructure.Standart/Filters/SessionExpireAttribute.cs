using ID.Infrastructure.Enums;
using ID.Infrastructure.Extensions;
using ID.Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Generic;

namespace ID.Infrastructure.Filters
{
    public class SessionExpireAttribute : ActionFilterAttribute // Attribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            IList<string> actions = new[] { "index" };

            string actionName = filterContext.RouteData.Values["action"].ToString().ToLower();
            string controllerName = filterContext.RouteData.Values["controller"].ToString().ToLower();

            HttpContext context = filterContext.HttpContext;

            if (actions.Contains(actionName) && filterContext.HttpContext.Session?.Get<AppUser>(SessionKeys.AppUser) == null)
            {
                filterContext.Result = new RedirectResult("~/Home/Login");
                return;
            }

            base.OnActionExecuting(filterContext);
        }

    }
}
