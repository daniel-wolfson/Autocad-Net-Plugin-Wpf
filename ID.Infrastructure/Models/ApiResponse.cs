using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace ID.Infrastructure.Models
{
    public class ApiResponse
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

        public ApiResponse(int statusCode, IEnumerable<ApiValidationError> validationErrors = null)
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
}
