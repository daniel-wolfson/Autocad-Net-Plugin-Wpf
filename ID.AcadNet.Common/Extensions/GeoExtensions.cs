using System;
using System.Diagnostics;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Intellidesk.AcadNet.Common.Extensions;
using Intellidesk.AcadNet.Common.Utils;

namespace Intellidesk.AcadNet.Common.Extentions
{
    public static class GeoExtensions
    {
        public static Database GetCurDwg()
        {
            Database db = Application.DocumentManager.MdiActiveDocument.Database;
            return db;
        }

        public static Matrix3d GetEcsToWcsMatrix(Point3d origin, Vector3d zAxis)
        {
            const double kArbBound = 0.015625;         //  1/64th
            // short circuit if in WCS already
            if (zAxis == Ge.kZAxis)
            {
                return Matrix3d.Identity;
            }

            Vector3d xAxis, yAxis;
            Debug.Assert(zAxis.IsUnitLength(Tolerance.Global));

            if ((Math.Abs(zAxis.X) < kArbBound) && (Math.Abs(zAxis.Y) < kArbBound))
                xAxis = Ge.kYAxis.CrossProduct(zAxis);
            else
                xAxis = Ge.kZAxis.CrossProduct(zAxis);

            xAxis = xAxis.GetNormal();
            yAxis = zAxis.CrossProduct(xAxis).GetNormal();

            return Matrix3d.AlignCoordinateSystem(Ge.kOrigin, Ge.kXAxis, Ge.kYAxis, Ge.kZAxis,
                origin, xAxis, yAxis, zAxis);
        }

        /// <summary>
        /// Get a transformed copy of a point from UCS to WCS
        /// </summary>
        /// <param name="pt">Point to transform</param>
        /// <returns>Transformed copy of point</returns>

        public static Point3d UcsToWcs(Point3d pt)
        {
            Matrix3d m = GetCurDwg().GetUcsMatrix();
            return pt.TransformBy(m);
        }

        /// <summary>
        /// Get a transformed copy of a vector from UCS to WCS
        /// </summary>
        /// <param name="vec">Vector to transform</param>
        /// <returns>Transformed copy of vector</returns>

        public static Vector3d UcsToWcs(Vector3d vec)
        {
            Matrix3d m = GetCurDwg().GetUcsMatrix();
            return vec.TransformBy(m);
        }

        /// <summary>
        /// Get a transformed copy of a point from WCS to UCS
        /// </summary>
        /// <param name="pt">Point to transform</param>
        /// <returns>Transformed copy of point</returns>

        public static Point3d WcsToUcs(Point3d pt)
        {
            Matrix3d m = GetCurDwg().GetUcsMatrix();
            return pt.TransformBy(m.Inverse());
        }
    }
}