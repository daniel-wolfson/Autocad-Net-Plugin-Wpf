using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using ID.Infrastructure.Extensions;
using Intellidesk.AcadNet.Common.Enums;
using Intellidesk.AcadNet.Common.Extensions;
using Intellidesk.AcadNet.Common.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using Polyline = Autodesk.AutoCAD.DatabaseServices.Polyline;

namespace Intellidesk.AcadNet.Common.Drawing
{
    // todo: add more entities
    /// <summary>
    /// The "NoDraw" module: create in-memory entities
    /// </summary>
    public static class DrawObject
    {
        //public static double LinetypeScaleCurrentMyProperty { get; set; } = 0;

        /// <summary>
        /// Creates a line.
        /// </summary>
        /// <param name="point1">The point 1.</param>
        /// <param name="point2">The point 2.</param>
        /// <returns>The result.</returns>
        public static Line Line(Point3d point1, Point3d point2)
        {
            return new Line(point1, point2);
        }

        /// <summary>
        /// Creates multiple lines from a sequence of points.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <returns>The result.</returns>
        public static Line[] Line(params Point3d[] points)
        {
            return Enumerable
                .Range(0, points.Length - 1)
                .Select(index => Line(points[index], points[index + 1]))
                .ToArray();
        }

        /// <summary>
        /// Creates an arc from 3 points.
        /// </summary>
        /// <param name="point1">The point 1.</param>
        /// <param name="point2">The point 2.</param>
        /// <param name="point3">The point 3.</param>
        /// <returns>The result.</returns>
        public static Arc Arc3P(Point3d point1, Point3d point2, Point3d point3)
        {
            var arc = new CircularArc3d(point1, point2, point3);
            return ArcFromGeometry(arc);
        }

        //public static Arc ArcSCE(Point3d start, Point3d center, Point3d end)
        //{
        //}

        /// <summary>
        /// Creates an arc from start, center, and angle.
        /// </summary>
        /// <param name="start">The start point.</param>
        /// <param name="center">The center point.</param>
        /// <param name="angle">The angle.</param>
        /// <returns>The result.</returns>
        public static Arc ArcSCA(Point3d start, Point3d center, double angle)
        {
            double radius = center.DistanceTo(start);
            var dir1 = start - center;
            double startangle = dir1.GetAngleTo(Vector3d.XAxis);
            if (dir1.Y < 0)
            {
                startangle = Math.PI * 2 - startangle;
            }
            double endangle;
            if (angle > 0)
            {
                endangle = startangle + angle;
            }
            else
            {
                double a = startangle;
                startangle = startangle + angle;
                endangle = a;
            }
            var arc = new CircularArc3d(center, Vector3d.ZAxis, Vector3d.XAxis, radius, startangle, endangle);
            return ArcFromGeometry(arc);
        }

        //public static Arc ArcSCL(Point3d start, Point3d center, double length)
        //{
        //}

        //public static Arc ArcSEA(Point3d start, Point3d end, double angle)
        //{
        //}

        //public static Arc ArcSED(Point3d start, Point3d end, Vector3d dir)
        //{
        //}

        //public static Arc ArcSER(Point3d start, Point3d end, double radius)
        //{
        //}

        /// <summary>
        /// Creates an arc from a geometry arc.
        /// </summary>
        /// <param name="arc">The geometry arc.</param>
        /// <returns>The result.</returns>
        public static Arc ArcFromGeometry(CircularArc3d arc)
        {
            return new Arc(arc.Center, arc.Normal, arc.Radius, arc.StartAngle, arc.EndAngle);
        }

        /// <summary>
        /// Creates an arc from a geometry arc.
        /// </summary>
        /// <param name="arc">The geometry arc.</param>
        /// <returns>The result.</returns>
        public static Arc ArcFromGeometry(CircularArc2d arc)
        {
            return new Arc(arc.Center.ToPoint3d(), Vector3d.ZAxis, arc.Radius, arc.StartAngle, arc.EndAngle);
        }

        /// <summary>
        /// Creates a circle from center and radius.
        /// </summary>
        /// <param name="center">The center.</param>
        /// <param name="radius">The radius.</param>
        /// <returns>The result.</returns>
        public static Circle Circle(Point3d center, double radius)
        {
            return new Circle(center, Vector3d.ZAxis, radius);
        }

        /// <summary>
        /// Creates a circle from diameter ends.
        /// </summary>
        /// <param name="point1">The point 1.</param>
        /// <param name="point2">The point 2.</param>
        /// <returns>The result.</returns>
        public static Circle Circle(Point3d point1, Point3d point2)
        {
            return Circle(
                center: Point3d.Origin + 0.5 * (point1 - Point3d.Origin + (point2 - Point3d.Origin)),
                radius: 0.5 * point1.DistanceTo(point2));
        }

        /// <summary>
        /// Creates a circle from 3 points.
        /// </summary>
        /// <param name="point1">The point 1.</param>
        /// <param name="point2">The point 2.</param>
        /// <param name="point3">The point 3.</param>
        /// <returns>The result.</returns>
        public static Circle Circle(Point3d point1, Point3d point2, Point3d point3)
        {
            var geo = new CircularArc3d(point1, point2, point3);
            return Circle(geo);
        }

        /// <summary>
        /// Creates a circle from a geometry circle.
        /// </summary>
        /// <param name="circle">The geometry circle.</param>
        /// <returns>The result.</returns>
        public static Circle Circle(CircularArc3d circle)
        {
            return new Circle(circle.Center, circle.Normal, circle.Radius);
        }

        /// <summary>
        /// Creates a circle from a geometry circle.
        /// </summary>
        /// <param name="circle">The geometry circle.</param>
        /// <returns>The result.</returns>
        public static Circle Circle(CircularArc2d circle)
        {
            return new Circle(circle.Center.ToPoint3d(), Vector3d.ZAxis, circle.Radius);
        }

        /// <summary>
        /// Creates an ellipse by center, endX, and radiusY.
        /// </summary>
        /// <remarks>
        /// The ellipse will be created on the XY plane.
        /// </remarks>
        /// <param name="center">The center.</param>
        /// <param name="endX">The intersection point of the ellipse and its X axis.</param>
        /// <param name="radiusY">The Y radius.</param>
        /// <returns>The result.</returns>
        public static Ellipse Ellipse(Point3d center, Point3d endX, double radiusY)
        {
            var radiusRatio = center.DistanceTo(endX) / radiusY;
            var axisX = endX - center;
            if (center.DistanceTo(endX) > radiusY)
            {
                radiusRatio = radiusY / center.DistanceTo(endX);
            }
            else
            {
                axisX = axisX.RotateBy(Math.PI / 2.0, Vector3d.ZAxis);
                axisX = axisX.MultiplyBy(radiusY / center.DistanceTo(endX));
            }

            return new Ellipse(
                center: center,
                unitNormal: Vector3d.ZAxis,
                majorAxis: axisX,
                radiusRatio: radiusRatio,
                startAngle: 0,
                endAngle: 2 * Math.PI);
        }

        /// <summary>
        /// Creates a polyline by a sequence of points.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <returns>The result.</returns>
        public static Polyline Pline(params Point3d[] points)
        {
            return Pline(points.ToList());
        }

        /// <summary>
        /// Creates a polyline by a sequence of points and a global width.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="globalWidth">The global width. Default is 0.</param>
        /// <returns>The result.</returns>
        public static Polyline Pline(IEnumerable<Point3d> points, double globalWidth = 0)
        {
            return Pline(
                vertices: points.Select(point => Tuple.Create(point, 0d)).ToList(),
                globalWidth: globalWidth);
        }

        /// <summary>
        /// Creates a polyline by a sequence of vertices (position + bulge).
        /// </summary>
        /// <param name="vertices">The vertices.</param>
        /// <param name="globalWidth">The global width. Default is 0.</param>
        /// <returns>The result.</returns>
        public static Polyline Pline(List<Tuple<Point3d, double>> vertices, double globalWidth = 0)
        {
            var poly = new Polyline();
            Enumerable
                .Range(0, vertices.Count)
                .ForEach(index => poly.AddVertexAt(
                    index: index,
                    pt: vertices[index].Item1.XGetPoint2d(),
                    bulge: vertices[index].Item2,
                    startWidth: globalWidth,
                    endWidth: globalWidth));

            return poly;
        }

        /// <summary>
        /// Creates a spline by fit points.
        /// </summary>
        /// <param name="points">The points to fit.</param>
        /// <returns>The result.</returns>
        public static Spline SplineFit(Point3d[] points)
        {
            return new Spline(
                point: new Point3dCollection(points),
                order: 3,
                fitTolerance: Consts.Epsilon);
        }

        /// <summary>
        /// Creates a spline by control points.
        /// </summary>
        /// <param name="points">The control points.</param>
        /// <param name="closed">Whether to close the spline.</param>
        /// <returns>The result.</returns>
        public static Spline SplineCV(Point3d[] points, bool closed = false)
        {
            var controlPoints = new Point3dCollection(points);
            DoubleCollection knots;
            DoubleCollection weights;
            if (!closed)
            {
                knots = new DoubleCollection(Enumerable.Range(0, points.Length - 2).Select(index => (double)index).ToArray());
                knots.Insert(0, 0);
                knots.Insert(0, 0);
                knots.Insert(0, 0);
                knots.Add(points.Length - 3);
                knots.Add(points.Length - 3);
                knots.Add(points.Length - 3);
                weights = new DoubleCollection(Enumerable.Repeat(1, points.Length).Select(index => (double)index).ToArray());
            }
            else
            {
                controlPoints.Add(points[0]);
                controlPoints.Add(points[1]);
                controlPoints.Add(points[2]);
                knots = new DoubleCollection(Enumerable.Range(0, points.Length + 7).Select(index => (double)index).ToArray());
                weights = new DoubleCollection(Enumerable.Repeat(1, points.Length + 3).Select(index => (double)index).ToArray());
            }

            return new Spline(
                degree: 3,
                rational: true,
                closed: closed,
                periodic: closed,
                controlPoints: controlPoints,
                knots: knots,
                weights: weights,
                controlPointTolerance: 0,
                knotTolerance: 0);
        }

        /// <summary>
        /// Creates a rectangle.
        /// </summary>
        /// <param name="point1">The point 1.</param>
        /// <param name="point2">The point 2.</param>
        /// <returns>The result.</returns>
        public static Polyline Rectang(Point3d point1, Point3d point2)
        {
            var result = Pline(new[]
            {
                point1,
                new Point3d(point1.X, point2.Y, 0),
                point2,
                new Point3d(point2.X, point1.Y, 0)
            });
            result.Closed = true;
            return result;
        }

        /// <summary>
        /// Creates a DT.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="height">The height.</param>
        /// <param name="position">The position.</param>
        /// <param name="rotation">The rotation.</param>
        /// <param name="centerAligned">Whether to center align.</param>
        /// <param name="textStyle">The text style name.</param>
        /// <returns>The result.</returns>
        public static DBText Text(string text, double height, Point3d position, double rotation = 0, bool centerAligned = false, string textStyle = Consts.TextStyleName)
        {
            var textStyleId = DbHelper.GetTextStyleId(textStyle);
            var style = textStyleId.XOpenForRead<TextStyleTableRecord>();
            var dbText = new DBText
            {
                TextString = text,
                Position = position,
                Rotation = rotation,
                TextStyleId = textStyleId,
                Height = height,
                Oblique = style.ObliquingAngle,
                WidthFactor = style.XScale
            };

            if (centerAligned) // todo: centerAligned=true makes DT vanished
            {
                dbText.HorizontalMode = TextHorizontalMode.TextCenter;
                dbText.VerticalMode = TextVerticalMode.TextVerticalMid;
            }

            return dbText;
        }

        /// <summary>
        /// Creates an MT.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="height">The height.</param>
        /// <param name="position">The position.</param>
        /// <param name="rotation">The rotation.</param>
        /// <param name="centerAligned">Whether to center align.</param>
        /// <param name="width">The width.</param>
        /// <returns>The result.</returns>
        public static MText MText(string text, double height, Point3d position, double rotation = 0, bool centerAligned = false, double width = 0)
        {
            var mText = new MText
            {
                Contents = text,
                TextHeight = height,
                Location = position,
                Rotation = rotation,
                Width = width
            };

            if (centerAligned)
            {
                mText.Move(mText.Location - mText.GetCenter());
            }

            return mText;
        }

        /// <summary>
        /// Creates a point.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns>The result.</returns>
        public static DBPoint Point(Point3d position)
        {
            return new DBPoint(position);
        }

        /// <summary>
        /// Creates a block reference.
        /// </summary>
        /// <param name="blockTableRecordId">The block table record ID.</param>
        /// <param name="position">The position.</param>
        /// <param name="rotation">The rotation.</param>
        /// <param name="scale">The scale.</param>
        /// <returns>The result.</returns>
        public static BlockReference Insert(ObjectId blockTableRecordId, Point3d position, double rotation = 0, double scale = 1)
        {
            return new BlockReference(position, blockTableRecordId)
            {
                Rotation = rotation,
                ScaleFactors = new Scale3d(scale)
            };
        }

        /// <summary> Donut </summary>
        public static Entity Donut(double x, double y, double radius, short colorIndex = -1)
        {
            Polyline pline = new Polyline
            {
                Color = Colors.GetColorFromIndex(colorIndex),
                LineWeight = LineWeight.LineWeight013
            };

            radius = radius / 2;
            var pt1 = new Point2d(x, y);
            var pt2 = new Point2d(x + (radius / 2), y);

            pline.AddVertexAt(0, pt1, 1.0, radius, radius);
            pline.AddVertexAt(1, pt2, 1.0, radius, radius);

            return pline;
        }

        /// <summary> Donut </summary>
        public static Entity Donut(double x, double y, double insideDiameter, double outsideDiameter,
            short colorIndex = 255, double? width = null)
        {
            Polyline pline = new Polyline() { Color = Color.FromColorIndex(ColorMethod.ByAci, colorIndex) };

            double rad1 = insideDiameter / 2;
            double rad2 = outsideDiameter / 2;
            var w = width ?? (rad2 - rad1) / 2;

            Point2d pt1 = new Point2d(x - (rad1 + w / 2), y);
            Point2d pt2 = new Point2d(x + (rad1 + w / 2), y);

            pline.AddVertexAt(0, pt1, 1.0, w, w);
            pline.AddVertexAt(1, pt2, 1.0, w, w);
            pline.Closed = true;

            return pline;
        }

        /// <summary> Line </summary>
        public static Entity Line(double tPntX, double tPntY, double tPntX2, double tPntY2, int tTypeLine = 0,
            short tColor = Colors.ByLayer, double tRotation = 0, Point2d? tPntRotate = null)
        {
            var obj = new Line(new Point3d(tPntX, tPntY, 0), new Point3d(tPntX2, tPntY2, 0))
            {
                Color = Colors.GetColorFromIndex(tColor),
                //LinetypeScale = LinetypeScaleCurrent
            };
            obj.SetDatabaseDefaults();
            obj.LinetypeId = HostApplicationServices.WorkingDatabase.LinetypeTableId;

            if (tRotation > 0.0)
            {
                tPntRotate = tPntRotate ?? new Point2d(tPntX, tPntY);
                obj.XRotate(tRotation, new Point3d(((Point2d)tPntRotate).X, ((Point2d)tPntRotate).Y, 0));
            }
            return obj;
        }

        /// <summary> Polyline </summary>
        public static Entity Polyline(Extents3d tExtents3D, string layer = "0", int colorIndex = Colors.ByLayer,
            double width = 0.0, byte close = 1, Point3d? basePointTransform = null, double tRotation = 0.0,
            double scaleFactor = 0.0, Vector3d? displacement = null)
        {
            return DrawObject.Polyline(tExtents3D.MinPoint, tExtents3D.MaxPoint, layer, colorIndex, width, close,
                basePointTransform, tRotation, scaleFactor, displacement);
        }

        /// <summary> Polyline </summary>
        public static Entity Polyline(Point3d tPointStart, Point3d tPointEnd, string layer = "0",
            int colorIndex = Colors.ByLayer,
            double tWidth = 0.0, byte close = 1, Point3d? basePointTransform = null, double rotation = 0.0,
            double scaleFactor = 0.0, Vector3d? displacement = null)
        {
            return Polyline(
                new Point3dCollection
                {
                    tPointStart,
                    new Point3d(tPointEnd.X, tPointStart.Y, 0),
                    tPointEnd,
                    new Point3d(tPointStart.X, tPointEnd.Y, 0)
                },
                layer, colorIndex, tWidth, close, basePointTransform, rotation, scaleFactor, displacement);
        }

        /// <summary> Polyline </summary>
        public static Entity Polyline(Point3dCollection points, string layer = "0", int color = Colors.ByLayer,
            double width = 0.0, byte close = 1, Point3d? basePointTransform = null,
            double rotation = 0.0, double scaleFactor = 0.0,
            Vector3d? tDisplacement = null)
        {
            var obj = new Polyline
            {
                Layer = layer,
                ColorIndex = color,
                Closed = close > 0,
                //LinetypeScale = LinetypeScaleCurrent
            };
            obj.SetDatabaseDefaults();
            obj.LinetypeId = obj.Database.LinetypeTableId;

            for (var i = 0; i <= points.Count - 1; i++)
            {
                obj.AddVertexAt(i, new Point2d(points[i].X, points[i].Y), 0, 0, 0);
                if (width <= 0.0001) continue;
                obj.SetStartWidthAt(i, width);
                obj.SetEndWidthAt(i, width);
            }

            if (basePointTransform == null && (scaleFactor > 0.0 | tDisplacement != null | rotation > 0.0))
            {
                var p3D = new Polyline3d(Poly3dType.SimplePoly, points, true);
                if (p3D.Bounds.HasValue) basePointTransform = p3D.Bounds.Value.MinPoint;
            }

            if (scaleFactor > 0.0 && basePointTransform != null)
                obj.XScale(scaleFactor, (Point3d)basePointTransform);

            if (tDisplacement != null)
                obj.TransformBy(Matrix3d.Displacement((Vector3d)tDisplacement));

            if (rotation > 0.0 && basePointTransform != null)
                obj.XRotate(rotation, (Point3d)basePointTransform);

            return obj;
        }

        /// <summary> Pline </summary>
        public static Entity Polyline(Transaction tr, double[] x, double[] y, short tColor = Colors.ByLayer,
            double tWidth = 0.0,
            byte tClose = 0, double tPntXRotate = double.MinValue, double tPntYRotate = double.MinValue,
            double tRotation = 0.0)
        {
            var obj = new Polyline(); // { LinetypeScale = LinetypeScaleCurrent };
            obj.SetDatabaseDefaults();
            obj.LinetypeId = obj.Database.LinetypeTableId;

            for (var i = 0; i <= x.Length - 1; i++)
            {
                obj.AddVertexAt(i, new Point2d(x[i], y[i]), 0, 0, 0);
                if (tWidth <= 0.0001) continue;
                obj.SetStartWidthAt(i, tWidth);
                obj.SetEndWidthAt(i, tWidth);
            }
            obj.Color = Colors.GetColorFromIndex(tColor);
            if (tClose > 0)
            {
                obj.Closed = true;
            }
            obj.XSaveChanges();

            if (tRotation > 0.0)
            {
                if (tPntXRotate.Equals(Double.MinValue))
                {
                    tPntXRotate = x[0];
                    tPntYRotate = y[0];
                }
                obj.XRotate(tRotation, new Point3d(tPntXRotate, tPntYRotate, 0));
            }
            //ObjectIds.Add(obj.ObjectId);

            return obj;
        } // Polyline

        /// <summary> Solid </summary>
        public static object Solid(double[] x, double[] y, short tColor = Colors.ByLayer)
        {
            var obj = new Solid(new Point3d(x[0], y[0], 0), new Point3d(x[1], y[1], 0),
                new Point3d(x[2], y[2], 0), new Point3d(x[3], y[3], 0))
            {
                Color = Colors.GetColorFromIndex(tColor),
                //LinetypeScale = LinetypeScaleCurrent
            };
            obj.SetDatabaseDefaults();
            obj.LinetypeId = HostApplicationServices.WorkingDatabase.LinetypeTableId;
            return obj;
        }

        /// <summary> Point </summary>
        public static Entity Point(double tPntX, double tPntY, short tColor = Colors.ByLayer,
            int tFmod = 0, int tPntMod = 34, double tPntSize = 5.0)
        {
            var currentObject = new DBPoint(new Point3d(tPntX, tPntY, 0));
            var obj = (DBPoint)currentObject;
            obj.SetDatabaseDefaults();

            if (tFmod > 0)
            {
                HostApplicationServices.WorkingDatabase.Pdmode = tPntMod;
                HostApplicationServices.WorkingDatabase.Pdsize = tPntSize;
            }
            obj.Color = Colors.GetColorFromIndex(tColor);

            return currentObject;
        }

        public static DBText Text(AcadNet.Common.Models.DbTextArgs args)

        {
            DBText ent = new DBText
            {
                Position = new Point3d(args.Position.X, args.Position.Y, 0),
                Height = args.Height < 0.8 ? 0.8 : args.Height,
                TextString = args.TextString,
                //Layer = CurrentLayerName
            };

            switch (args.Alignment)
            {
                case AlignmentOptions.ByCenterHorizontalMode:
                    ent.HorizontalMode = TextHorizontalMode.TextCenter;
                    ent.AlignmentPoint = new Point3d(args.Position.X, args.Position.Y, 0);
                    args.Rotation = 0;
                    break;
                case AlignmentOptions.ByCenterVerticalMode:
                    ent.HorizontalMode = TextHorizontalMode.TextCenter;
                    ent.VerticalMode = TextVerticalMode.TextVerticalMid;
                    ent.AlignmentPoint = new Point3d(args.Position.X, args.Position.Y, 0);
                    break;
                case AlignmentOptions.ByLeft:
                    ent.HorizontalMode = TextHorizontalMode.TextLeft;
                    break;
            }

            ent.Color = Color.FromColorIndex(ColorMethod.ByAci, (short)args.ColorIndex);
            ent.SetDatabaseDefaults();

            if (args.Rotation > 0.0 && args.otherPointRotate == null)
                ent.XRotate(args.Rotation, new Point3d(ent.Position.X, ent.Position.Y, 0));

            return ent;
        }

        /// <summary> RecHat </summary>
        public static Entity RecHat(double[] x, double[] y, short tColor = Colors.ByLayer)
        {

            var db = HostApplicationServices.WorkingDatabase;
            var obj = new Hatch();
            try
            {
                obj.SetDatabaseDefaults();
                using (var tr = db.TransactionManager.StartTransaction())
                {
                    var bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    // Open the Block table for read
                    var documentManager = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager;
                    var btr = tr.GetObject(bt[documentManager.GetCurrentSpace()], OpenMode.ForWrite) as BlockTableRecord;
                    var acObjIn = Polyline(tr, x, y);
                    var acObjIds = new ObjectIdCollection { acObjIn.ObjectId };

                    obj.HatchObjectType = HatchObjectType.HatchObject;
                    obj.PatternScale = 2.0;
                    obj.SetHatchPattern(HatchPatternType.PreDefined, "ANSI31");
                    obj.Color = Colors.GetColorFromIndex(tColor);

                    if (btr != null) btr.AppendEntity(obj);
                    tr.AddNewlyCreatedDBObject(obj, true);

                    obj.Associative = true;
                    obj.AppendLoop(HatchLoopTypes.Default, acObjIds);
                    obj.EvaluateHatch(true);
                    //ObjectIds.Add(obj.ObjectId);

                    foreach (ObjectId objId in acObjIds)
                    {
                        tr.GetObject(objId, OpenMode.ForWrite).Erase();
                    }
                }
            }
            catch (Exception ex)
            {
                //Plugin.Logger.Error($"{nameof(InsertJig)}.{nameof(RecHat)} error: ", ex);
            }
            return obj;
        }
    }
}