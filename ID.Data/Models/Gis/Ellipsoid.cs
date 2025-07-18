using System;

namespace Intellidesk.Data.Models.Gis
{
    public class Ellipsoid
    {
        public int a;
        public double b;
        public double e2 { get; set; }

        public Ellipsoid(int a, double b)
        {
            this.a = a;
            this.b = b;

            // Compute eccentricity squared
            this.e2 = (Math.Pow(a, 2) - Math.Pow(b, 2)) / Math.Pow(a, 2);
        }

        public Cad.Point LatLngToPoint(Coordinate latlng, int i)
        {
            // Convert angle measures to radians
            var radlat = latlng.Latitude * (Math.PI / 180);
            var radlng = latlng.Longitude * (Math.PI / 180);

            // Compute nu
            var V = this.a / Math.Sqrt(1 - this.e2 * Math.Pow(Math.Sin(radlat), 2));

            // Compute XYZ
            var x = (V + latlng.alt) * Math.Cos(radlat) * Math.Cos(radlng);
            var y = (V + latlng.alt) * Math.Cos(radlat) * Math.Sin(radlng);
            var z = (V * (1 - this.e2) + latlng.alt) * Math.Sin(radlat);

            return new Cad.Point(x, y, z);
        }

        public Coordinate PointToLatLng(Cad.Point point)
        {
            var RootXYSqr = Math.Sqrt(Math.Pow(point.X, 2) + Math.Pow(point.Y, 2));

            var radlat1 = Math.Atan2(point.Z, (RootXYSqr * (1 - this.e2)));
            double radlat2;
            do
            {
                var V = this.a / (Math.Sqrt(1 - (this.e2 * Math.Pow(Math.Sin(radlat1), 2))));
                radlat2 = Math.Atan2((point.Z + (this.e2 * V * (Math.Sin(radlat1)))), RootXYSqr);
                if (Math.Abs(radlat1 - radlat2) > 0.000000001)
                {
                    radlat1 = radlat2;
                }
                else
                {
                    break;
                }
            }
            while (true);

            var lat = radlat2 * (180 / Math.PI);
            var lng = Math.Atan2(point.Y, point.X) * (180 / Math.PI);

            return new Coordinate(lat, lng);
        }
    }
}