using System;
using System.Data.Entity.Spatial;
using System.Drawing;
using System.Linq;
using Point = Intellidesk.Data.Models.Cad.Point;

namespace Intellidesk.Data.Models.Gis
{
    public class DbGeographyHelpers
    {

        public static DbGeography CreatePoligon(Point[] points)
        {
            //DbGeography polygon = DbGeography.PolygonFromText("POLYGON((53.095124 -0.864716, 53.021255 -1.337128, 52.808019 -1.345367, 53.095124 -0.864716))", 4326);
            var poligonWellKnownValues = string.Join(",", points.Select(pnt => pnt.X + " " + pnt.Y));
            var poligonWellKnownText = $"POLYGON(({poligonWellKnownValues}))";
            return DbGeography.PolygonFromText(poligonWellKnownText, 4326);// 4326 is most common coordinate system used by GPS/Maps
        }

        /// <summary>Create a GeoLocation point based on latitude and longitude </summary>
        public static DbGeography CreatePoint(double latitude, double longitude)
        {
            var pointWellKnownText = string.Format("POINT({0} {1})", longitude, latitude);
            // 4326 is most common coordinate system used by GPS/Maps
            return DbGeography.PointFromText(pointWellKnownText, 4326);
        }

        /// <summary> Create a GeoLocation point based on latitude and longitude </summary>
        /// <param name="latitudeLongitude">
        /// String should be two values either single comma or space delimited
        /// 45.710030,-121.516153
        /// 45.710030 -121.516153
        /// </param>
        public static DbGeography CreatePoint(string latitudeLongitude)
        {
            var tokens = latitudeLongitude.Split(',', ' ');
            if (tokens.Length != 2)
                throw new ArgumentException("InvalidLocationStringPassed");

            var text = $"POINT({tokens[1]} {tokens[0]})";
            return DbGeography.PointFromText(text, 4326);
        }

        public static bool IsInPolygon(PointF[] poly, PointF point)
        {
            var coef = poly.Skip(1).Select((p, i) =>
                (point.Y - poly[i].Y) * (p.X - poly[i].X) - (point.X - poly[i].X) * (p.Y - poly[i].Y)).ToList();

            if (coef.Any(p => p == 0))
                return true;

            for (int i = 1; i < coef.Count(); i++)
            {
                if (coef[i] * coef[i - 1] < 0)
                    return false;
            }
            return true;
        }

        public static DbGeography Union(DbGeography source, DbGeography other)
        {
            return source.Union(other);
        }
    }
}