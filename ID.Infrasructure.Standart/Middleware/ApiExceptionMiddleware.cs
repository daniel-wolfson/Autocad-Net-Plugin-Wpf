using ID.Infrastructure.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using IHostingEnvironment = Microsoft.Extensions.Hosting.IHostingEnvironment;

namespace ID.Infrastructure.Middleware
{
    public class ApiExceptionMiddleware
    {
        private readonly IHostingEnvironment _environment;
        private const string DefaultErrorMessage = "A server error occurred.";

        public ApiExceptionMiddleware(IHostingEnvironment environment)
        {
            _environment = environment;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var ex = httpContext.Features.Get<IExceptionHandlerFeature>()?.Error;
            if (ex == null)
                return;

            var error = new ApiError(ex);
            if (_environment.IsDevelopment())
            {
                error.Message = ex.Message;
                error.Detail = ex.StackTrace;
                error.InnerException = ex.InnerException != null ? ex.InnerException.Message : string.Empty;
            }
            else
            {
                error.Message = DefaultErrorMessage;
                error.Detail = ex.Message;
            }

            httpContext.Response.ContentType = "application/json";

            using (var writer = new StreamWriter(httpContext.Response.Body))
            {
                new JsonSerializer().Serialize(writer, error);
                await writer.FlushAsync().ConfigureAwait(false);
            }
        }
    }
}
