using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace ID.Infrastructure.Helpers
{
    public partial class Util
    {
        public static TResult JsonConvert<TResult>(object data, string dateTimeformat)
        {
            var dateTimeConverter = new IsoDateTimeConverter { DateTimeFormat = dateTimeformat };
            return JsonConvert<TResult>(data, dateTimeConverter);
        }

        public static TResult JsonConvert<TResult>(object data, params JsonConverter[] converters)
        {
            var jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            var settings = new JsonSerializerSettings { Formatting = Formatting.Indented };

            if (converters != null && converters.Length > 0)
                settings.Converters = new List<JsonConverter>(converters);

            TResult result = Newtonsoft.Json.JsonConvert.DeserializeObject<TResult>(jsonData, settings);
            return result;
        }

        public static string JsonContractSerializer<T>(T t)
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            MemoryStream ms = new MemoryStream();
            ser.WriteObject(ms, t);
            string jsonString = Encoding.UTF8.GetString(ms.ToArray());
            ms.Close();
            return jsonString;
        }

        public static T JsonContractDeserialize<T>(string jsonString)
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
            T obj = (T)ser.ReadObject(ms);
            return obj;
        }
    }
}
