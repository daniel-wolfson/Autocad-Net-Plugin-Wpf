using ID.Infrastructure.Enums;
using ID.Infrastructure.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ID.Infrastructure.Filters
{
    //[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class PreventDuplicateRequestAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.Session.Get(SessionKeys.RequestVerificationToken) == null) return;

            var currentToken = filterContext.HttpContext.Session.Get(SessionKeys.RequestVerificationToken);

            if (filterContext.HttpContext.Session.Get(SessionKeys.LastProcessedToken) == null)
            {
                filterContext.HttpContext.Session.Set(SessionKeys.LastProcessedToken, currentToken);
                return;
            }

            lock (filterContext.HttpContext.Session.Get(SessionKeys.LastProcessedToken))
            {
                var lastToken = filterContext.HttpContext.Session.Get(SessionKeys.LastProcessedToken);
                if (lastToken == currentToken)
                {
                    ////  filterContext.Controller.ViewData.ModelState.AddModelError("", "Looks like you accidentally tried to double post.");
                    return;
                }
                filterContext.HttpContext.Session.Set(SessionKeys.LastProcessedToken, currentToken);
            }
        }
    }
}
