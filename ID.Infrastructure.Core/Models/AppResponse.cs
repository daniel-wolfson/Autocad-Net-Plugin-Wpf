using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization;

namespace ID.Infrastructure.Models
{
    public class ApiResponse1
    {
        public string TraceId { get; set; }
        public string Message { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object Value { get; set; }

        public string RequestUrl { get; set; }

        public Type ActionType { get; set; }

        public string StatusCode { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ApiError Error { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<ApiValidationError> Errors { get; set; }

        public ApiResponse1(int statusCode, IEnumerable<ApiValidationError> validationErrors = null)
        {
            StatusCode = ((HttpStatusCode)statusCode).ToString();
            Errors = validationErrors; //Error = new ApiError(400, validationErrors);
        }

        public override string ToString()
        {
            return $"Crpm ApiResponse {base.ToString()}:{Environment.NewLine}" +
                   $"TraceId:{this.TraceId} " +
                   $"RequestUrl: {this.RequestUrl} " +
                   $"Value: {this.Value} " +
                   $"Message: {this.Message} " +
                   $"Error time: {this.Error} " +
                   $"StatusCode: {this.StatusCode}";
        }
    }

    [DataContract]
    public class AppResponse
    {
        [DataMember]
        public string Version { get; set; } = "1.0.0.0";

        [DataMember]
        public int StatusCode { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public string Description { get; set; }


        [DataMember(EmitDefaultValue = false)]
        public ApiError ResponseException { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public object Result { get; set; }

        public string ClassName { get; set; }
        public string MethodName { get; set; }
        public int? LineNumber { get; set; }

        public string StackTrace { get; set; }

        public AppResponse(int statusCode, string description = "", object result = null, ApiError apiError = null)
        {
            this.StatusCode = statusCode;
            this.Description = description;
            this.Result = result;
            this.StackTrace = apiError != null ? apiError.StackTrace : "";
            this.ResponseException = apiError;
        }
    }
}
