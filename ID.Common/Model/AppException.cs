using System;
using System.Collections.Generic;
using System.Globalization;

namespace General.Models
{
    public class AppException : Exception
    {
        public int StatusCode { get; set; }
        public IEnumerable<ValidationError> Errors { get; set; }
        public string ReferenceErrorCode { get; set; }
        public string ReferenceDocumentLink { get; set; }

        public AppException(string message,
                            int statusCode = 500,
                            IEnumerable<ValidationError> errors = null,
                            string errorCode = "",
                            string refLink = "") :
            base(message)
        {
            this.StatusCode = statusCode;
            this.Errors = errors;
            this.ReferenceErrorCode = errorCode;
            this.ReferenceDocumentLink = refLink;
        }
        public AppException(Exception ex, params object[] args)
            : base(string.Format(new CultureInfo("en-US"), (ex.InnerException ?? ex).ToString(), args))
        {
        }

        public AppException(Exception ex, int statusCode = 500) : base(ex.Message)
        {
            StatusCode = statusCode;
        }
    }
}
