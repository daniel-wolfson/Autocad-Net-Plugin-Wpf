using ID.Infrastructure.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Hosting;

namespace ID.Infrastructure.Models
{
    public class ApiError
    {
        private const string DefaultErrorMessage = "Crpm server unexpected error occurred.";

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Message { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string StackTrace { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ReferenceErrorCode { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Detail { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string InnerException { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<ApiValidationError> Errors { get; set; }

        public string StatusCode { get; set; }

        public ApiError(int statusCode, IEnumerable<ApiValidationError> errors)
        {
            StatusCode = $"{statusCode}: {(HttpStatusCode)statusCode}";
            Errors = new List<ApiValidationError>();
            Errors.AddRange(errors);
        }
        public ApiError(int statusCode, string message)
        {
            StatusCode = $"{statusCode}: {(HttpStatusCode)statusCode}";
            Message = Detail = message;
        }

        public ApiError(Exception ex)
        {
            var environmentName = HostingEnvironment.IsDevelopmentEnvironment ? "Development" : "Production"; //GeneralContext.GetService<IHostingEnvironment>();
            string message = ex.GetApiMessageInfo();
            StatusCode = $"500: {HttpStatusCode.InternalServerError}";

            if (environmentName == "Development")
            {
                this.Message = message;
                this.Detail = (ex.GetType() != typeof(UnauthorizedAccessException)) ? ex.StackTrace : "ApiError";
                this.InnerException = ex.InnerException?.Message;
            }
            else
            {
                this.Message = DefaultErrorMessage;
                this.Detail = message;
            }

            if (message != null)
                this.Message = message;
        }
    }
}
