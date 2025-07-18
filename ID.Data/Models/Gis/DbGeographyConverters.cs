using System;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Text;

namespace Intellidesk.Data.Models.Gis
{
    public class DbGeographyConverters
    {
        public static DbGeography ConvertPointToDbGeography(double longitude, double latitude)
        {
            var point = string.Format("POINT({1} {0})", latitude, longitude);
            return DbGeography.FromText(point);
        }

        public static IEnumerable<CoordinateGeo> ConvertStringArrayToGeographicCoordinates(string pointString)
        {
            var points = pointString.Split(',');
            var coordinates = new List<CoordinateGeo>();

            for (var i = 0; i < points.Length / 2; i++)
            {
                var geoPoint = points.Skip(i * 2).Take(2).ToList();
                coordinates.Add(new CoordinateGeo(double.Parse(geoPoint.First()), double.Parse(geoPoint.Last())));
            }

            return coordinates;
        }

        public static DbGeography ConvertGeoCoordinatesToPolygon(IEnumerable<Coordinate> coordinates)
        {
            var coordinateList = coordinates.ToList();
            if (coordinateList.First() != coordinateList.Last())
            {
                throw new Exception("First and last point do not match. This is not         a valid polygon");
            }

            var count = 0;
            var sb = new StringBuilder();
            sb.Append(@"POLYGON((");
            foreach (var coordinate in coordinateList)
            {
                if (count == 0)
                {
                    sb.Append(coordinate.Longitude + " " + coordinate.Latitude);
                }
                else
                {
                    sb.Append("," + coordinate.Longitude + " " + coordinate.Latitude);
                }

                count++;
            }

            sb.Append(@"))");

            return DbGeography.PolygonFromText(sb.ToString(), 4326);
        }
    }
}