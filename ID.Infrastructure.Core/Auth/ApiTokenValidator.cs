using ID.Infrastructure.Core;
using ID.Infrastructure.Helpers;
using ID.Infrastructure.Interfaces;
using ID.Infrastructure.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ID.Infrastructure.Auth
{
    public class ApiTokenValidator : ISecurityTokenValidator
    {
        public bool CanValidateToken => true;
        public int MaximumTokenSizeInBytes { get; set; }

        public bool CanReadToken(string securityToken)
        {
            return true;
        }

        public ClaimsPrincipal ValidateToken(string securityToken, TokenValidationParameters validationParameters, out SecurityToken validatedToken)
        {
            ClaimsPrincipal result = null;
            SecurityToken token = null;

            Task.WaitAll(Task.Run(() =>
            {
                IAuthOptions authOptions = GeneralContext.GetService<IAuthOptions>();
                AppUser appUser = Util.ReadToken<AppUser>(securityToken, authOptions.KEY);
                if (appUser == null)
                {
                    result = null;
                    token = null;
                }
                else
                {
                    var claims = new List<Claim> { new Claim(ClaimTypes.Name, appUser.UserName) };
                    var newClaimsIdentity = new ClaimsIdentity(claims);
                    result = new ClaimsPrincipal(newClaimsIdentity);

                    var key = Encoding.ASCII.GetBytes(authOptions.KEY);
                    token = new ApiSecurityToken(appUser.Id, "Token",
                                                  new SymmetricSecurityKey(key),
                                                  new SymmetricSecurityKey(key),
                                                  DateTime.Now,
                                                  DateTime.Now.AddDays(authOptions.LIFETIME));

                    var authProperties = new AuthenticationProperties
                    {
                        AllowRefresh = true,
                        ExpiresUtc = DateTimeOffset.Now.AddDays(365),
                        IsPersistent = true,
                    };

                    //AuthenticationOptions
                    //await GeneralContext.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, result, authProperties);
                    //if (GeneralContext.HttpContext.User.Identity.IsAuthenticated)
                    //{
                    //    //IAuthenticationService authenticationService = GeneralContext.GetService<IAuthenticationService>();
                    //    await GeneralContext.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, result);
                    //}
                }
            }));

            validatedToken = token;
            return result;
        }
    }
}
