namespace ID.Api.Models
{
    public class RootData
    {
        public string type { get; set; }
        public MetaData metadata { get; set; }
        public Feature[] features { get; set; }
        public double[] bbox { get; set; }
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

    public class Geometry
    {
        public string type { get; set; }
        public double[] coordinates { get; set; }
    }

    public class Feature
    {
        public string type { get; set; }
        public PropertyData properties { get; set; }
        public Geometry geometry { get; set; }
        public string id { get; set; }
    }
}
