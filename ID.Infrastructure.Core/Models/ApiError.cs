using ID.Infrastructure.Core;
using ID.Infrastructure.Extensions;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;

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
            var environment = GeneralContext.GetService<IWebHostEnvironment>();
            string message = ex.GetApiMessageInfo();
            StatusCode = $"500: {HttpStatusCode.InternalServerError}";

            if (environment.EnvironmentName == "Development")
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
