using ID.Infrastructure.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;

namespace ID.Infrastructure.Extensions
{
    public static class IdentityExtensions
    {
        public static List<string> Roles(this ClaimsIdentity identity)
        {
            return identity.Claims
                           .Where(c => c.Type == ClaimTypes.Role)
                           .Select(c => c.Value)
                           .ToList();
        }

        public static AppUser ToAppUser<T>(this IPrincipal principal)
        {
            AppUser appUser = null;
            //TODO: ???
            //var userData = principal.Claims.FirstOrDefault(x => x.Type == ClaimTypes.UserData);
            //if (userData != null)
            //{
            //    IAuthOptions authOptions = GeneralContext.GetService<IAuthOptions>();
            //    appUser = Util.DecryptData<AppUser>(userData.Value, authOptions.KEY);
            //}
            return appUser;
        }

        public static T GetClaim<T>(this ClaimsPrincipal principal, string claimName)
        {
            T result = default;
            var value = principal.FindFirst(claimName);
            if (value != null)
            {
                if (typeof(T) == typeof(Dictionary<string, IEnumerable<RolePermissionsTabsInfoExt>>))
                    result = (T)Convert.ChangeType(JsonConvert.DeserializeObject<Dictionary<string, IEnumerable<RolePermissionsTabsInfoExt>>>(value.Value), typeof(Dictionary<string, IEnumerable<RolePermissionsTabsInfoExt>>));
                else
                    result = JsonConvert.DeserializeObject<T>(value.Value);
            }
            return result;
        }

        public static void AddClaim<T>(this ClaimsPrincipal principal, string claimName, T tabs)
        {
            ClaimsIdentity claimsIdentity = (ClaimsIdentity)principal.Identity;
            claimsIdentity.AddClaim(new Claim(claimName, JsonConvert.SerializeObject(tabs)));
        }
    }
}
