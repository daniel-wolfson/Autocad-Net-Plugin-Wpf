using ID.Infrastructure.Core;
using ID.Infrastructure.Helpers;
using ID.Infrastructure.Interfaces;
using ID.Infrastructure.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

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

        public static IAppUser ToAppUser<T>(this ClaimsPrincipal principal)
        {
            IAppUser appUser = null;
            var userData = principal.Claims.FirstOrDefault(x => x.Type == ClaimTypes.UserData);
            if (userData != null)
            {
                IAuthOptions AuthOptions = GeneralContext.GetService<IAuthOptions>();
                appUser = Util.DecryptData<AppUser>(userData.Value, AuthOptions.KEY);
            }
            return appUser;
        }
        public static Dictionary<string, IEnumerable<IAppPermissionsInfo>> GetClaimPermissionTabs(this ClaimsPrincipal principal, string claimName)
        {
            //var permissionTabs = GeneralContext.GetClaim<Dictionary<string, IEnumerable<RolePermissionsTabsInfoExt>>>("");
            //if (permissionTabs != null)
            //    return permissionTabs.ToDictionary(k => k.Key, v => v.Value.Cast<IAppPermissionsInfo>());
            //else
            return null;
        }

        public static IEnumerable<IAppPermissionsInfo> GetClaimRoles<T>(this ClaimsPrincipal principal, string claimName)
        {
            var results = new List<IAppPermissionsInfo>();
            IEnumerable<Claim> values = principal.FindAll(claimName);
            if (values != null)
            {
                foreach (var value in values)
                {
                    results.Add(JsonConvert.DeserializeObject<RolePermissionsTabsInfoExt>(value.Value));
                }
            }
            return results;
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
