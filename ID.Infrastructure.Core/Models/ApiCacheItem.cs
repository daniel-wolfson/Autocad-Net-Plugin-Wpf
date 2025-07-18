using ID.Infrastructure.Helpers;
using System;

namespace ID.Infrastructure.Models
{
    [Serializable]
    public class ApiCacheItem
    {
        public ApiCacheItem(object data, bool preload = true, bool scoped = true, string query = "")
        {
            Query = query;
            Data = Util.ConvertObjectToByteArray(data);
            Scoped = scoped;
            Preload = preload;
            ModelType = GetModelType(data.GetType()).Name;
        }

        public string ModelType { get; set; }
        public string Query { get; set; }
        public Byte[] Data { get; set; }
        public DateTime LastUpdated => DateTime.Now;
        public bool Scoped { get; set; } = true;
        public bool Preload { get; set; } = true;

        private Type GetModelType(Type type)
        {
            Type resultType;
            if (type.IsGenericType && type.GenericTypeArguments.Length == 1)
                resultType = type.GenericTypeArguments[0];
            else
                resultType = type;
            return resultType;
        }
    }


}
