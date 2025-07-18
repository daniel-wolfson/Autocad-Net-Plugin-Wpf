using System;
using System.Collections.Generic;

namespace ID.Infrastructure.Models
{
    public class ApiException : Exception //IExceptionHandlerFeature
    {
        Exception _error { get; }
        public ApiException(string message) : base(message)
        {
            _error = this;
            //var contextAccessor = GeneralContext.GetService<IHttpContextAccessor>();
            //contextAccessor.HttpContext.Features.Set<IExceptionHandlerFeature>(this);
        }

        public Exception Error => _error;
    }

    public class ApiException1 : Exception
    {
        public int StatusCode { get; set; }
        public IEnumerable<ApiValidationError> Errors { get; set; }

        public string ReferenceErrorCode { get; set; }
        public string ReferenceDocumentLink { get; set; }

        public ApiException1(string message,
                            int statusCode = 500,
                            IEnumerable<ApiValidationError> errors = null,
                            string errorCode = "",
                            string refLink = "") :
            base(message)
        {
            this.StatusCode = statusCode;
            this.Errors = errors;
            this.ReferenceErrorCode = errorCode;
            this.ReferenceDocumentLink = refLink;


            //var contextAccessor = GeneralContext.GetService<IHttpContextAccessor>();
            //contextAccessor.HttpContext.Features.Set<IExceptionHandlerFeature>(new ApiException(""));
            //var hashCodeItemKey = nameof(ApiException);
            //if (!contextAccessor.HttpContext.Items.Keys.Contains(hashCodeItemKey))
            //    contextAccessor.HttpContext.Items.Add(hashCodeItemKey, new ApiBadResponse(errors));
            //contextAccessor.HttpContext.Abort();
        }
        //public ApiException(Exception ex, params object[] args)
        //    : base(string.Format(new CultureInfo("en-US"), (ex.InnerException ?? ex).ToString(), args))
        //{
        //}

        //public ApiException(Exception ex, int statusCode = 500) : base(ex.Message)
        //{
        //    StatusCode = statusCode;
        //}
    }

    //public class ValidationException : Exception
    //{
    //    public ValidationException()
    //        : base("One or more validation failures have occurred.")
    //    {
    //        Errors = new Dictionary<string, string[]>();
    //    }

    //    public ValidationException(IEnumerable<ValidationFailure> failures)
    //        : this()
    //    {
    //        var failureGroups = failures
    //            .GroupBy(e => e.PropertyName, e => e.ErrorMessage);

    //        foreach (var failureGroup in failureGroups)
    //        {
    //            var propertyName = failureGroup.Key;
    //            var propertyFailures = failureGroup.ToArray();

    //            Errors.Add(propertyName, propertyFailures);
    //        }
    //    }

    //    public IDictionary<string, string[]> Errors { get; }
    //}
}
