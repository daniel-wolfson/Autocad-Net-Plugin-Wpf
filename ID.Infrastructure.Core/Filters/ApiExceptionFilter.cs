using ID.Infrastructure.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;

namespace ID.Infrastructure.Filters
{
    public class ApiExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            ApiException apiException = context.Exception as ApiException;
            if (apiException != null)
            {
                string json = JsonConvert.SerializeObject(apiException.Message);

                context.Result = new BadRequestObjectResult(json);
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;

                //using (LogContext.PushProperty("Url", $" Url={context.HttpContext.Request.Path.ToString()}"))
                //{
                //    GeneralContext.Logger.Error(apiError.Message);
                //}
            }
        }
    }
}
