using Newtonsoft.Json;

namespace Intellidesk.Data.Gis
{
    public class TypedFeatureProps
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("value")]
        public double Value { get; set; }
    }
}