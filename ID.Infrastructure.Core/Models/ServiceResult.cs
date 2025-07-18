using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Net;

namespace ID.Infrastructure.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    [Serializable]
    public class ServiceResult<TResult> : ObjectResult
    {
        public ServiceResult(TResult value, HttpStatusCode statusCode, string message = "") : base(value)
        {
            StatusCode = (int)statusCode;
            Message = message;
            Value = value;
        }

        //[JsonProperty]
        //public int? StatusCode { get; set; }
        //[JsonProperty]
        //public TResult Value { get; set; }
        [JsonProperty]
        public string Message { get; set; }
        [JsonProperty]
        public string Description { get; set; }
        [JsonProperty]
        public string RequestUrl { get; set; }

        public bool IsSuccessStatusCode()
        {
            return StatusCode >= 200 && StatusCode <= 299;
        }
    }

    public class ServiceResult : ServiceResult<object>
    {
        public ServiceResult(object value, HttpStatusCode statusCode, string message = "") : base(value, statusCode, message)
        {
            StatusCode = (int)statusCode;
            Message = message;
            Value = value;
        }
    }
}
