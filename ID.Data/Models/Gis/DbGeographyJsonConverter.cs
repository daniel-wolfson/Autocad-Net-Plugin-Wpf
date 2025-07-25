using System;
using System.Data.Entity.Spatial;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Intellidesk.Data.Models.Gis
{
    public class DbGeographyJsonConverter : JsonConverter
    {
        private const string LATITUDE_KEY = "latitude";
        private const string LONGITUDE_KEY = "longitude";

        public override bool CanConvert(Type objectType)
        {
            return objectType.Equals(typeof(DbGeography));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return default(DbGeography);

            var jObject = JObject.Load(reader);

            if (!jObject.HasValues || (jObject.Property(LATITUDE_KEY) == null || jObject.Property(LONGITUDE_KEY) == null))
                return default(DbGeography);

            string wkt = string.Format(CultureInfo.InvariantCulture, "POINT({1} {0})", jObject[LATITUDE_KEY], jObject[LONGITUDE_KEY]);
            return DbGeography.FromText(wkt, DbGeography.DefaultCoordinateSystemId);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var dbGeography = value as DbGeography;

            serializer.Serialize(writer, dbGeography == null || dbGeography.IsEmpty ? null : new { latitude = dbGeography.Latitude.Value, longitude = dbGeography.Longitude.Value });
        }
    }
}