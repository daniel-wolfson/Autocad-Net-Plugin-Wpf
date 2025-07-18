using Newtonsoft.Json;
using System;

namespace Intellidesk.Common.Extensions
{
    public static class StringExtensions
    {
        public static bool IsNullOrEmpty(this string str)
        {
            bool result = string.IsNullOrEmpty(str);
            return result;
        }

        /// <summary>
        /// same as string.StartsWith, IgnoreCase
        /// </summary>
        /// <param name="strA"></param>
        /// <param name="strB"></param>
        /// <returns></returns>
        public static bool IsStartsWith(this string strA, string strB)
        {
            if (string.IsNullOrEmpty(strA) || string.IsNullOrEmpty(strB)) return false;
            return strA.StartsWith(strB, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// same as string.contains, IgnoreCase
        /// </summary>
        /// <param name="sourceValue"></param>
        /// <param name="testValue"></param>
        /// <returns></returns>
        public static bool IsContains(this string sourceValue, string testValue)
        {
            if (string.IsNullOrEmpty(sourceValue) || string.IsNullOrEmpty(testValue)) return false;
            return sourceValue.ToUpperInvariant().Contains(testValue.ToUpperInvariant());
        }

        public static string ParseJson(this string data)
        {
            var json = "";
            dynamic fc = JsonConvert.DeserializeObject(data);
            return json;
        }
    }
}

