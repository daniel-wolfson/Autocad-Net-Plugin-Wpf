using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.Localization;
using System.Web;

namespace ID.Infrastructure.Extensions
{
    public static class LocalizeExtensions
    {
        public static HtmlString Decode(this LocalizedString localizedString)
        {
            return new HtmlString(HttpUtility.HtmlDecode(localizedString.Value));
        }
    }
}
