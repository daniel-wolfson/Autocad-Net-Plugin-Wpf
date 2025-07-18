using ID.Infrastructure.Interfaces;
using Newtonsoft.Json;
using System.Linq;
using System.Security.Claims;

namespace ID.Infrastructure.Extensions
{
    public static class IdentityExtensions
    {
        public static IAppUser ToAppUser<T>(this ClaimsPrincipal principal)
        {
            IAppUser appUser = null;
            var userData = principal.Claims.FirstOrDefault(x => x.Type == ClaimTypes.UserData);
            if (userData != null)
                appUser = JsonConvert.DeserializeObject<T>(userData.Value) as IAppUser;
            return appUser;
        }
    }
}
