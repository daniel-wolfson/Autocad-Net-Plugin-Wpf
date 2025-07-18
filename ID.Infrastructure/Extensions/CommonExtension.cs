using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ID.Infrastructure.Extensions
{
    public static class CommonExtension
    {
        public static bool IsNull(this object obj)
        {
            bool result = obj == null;
            return result;
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> list)
        {
            bool result = list == null || !list.Any();
            return result;
        }

        public static bool IsNullOrEmpty(this string input) => string.IsNullOrEmpty(input);

        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var e in enumerable)
            {
                action(e);
            }
        }

        public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int chunksize)
        {
            while (source.Any())
            {
                yield return source.Take(chunksize);
                source = source.Skip(chunksize);
            }
        }

        public static string ToBase64(this System.Text.Encoding encoding, string text)
        {
            if (text == null)
            {
                return null;
            }

            byte[] textAsBytes = encoding.GetBytes(text);
            return Convert.ToBase64String(textAsBytes);
        }

        public static bool TryParseBase64(this System.Text.Encoding encoding, string encodedText, out string decodedText)
        {
            if (encodedText == null)
            {
                decodedText = null;
                return false;
            }

            try
            {
                byte[] textAsBytes = Convert.FromBase64String(encodedText);
                decodedText = encoding.GetString(textAsBytes);
                return true;
            }
            catch (Exception)
            {
                decodedText = null;
                return false;
            }
        }

        public static T CreateFromJsonStream<T>(this Stream stream)
        {
            T data;
            using (StreamReader streamReader = new StreamReader(stream))
            {
                JsonSerializer serializer = new JsonSerializer();
                data = (T)serializer.Deserialize(streamReader, typeof(T));
            }
            return data;
        }

        public static T CreateFromJsonString<T>(this string json)
        {
            T data;
            using (MemoryStream stream = new MemoryStream(Encoding.Default.GetBytes(json)))
            {
                data = CreateFromJsonStream<T>(stream);
            }
            return data;
        }

        //public static T CreateFromJsonFile<T>(String path)
        //{
        //    T data;
        //    using (FileStream fileStream = new FileStream(path, FileMode.Open))
        //    {
        //        data = CreateFromJsonStream<T>(fileStream);
        //    }
        //    return data;
        //}

        public static T CreateFromJsonFile<T>(this string path)
        {
            T data;
            using (FileStream fileStream = new FileStream(path, FileMode.Open))
            {
                data = CreateFromJsonStream<T>(fileStream);
            }
            return data;
        }

        public static void SaveToJsonFile<T>(this T data, string path) where T : class
        {
            using (StreamWriter sw = new StreamWriter(path))
            using (JsonWriter jsonWriter = new JsonTextWriter(sw))
            {
                JsonSerializer serializer = new JsonSerializer { Formatting = Newtonsoft.Json.Formatting.Indented };
                serializer.Serialize(jsonWriter, data);
                //jsonWriter.Flush();
            }
            //string jsonData = JsonConvert.SerializeObject(data, Formatting.None);
            //System.IO.File.WriteAllText(Server.MapPath("~/JsonData/jsondata.txt"), jsonData);
        }

        public static bool IsValidJson(this string input)
        {
            input = input.Trim();
            if (input.StartsWith("{") && input.EndsWith("}"))
                return true;
            return input.StartsWith("[") && input.EndsWith("]");
        }

        public static void NewtonSerializeToFile<T>(this T obj, string path) where T : class
        {
            using (StreamWriter sw = new StreamWriter(path))
            using (JsonWriter jsonWriter = new JsonTextWriter(sw))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(jsonWriter, obj);
                jsonWriter.Flush();
            }
        }

        public static T NewtonDeserializeFromFile<T>(this T obj, string path) where T : class
        {
            using (StreamReader reader = new StreamReader(new FileStream(path, FileMode.Open)))
            using (JsonTextReader jsonReader = new JsonTextReader(reader))
            {
                JsonSerializer ser = new JsonSerializer();
                return ser.Deserialize<T>(jsonReader);
            }
        }

        public static Expression<TDelegate> AndAlso<TDelegate>(this Expression<TDelegate> left, Expression<TDelegate> right)
        {
            return Expression.Lambda<TDelegate>(Expression.AndAlso(left, right), left.Parameters);
        }

        public static string ToCamelCase(this string text)
        {
            return Char.ToUpperInvariant(text[0]) + text.ToLowerInvariant().Substring(1);
            //return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text);
        }

        public static IEnumerable<T> Flatten<T>(this IEnumerable<T> e, Func<T, IEnumerable<T>> f)
        {
            return e.SelectMany(c => f(c).Flatten(f)).Concat(e);
        }

        public static string ToStringMessage(this Exception ex, string action = null)
        {
            var errorMessage = "";
            if (!string.IsNullOrEmpty(ex.Message))
                errorMessage = ex.Message;
            if (ex.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.Message))
                errorMessage = ex.InnerException.Message;
            else if (ex.InnerException != null && ex.InnerException.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.InnerException.Message))
            {
                errorMessage = ex.InnerException.InnerException.Message;
            }
            var result = (action != null ? action + ": " : "") + errorMessage;
            return result;
        }

    }
}