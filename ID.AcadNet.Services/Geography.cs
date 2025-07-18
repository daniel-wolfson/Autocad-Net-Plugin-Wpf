using System;
using System.Collections.Generic;
using System.Data.Spatial;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using Intellidesk.Data.Repositories.EF6.DataContext;
using Microsoft.SqlServer.Types;

namespace Intellidesk.AcadNet.Services
{
    public class DbGeographyEx
    {
        // ReSharper disable once InconsistentNaming
        /// <summary> The most common SRID/coordinateSystemId in GPS and mapping software is 4326. </summary>
        public const int SRID = 4326;

        public static void Run()
        {
            //var point1 = SqlGeography.Point(point.Latitude.GetValueOrDefault(), point.Longitude.GetValueOrDefault(), 4326);
            //var poly = point1.BufferWithTolerance(radiusInMeter, 0.01, true); //0.01 is to simplify the polygon to keep only a few sides
        }

        /// <summary> Create DbGeography object with given latitude and longitude in decimal degrees. </summary>
        public static DbGeography CreatePoint(double latitude, double longitude)
        {
            Contract.Requires<ArgumentOutOfRangeException>(latitude >= -90 && latitude <= 90);
            Contract.Requires<ArgumentOutOfRangeException>(longitude >= -180 && longitude <= 180);

            string wellKnownText = string.Format(CultureInfo.InvariantCulture.NumberFormat,
               "POINT({0} {1})", longitude, latitude);
            return DbGeography.FromText(wellKnownText);
        }

        public static DbGeography CreateLineString(List<DbGeography> points)
        {
            var builder = new SqlGeographyBuilder();
            builder.SetSrid(4326);
            builder.BeginGeography(OpenGisGeographyType.LineString);

            var i = 0;
            foreach (var point in points)
            {
                if (i == 0)
                {
                    builder.BeginFigure(point.Latitude.Value, point.Longitude.Value);
                    i++;
                }
                else
                {
                    builder.AddLine(point.Latitude.Value, point.Longitude.Value);
                }
            }
            builder.EndFigure();
            builder.EndGeography();

            var dbGeography = DbGeography.FromBinary(
              builder.ConstructedGeography.STAsBinary().Value, SRID);
            return dbGeography;
        }

        /// <summary>
        /// Create geography type of type LineString or Polygon with given list of points
        /// (=coordinates with latitude and longitude).
        /// </summary>
        /// <param name="points">List of points (DbGeography objects).</param>
        /// <param name="geographyType">LineString or Polygon.</param>
        /// <returns>DbGeography object.</returns>
        public static DbGeography CreateGeographyType(List<DbGeography> points, OpenGisGeographyType geographyType)
        {
            Contract.Requires<ArgumentOutOfRangeException>(points != null);
            Contract.Requires<ArgumentOutOfRangeException>(points.Any());
            Contract.Requires<ArgumentOutOfRangeException>(geographyType == OpenGisGeographyType.LineString
              || geographyType == OpenGisGeographyType.Polygon);

            var builder = new SqlGeographyBuilder();
            builder.SetSrid(4326);
            builder.BeginGeography(geographyType);

            var i = 0;
            foreach (var point in points)
            {
                if (i == 0)
                {
                    builder.BeginFigure(point.Latitude.Value, point.Longitude.Value);
                    i++;
                }
                else
                {
                    builder.AddLine(point.Latitude.Value, point.Longitude.Value);
                }
            }
            builder.EndFigure();
            builder.EndGeography();

            var dbGeography = DbGeography.FromBinary(
              builder.ConstructedGeography.STAsBinary().Value, SRID);
            return dbGeography;
        }

        public static DbGeography GetClosestPointOnRoute(DbGeography line, DbGeography point)
        {

            //Geography.ShortestLineTo(OtherGeography)
            SqlGeography sqlLine = SqlGeography.Parse(line.AsText()).MakeValid(); //the linestring
            SqlGeography sqlPoint = SqlGeography.Parse(point.AsText()).MakeValid(); //the point i want on the line

            SqlGeography shortestLine = sqlPoint.ShortestLineTo(sqlLine);
            //find the shortest line from the linestring to point

            //lines have a start, and an end
            SqlGeography start = shortestLine.STStartPoint();
            SqlGeography end = shortestLine.STEndPoint();

            DbGeography newGeography = DbGeography.FromText(end.ToString(), 4326);

            var distance = newGeography.Distance(line);

            return newGeography;

            //var param = new SqlParameter(@"Polygon", poly);
            //param.UdtTypeName = "Geography";
            //command.Add(param);
        }

        public static void QueryCastlesInArea(IDataContext context)
        {
            var points = new List<DbGeography>
            {
                CreatePoint(51.31391191024072, 4.266112120769479),
                CreatePoint(51.27651498062908, 4.332895687078189),
                CreatePoint(51.24748068635581, 4.304982047677257),
                CreatePoint(51.18347621420882, 4.336282813016014),
                CreatePoint(51.31391191024072, 4.266112120769479)
            };

            var polygon = CreateGeographyType(points, OpenGisGeographyType.Polygon);

            //var castles =
            //    from c in context.Castles
            //    where c.Location.Intersects(polygon)
            //    select new
            //    {
            //        c.Description,
            //        c.Location,
            //    };

            //foreach (var castle in castles.OrderBy(c => c.Description))
            //{
            //    Console.WriteLine(string.Format("{0}: {1} {2}",
            //        castle.Description,
            //        castle.Location.Latitude,
            //        castle.Location.Longitude));
            //}
        }

        //public static ewer()
        //{
        //    using (var db = new LocationContext())
        //    {
        //        //Select Locations known to be within a certain area which should define the polygon.
        //        foreach (var item in db.Locations)
        //        {
        //            PolygonFromMultiplePoints.Union(item.GeoLocation);
        //        }
        //    }
        //    var temp_multipointgeometry = DbGeometry.MultiPointFromBinary(PolygonFromMultiplePoints.AsBinary(), DbGeometry.DefaultCoordinateSystemId);
        //    PolygonFromMultiplePoints = DbGeography.PolygonFromBinary(temp_multipointgeometry.ConvexHull.AsBinary(), DbGeography.DefaultCoordinateSystemId);
        //}
    }
}