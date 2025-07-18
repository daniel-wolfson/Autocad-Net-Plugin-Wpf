using System;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Data.SqlTypes;
using System.Drawing;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Types;

namespace Intellidesk.Data.Models.Gis
{
    public static class DbGeographyExtensions
    {
        public static IEnumerable<Coordinate> GetPointsFromPolygon(this DbGeography geo)
        {
            for (int i = 1; i < geo.PointCount; i++)
            {
                var p = geo.PointAt(i);
                yield return new Coordinate(p.Latitude.GetValueOrDefault(), p.Longitude.GetValueOrDefault());
            }
        }

        public static SqlGeography GetSqlGeography(this DbGeography geo) //yourEntity.geoColumn
        {
            var poly = SqlGeography.STPolyFromText(
                new SqlChars(geo.WellKnownValue.WellKnownText), geo.CoordinateSystemId);

            for (int i = 1; i <= poly.STNumPoints(); i++)
            {
                SqlGeography point = poly.STPointN(i);
                //do something with point
            }
            return poly;
        }

        public static bool IsIntersect(this DbGeography source, DbGeography target)
        {
           return source.Intersects(target);
        }

        public static double? Distance(this DbGeography source, DbGeography target)
        {
            return source.Distance(target);
        }
    }
}