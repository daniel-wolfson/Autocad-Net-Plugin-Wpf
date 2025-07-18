using ID.Infrastructure.Core;
using ID.Infrastructure.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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

    public class AppError
    {
        public bool IsError { get; set; }
        public string ExceptionMessage { get; set; }
        public string StackTrace { get; set; }
        public string ReferenceErrorCode { get; set; }
        public string ReferenceDocumentLink { get; set; }
        public object Data { get; set; }
        public IEnumerable<ValidationError> ValidationErrors { get; set; }

        public AppError(string message)
        {
            this.ExceptionMessage = message;
            this.IsError = true;
        }

        public AppError(ModelStateDictionary modelState)
        {
            this.IsError = true;
            if (modelState != null && modelState.Any(m => m.Value.Errors.Count > 0))
            {
                this.ExceptionMessage = "Please correct the specified validation errors and try again.";
                this.ValidationErrors = modelState.Keys
                .SelectMany(key => modelState[key].Errors.Select(x => new ValidationError(key, x.ErrorMessage)))
                .ToList();

            }
        }
    }

    public class ValidationError
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Field { get; }

        public string Message { get; }

        public ValidationError(string field, string message)
        {
            Field = field != string.Empty ? field : null;
            Message = message;
        }
    }
}
