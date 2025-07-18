using ID.Infrastructure.Core;
using ID.Infrastructure.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net;
using System.Reflection;

namespace ID.Infrastructure.Extensions
{
    public static class ExceptionExtensions
    {
        public static ApiResponse GetResponseDetails(this HttpContext context, Exception apiException = null, string message = null)
        {
            ApiResponse response = null;
            try
            {
                // get action  action ReturnType
                Type responseDeclaredType = context.GetActionReturnType();
                HttpStatusCode statusCode = apiException != null ? HttpStatusCode.Conflict : (HttpStatusCode)context.Response.StatusCode;

                // create api response
                response = new ApiResponse(context.Response.StatusCode)
                {
                    TraceId = GeneralContext.CreateTraceId(),
                    RequestUrl = context.GetRequestUrl(),
                    Value = responseDeclaredType?.GetDefault(),
                    ActionType = responseDeclaredType,
                    StatusCode = $"{(int)statusCode}: {statusCode}",
                    Message = message ?? apiException?.GetApiMessageInfo() ?? ((HttpStatusCode)context.Response.StatusCode).ToString(),
                    Error = apiException != null ? new ApiError(apiException) : null
                };

                GeneralContext.Logger.Error($"traceId: {response.TraceId}, error: {response.Message}");
            }
            catch (Exception ex)
            {
                GeneralContext.Logger.Error(ex.GetApiMessageInfo("Unhandled exception"));
                //context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            }

            return response;
        }

        /// <summary> get message from innerException or exception message </summary>
        public static string GetApiMessageInfo(this Exception exception, string headerMessage = null)
        {
            return $"{Assembly.GetEntryAssembly().GetName().Name} " +
                   $"{exception.GetType().Name}: " +
                   $"{(headerMessage != null ? headerMessage + "; " : "")}" +
                   $"{(exception.InnerException ?? exception).Message}";
        }

        public static StackFrame GetStackFrameInstance(this Exception exception)
        {
            var stackTrace = new StackTrace(exception, true);
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

    public static class ExceptionMiddlewareExtensions
    {
        public static void UseApiExceptionHandler(this IApplicationBuilder app)
        {
            //app.UseExceptionHandler(new ExceptionHandlerOptions
            //{
            //    ExceptionHandler = new JsonExceptionMiddleware(env).Invoke
            //});
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    context.Response.StatusCode = 500;
                    context.Response.ContentType = "application/json";

                    var exception = context.Features.Get<IExceptionHandlerPathFeature>();

                    // Use exceptionHandlerPathFeature to process the exception (for example, 
                    // logging), but do NOT expose sensitive error information directly to 
                    // the client.

                    var response = context.GetResponseDetails(exception?.Error);

                    var json = JsonConvert.SerializeObject(response);
                    await context.Response.WriteAsync(json);
                });
            });
        }
    }

    public class ErrorDetails
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
