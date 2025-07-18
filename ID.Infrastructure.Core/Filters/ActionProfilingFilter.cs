using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ID.Infrastructure.Core;
using ID.Infrastructure.Models;

namespace ID.Infrastructure.Filters
{
    public class ActionProfilingFilter : IActionFilter
    {
        //private Stopwatch stopwatch;
        //private IDisposable actionProp;
        //private IDisposable requestStartProp;
        //private IDisposable requestUrlProp;

        public void OnActionExecuting(ActionExecutingContext context)
        {
            //requestStartProp = LogContext.PushProperty("RequestStart", $"RequestStart={context.HttpContext.TraceIdentifier}");
            //requestUrlProp = LogContext.PushProperty("Url", $" Url={context.HttpContext.Request.Path.ToString()}");

            //GeneralContext.Logger.Information("");

            //stopwatch = Stopwatch.StartNew();
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            //stopwatch.Stop();
            //requestStartProp?.Dispose();

            //using (LogContext.PushProperty("Url", $" Url={context.HttpContext.Request.Path.ToString()}"))
            //using (LogContext.PushProperty("RequestEnd", $"RequestEnd={context.HttpContext.TraceIdentifier}"))
            //{
            //    if (context.Result != null)
            //    {
            //        var resultType = context.Result.GetType();
            //        if (resultType == typeof(ObjectResult) || resultType == typeof(ServiceResult<>) || resultType == typeof(ServiceResult))
            //        {
            //            object value = ((ObjectResult)context.Result).Value;

            //            if (typeof(IEnumerable).IsAssignableFrom(resultType))
            //            {
            //                var listCount = ((List<object>)value).Count;
            //                GeneralContext.Logger.Information($"Count of items: {listCount}");
            //            }
            //        }
            //    }
            //    else {
            //        GeneralContext.Logger.Warning($"Result is null");
            //    }

            //    using (LogContext.PushProperty("TimeElapsed", $"={stopwatch.Elapsed}"))
            //    {
            //        GeneralContext.Logger.Information("");                    
            //    }
            //}
        }
    }
}