using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ID.Infrastructure.Enums
{
    public enum HttpRequestXHeader
    {

        [Display(Name = "X-APP-USER", Description = "Request custom header[X-APP-USER]")]
        [DefaultValue("")]
        User,

        [Display(Name = "X-APP-DATA", Description = "Request custom header[X-APP-DATA]")]
        [DefaultValue("{}")]
        Data,

        [Display(Name = "X-APP-CLIENT", Description = "Request custom header[X-APP-CLIENT]")]
        [DefaultValue("TMS")]
        Client,

        [Display(Name = "X-APP-ERR", Description = "Request custom header[X-APP-ERR]")]
        [DefaultValue("")]
        Error,

        [Display(Name = "X-APP-MSG", Description = "Request custom header[X-APP-MSG]")]
        [DefaultValue("")]
        Message,

        [Display(Name = "X-APP-RETURNURL", Description = "Request custom header[X-APP-RETURNURL]")]
        [DefaultValue("")]
        ReturnUrl,

        [Display(Name = "X-NAMED-CLIENT", Description = "Request custom header[X-NAMED-CLIENT]")]
        [DefaultValue("X-Named-Client")]
        XNamedClient
    }
}
