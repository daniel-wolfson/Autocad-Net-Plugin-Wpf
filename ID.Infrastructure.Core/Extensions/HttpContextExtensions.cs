using ID.Infrastructure.Auth;
using ID.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ID.Infrastructure.Extensions
{
    public static class HttpContextExtensions
    {
        /// <summary> add header to current http request </summary>
        public static void AddRequesteader(this HttpContext httpContext, string headerName, string headerValue)
        {
            //httpContext.Request.Headers[headerName] = headerValue;
            var existHeaderValue = httpContext.Request.Headers[headerName];
            if (!string.IsNullOrEmpty(existHeaderValue))
                httpContext.Request.Headers.Add(headerName, headerValue);
            else
                httpContext.Request.Headers[headerName] = headerValue;
        }

        public static string GetRequestToken(this HttpContext httpContext)
        {
            string authHeader = httpContext.Request.Headers["Authorization"];
            return authHeader?.Replace("Bearer ", "") ?? string.Empty;
        }

        public static string GetRequestHeader(this HttpContext httpContext, string headerName)
        {
            string authHeader = httpContext.Request.Headers[headerName];
            return authHeader ?? string.Empty;
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

            var identity = new ClaimsIdentity(claims, ApiAuthSchemes.DefaultAuthScheme);
            var principal = new ClaimsPrincipal(identity);
            var authProperties = new AuthenticationProperties
            {
                AllowRefresh = true,
                ExpiresUtc = DateTimeOffset.Now.AddDays(365),
                IsPersistent = true,
            };

            await httpContext.SignInAsync(ApiAuthSchemes.DefaultAuthScheme, principal, authProperties);

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

        public static Task<byte[]> GetResponse(this HttpContext context)
        {
            var responseStream = context.Response.Body;

            using (var buffer = new MemoryStream())
            {
                try
                {
                    context.Response.Body = buffer;
                }
                finally
                {
                    context.Response.Body = responseStream;
                }

                if (buffer.Length == 0) return null;

                var bytes = buffer.ToArray(); // you could gzip here
                responseStream.Write(bytes, 0, bytes.Length);

                return Task.FromResult(bytes);
            }
        }
    }
}
