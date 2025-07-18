using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace ID.Infrastructure.Extensions
{
    /// <summary>
    /// <see cref="IUrlHelper"/> extension methods.
    /// </summary>
    public static class UrlHelperExtensions
    {
        /// <summary>
        /// Generates a fully qualified URL to an action method by using the specified action name, controller name and
        /// route values.
        /// </summary>
        /// <param name="url">The URL helper.</param>
        /// <param name="actionName">The name of the action method.</param>
        /// <param name="controllerName">The name of the controller.</param>
        /// <param name="routeValues">The route values.</param>
        /// <returns>The absolute URL.</returns>
        public static string AbsoluteAction(this IUrlHelper url, string actionName, string controllerName, object routeValues = null)
        {
            return url.Action(actionName, controllerName, routeValues, url.ActionContext.HttpContext.Request.Scheme);
        }

        /// <summary>
        /// Generates a fully qualified URL to the specified content by using the specified content path. Converts a
        /// virtual (relative) path to an application absolute path.
        /// </summary>
        /// <param name="url">The URL helper.</param>
        /// <param name="contentPath">The content path.</param>
        /// <returns>The absolute URL.</returns>
        public static string AbsoluteContent(this IUrlHelper url, string contentPath)
        {
            HttpRequest request = url.ActionContext.HttpContext.Request;
            return new Uri(new Uri(request.Scheme + "://" + request.Host.Value), url.Content(contentPath)).ToString();
        }

        /// <summary>
        /// Generates a fully qualified URL to the specified route by using the route name and route values.
        /// </summary>
        /// <param name="url">The URL helper.</param>
        /// <param name="routeName">Name of the route.</param>
        /// <param name="routeValues">The route values.</param>
        /// <returns>The absolute URL.</returns>
        public static string AbsoluteRouteUrl(this IUrlHelper url, string routeName, object routeValues = null)
        {
            return url.RouteUrl(routeName, routeValues, url.ActionContext.HttpContext.Request.Scheme);
        }

        public static string GetAbsoluteUrl(this IUrlHelper url)
        {
            string resultUrl;
            try
            {
                var rd = url.ActionContext.RouteData;
                var request = url.ActionContext.HttpContext.Request;

                string controllerName = rd.Values["controller"].ToString();
                string actionName = rd.Values["action"].ToString();
                //var hostAddresses = Dns.GetHostAddresses(Dns.GetHostName());
                var localPort = url.ActionContext.HttpContext.Connection?.LocalPort;
                var hostName = string.IsNullOrEmpty(request.Host.Value) ? "localhost" : request.Host.Host;
                var queryString = request.QueryString.HasValue ? "/" + request.QueryString : "";

                resultUrl = $"{request.Scheme}://{hostName}:{localPort}/{controllerName}/{actionName}{queryString}";

            }
            catch (Exception)
            {
                resultUrl = "not defined";
            }
            return resultUrl;
        }
    }
}
