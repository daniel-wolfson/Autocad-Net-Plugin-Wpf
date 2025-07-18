// Coordinate.cs
// By Jaime Olivares
// Location: http://github.com/jaime-olivares/coordinate

using Intellidesk.Data.Models.Cad;

namespace Intellidesk.Data.Models.Gis
{
    public class Coordinate
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double alt { get; set; }
        public double precision { get; set; }

        private const double Tolerance = 10.0 * .1;

        public Coordinate(double latitude, double longitude, double? alt = null, double? precision = null)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.alt = alt ?? 0;
            this.precision = precision ?? 5;
        }

        public Coordinate ConvertGrid(Ellipsoid from, Ellipsoid to, Translation translation)
        {

            Cad.Point point = from.LatLngToPoint(this, 0);

            //removed 7 point Helmert translation (not needed in Israel's grids)
            Cad.Point translated = translation.translate(point);

            return to.PointToLatLng(translated);
        }
    }
}
