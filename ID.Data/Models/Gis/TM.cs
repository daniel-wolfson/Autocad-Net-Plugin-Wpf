using System;
using Intellidesk.Data.Models.Cad;

namespace Intellidesk.Data.Models.Gis
{
    /// <summary>
    /// Conversion routines for Google, TMS, and Microsoft Quadtree tile representations, derived from
    /// http://www.maptiler.org/google-maps-coordinates-tile-bounds-projection/ 
    /// </summary>
    public class TM
    {
        public Ellipsoid ellipsoid { get; set; }
        public double e0 { get; set; }
        public double n0 { get; set; }
        public double f0 { get; set; }
        public double lat0 { get; set; }
        public double lng0 { get; set; }
        public double radlat0 { get; set; }
        public double radlng0 { get; set; }
        public double af0 { get; set; }
        public double bf0 { get; set; }
        public double e2 { get; set; }
        public double n { get; set; }
        public double n2 { get; set; }
        public double n3 { get; set; }

        public TM(Ellipsoid ellipsoid, double e0, double n0, double f0, double lat0, double lng0)
        {
            this.ellipsoid = ellipsoid;
            this.e0 = e0;
            this.n0 = n0;
            this.f0 = f0;
            this.lat0 = lat0;
            this.lng0 = lng0;

            this.radlat0 = lat0 * (Math.PI / 180);
            this.radlng0 = lng0 * (Math.PI / 180);

            this.af0 = ellipsoid.a * f0;
            this.bf0 = ellipsoid.b * f0;
            this.e2 = (Math.Pow(this.af0, 2) - Math.Pow(this.bf0, 2)) / Math.Pow(this.af0, 2);
            this.n = (this.af0 - this.bf0) / (this.af0 + this.bf0);
            this.n2 = this.n * this.n; //for optimizing and clarity of Marc()
            this.n3 = this.n2 * this.n; //for optimizing and clarity of Marc()
        }

        public Coordinate Unproject(Point point)
        {
            //Compute Et
            var Et = point.X - this.e0;

            //Compute initial value for latitude (PHI) in radians
            var PHId = this.InitialLat(point.Y);

            //Compute nu, rho and eta2 using value for PHId
            var nu = this.af0 / (Math.Sqrt(1 - (this.e2 * (Math.Pow(Math.Sin(PHId), 2)))));
            var rho = (nu * (1 - this.e2)) / (1 - (this.e2 * Math.Pow(Math.Sin(PHId), 2)));
            var eta2 = (nu / rho) - 1;

            //Compute Latitude
            var VII = (Math.Tan(PHId)) / (2 * rho * nu);
            var VIII = ((Math.Tan(PHId)) / (24 * rho * Math.Pow(nu, 3))) *
                       (5 + (3 * Math.Pow(Math.Tan(PHId), 2)) + eta2 - (9 * eta2 * (Math.Pow(Math.Tan(PHId), 2))));
            var IX = (Math.Tan(PHId) / (720 * rho * Math.Pow(nu, 5))) *
                     (61 + (90 * Math.Pow(Math.Tan(PHId), 2)) + (45 * (Math.Pow(Math.Tan(PHId), 4))));

            var lat = (180 / Math.PI) * (PHId - (Math.Pow(Et, 2) * VII) + (Math.Pow(Et, 4) * VIII) - (Math.Pow(Et, 6) * IX));

            //Compute Longitude
            var X = (Math.Pow(Math.Cos(PHId), -1)) / nu;
            var XI = ((Math.Pow(Math.Cos(PHId), -1)) / (6 * Math.Pow(nu, 3))) * ((nu / rho) + (2 * (Math.Pow(Math.Tan(PHId), 2))));
            var XII = ((Math.Pow(Math.Cos(PHId), -1)) / (120 * Math.Pow(nu, 5))) *
                      (5 + (28 * (Math.Pow(Math.Tan(PHId), 2))) + (24 * (Math.Pow(Math.Tan(PHId), 4))));
            var XIIA = ((Math.Pow(Math.Cos(PHId), -1)) / (5040 * Math.Pow(nu, 7))) *
                       (61 + (662 * (Math.Pow(Math.Tan(PHId), 2))) + (1320 * (Math.Pow(Math.Tan(PHId), 4))) +
                        (720 * (Math.Pow(Math.Tan(PHId), 6))));

            var lng = (180 / Math.PI) *
                      (this.radlng0 + (Et * X) - (Math.Pow(Et, 3) * XI) + (Math.Pow(Et, 5) * XII) - (Math.Pow(Et, 7) * XIIA));

            return new Coordinate(lat, lng);
        }

        private double InitialLat(double y)
        {
            var radlat1 = ((y - this.n0) / this.af0) + this.radlat0;
            var M = this.Marc(radlat1);
            var radlat2 = ((y - this.n0 - M) / this.af0) + radlat1;

            while (Math.Abs(y - this.n0 - M) > 0.00001)
            {
                radlat2 = ((y - this.n0 - M) / this.af0) + radlat1;
                M = this.Marc(radlat2);
                radlat1 = radlat2;
            }
            return radlat2;
        }

        private double Marc(double radlat)
        {
            return (
                this.bf0 * (
                    ((1 + this.n + ((5 / 4) * this.n2) + ((5 / 4) * this.n3)) * (radlat - this.radlat0)) -
                    (((3 * this.n) + (3 * this.n2) + ((21 / 8) * this.n3)) * (Math.Sin(radlat - this.radlat0)) *
                     (Math.Cos(radlat + this.radlat0))) +
                    ((((15 / 8) * this.n2) + ((15 / 8) * this.n3)) * (Math.Sin(2 * (radlat - this.radlat0))) *
                     (Math.Cos(2 * (radlat + this.radlat0)))) -
                    (((35 / 24) * this.n3) * (Math.Sin(3 * (radlat - this.radlat0))) * (Math.Cos(3 * (radlat + this.radlat0))))
                    )
                );
        }
    }
}