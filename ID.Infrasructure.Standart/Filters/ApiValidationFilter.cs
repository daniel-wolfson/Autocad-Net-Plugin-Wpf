using ID.Infrastructure.Core;
using ID.Infrastructure.Extensions;
using ID.Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Threading.Tasks;

namespace ID.Infrastructure.Filters
{
    public class ApiValidationFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.ModelState.IsValid)
            {
                var contextAccessor = GeneralContext.GetService<IHttpContextAccessor>();
                var errors = context.ModelState.GetModelErrors();

                GeneralContext.Logger.Error($"Crpm ModelState Error Information: {Environment.NewLine}" +
                                 $"Schema: {contextAccessor.HttpContext.Request.Scheme} " +
                                 $"Host: {contextAccessor.HttpContext.Request.Host} " +
                                 $"Path: {contextAccessor.HttpContext.Request.Path} " +
                                 $"QueryString: {contextAccessor.HttpContext.Request.QueryString} " +
                                 $"Models Errors: {string.Join(",", errors)}");

                context.Result = new BadRequestObjectResult(new ApiResponse(400, errors));
                return;
            }

            await next();
        }
    }
}
