using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Intellidesk.AcadNet.Common.Internal;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Intellidesk.AcadNet.Common.Extensions
{
    public static class PointsExtensions
    {
        /// <summary> Find chained elements in list of entity and returning groups of Point3dCollection </summary>
        public static List<Point3dCollection> XParseToChainsPoints(this List<Entity> ents)
        {
            var chain = new List<Point3dCollection>();
            var objectPointsVertices = ents.Select(ent => new Point3dCollection(ent.XGetPoints().ToArray())).ToList();
            var iterator = objectPointsVertices.GetEnumerator();
            while (iterator.MoveNext())
            {
                var pointsRelevants = new Point3dCollection();
                pointsRelevants.XAddPointsRelevants(objectPointsVertices, iterator.Current);
                chain.Add(pointsRelevants);
                iterator = objectPointsVertices.GetEnumerator();
            }
            return chain;
        }

        /// <summary> Add points candidates of chains by distance between two pairs points of objects </summary>
        public static Point3dCollection XAddPointsRelevants(this Point3dCollection chainPoins,
            List<Point3dCollection> heapObjectsPoints, Point3dCollection chainPointsCandidates)
        {
            if (chainPointsCandidates.Count > 0)
            {
                chainPointsCandidates.Cast<Point3d>()
                    .ToList()
                    .ForEach(x =>
                    { if (!chainPoins.Contains(x)) chainPoins.Add(x); });

                heapObjectsPoints.Remove(chainPointsCandidates);
            }
            var iterator = heapObjectsPoints.GetEnumerator();

            while (iterator.MoveNext())
            {
                foreach (Point3d pointCandidate in chainPointsCandidates)
                {
                    if (iterator.Current == null) break;
                    foreach (Point3d pnt in iterator.Current)
                    {
                        var rect = new RectangleF(new PointF(
                            (float)pointCandidate.X - ((float)Configs.ChainDistance),
                            (float)pointCandidate.Y - ((float)Configs.ChainDistance)),
                            new SizeF(
                                (float)Configs.ChainDistance * 2,
                                (float)Configs.ChainDistance * 2));

                        if (rect.Contains(new PointF((float)pnt.X, (float)pnt.Y)) && (iterator.Current != null))
                        {
                            if (!chainPoins.Contains(pnt))
                                iterator.Current.Cast<Point3d>().ToList().ForEach(x => chainPoins.Add(x));

                            heapObjectsPoints.Remove(iterator.Current);

                            var lst = iterator.Current.Cast<Point3d>().ToList();
                            lst.Remove(pnt);
                            chainPoins.XAddPointsRelevants(heapObjectsPoints, new Point3dCollection(lst.ToArray()));

                            iterator = heapObjectsPoints.GetEnumerator();
                        }
                    }
                }
            }
            return chainPoins;
        }

        /// <summary> Get Point3dCollection from extents points </summary>
        public static Point3dCollection XGetPoints(this Extents3d extents3D)
        {
            return new Point3dCollection
            {
                extents3D.MinPoint,
                new Point3d(extents3D.MaxPoint.X, extents3D.MinPoint.Y, 0),
                extents3D.MaxPoint,
                new Point3d(extents3D.MinPoint.X, extents3D.MaxPoint.Y, 0)
            };
        }

        /// <summary> Get points as extents from Point3dCollection </summary>
        public static Extents3d XGetPointsExtents3d(this Point3dCollection tPoints)
        {
            var p3DColl = new Polyline().XCreateVertices(tPoints); //new Polyline3d(Poly3dType.SimplePoly, tPoints, true);
            if (p3DColl.Bounds.HasValue)
                return new Extents3d(p3DColl.Bounds.Value.MinPoint, p3DColl.Bounds.Value.MaxPoint);
            return new Extents3d(p3DColl.GeometricExtents.MinPoint, p3DColl.GeometricExtents.MaxPoint);
        }
        public static Extents2d XGetPointsExtents2d(this Point3dCollection tPoints)
        {
            return tPoints.XGetPointsExtents3d().ToExtents2d();
        }

        /// <summary> Get Point3D Min from Point3dCollection </summary>
        public static Point3d XGetPoint3DMin(this Point3dCollection tPoints)
        {
            if (tPoints.Count == 0) return Point3d.Origin;
            var p3DColl = new Polyline().XCreateVertices(tPoints); //new Polyline3d(Poly3dType.SimplePoly, tPoints, true);
            return p3DColl.Bounds?.MinPoint ?? tPoints[0];
        }

        /// <summary> Get Point3D Min from List of Entity </summary>
        public static Point3d XGetPoint3DMin(this List<Entity> tObjects)
        {
            if (tObjects.Count == 0) return Point3d.Origin;
            return new Point3d(tObjects.Min(n => n.Bounds?.MinPoint.X ?? 0),
                tObjects.Min(n => n.Bounds?.MinPoint.Y ?? 0), 0);
        }

        /// <summary> Get Point3D Min from List of Entity </summary>
        public static Point3d XGetPoint3DMin(this List<ObjectId> tObjects)
        {
            if (tObjects.Count == 0) return Point3d.Origin;
            return
                new Point3d(
                    tObjects.Select(n => n.GetObject(OpenMode.ForRead) as Entity).Min(n => n.Bounds.HasValue ? n.Bounds.Value.MinPoint.X : 0),
                    tObjects.Select(n => n.GetObject(OpenMode.ForRead) as Entity).Min(n => n.Bounds.HasValue ? n.Bounds.Value.MinPoint.Y : 0),
                    0);
        }

        /// <summary> Get Point3D Max from List of Entity </summary>
        public static Point3d XGetPoint3DMax(this List<Entity> tObjects)
        {
            if (tObjects.Count == 0) return Point3d.Origin;
            return new Point3d(tObjects.Max(n => n.Bounds.HasValue ? n.Bounds.Value.MaxPoint.X : 0),
                tObjects.Max(n => n.Bounds.HasValue ? n.Bounds.Value.MaxPoint.Y : 0), 0);
        }

        /// <summary> Get Point3D Max from List of Entity </summary>
        public static Point3d XGetPoint3DMax(this List<ObjectId> tObjects)
        {
            return
                new Point3d(
                    tObjects.Select(n => n.GetObject(OpenMode.ForRead) as Entity).Max(n => n.GeometricExtents.MaxPoint.X),
                    tObjects.Select(n => n.GetObject(OpenMode.ForRead) as Entity).Max(n => n.GeometricExtents.MaxPoint.Y),
                    0);
        }

        /// <summary> Get Point3D Max from Point3dCollection </summary>
        public static Point3d XGetPoint3DMax(this Point3dCollection tPoints)
        {
            if (tPoints.Count == 0) return Point3d.Origin;
            var p3DColl = new Polyline().XCreateVertices(tPoints); //new Polyline3d(Poly3dType.SimplePoly, tPoints, true);
            return p3DColl.Bounds.HasValue ? p3DColl.Bounds.Value.MaxPoint : tPoints[0];
        }

        /// <summary> Return limits(MinPoint,MaxPoint) for List of Entity </summary>
        public static Extents3d XLimits(this List<Entity> ents)
        {
            return ents.Cast<ObjectId>()
                .Select(n => n.GetObject(OpenMode.ForRead) as Entity)
                .Where(n => n != null && !(n is AttributeDefinition))
                .Max(n => new Extents3d(n.GeometricExtents.MinPoint, n.GeometricExtents.MaxPoint));
        }

        /// <summary> Return limits(MinPoint,MaxPoint) for Point3dCollection </summary>
        public static Extents3d XGetExtents3D(this Point3dCollection points)
        {
            return new Extents3d(
                new Point3d(points.Cast<Point3d>().Min(n => n.X), points.Cast<Point3d>().Min(n => n.Y), 0),
                new Point3d(points.Cast<Point3d>().Max(n => n.X), points.Cast<Point3d>().Max(n => n.Y), 0));
        }

        /// <summary> Get Intersect for Entity With Curve object </summary>
        public static Point3dCollection XIntersectWith(this Entity obj1, Curve obj2)
        {
            var pts = new Point3dCollection();
            obj1.IntersectWith(obj2, Intersect.OnBothOperands, pts, IntPtr.Zero, IntPtr.Zero);
            return pts;
        }

        /// <summary> XGetBasePoint </summary>
        public static Point3d XGetBasePoint(this Entity ent)
        {
            if (ent.GetType() == typeof(Line))
                return ((Line)ent).StartPoint;
            if (ent.GetType() == typeof(Circle))
                return ((Circle)ent).Center;
            if (ent.GetType() == typeof(DBPoint))
                return ((DBPoint)ent).Position;
            if (ent.GetType() == typeof(DBText))
                return ((DBText)ent).Position;
            if (ent.GetType() == typeof(MText))
                return ((MText)ent).Location;
            if (ent.GetType() == typeof(BlockReference))
                return ((BlockReference)ent).Position;
            if (ent.GetType() == typeof(Polyline))
                if (ent.Bounds != null)
                    return new Point3d(ent.Bounds.Value.MinPoint.X, ent.Bounds.Value.MinPoint.Y, 0);
            //return new Point3d(((Polyline)tEnt).GetPoint2dAt(0).X, ((Polyline)tEnt).GetPoint2dAt(0).Y, 0);
            if (ent.GetType() == typeof(Polyline3d))
                if (ent.Bounds != null)
                    return new Point3d(ent.Bounds.Value.MinPoint.X, ent.Bounds.Value.MinPoint.Y, 0);
            //foreach (ObjectId vId in (Polyline3d)tEnt)
            //{
            //    using (var Tr = Tools.Db.TransactionManager.StartTransaction())
            //    {
            //        var v3D = (PolylineVertex3d)Tr.GetObject(vId, OpenMode.ForRead);
            //        return v3D.Position;
            //    }
            //}
            return Point3d.Origin;
        }

        /// <summary> Point3DToArray </summary>
        public static Array Point3DToArray(this Point3d ent)
        {
            return new[] { ent.X, ent.Y, ent.Z };
        }

        /// <summary> XPoint2DtoPoint3D </summary>
        public static Point3d XPoint2DtoPoint3D(this Point2d tPnt2D)
        {
            return new Point3d(tPnt2D.X, tPnt2D.Y, 0);
        }

        public static Point2d XtoPoint2D(this Point3d tPnt3D)
        {
            return new Point2d(tPnt3D.X, tPnt3D.Y);
        }

        /// <summary> Get Coords </summary>
        public static List<Point3d> XGetPoints(this Entity ent)
        {
            var points = new List<Point3d>();
            if (ent.GetType() == typeof(Polyline))
            {
                for (var nPoint = 0; nPoint < ((Polyline)ent).NumberOfVertices; nPoint++)
                {
                    var pnt = ((Polyline)ent).GetPoint3dAt(nPoint);
                    points.Add(new Point3d(pnt.X, pnt.Y, 0));
                }
            }
            else if (ent.GetType() == typeof(Polyline2d))
            {
                foreach (ObjectId vId in (Polyline2d)ent)
                {
                    using (var tr = EntityExtensions.Db.TransactionManager.StartTransaction())
                    {
                        Vertex2d v2d = (Vertex2d)tr.GetObject(vId, OpenMode.ForRead);
                        points.Add(v2d.Position);
                    }
                }
            }
            else if (ent.GetType() == typeof(Polyline3d))
            {
                foreach (ObjectId vId in (Polyline3d)ent)
                {
                    using (var tr = EntityExtensions.Db.TransactionManager.StartTransaction())
                    {
                        PolylineVertex3d v3D = (PolylineVertex3d)tr.GetObject(vId, OpenMode.ForRead);
                        points.Add(v3D.Position);
                    }
                }
            }
            else if (ent.GetType() == typeof(Line))
            {
                points.AddRange(new List<Point3d> { ((Line)ent).StartPoint, ((Line)ent).EndPoint });
            }
            else if (ent.GetType() == typeof(Circle))
            {
                points.Add(((Circle)ent).Center);
            }
            else if (ent.GetType() == typeof(DBPoint))
            {
                points.Add(((DBPoint)ent).Position);
            }
            else if (ent.GetType() == typeof(DBText))
            {
                points.Add(((DBText)ent).Position);
            }
            else if (ent.GetType() == typeof(MText))
            {
                points.Add(((MText)ent).Location);
            }
            return points;
        }

        /// <summary> XGetPointsVertices </summary>
        public static Point3dCollection XGetPointsVertices(this Entity ent)
        {
            if (ent.GetType() == typeof(Polyline))
            {
                var result = new Point3dCollection();
                for (int i = 0; i < ((Polyline)ent).NumberOfVertices; i++)
                {
                    result.Add(((Polyline)ent).GetPoint3dAt(i));
                }
                return result;
            }

            if (ent.GetType().BaseType == typeof(Curve))
            {
                return new Point3dCollection(new[] { ((Curve)ent).StartPoint, ((Curve)ent).EndPoint });
            }

            return new Point3dCollection(new[] { ent.GeometricExtents.MinPoint, ent.GeometricExtents.MaxPoint });
        }

        /// <summary> XPoint2DtoPoint3D </summary>
        public static Point3dCollection XGetPointsOrthogonal(this Entity ent)
        {
            return new Point3dCollection
            {
                ent.GeometricExtents.MinPoint,
                new Point3d(ent.GeometricExtents.MaxPoint.X, ent.GeometricExtents.MinPoint.Y, 0),
                ent.GeometricExtents.MaxPoint,
                new Point3d(ent.GeometricExtents.MinPoint.X, ent.GeometricExtents.MaxPoint.Y, 0)
            };
        }

        public static bool IsPointInPolyLine2(Polyline Polyline, Point3d Point)
        {
            return false;
            //using (DBObject obj = validatedGroundElevations[iofTextDistanceBetweenElevations].Open(OpenMode.ForRead))
            //{
            //    Extents3d extents2 = obj.Bounds.Value;
            //    Point3d midPointGroundElevation = extents2.MaxPoint + extents2.MaxPoint.GetVectorTo(extents2.MinPoint).DivideBy(2);

            //    Point3d midPointBase = midPoint;
            //    using (Line line = new Line(midPoint, midPointGroundElevation))
            //    {
            //        Point3dCollection intersects = new Point3dCollection();
            //        (obj as Entity).BoundingBoxIntersectWith(line, Intersect.OnBothOperands, intersects, IntPtr.Zero, IntPtr.Zero);
            //        if (intersects.Count > 0)
            //            midPointGroundElevation = intersects[0];

            //        intersects.Clear();
            //        (drawable as Entity).BoundingBoxIntersectWith(line, Intersect.OnBothOperands, intersects, IntPtr.Zero, IntPtr.Zero);
            //        if (intersects.Count > 0)
            //            midPointBase = intersects[0];

            //        Vector3d v = (midPointBase - midPointGroundElevation).MultiplyBy(0.1);
            //        midPointBase -= v;
            //        midPointGroundElevation += v;
            //    }

            //    wd.SubEntityTraits.Color = 253;
            //    wd.Geometry.WorldLine(midPointBase, midPointGroundElevation);
            //}
        }

        public static bool XIsPointInside(this Point3d testObj, Point3dCollection targetObjPoints)
        {

            var targetObj = targetObjPoints.XGetExtents3D();

            var point1Exist = (testObj.X >= targetObj.MinPoint.X) &&
                              (testObj.X <= targetObj.MaxPoint.X);

            var point2Exist = (testObj.Y >= targetObj.MinPoint.Y) &&
                              (testObj.Y <= targetObj.MaxPoint.Y);

            return point1Exist && point2Exist;
        }

        /// <summary> true is if testObject exist in targetObject </summary>
        public static bool XIsPointsInside(this Point3dCollection testObjPoints, Point3dCollection targetObjPoints)
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

        public static Point3d XGetPoint3d(this Point2d point2d)
        {
            return new Point3d(point2d.X, point2d.Y, 0);
        }

        public static Point2d XGetPoint2d(this Point3d point3d)
        {
            return new Point2d(point3d.X, point3d.Y);
        }

        public static Point3d XGetPolarPoint(this Point3d basepoint, double angle, double distance)
        {
            return new Point3d(
                basepoint.X + distance * Math.Cos(angle),
                basepoint.Y + distance * Math.Sin(angle),
                basepoint.Z);
        }

        public static Entity RectangleFromPoints(this Point3dCollection pts, CoordinateSystem3d ucs, double buffer)
        {
            // Get the plane of the UCS
            Plane pl = new Plane(ucs.Origin, ucs.Zaxis);

            // We will project these (possibly 3D) points onto
            // the plane of the current UCS, as that's where
            // we will create our circle

            // Project the points onto it
            List<Point2d> pts2d = new List<Point2d>(pts.Count);
            for (int i = 0; i < pts.Count; i++)
            {
                pts2d.Add(pl.ParameterOf(pts[i]));
            }

            // Assuming we have some points in our list...
            if (pts.Count > 0)
            {
                // Set the initial min and max values from the first entry
                double minX = pts2d[0].X,
                    maxX = minX,
                    minY = pts2d[0].Y,
                    maxY = minY;

                // Perform a single iteration to extract the min/max X and Y
                for (int i = 1; i < pts2d.Count; i++)
                {
                    Point2d pt = pts2d[i];
                    if (pt.X < minX) minX = pt.X;
                    if (pt.X > maxX) maxX = pt.X;
                    if (pt.Y < minY) minY = pt.Y;
                    if (pt.Y > maxY) maxY = pt.Y;
                }

                // Our final buffer amount will be the percentage of the
                // smallest of the dimensions
                double buf = Math.Min(maxX - minX, maxY - minY) * buffer;

                // Apply the buffer to our point ordinates
                minX -= buf;
                minY -= buf;
                maxX += buf;
                maxY += buf;

                // Create the boundary points
                Point2d pt0 = new Point2d(minX, minY),
                    pt1 = new Point2d(minX, maxY),
                    pt2 = new Point2d(maxX, maxY),
                    pt3 = new Point2d(maxX, minY);

                // Finally we create the polyline
                var p = new Polyline(4);

                p.Normal = pl.Normal;
                p.AddVertexAt(0, pt0, 0, 0, 0);
                p.AddVertexAt(1, pt1, 0, 0, 0);
                p.AddVertexAt(2, pt2, 0, 0, 0);
                p.AddVertexAt(3, pt3, 0, 0, 0);
                p.Closed = true;

                return p;
            }

            return null;
        }

        public static Entity CircleFromPoints(this Point3dCollection pts, CoordinateSystem3d ucs, double buffer)
        {
            // Get the plane of the UCS
            Plane pl = new Plane(ucs.Origin, ucs.Zaxis);

            // We will project these (possibly 3D) points onto
            // the plane of the current UCS, as that's where
            // we will create our circle

            // Project the points onto it
            List<Point2d> pts2d = new List<Point2d>(pts.Count);
            for (int i = 0; i < pts.Count; i++)
            {
                pts2d.Add(pl.ParameterOf(pts[i]));
            }

            // Assuming we have some points in our list...

            if (pts.Count > 0)
            {
                // We need the center and radius of our circle
                Point2d center;
                double radius = 0;

                // Use our fast approximation algorithm to
                // calculate the center and radius of our
                // circle to within 1% (calling the function
                // with 100 iterations gives 10%, calling it
                // with 10K gives 1%)
                BadoiuClarksonIteration(pts2d, 10000, out center, out radius);

                // Get our center point in WCS (on the plane
                // of our UCS)

                Point3d cen3d = pl.EvaluatePoint(center);
                // Create the circle and add it to the drawing

                return new Circle(cen3d, ucs.Zaxis, radius * (1.0 + buffer));
            }

            return null;
        }

        private static void BadoiuClarksonIteration(List<Point2d> set, int iter, out Point2d cen, out double rad)
        {
            // Choose any point of the set as the initial
            // circumcenter
            cen = set[0];
            rad = 0;

            for (int i = 0; i < iter; i++)
            {
                int winner = 0;
                double distmax = (cen - set[0]).Length;

                // Maximum distance point
                for (int j = 1; j < set.Count; j++)
                {
                    double dist = (cen - set[j]).Length;
                    if (dist > distmax)
                    {
                        winner = j;
                        distmax = dist;
                    }
                }
                rad = distmax;

                // Update
                cen = new Point2d(cen.X + (1.0 / (i + 1.0)) * (set[winner].X - cen.X),
                        cen.Y + (1.0 / (i + 1.0)) * (set[winner].Y - cen.Y));

            }

        }

        /// <summary> Returnig extens as result of select the two points: first - left_down corner; second - up_right corner </summary>
        public static Extents2d Normalize(this Extents2d extents)
        {
            List<Point2d> points = new List<Point2d>() { extents.MinPoint, extents.MaxPoint };

            var minX = points.Min(p => p.X);
            var minY = points.Min(p => p.Y);
            var maxX = points.Max(p => p.X);
            var maxY = points.Max(p => p.Y);

            return new Extents2d(new Point2d(minX, minY), new Point2d(maxX, maxY));
        }

        public static Extents2d Notmalize(this List<Point2d> points)
        {
            // Add checks here, if necessary, to make sure that points is not null,
            // and that it contains at least one (or perhaps two?) elements

            var minX = points.Min(p => p.X);
            var minY = points.Min(p => p.Y);
            var maxX = points.Max(p => p.X);
            var maxY = points.Max(p => p.Y);

            return new Extents2d(new Point2d(minX, minY), new Point2d(maxX, maxY));
        }

        /// <summary> Reverses the order of the X and Y properties of a Point2d. </summary>
        /// <param name="flip">Boolean indicating whether to reverse or not.</param>
        /// <returns>The original Point2d or the reversed version.</returns>
        public static Point2d Swap(this Point2d pt, bool flip = true)
        {
            return flip ? new Point2d(pt.Y, pt.X) : pt;
        }

        /// <summary>
        /// Pads a Point2d with a zero Z value, returning a Point3d.
        /// </summary>
        /// <param name="pt">The Point2d to pad.</param>
        /// <returns>The padded Point3d.</returns>

        public static Point3d Pad(this Point2d pt)
        {
            return new Point3d(pt.X, pt.Y, 0);
        }

        /// <summary>
        /// Strips a Point3d down to a Point2d by simply ignoring the Z ordinate.
        /// </summary>
        /// <param name="pt">The Point3d to strip.</param>
        /// <returns>The stripped Point2d.</returns>

        public static Point2d ToPoint2d(this Point3d pt)
        {
            return new Point2d(pt.X, pt.Y);
        }
        public static Point3d ToPoint3d(this Point2d pt)
        {
            return new Point3d(pt.X, pt.Y, 0);
        }

        public static Extents2d ToExtents2d(this Extents3d ext)
        {
            return new Extents2d(ext.MinPoint.ToPoint2d(), ext.MaxPoint.ToPoint2d());
        }
        public static Extents3d ToExtents3d(this Extents2d ext)
        {
            return new Extents3d(ext.MinPoint.ToPoint3d(), ext.MaxPoint.ToPoint3d());
        }

        public static Extents2d Margin(this Extents2d ext, Extents2d margins)
        {
            return new Extents2d(
                new Point2d(ext.MinPoint.X - margins.MinPoint.X, ext.MinPoint.Y - margins.MinPoint.Y),
                new Point2d(ext.MaxPoint.X + margins.MaxPoint.X, ext.MaxPoint.Y + margins.MaxPoint.Y));
        }
        public static Extents2d Margin(this Extents2d ext, double margin = 0.0)
        {
            return new Extents2d(
                new Point2d(ext.MinPoint.X - margin, ext.MinPoint.Y - margin),
                new Point2d(ext.MaxPoint.X + margin, ext.MaxPoint.Y + margin));
        }

        public static void Rotate2(this Point2d center, float degrees)
        {
            Matrix mat = new Matrix();
            // NOTE: Matrix.RotateAt() rotates clockwise.
            //mat.RotateAt(degrees, center.X, center.Y);
            //this.A = mat.Transform(this.A);
            //this.B = mat.Transform(this.B);
            //this.C = mat.Transform(this.C);
            //this.D = mat.Transform(this.D);
        }

        public static void Rotate(this Point3d center, float degrees)
        {
            Matrix3d acMat3d = new Matrix3d();
            var doc = acadApp.DocumentManager.MdiActiveDocument;
            Matrix3d ucs = doc.Editor.CurrentUserCoordinateSystem;
            CoordinateSystem3d cs = ucs.CoordinateSystem3d;

            acMat3d = Matrix3d.Rotation(Math.PI / 2, cs.Zaxis, center);
            center.TransformBy(acMat3d);
        }

        public static Extents2d SetRotate(this Extents2d extents, double angle, Point3d center)
        {
            using (Polyline acPoly = new Polyline())
            {
                Point3dCollection pts = new Point3dCollection(new[] { extents.MinPoint.ToPoint3d(), extents.MaxPoint.ToPoint3d() });
                acPoly.XCreateVertices(pts);

                // Reduce the object by a factor of 0.5 
                //acPoly.TransformBy(Matrix3d.Scaling(0.5, new Point3d(4, 4.25, 0)));

                Matrix3d acMat3d = new Matrix3d();
                var doc = acadApp.DocumentManager.MdiActiveDocument;
                Matrix3d ucs = doc.Editor.CurrentUserCoordinateSystem;
                CoordinateSystem3d cs = ucs.CoordinateSystem3d;

                acMat3d = Matrix3d.Rotation(angle, cs.Zaxis, center); //Math.PI / 2
                acPoly.TransformBy(acMat3d);

                return new Extents2d(acPoly.GeometricExtents.MinPoint.ToPoint2d(), acPoly.GeometricExtents.MaxPoint.ToPoint2d());
            }
        }
    }
}