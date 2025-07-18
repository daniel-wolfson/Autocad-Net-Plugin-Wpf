using ID.Infrastructure.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;
using Serilog.Context;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;

namespace ID.Infrastructure.Filters
{
    public class ApiExceptionFilter : ExceptionFilterAttribute
    {
        private IDictionary<string, object> _actionArguments;

        public override void OnException(ExceptionContext context)
        {
            ApiError apiError = new ApiError(500, "Error");
            AppResponse apiResponse = null;
            int lineNumber = 0;
            int code = 0;

            if (context.Exception is ApiException)
            {
                var ex = context.Exception as ApiException;
                apiError = new ApiError(500, ex.Message)
                {
                    //ValidationErrors = ex.Errors,
                    //ReferenceErrorCode = ex.ReferenceErrorCode,
                };
                code = 500; //ex.StatusCode;
            }
            else if (context.Exception is UnauthorizedAccessException)
            {
                apiError = new ApiError(401, "Unauthorized Access");
                code = (int)HttpStatusCode.Unauthorized;
            }
            else
            {

#if DEBUG
                apiError.StackTrace = context.Exception.StackTrace;
                StackFrame stackFrameInstance = GetStackFrameInstance(context);
                lineNumber = 0; // (int)stackFrameInstance?.GetFileLineNumber();
#endif
                apiError.Message = context.Exception.GetBaseException().Message;
                code = (int)HttpStatusCode.InternalServerError;
            }

            ControllerActionDescriptor controllerActionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;

            var actionAttributes = controllerActionDescriptor?.MethodInfo?.GetCustomAttributes(true);
            var action = controllerActionDescriptor?.ActionName;
            var type = controllerActionDescriptor?.MethodInfo.DeclaringType;
            //var method = type?.GetMethod(action); not work

            apiResponse = new AppResponse(code, apiError.Message, null, apiError)
            {
                MethodName = action,
                ClassName = type?.Name,
                LineNumber = lineNumber,
                Result = type != null && type.IsValueType && Nullable.GetUnderlyingType(type) == null
                    ? Activator.CreateInstance(type) : null
            };

            using (LogContext.PushProperty("Url", $" Url={context.HttpContext.Request.Path.ToString()}"))
            {
                Log.Logger.Error(apiError.Message);
            }

            context.ExceptionHandled = true;
            context.Result = new ObjectResult(apiResponse)
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                DeclaredType = typeof(AppResponse)
            };
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            _actionArguments = context.ActionArguments;
        }

        private StackFrame GetStackFrameInstance(ExceptionContext context)
        {
            var stackTrace = new StackTrace(context.Exception, true);
            StackFrame stackFrameInstance = null;

            if (stackTrace.GetFrames().Length > 0)
            {
                for (int i = 0; i < stackTrace.GetFrames().Length; i++)
                {
                    if (stackTrace.GetFrames()[i].ToString().Contains("Service"))
                    {
                        stackFrameInstance = stackTrace.GetFrames()[i];
                        break;
                    }
                }
            }
            return stackFrameInstance;
        }
    }


}
