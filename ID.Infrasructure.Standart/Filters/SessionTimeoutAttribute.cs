using ID.Infrastructure.Enums;
using ID.Infrastructure.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ID.Infrastructure.Filters
{
    public class SessionTimeoutAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.User.Identity.IsAuthenticated
                && !filterContext.HttpContext.Session.Contains(SessionKeys.AppTimeStamp))
            //&& !filterContext.HttpContext.Request.Headers["Referer"].ToString().Contains("AllEvents"))
            {
                filterContext.HttpContext.Request.Cookies.Clear();
                filterContext.Result = new RedirectResult("/");
                return;
            }
            base.OnActionExecuting(filterContext);
        }
    }
}
