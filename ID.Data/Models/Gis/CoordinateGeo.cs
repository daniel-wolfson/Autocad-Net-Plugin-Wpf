using System;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Text;

namespace Intellidesk.Data.Models.Gis
{
    public class CoordinateGeo
    {
        private const double Tolerance = 10.0 * .1;

        public CoordinateGeo(double longitude, double latitude)
        {
            this.Longitude = longitude;
            this.Latitude = latitude;
        }

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public static bool operator ==(CoordinateGeo a, CoordinateGeo b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            var latResult = Math.Abs(a.Latitude - b.Latitude);
            var lonResult = Math.Abs(a.Longitude - b.Longitude);
            return (latResult < Tolerance) && (lonResult < Tolerance);
        }

        public static bool operator !=(CoordinateGeo a, CoordinateGeo b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var p = (CoordinateGeo)obj;
            var latResult = Math.Abs(this.Latitude - p.Latitude);
            var lonResult = Math.Abs(this.Longitude - p.Longitude);
            return (latResult < Tolerance) && (lonResult < Tolerance);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (this.Latitude.GetHashCode() * 397) ^ this.Longitude.GetHashCode();
            }
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

        public static DbGeography ConvertGeoCoordinatesToPolygon(IEnumerable<CoordinateGeo> coordinates)
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