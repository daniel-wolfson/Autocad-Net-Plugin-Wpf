using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace FileExplorer.Helper
{
    public static class FileHelper
    {
        public static IEnumerable<T> DeserializeIEnumerableOf<T>(string filePath) where T : class
        {
            using (TextReader textReader = new StreamReader(filePath))
            {
                using (var reader = new JsonTextReader(textReader))
                {
                    var cachedDeserializer = JsonSerializer.Create();

                    while (reader.Read())
                    {
                        if (reader.TokenType == JsonToken.StartObject)
                        {
                            var deserializedItem = cachedDeserializer.Deserialize<T>(reader);
                            yield return deserializedItem;
                        }
                    }
                }
            }
        }
    }
}