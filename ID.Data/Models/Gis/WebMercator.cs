using System;
using Intellidesk.Data.Models.Cad;

namespace Intellidesk.Data.Models.Gis
{
    public class WebMercator
    {
        private const int TileSize = 256;
        private const int EarthRadius = 6378137;
        private const double InitialResolution = 2 * Math.PI * EarthRadius / TileSize;
        private const double OriginShift = 2 * Math.PI * EarthRadius / 2;

        public static Coordinate itm2gps(Point point)
        {
            Ellipsoid GRS80 = new Ellipsoid(6378137, 6356752.31414);
            Ellipsoid WGS84 = new Ellipsoid(6378137, 6356752.314245);
            Translation GRS80toWGS84 = new Translation(-48, 55, 52);

            TM ITM = new TM(GRS80, 219529.58400, 626907.38999, 1.000006700000000, 31.734394, 35.204517);
            Coordinate latlng = ITM.Unproject(point); //however, latlng is still on GRS80!
            latlng.ConvertGrid(GRS80, WGS84, GRS80toWGS84);
            return latlng;
        }
    }
}