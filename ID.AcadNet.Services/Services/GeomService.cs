using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Intellidesk.AcadNet.Services.Interfaces;
using Intellidesk.Infrastructure;
using Microsoft.Practices.Unity;
using Exception = Autodesk.AutoCAD.Runtime.Exception;

namespace  Intellidesk.AcadNet.Services
{
    public static class GeomServiceExt
    {
        ///<summary> get all nested objets into Countour from collection (tObjSet) </summary>
        public static List<Entity> XGetNestedFor(this List<Entity> tObjects, Entity extCountour)
        {
            var nestedObjects = new List<Entity>();
            if (tObjects.Count <= 0) return nestedObjects;

            foreach (var testObj in tObjects)
            {
                if ((testObj.ObjectId != extCountour.ObjectId) || (!testObj.Equals(extCountour)))
                {
                    if (testObj.XExist(extCountour)) nestedObjects.Add(testObj);
                }
            }
            return nestedObjects;
        }

        /// <summary> true is if testObject exist in targetObject </summary>
        public static bool XExist(this Entity testObj, Entity targetObj)
        {
            var point1Exist = (testObj.GeometricExtents.MinPoint.X >= targetObj.GeometricExtents.MinPoint.X) &&
                              (testObj.GeometricExtents.MinPoint.X <= targetObj.GeometricExtents.MaxPoint.X);

            var point2Exist = (testObj.GeometricExtents.MinPoint.Y >= targetObj.GeometricExtents.MinPoint.Y) &&
                              (testObj.GeometricExtents.MinPoint.Y <= targetObj.GeometricExtents.MaxPoint.Y);

            var point3Exist = (testObj.GeometricExtents.MaxPoint.X >= targetObj.GeometricExtents.MinPoint.X) &&
                              (testObj.GeometricExtents.MaxPoint.X <= targetObj.GeometricExtents.MaxPoint.X);

            var point4Exist = (testObj.GeometricExtents.MaxPoint.Y >= targetObj.GeometricExtents.MinPoint.Y) &&
                              (testObj.GeometricExtents.MaxPoint.Y <= targetObj.GeometricExtents.MaxPoint.Y);

            return point1Exist && point2Exist && point3Exist && point4Exist;
        }

        /// <summary> true is if testObject exist in targetObject </summary>
        public static bool XExist(this Point3dCollection testObjPoints, Point3dCollection targetObjPoints)
        {
            var targetObj = targetObjPoints.XGetExtents3D();
            var testObj = testObjPoints.XGetExtents3D();

            var point1Exist = (testObj.MinPoint.X >= targetObj.MinPoint.X) &&
                              (testObj.MinPoint.X <= targetObj.MaxPoint.X);

            var point2Exist = (testObj.MinPoint.Y >= targetObj.MinPoint.Y) &&
                              (testObj.MinPoint.Y <= targetObj.MaxPoint.Y);

            var point3Exist = (testObj.MaxPoint.X >= targetObj.MinPoint.X) &&
                              (testObj.MaxPoint.X <= targetObj.MaxPoint.X);

            var point4Exist = (testObj.MaxPoint.Y >= targetObj.MinPoint.Y) &&
                              (testObj.MaxPoint.Y <= targetObj.MaxPoint.Y);

            //var targetObj = new Polyline().XInit(targetObjPoints);
            //var testObj = new Polyline().XInit(testObjPoints);
            //var point1Exist = (testObj.GeometricExtents.MinPoint.X >= targetObj.GeometricExtents.MinPoint.X) &&
            //                  (testObj.GeometricExtents.MinPoint.X <= targetObj.GeometricExtents.MaxPoint.X);
            //var point2Exist = (testObj.GeometricExtents.MinPoint.Y >= targetObj.GeometricExtents.MinPoint.Y) &&
            //                  (testObj.GeometricExtents.MinPoint.Y <= targetObj.GeometricExtents.MaxPoint.Y);
            //var point3Exist = (testObj.GeometricExtents.MaxPoint.X >= targetObj.GeometricExtents.MinPoint.X) &&
            //                  (testObj.GeometricExtents.MaxPoint.X <= targetObj.GeometricExtents.MaxPoint.X);
            //var point4Exist = (testObj.GeometricExtents.MaxPoint.Y >= targetObj.GeometricExtents.MinPoint.Y) &&
            //                  (testObj.GeometricExtents.MaxPoint.Y <= targetObj.GeometricExtents.MaxPoint.Y);

            return point1Exist && point2Exist && point3Exist && point4Exist;
        }
    }

    public class GeomService : IGeomService
    {
        private IUnityContainer _unityContainer;
        private IOperationService _operationService;

        public GeomService(IUnityContainer unityContainer, IOperationService operationService)
        {
            _unityContainer = unityContainer;
            _operationService = operationService; //_unityContainer.Resolve<IOperationService>();
        }
        /// <summary> Degree to Radian </summary>
        public double DegreeToRadian(int angle)
        {
            return Math.PI * angle / 180.0;
        }

        /// <summary> Radian to Degree </summary>
        public double RadianToDegree(double radian)
        {
            return (int)(radian * (180.0 / Math.PI));
        }

        public void GetIntersect()
        {
            try
            {
                using (var tr = _operationService.Ed.Document.Database.TransactionManager.StartTransaction())
                {
                    var opt = new PromptEntityOptions("\nSelect first polyline: ");
                    opt.SetRejectMessage("\nSelect polyline only");
                    opt.AllowNone = false;
                    opt.AllowObjectOnLockedLayer = true;
                    opt.AddAllowedClass(typeof(Polyline), true);

                    var res = _operationService.Ed.GetEntity(opt);
                    if (res.Status != PromptStatus.OK) return;
                    var lid = res.ObjectId;
                    opt.AddAllowedClass(typeof(Polyline), true);
                    opt.SetRejectMessage("\nSelect polyline only");
                    opt.Message = "\nSelect second polyline: ";
                    res = _operationService.Ed.GetEntity(opt);

                    if (res.Status != PromptStatus.OK) return;
                    var pid = res.ObjectId;
                    var pts = new Point3dCollection();
                    _operationService.Ed.WriteMessage("\n{0} <> {1};", lid, pid);
                    var enl = (Polyline)tr.GetObject(lid, OpenMode.ForRead);
                    var enp = (Polyline)tr.GetObject(pid, OpenMode.ForRead);
                    //var plane = new Plane(WorkSpace.Ed.CurrentUserCoordinateSystem.CoordinateSystem3d.Origin, WorkSpace.Ed.CurrentUserCoordinateSystem.CoordinateSystem3d.Zaxis);
                    //enl.IntersectWith(enp, Intersect.OnBothOperands, plane, pts, 0, 0);
                    enl.IntersectWith(enp, Intersect.OnBothOperands, pts, IntPtr.Zero, IntPtr.Zero);

                    _operationService.Ed.WriteMessage("\nNumber of intersections: {0}", pts.Count);
                    var i = 1;
                    foreach (Point3d pt in pts)
                    {
                        _operationService.Ed.WriteMessage("\nPoint number {0}: ({1}, {2}, {3})", i, pt.X, pt.Y, pt.Z);
                        i += 1;
                    }

                    tr.Commit();

                }
            }
            catch (Exception ex)
            {
                Log.Add(ex);
                throw;
            }
        }

        public bool IsOrthModeOn()
        {
            // Check the value of the ORTHOMODE sysvar

            var orth = Application.GetSystemVariable("ORTHOMODE");
            return Convert.ToInt32(orth) > 0;
        }

        // Transform from UCS to WCS
        public Matrix3d UcsToWcs()
        {
            Matrix3d ucs = _operationService.Ed.CurrentUserCoordinateSystem;
            CoordinateSystem3d cs = ucs.CoordinateSystem3d;
            Matrix3d mat = Matrix3d.AlignCoordinateSystem(Point3d.Origin, Vector3d.XAxis, Vector3d.YAxis,
                    Vector3d.ZAxis, cs.Origin, cs.Xaxis, cs.Yaxis, cs.Zaxis);
            return mat;
        }

        // Translate the OCS to WCS
        public Matrix3d WorldToPlane(Vector3d vec) //WorldToPlane(acPoly2d.Normal)
        {
            return Matrix3d.WorldToPlane(vec);
        }

        public Point3d Point3DUcsToWcs(Point3d ucsPoint)
        {
            return ucsPoint.TransformBy(UcsToWcs());
        }

        /// <summary>
        /// Represents a quadrilateral (polygon with four points, eg. a rect or a square). Note that the points are supposed
        /// to be in counter-clockwise order (usually starting with the upper left corner).
        /// </summary>
        public struct Quadrilateral
        {
            public Point2d A { get; set; }
            public Point2d B { get; set; }
            public Point2d C { get; set; }
            public Point2d D { get; set; }

            public Quadrilateral(Rectangle3d rect)
                : this()
            {
                A = new Point2d(rect.UpperLeft.X, rect.UpperLeft.Y);
                B = new Point2d(rect.UpperRight.X, rect.UpperRight.Y);
                C = new Point2d(rect.LowerRight.X, rect.LowerRight.Y);
                D = new Point2d(rect.LowerLeft.X, rect.LowerLeft.Y);
            }

            /// <summary>
            /// Rotates the quadrilateral clockwise around the specified point.
            /// </summary>
            /// <param name="degrees">angle in degrees (clockwise)</param>
            /// <param name="center">center point around which the quadrilateral is rotated</param>
            public void Rotate(double degrees, Point2d center)
            {
                //Matrix mat = new Matrix();
                //// NOTE: Matrix.RotateAt() rotates clockwise.
                //mat.RotateAt(degrees, center.X, center.Y);
                //this.A = mat.Transform(this.A);
                //this.B = mat.Transform(this.B);
                //this.C = mat.Transform(this.C);
                //this.D = mat.Transform(this.D);
            }
        }
    }
    
    public static class IntersectionTest
        {
            public static bool CheckRectRectIntersection(GeomService.Quadrilateral quad1, Rectangle3d quad2)
            {
                return CheckRectRectIntersection(quad1, new GeomService.Quadrilateral(quad2));
            }

            /// <summary>
            /// Checks whether the two convex quadrilaterals intersect. Note that if one of the quadrilaterals is concave, the
            /// test may return a wrong result (so don't use them here).
            /// </summary>
            /// <returns>Returns <c>true</c> if both quadrilaterals intersect/overlap; returns <c>false</c> if they don't.
            /// </returns>
            public static bool CheckRectRectIntersection(GeomService.Quadrilateral quad1, GeomService.Quadrilateral quad2)
            {
                var quad1Points = new[] { quad1.A, quad1.B, quad1.C, quad1.D };
                var quad2Points = new[] { quad2.A, quad2.B, quad2.C, quad2.D };

                //
                // Check quad1 edges
                // 
                if (DoAxisSeparationTest(quad1.A, quad1.B, quad1.C, quad2Points))
                {
                    return false;
                }

                if (DoAxisSeparationTest(quad1.A, quad1.D, quad1.C, quad2Points))
                {
                    return false;
                }

                if (DoAxisSeparationTest(quad1.D, quad1.C, quad1.A, quad2Points))
                {
                    return false;
                }

                if (DoAxisSeparationTest(quad1.C, quad1.B, quad1.A, quad2Points))
                {
                    return false;
                }

                //
                // Check quad2 edges
                // 
                if (DoAxisSeparationTest(quad2.A, quad2.B, quad2.C, quad1Points))
                {
                    return false;
                }

                if (DoAxisSeparationTest(quad2.A, quad2.D, quad2.C, quad1Points))
                {
                    return false;
                }

                if (DoAxisSeparationTest(quad2.D, quad2.C, quad2.A, quad1Points))
                {
                    return false;
                }

                if (DoAxisSeparationTest(quad2.C, quad2.B, quad2.A, quad1Points))
                {
                    return false;
                }

                // If we found no separating axis, then the quadrilaterals intersect.
                return true;
            }

            /// <summary>
            /// Does axis separation test for a convex quadrilateral.
            /// </summary>
            /// <param name="x1">Defines together with x2 the edge of quad1 to be checked whether its a separating axis.</param>
            /// <param name="x2">Defines together with x1 the edge of quad1 to be checked whether its a separating axis.</param>
            /// <param name="x3">One of the remaining two points of quad1.</param>
            /// <param name="otherQuadPoints">The four points of the other quad.</param>
            /// <returns>Returns <c>true</c>, if the specified edge is a separating axis (and the quadrilaterals therefor don't 
            /// intersect). Returns <c>false</c>, if it's not a separating axis.</returns>
            private static bool DoAxisSeparationTest(Point2d x1, Point2d x2, Point2d x3, Point2d[] otherQuadPoints)
            {
                var vec = x2 - x1;
                var rotated = new Vector2d(-vec.Y, vec.X);

                bool refSide = (rotated.X * (x3.X - x1.X)
                              + rotated.Y * (x3.Y - x1.Y)) >= 0;

                foreach (var pt in otherQuadPoints)
                {
                    var side = (rotated.X * (pt.X - x1.X)
                               + rotated.Y * (pt.Y - x1.Y)) >= 0;
                    if (side == refSide)
                    {
                        // At least one point of the other quad is one the same side as x3. Therefor the specified edge can't be a
                        // separating axis anymore.
                        return false;
                    }
                }

                // All points of the other quad are on the other side of the edge. Therefor the edge is a separating axis and
                // the quads don't intersect.
                return true;
            }

            //public static bool CheckLineRectIntersection(Point3d pt1, Point3d pt2, Rectangle3d rect)
            //{
            //    if (rect.Contains(pt1) || rect.Contains(pt2))
            //    {
            //        // If one of the points is inside the rect, it intersects/overlaps.
            //        return true;
            //    }

            //    // If both points are outside, check each edge for intersecting. First we check whether the line is parallel
            //    // to one of the four edges.

            //    if (pt1.X == pt2.X && pt1.X >= rect.Left && pt1.X <= rect.Right)
            //    {
            //        // Match vertical edge
            //        return true;
            //    }
            //    if (pt1.Y == pt2.Y && pt1.Y >= rect.Top && pt1.Y <= rect.Bottom)
            //    {
            //        // Match horizontal edge
            //        return true;
            //    }

            //    // Didn't match. Now check for "real" intersection. Note that if the line intersects with one edge, it must also
            //    // intersect with another one (since none of the points is inside of the rect). So we only need to check the first
            //    // three edges.
            //    return CheckLineLineIntersection(rect.TopLeft, rect.BottomLeft, pt1, pt2)  // left edge
            //        || CheckLineLineIntersection(rect.TopLeft, rect.TopRight, pt1, pt2)  // top edge
            //        || CheckLineLineIntersection(rect.TopRight, rect.BottomRight, pt1, pt2);  // right edge
            //}

            // Checks for intersection of two lines.
            //public static bool CheckLineLineIntersection(Point line1Pt1, Point line1Pt2, Point line2Pt1, Point line2Pt2)
            //{
            //    Vector b = line1Pt2 - line1Pt1;
            //    Vector d = line2Pt2 - line2Pt1;
            //    double bDotDPerp = b.X * d.Y - b.Y * d.X;

            //    // if b dot d == 0, it means the lines are parallel so have infinite intersection points
            //    if (bDotDPerp == 0)
            //    {
            //        return false;
            //    }

            //    Vector c = line2Pt1 - line1Pt1;
            //    double lineFactor = (c.X * d.Y - c.Y * d.X) / bDotDPerp;
            //    if (lineFactor < 0 || lineFactor > 1)
            //    {
            //        return false;
            //    }

            //    lineFactor = (c.X * b.Y - c.Y * b.X) / bDotDPerp;
            //    if (lineFactor < 0 || lineFactor > 1)
            //    {
            //        return false;
            //    }

            //    return true;
            //}
        }
}
