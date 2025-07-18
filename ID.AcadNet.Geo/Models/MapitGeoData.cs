using GeoJSON.Net;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Intellidesk.AcadNet.Geo.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class EsriFeatureCollection : FeatureCollection
    {
        [JsonProperty(PropertyName = "type")]
        public new string Type => GeoJSONObjectType.FeatureCollection.ToString();

        [JsonProperty(PropertyName = "metadata")]
        public MetaData Metadata { get; set; }

        [JsonProperty(PropertyName = "bbox")]
        public double[] Bbox { get; set; }

        [JsonProperty(PropertyName = "features")]
        public List<Feature> _features { get { return this.Features; } }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class FeatureEsri : Feature
    {
        public FeatureEsri(IGeometryObject geometry, IDictionary<string, object> properties = null, string id = null)
            : base(geometry, properties, id)
        {
        }

        [JsonProperty(PropertyName = "type")]
        public new string Type => GeoJSONObjectType.Feature.ToString();

        [JsonProperty(PropertyName = "id")]
        public string _id { get { return this.Id; } }

        [JsonProperty(PropertyName = "properties")]
        public IDictionary<string, object> _properties { get { return this.Properties; } }

        [JsonProperty(PropertyName = "geometry")]
        public IGeometryObject _geometry { get { return this.Geometry; } }
    }

    public class MetaData
    {
        public long generated { get; set; }
        public string url { get; set; }
        public string title { get; set; }
        public int status { get; set; }
        public string api { get; set; }
        public int count { get; set; }
    }

    public class PropertyData
    {
        public double mag { get; set; }
        public string place { get; set; }
        public long time { get; set; }
        public long updated { get; set; }
        public int tz { get; set; }
        public string url { get; set; }
        public string detail { get; set; }
        public object alert { get; set; }
        public string status { get; set; }
        public int tsunami { get; set; }
        public int sig { get; set; }
        public string net { get; set; }
        public string code { get; set; }
        public string id { get; set; }
        public string sources { get; set; }
        public string types { get; set; }
        public double dmin { get; set; }
        public string magType { get; set; }
        public string type { get; set; }
        public string title { get; set; }
    }

    public class GeometryEsri
    {
        public string type { get; set; }
        public double[] coordinates { get; set; }
    }

    public class Coordinates
    {
        public Coordinates(double x, double y, double z)
        {
            X = x; Y = y; Z = z;
        }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class PointEsri : Point
    {
        public PointEsri(IPosition coordinates) : base(coordinates)
        {
            Coords = new[] { coordinates.Longitude, coordinates.Latitude, 0 };
        }

        [JsonProperty(PropertyName = "coordinates")]
        public double[] Coords { get; }

        [JsonIgnore]
        public new IPosition Coordinates { get; }

        [JsonProperty(PropertyName = "type")]
        public new string Type => GeoJSONObjectType.Point.ToString();
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class LineStringEsri : LineString
    {
        public LineStringEsri(IEnumerable<IEnumerable<double>> coordinates) : base(coordinates)
        {
            Coords = coordinates;
        }
        public LineStringEsri(IEnumerable<IPosition> coordinates) : base(coordinates)
        {
            Coords = coordinates.ToList().Select(x => new double[] { x.Longitude, x.Latitude });
        }

        [JsonProperty(PropertyName = "coordinates")]
        public IEnumerable<IEnumerable<double>> Coords { get; }

        [JsonIgnore]
        public new ReadOnlyCollection<IPosition> Coordinates { get; }

        [JsonProperty(PropertyName = "type")]
        public new string Type => GeoJSONObjectType.LineString.ToString();
    }

}
