using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using System.Net;

namespace General.Models
{

    public class ServiceObjectResult<TResult> : ObjectResult
    {
        public ServiceObjectResult(TResult value, HttpStatusCode statusCode, string message) : base(value)
        {
            ModelState = new ModelStateDictionary();

            if (!string.IsNullOrEmpty(message))
                ModelState.AddModelError("message", message);

            DeclaredType = typeof(TResult);
            StatusCode = (int)statusCode;
            Value = value;
        }

        public ServiceObjectResult(TResult value, HttpStatusCode statusCode, ModelStateDictionary messages = null) : base(value)
        {
            ModelState = new ModelStateDictionary();

            if (messages != null)
                ModelState = messages;

            DeclaredType = typeof(TResult);
            StatusCode = (int)statusCode;
            Value = value;
        }

        [JsonProperty]
        public new int? StatusCode { get; set; }
        [JsonProperty]
        public new TResult Value { get; set; }
        [JsonProperty]
        public ModelStateDictionary ModelState { get; set; }
        //public string RequestUrl { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class ServiceResult : ServiceObjectResult<object>
    {
        public ServiceResult(object value, HttpStatusCode statusCode, string message = "") : base(value, statusCode, message)
        {
        }

        public ServiceResult(object value, HttpStatusCode statusCode, ModelStateDictionary messages = null) : base(value, statusCode, messages)
        {
        }
    }
}
