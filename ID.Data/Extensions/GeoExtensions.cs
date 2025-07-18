using System;
using System.Collections.Generic;
using Intellidesk.Data.Models.Cad;
using Intellidesk.Data.Models.Gis;

namespace Intellidesk.Data.Extensions
{
    public static class GeoExtensions
    {
        private static readonly Dictionary<string, string> Replacements = new Dictionary<string, string>();

        static GeoExtensions()
        {
            Replacements["LINESTRING"] = "";
            Replacements["CIRCLE"] = "";
            Replacements["POLYGON"] = "";
            Replacements["POINT"] = "";
            Replacements["("] = "";
            Replacements[")"] = "";
        }

        public static List<Point> ParseGeometryData(this string s)
        {
            var points = new List<Point>();

            foreach (string to_replace in Replacements.Keys)
            {
                s = s.Replace(to_replace, Replacements[to_replace]);
            }

            string[] pointsArray = s.Split(',');

            for (int i = 0; i < pointsArray.Length; i++)
            {
                double[] coordinates = new double[2];

                //gets X and Y coordinates split by space, trims whitespace at pos 0, converts to double array
                coordinates = Array.ConvertAll(pointsArray[i].Remove(0, 1).Split(null), double.Parse);

                points.Add(new Point(coordinates[0], coordinates[1]));
            }

            return points;
        }
    }
}