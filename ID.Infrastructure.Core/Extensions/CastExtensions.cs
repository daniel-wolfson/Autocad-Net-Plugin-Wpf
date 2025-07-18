using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ID.Infrastructure.Core;

namespace ID.Infrastructure.Extensions
{
    public static partial class CastExtensions
    {
        public static bool IsValidJson(this string strInput)
        {
            strInput = strInput.Trim();
            return (strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
                   (strInput.StartsWith("[") && strInput.EndsWith("]"));   //For array
        }

        public static Dictionary<string, object> ToDictionary(this ISession obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            return dictionary;
        }

        private static Dictionary<string, object> ToDictionary_(this object obj)
        {
            var dataItems = new Dictionary<string, object>();
            if (obj != null && typeof(IDictionary<string, object>).IsAssignableFrom(obj.GetType()))
            {
                IDictionary<string, object> idict = (IDictionary<string, object>)obj;
                Dictionary<string, string> newDict = new Dictionary<string, string>();
                foreach (string key in idict.Keys)
                {
                    newDict.Add(key.ToString(), idict[key].ToString());
                }
                //var type = obj.GetType();
                //var props = type.GetProperties();
                //dataItems = props.ToDictionary(x => x.Name, x => x.GetValue(obj, null));
            }
            return dataItems;
        }

        /// <summary> Convert byte attay to string UTF-8. </summary>
        public static string ToStringUtf8(this byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }

        public static string ToStringUtf8(this string base64String)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(base64String));
        }

        public static string ToBase64String(this string str)
        {
            return Convert.ToBase64String(str.ToByteArrayUtf8());
        }

        /// <summary> Convert string to byte attay UTF-8. </summary>
        public static byte[] ToByteArrayUtf8(this string str)
        {
            return Encoding.UTF8.GetBytes(str);
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

        public static IActionResult SaveAs(this IFormFile formFile, string filePath)
        {
            if (formFile.Length <= 0) return new ConflictResult();

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                formFile.CopyToAsync(stream).ConfigureAwait(false);
            }

            return new OkResult();
        }

        public static bool IsImplementsInterface(this Type type, Type @interface)
        {
            bool implemented = type.GetInterfaces().Contains(@interface);
            return implemented;
        }
    }
}
