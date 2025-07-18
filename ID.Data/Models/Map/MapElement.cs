namespace Intellidesk.Data.Models.Map
{
    public class MapElement
    {
        public int Id { get; set; }
        public int Leaflet_id { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double? Lat { get; set; }
        public double? Lng { get; set; }
        public double? CoordX { get; set; }
        public double? CoordY { get; set; }
        public ElementType ElementType { get; set; }
        public MapElement(int id, string name)
        {
            ElementType = new ElementType(id, name);
        }
    }
}