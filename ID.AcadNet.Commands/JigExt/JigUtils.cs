using Autodesk.AutoCAD.Geometry;
using System;

namespace Intellidesk.AcadNet.Commands.Jig
{
    public class JigUtils
    {
        // Custom ArcTangent method, as the Math.Atan
        // doesn't handle specific cases

        public static double Atan(double y, double x)
        {
            if (x > 0)
                return Math.Atan(y / x);
            else if (x < 0)
                return Math.Atan(y / x) - Math.PI;
            else  // x == 0
            {
                if (y > 0)
                    return Math.PI;
                else if (y < 0)
                    return -Math.PI;
                else // if (y == 0) theta is undefined
                    return 0.0;
            }
        }

        // Computes Angle between a provided vector and that
        // defined by the vector between two points

        public static double ComputeAngle(
          Point3d startPoint, Point3d endPoint,
          Vector3d xdir, Matrix3d ucs
        )
        {
            var v =
              new Vector3d(
                (endPoint.X - startPoint.X) / 2,
                (endPoint.Y - startPoint.Y) / 2,
                (endPoint.Z - startPoint.Z) / 2
              );

            var cos = v.DotProduct(xdir);
            var sin =
              v.DotProduct(
                Vector3d.ZAxis.TransformBy(ucs).CrossProduct(xdir)
              );

            return Atan(sin, cos);
        }
    }
}
