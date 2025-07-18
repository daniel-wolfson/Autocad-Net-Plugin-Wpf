using General.Infrastructure.Core;
using General.Infrastructure.Enums;
using Microsoft.AspNet.Http.Features;
using Newtonsoft.Json;
using Serilog;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace General.Infrastructure.Extensions
{
    public static class SessionExtensions
    {
        /// <summary> Contains key </summary>
        public static bool Contains(this ISession session, SessionKeys key)
        {
            var keyName = key.GetAttribute<DisplayAttribute>().Name;
            return session.Keys.Contains(keyName);
        }

        /// <summary> set value to ssesion </summary>
        public static void Set(this ISession session, SessionKeys key, string value)
        {
            try
            {
                var keyName = key.GetAttribute<DisplayAttribute>().Name;
                session.SetString(keyName, value);
            }
            catch (Exception ex)
            {
                Log.Logger.ErrorEx(ex);
            }
        }

        /// <summary> set value to ssesion </summary>
        public static void Set<T>(this ISession session, SessionKeys key, T value)
        {
            try
            {
                var sessionKey = key.GetAttribute<DisplayAttribute>().Name;
                // int value
                if (typeof(T) == typeof(int))
                {
                    session.SetInt32(sessionKey, Convert.ToInt32(value));
                }
                // string value
                else if (typeof(T) == typeof(string))
                {
                    session.SetString(sessionKey, value.ToString());
                }
                // object
                else
                {
                    session.SetString(sessionKey, JsonConvert.SerializeObject(value));
                }
            }
            catch (Exception ex)
            {
                Log.Logger.ErrorEx(ex);
            }
        }

        /// <summary> get value from ssesion </summary>
        public static string Get(this ISession session, SessionKeys key)
        {
            try
            {
                var sessionKey = key.GetAttribute<DisplayAttribute>().Name;
                var value = session.GetString(sessionKey);
                return value;
            }
            catch (Exception ex)
            {
                Log.Logger.ErrorEx(ex);
            }

            return string.Empty;
        }

        /// <summary> get value from session </summary>
        public static T Get<T>(this ISession session, SessionKeys key)
        {
            var result = default(T);
            try
            {
                var sessionKey = key.GetAttribute<DisplayAttribute>().Name;

                if (!session.Keys.Contains(sessionKey))
                {
                    Log.Logger.ErrorCall($"Get session key {sessionKey}: not exist in session!");
                    return result;
                }

                // int value
                if (typeof(T) == typeof(int))
                {
                    result = (T)Convert.ChangeType(session.GetInt32(sessionKey), typeof(T));
                }
                // string value
                else if (typeof(T) == typeof(string))
                {
                    result = (T)Convert.ChangeType(session.GetString(sessionKey), typeof(T));
                }
                // object
                else
                {
                    result = JsonConvert.DeserializeObject<T>(session.GetString(sessionKey));
                }
            }
            catch (Exception ex)
            {
                Log.Logger.ErrorEx(ex);
            }

            return result;
        }

        public static void Clear(this IRequestCookieCollection cookies)
        {
            var httpContext = GeneralContext.GetService<IHttpContextAccessor>().HttpContext;
            foreach (var cookie in cookies.Keys)
            {
                httpContext.Response.Cookies.Delete(cookie);
            }
        }
    }
}
