using ID.Infrastructure.Enums;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ID.Infrastructure.Extensions
{
    public static class HttpRequestItemsExtensions
    {
        /// <summary> Define if not contains ont key and string.IsNullOrEmpty </summary>
        public static bool Contains(this IDictionary<object, object> items, ItemsKeys key)
        {
            var keyName = key.GetAttribute<DisplayAttribute>().Name;
            return items.Keys.Contains(keyName);
        }

        /// <summary> set value to ssesion </summary>
        public static void Set(this IDictionary<object, object> items, ItemsKeys key, string value)
        {
            try
            {
                var keyName = key.GetAttribute<DisplayAttribute>().Name;
                items[keyName] = value;
            }
            catch (Exception ex)
            {
                Log.Logger.ErrorEx(ex);
            }
        }

        /// <summary> set value to ssesion </summary>
        public static void Set<T>(this IDictionary<object, object> items, ItemsKeys key, T value)
        {
            try
            {
                var keyName = key.GetAttribute<DisplayAttribute>().Name;
                // int value
                if (typeof(T) == typeof(int))
                {
                    items[keyName] = Convert.ToInt32(value);
                }
                // string value
                else if (typeof(T) == typeof(string))
                {
                    items[keyName] = value.ToString();
                }
                // object
                else
                {
                    items[keyName] = JsonConvert.SerializeObject(value);
                }
            }
            catch (Exception ex)
            {
                Log.Logger.ErrorEx(ex);
            }
        }

        /// <summary> get value from ssesion </summary>
        public static string Get(this IDictionary<object, object> items, ItemsKeys key)
        {
            try
            {
                var keyName = key.GetAttribute<DisplayAttribute>().Name;
                var value = items[keyName];
                return value.ToString();
            }
            catch (Exception ex)
            {
                Log.Logger.ErrorEx(ex);
            }

            return string.Empty;
        }

        /// <summary> get value from session </summary>
        public static T Get<T>(this IDictionary<object, object> items, ItemsKeys key)
        {
            var result = default(T);
            try
            {
                var keyName = key.GetAttribute<DisplayAttribute>().Name;

                if (!items.Keys.Contains(keyName))
                {
                    Log.Logger.ErrorCall($"Get session key {keyName}: not exist in session!");
                    return result;
                }

                // int value
                if (typeof(T) == typeof(int))
                {
                    result = (T)Convert.ChangeType(items[keyName], typeof(T));
                }
                // string value
                else if (typeof(T) == typeof(string))
                {
                    result = (T)Convert.ChangeType(items[keyName], typeof(T));
                }
                // object
                else
                {
                    result = JsonConvert.DeserializeObject<T>(items[keyName].ToString());
                }
            }
            catch (Exception ex)
            {
                Log.Logger.ErrorEx(ex);
            }

            return result;
        }
    }
}
