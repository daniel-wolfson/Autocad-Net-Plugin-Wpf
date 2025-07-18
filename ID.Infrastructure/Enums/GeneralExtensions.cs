using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;

namespace ID.Infrastructure.Extensions
{
    public static class GeneralExtensions
    {
        public static bool IsValidJson(this string strInput)
        {
            strInput = strInput.Trim();
            return strInput.StartsWith("{") && strInput.EndsWith("}") || //For object
                   strInput.StartsWith("[") && strInput.EndsWith("]");   //For array
        }

        public static Dictionary<string, object> ToDictionary(this ISession obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            return dictionary;
        }
        private static Dictionary<string, object> ToDictionary2(this object obj)
        {
            var dataItems = new Dictionary<string, object>();
            if (obj != null)
            {
                var type = obj.GetType();
                var props = type.GetProperties();
                dataItems = props.ToDictionary(x => x.Name, x => x.GetValue(obj, null));
            }
            return dataItems;
        }

        /// <summary> Convert byte attay to string UTF-8. </summary>
        public static string ToStringUtf8(this byte[] bytes)
        {
            try
            {
                return Encoding.UTF8.GetString(bytes);
            }
            catch (Exception ex)
            {
                Log.Logger.ErrorEx(ex);
                return null;
            }
        }

        public static string ToStringUtf8(this string base64String)
        {
            try
            {
                return Encoding.UTF8.GetString(Convert.FromBase64String(base64String));
            }
            catch (Exception ex)
            {
                Log.Logger.ErrorEx(ex);
                return null;
            }
        }

        public static string ToBase64String(this string str)
        {
            try
            {
                return Convert.ToBase64String(str.ToByteArrayUtf8());
            }
            catch (Exception ex)
            {
                Log.Logger.ErrorEx(ex);
                return null;
            }
        }

        /// <summary> Convert string to byte attay UTF-8. </summary>
        public static byte[] ToByteArrayUtf8(this string str)
        {
            try
            {
                return Encoding.UTF8.GetBytes(str);
            }
            catch (Exception ex)
            {
                Log.Logger.ErrorEx(ex);
                return null;
            }
        }

        public static T ToJson<T>(this object obj)
        {
            T result;
            if (typeof(T).IsValueType)
            {
                result = (T)Convert.ChangeType(obj, typeof(T));
            }
            else if (typeof(T) == typeof(string))
            {
                result = (T)Convert.ChangeType(JsonConvert.SerializeObject(obj), typeof(T));
            }
            else
            {
                result = (T)obj;
            }
            return result;
            //new HtmlString(result)
        }

        //public static IActionResult SaveAs(this IFormFile formFile, string filePath)
        //{
        //    if (formFile.Length <= 0) return new ConflictResult();
        //    using (var stream = new FileStream(filePath, FileMode.Create))
        //    {
        //        formFile.CopyToAsync(stream).ConfigureAwait(false);
        //    }
        //    return new OkResult();
        //}

        public static bool IsImplementsInterface(this Type type, Type @interface)
        {
            bool implemented = type.GetInterfaces().Contains(@interface);
            return implemented;
        }

        public static bool IsGuidEmpty(this Guid guid)
        {
            return guid == Guid.Parse("00000000-0000-0000-0000-000000000000");
        }
    }
}
