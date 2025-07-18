using System;
using System.Runtime.Serialization;

namespace General.Models
{
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
        public AppError ResponseException { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public object Result { get; set; }

        public string ClassName { get; set; }
        public string MethodName { get; set; }
        public int? LineNumber { get; set; }
        
        public string StackTrace { get; set; }

        public AppResponse(int statusCode, string description = "", object result = null, AppError apiError = null)
        {
            this.StatusCode = statusCode;
            this.Description = description;
            this.Result = result;
            this.StackTrace = apiError != null ? apiError.StackTrace : "";
            this.ResponseException = apiError;
        }
    }
}
