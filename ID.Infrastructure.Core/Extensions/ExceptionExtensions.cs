using System;
using System.Diagnostics;
using System.Reflection;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using ID.Infrastructure.Models;
using ID.Infrastructure.Core;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using System.Collections.Generic;

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
        public static string GetApiMessageInfo(this Exception error)
        {
            return error.GetApiMessageInfo();
        }
        public static string GetApiMessageInfo<T>(this Exception error) where T : class
        {
            return error.GetApiMessageInfo("", typeof(T));
        }
        public static string GetApiMessageInfo<T>(this ApiError error) where T : class
        {
            return error.GetApiMessageInfo("", typeof(T));
        }

        /// <summary> get message from innerException or exception message </summary>
        public static string GetApiMessageInfo(this object error, string message = "", Type operationType = null)
        {
            var errMsg = "error";

            if (error.GetType().BaseType == typeof(Exception) || error.GetType().BaseType == typeof(SystemException))
            {
                errMsg = ((Exception)error)?.InnerException.Message ?? ((Exception)error).Message ?? errMsg;
            }
            else if (error.GetType() == typeof(ApiError) || error.GetType().BaseType == typeof(ApiError))
            {
                errMsg = ((ApiError)error)?.InnerException ?? ((ApiError)error).Message ?? errMsg;
            }
            else if (error.GetType() == typeof(string))
            {
                errMsg = error.ToString();
            }

            return $"errorHeader: {Assembly.GetEntryAssembly().GetName().Name}: {message}" +
                   $"errorMessage: {errMsg}; " +
                   $"errorType: {error.GetType().Name}; " +
                   $"{"errorOperationType:" + operationType?.GetType().Name ?? ""}";
        }

        /// <summary> Add error to last errors collection with source that it indicates to causes of the error </summary>
        public static void AddError(this List<object> errors, object error)
        {
            errors.AddError<MigrationOperation>(error);
        }

        /// <summary> Add error to last errors collection with source that it indicates to causes of the error </summary>
        public static void AddError<T>(this List<object> errors, object error) where T : class
        {
            if (error.GetType().BaseType == typeof(Exception) || error.GetType().BaseType == typeof(SystemException))
                errors.Add(error);
            else
                errors.Add(error.GetApiMessageInfo("", typeof(T)));
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
