using System.ComponentModel;

namespace ID.Infrastructure.Enums
{
    public enum ResponseMessageTypes
    {
        [Description("Request successful")]
        Success,
        [Description("Request with exceptions")]
        Exception,
        [Description("Request denied.")]
        UnAuthorized,
        [Description("Request with validation error(s)")]
        ValidationError,
        [Description("Unable to process the request")]
        Failure
    }
}
