using ID.Infrastructure.Interfaces;
using ID.Infrastructure.Models;
using Microsoft.Owin.Security;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Web;

namespace ID.Infrastructure.Extensions
{
    public static class HttpContextExtensions
    {

        public static string GetRequestToken(this HttpContext httpContext)
        {
            string authHeader = httpContext.Request.Headers["Authorization"];
            return authHeader?.Replace("Bearer ", "") ?? string.Empty;
        }

        public static string GetRequestUserData(this HttpContext httpContext)
        {
            string result = string.Empty;
            var handler = new JwtSecurityTokenHandler();
            string authHeader = httpContext.Request.Headers["Authorization"];

            if (!string.IsNullOrEmpty(authHeader))
            {
                var SecurityToken = handler.ReadToken(authHeader) as JwtSecurityToken;
                result = SecurityToken.Claims.First(claim => claim.Type == ClaimTypes.UserData).Value;
            }

            return result;
        }

        public static bool SignInAsync(this HttpContext httpContext, IAppUser appUser)
        {
            var claims = new List<Claim>() { new Claim(ClaimTypes.Name, appUser.UserName) };
            claims.Add(new Claim(ClaimTypes.UserData, JsonConvert.SerializeObject(appUser)));

            var identity = new ClaimsIdentity(claims, "DefaultAuthScheme");
            var principal = new ClaimsPrincipal(identity);
            var authProperties = new AuthenticationProperties
            {
                AllowRefresh = true,
                ExpiresUtc = DateTimeOffset.Now.AddDays(365),
                IsPersistent = true,
            };

            //TODO:
            //await httpContext.SignInAsync("DefaultAuthScheme", principal, authProperties);

            return principal.Identity.IsAuthenticated;
        }

        public static Type GetActionReturnType(this HttpContext context)
        {
            Type responseDeclaredType = null;
            EndPoint endpoint = null; // context.GetEndpoint();

            if (endpoint != null)
            {
                //var controllerActionDescriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
                //if (controllerActionDescriptor != null)
                //{
                //    responseDeclaredType = controllerActionDescriptor.MethodInfo.ReturnType;

                //    if (controllerActionDescriptor.MethodInfo.ReturnType.IsGenericType)
                //    {
                //        if (controllerActionDescriptor.MethodInfo.ReturnType.GetGenericTypeDefinition() == typeof(ActionResult<>))
                //        {
                //            responseDeclaredType = controllerActionDescriptor.MethodInfo.ReturnType.GetGenericArguments()[0];
                //        }
                //    }
                //}
            }

            return responseDeclaredType;
        }

        public static string GetRequestUrl(this HttpContext contex)
        {
            return "";
            //return $"{contex.Request?.Scheme}://{contex.Request?.Host.Value}{contex.Request.Path}{contex.Request.QueryString}";
        }


    }

    public static class EndpointHttpContextExtensions
    {
        //
        // Summary:
        //     Extension method for getting the Microsoft.AspNetCore.Http.Endpoint for the current
        //     request.
        //
        // Parameters:
        //   context:
        //     The Microsoft.AspNetCore.Http.HttpContext context.
        //
        // Returns:
        //     The Microsoft.AspNetCore.Http.Endpoint.
        public static Endpoint GetEndpoint(this HttpContext context) { return new Endpoint(() => { }, null, ""); }
        //
        // Summary:
        //     Extension method for setting the Microsoft.AspNetCore.Http.Endpoint for the current
        //     request.
        //
        // Parameters:
        //   context:
        //     The Microsoft.AspNetCore.Http.HttpContext context.
        //
        //   endpoint:
        //     The Microsoft.AspNetCore.Http.Endpoint.
        public static void SetEndpoint(this HttpContext context, Endpoint endpoint) { }
    }
}
