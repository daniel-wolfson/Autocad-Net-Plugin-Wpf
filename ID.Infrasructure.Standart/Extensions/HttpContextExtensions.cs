using ID.Infrastructure.Enums;
using ID.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

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

        public static async Task<bool> SignInAsync(this HttpContext httpContext, IAppUser appUser)
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

            await httpContext.SignInAsync("DefaultAuthScheme", principal, authProperties);

            return principal.Identity.IsAuthenticated;
        }

        public static Type GetActionReturnType(this HttpContext context)
        {
            Type responseDeclaredType = null;
            Endpoint endpoint = context.GetEndpoint();

            if (endpoint != null)
            {
                var controllerActionDescriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
                if (controllerActionDescriptor != null)
                {
                    responseDeclaredType = controllerActionDescriptor.MethodInfo.ReturnType;

                    if (controllerActionDescriptor.MethodInfo.ReturnType.IsGenericType)
                    {
                        if (controllerActionDescriptor.MethodInfo.ReturnType.GetGenericTypeDefinition() == typeof(ActionResult<>))
                        {
                            responseDeclaredType = controllerActionDescriptor.MethodInfo.ReturnType.GetGenericArguments()[0];
                        }
                    }
                }
            }

            return responseDeclaredType;
        }

        public static string GetRequestUrl(this HttpContext contex)
        {
            return $"{contex.Request?.Scheme}://{contex.Request?.Host.Value}{contex.Request.Path}{contex.Request.QueryString}";
        }

        // ****************
        public static string GetRequestHeader(this HttpContext httpContext, HttpRequestXHeader key)
        {
            var headerValue = httpContext.Request?.Headers[key.GetDisplayName()].FirstOrDefault() ?? "";
            return !string.IsNullOrEmpty(headerValue) ? headerValue : key.GetDefaultValue<string>();
        }

        public static string GetRequestHeader(this HttpContext httpContext, HttpRequestHeader key)
        {
            string headerName = Enum.GetName(typeof(HttpRequestHeader), key);
            var headerValue = httpContext.Request?.Headers[key.ToString()];
            return headerValue.HasValue ? headerValue.Value.ToString() : "";
        }

        public static void AddResponseMessage(this HttpContext httpContext, string message)
        {
            var response = httpContext?.Response;
            if (response != null)
            {
                response.Headers.TryGetValue(HttpRequestXHeader.Error.GetDisplayName(), out StringValues messages);
                messages = StringValues.Concat(message, messages);
                response.Headers.Remove(HttpRequestXHeader.Error.GetDisplayName());
                response.Headers.Add(HttpRequestXHeader.Error.GetDisplayName(), messages);
            }
        }

        public static Endpoint GetEndpoint(this HttpContext httpContext)
        {
            //TODO:
            return default(Endpoint);
        }
    }
}
