using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Intellidesk.AcadNet.Services.Interfaces;
using Intellidesk.Infrastructure;
using Microsoft.Practices.Unity;

using Exception = Autodesk.AutoCAD.Runtime.Exception;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Intellidesk.AcadNet.Services
{
    /// <summary> DrawService (version 3.0) </summary>
    public static partial class Draw
    {
        public static Database db { get { return HostApplicationServices.WorkingDatabase; }}
        public static Entity XAddNew(this Entity tEnt)
        {
            Document doc = Application.DocumentManager.GetDocument(db);
            try
            {
                using (DocumentLock l = doc.LockDocument())
                {
                    using (var tr = db.TransactionManager.StartOpenCloseTransaction())
                    {
                        tEnt.SetDatabaseDefaults();

                        // Open the Block table for read
                        var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);

                        // Open the Block table record Model space for write
                        var btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                        // Add the new object to the block table record and the transaction
                        if (btr != null) btr.AppendEntity(tEnt);

                        // Save the new object to the database
                        tr.AddNewlyCreatedDBObject(tEnt, true);

                        tr.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Add(ex); //Log.Add("Error", ProjectManager.NameMsg + "<XAddNew> {0}, {1}", ex.Message, ex.Source);
            }
            return tEnt;
        }

        public static double LinetypeScaleCurrent;
        /// <summary> Polyline extension method </summary>
        public static Polyline XInit(this Polyline obj, Point3dCollection points)
        {
            for (var i = 0; i <= points.Count - 1; i++)
            {
                obj.AddVertexAt(i, new Point2d(points[i].X, points[i].Y), 0, 0, 0);
            }
            //if (width <= 0.0001) continue;
            //obj.SetStartWidthAt(i, width);
            //obj.SetEndWidthAt(i, width);
            return obj;
        }

        /// <summary> Polyline extension method </summary>
        public static Solid3d XInit(this Solid3d solid3D, IEnumerable<Entity> entities, BooleanOperationType booleanOperationType)
        {
            var dbObjects = new DBObjectCollection();
            DBObjectCollection objects = dbObjects;
            entities.ToList().ForEach(x => { objects.Add(x); x.XRemove(); });

            dbObjects = Region.CreateFromCurves(dbObjects);
            if (dbObjects.Count > 1)
            {
                for (int i = 1; i <= dbObjects.Count - 1; i++)
                {
                    ((Region)dbObjects[0]).BooleanOperation(booleanOperationType, (Region)dbObjects[i]);
                    dbObjects[i].Dispose();
                }
            }
            var acRegion = (Region)dbObjects[0];
            solid3D.Extrude(acRegion, 3, 0);
            return solid3D;
        }

        public static Entity Donut(double x, double y, double radius, short color = -1)
        {
            var obj = new Polyline { Color = Colors.GetColorFromIndex(color) };
            radius = radius / 2;
            var pt1 = new Point2d(x - (radius / 2), y);
            var pt2 = new Point2d(x + (radius / 2), y);
            obj.AddVertexAt(0, pt1, 1.0, radius, radius);
            obj.AddVertexAt(1, pt2, 1.0, radius, radius);
            obj.AddVertexAt(2, pt1, 0.0, radius, radius);
            obj.SetDatabaseDefaults();
            return obj;
        }

        public static Entity Circle(double tPntX, double tPntY, double tRadius, short tColor = Colors.ByLayer)
        {
            var obj = new Circle() { LinetypeScale = 1 };
            obj.SetDatabaseDefaults();
            obj.LinetypeId = db.LinetypeTableId;
            obj.Center = new Point3d(tPntX, tPntY, 0);
            obj.Radius = tRadius;
            obj.Color = Colors.GetColorFromIndex(tColor);
            return obj;
        }

        public static Entity Line(double tPntX, double tPntY, double tPntX2, double tPntY2, int tTypeLine = 0,
                                 short tColor = Colors.ByLayer, double tRotation = 0, Point2d? tPntRotate = null)
        {
            var obj = new Line(new Point3d(tPntX, tPntY, 0), new Point3d(tPntX2, tPntY2, 0))
            {
                Color = Colors.GetColorFromIndex(tColor),
                LinetypeScale = LinetypeScaleCurrent
            };
            obj.SetDatabaseDefaults();
            obj.LinetypeId = db.LinetypeTableId;

            if (tRotation > 0.0)
            {
                tPntRotate = tPntRotate ?? new Point2d(tPntX, tPntY);
                obj.XRotate(tRotation, new Point3d(((Point2d)tPntRotate).X, ((Point2d)tPntRotate).Y, 0));
            }
            return obj;
        }

        public static Entity Polyline(Extents3d tExtents3D, string layer = "0", int colorIndex = Colors.ByLayer,
                                      double width = 0.0, byte close = 1, Point3d? basePointTransform = null, double tRotation = 0.0, double scaleFactor = 0.0, Vector3d? displacement = null)
        {
            return Polyline(tExtents3D.MinPoint, tExtents3D.MaxPoint, layer, colorIndex, width, close,
                                            basePointTransform, tRotation, scaleFactor, displacement);
        }
        public static Entity Polyline(Point3d tPointStart, Point3d tPointEnd, string layer = "0", int colorIndex = Colors.ByLayer,
                                      double tWidth = 0.0, byte close = 1, Point3d? basePointTransform = null, double rotation = 0.0, double scaleFactor = 0.0, Vector3d? displacement = null)
        {
            return Polyline(new Point3dCollection { tPointStart, new Point3d(tPointEnd.X, tPointStart.Y, 0), tPointEnd, new Point3d(tPointStart.X, tPointEnd.Y, 0) },
                                            layer, colorIndex, tWidth, close, basePointTransform, rotation, scaleFactor, displacement);
        }

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
                LinetypeScale = LinetypeScaleCurrent
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

            if (scaleFactor > 0.0 && basePointTransform != null) obj.XScale(scaleFactor, (Point3d)basePointTransform);

            if (tDisplacement != null) obj.TransformBy(Matrix3d.Displacement((Vector3d)tDisplacement));

            if (rotation > 0.0 && basePointTransform != null) obj.XRotate(rotation, (Point3d)basePointTransform);

            return obj;
        }

        public static Entity Pline(Transaction tr, double[] x, double[] y, short tColor = Colors.ByLayer, double tWidth = 0.0,
                                      byte tClose = 0, double tPntXRotate = Double.MinValue, double tPntYRotate = Double.MinValue, double tRotation = 0.0)
        {
            var obj = new Polyline() { LinetypeScale = LinetypeScaleCurrent }; ;
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
            obj.XAddNew(tr);

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

        public static object Solid(double[] x, double[] y, short tColor = Colors.ByLayer)
        {
            var obj = new Solid(new Point3d(x[0], y[0], 0), new Point3d(x[1], y[1], 0),
                                new Point3d(x[2], y[2], 0), new Point3d(x[3], y[3], 0))
            {
                Color = Colors.GetColorFromIndex(tColor),
                LinetypeScale = LinetypeScaleCurrent
            };
            obj.SetDatabaseDefaults();
            obj.LinetypeId = db.LinetypeTableId;
            return obj;
        }

        public static Entity Point(double tPntX, double tPntY, short tColor = Colors.ByLayer,
                                   int tFmod = 0, int tPntMod = 34, double tPntSize = 5.0)
        {
            var currentObject = new DBPoint(new Point3d(tPntX, tPntY, 0));
            var obj = (DBPoint)currentObject;
            obj.SetDatabaseDefaults();

            if (tFmod > 0)
            {
                db.Pdmode = tPntMod;
                db.Pdsize = tPntSize;
            }
            obj.Color = Colors.GetColorFromIndex(tColor);

            return currentObject;
        }

        public static Entity Text(double tPntX, double tPntY, string tStr,
                                OptionsAlignment tFlgAlignment = OptionsAlignment.ByCenterHorizontalMode,
                                short tColor = Colors.ByLayer, double tRotation = 0.0,
                                double? tPntXRotate = null, double? tPntYRotate = null,
                                double tHeight = 8.0)
        {
            var obj = new DBText
            {
                Position = new Point3d(tPntX, tPntY, 0),
                Height = (tHeight < 8 ? 8 : tHeight),
                TextString = tStr
            };

            switch (tFlgAlignment)
            {
                case OptionsAlignment.ByCenterHorizontalMode:
                    obj.HorizontalMode = TextHorizontalMode.TextCenter;
                    obj.AlignmentPoint = new Point3d(tPntX, tPntY, 0);
                    tRotation = 0;
                    break;
                case OptionsAlignment.ByCenterVerticalMode:
                    obj.HorizontalMode = TextHorizontalMode.TextCenter;
                    obj.VerticalMode = TextVerticalMode.TextVerticalMid;
                    obj.AlignmentPoint = new Point3d(tPntX, tPntY, 0);
                    break;
                case OptionsAlignment.ByLeft:
                    obj.HorizontalMode = TextHorizontalMode.TextLeft;
                    break;
            }

            obj.Color = Colors.GetColorFromIndex(tColor);
            obj.SetDatabaseDefaults();
            if (tRotation > 0.0)
            {
                if (tPntXRotate == null)
                {
                    tPntXRotate = obj.Position.X;
                    tPntYRotate = obj.Position.Y;
                }
                if (tPntYRotate != null)
                    obj.XRotate(tRotation, new Point3d((double)tPntXRotate, (double)tPntYRotate, 0));
            }
            return obj;
        }

        public static Entity RecHat(double[] x, double[] y, short tColor = Colors.ByLayer)
        {
            var obj = new Hatch();
            try
            {
                obj.SetDatabaseDefaults();
                using (var tr = db.TransactionManager.StartTransaction())
                {
                    var bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    // Open the Block table for read
                    var btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                    var acObjIn = Pline(tr, x, y);
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
                Log.Add(ex);
            }
            return obj;
        }
    }

    //version 2.0
    public class DrawService : IDrawService
    {
        public bool IsVirtualMode;
        public ObjectId LinetypeIdCurrent;
        public double LinetypeScaleCurrent;
        //public Entity CurrentObject;
        private readonly IUnityContainer _unityContainer;
        private readonly ICommandLineService _commandLineService;
        private readonly IOperationService _operationService;

        public DrawService(IUnityContainer unityContainer, ICommandLineService commandLineService, IOperationService operationService)
        {
            _unityContainer = unityContainer;
            _commandLineService = commandLineService;
            _operationService = operationService;
        }
        /// <summary> Enable to create draw elements in virtual mode (i.e. not on drawing, only in memory) </summary>
        public class VirtualMode : IDisposable
        {
            /// <summary> Constructor </summary>
            public VirtualMode()
            {
                //IsVirtualMode = true;
            }

            public void Dispose()
            {
                //IsVirtualMode = false;
            }
        }

        #region "Drawing: 2d (Model space) for support old version"

        public Entity Circle(Transaction tr, double tPntX, double tPntY, double tRadius, short tColor = Colors.ByLayer)
        {
            var obj = new Circle() { LinetypeId = LinetypeIdCurrent, LinetypeScale = LinetypeScaleCurrent };
            obj.SetDatabaseDefaults();
            obj.Center = new Point3d(tPntX, tPntY, 0);
            obj.Radius = tRadius;
            obj.Color = Colors.GetColorFromIndex(tColor);
            obj.XAddNew(tr);
            return obj;
        }

        public Entity Donut(Transaction tr, double x, double y, double radius, short color = -1)
        {
            var obj = new Polyline { Color = Colors.GetColorFromIndex(color) };
            radius = radius / 2;
            var pt1 = new Point2d(x - (radius / 2), y);
            var pt2 = new Point2d(x + (radius / 2), y);
            obj.AddVertexAt(0, pt1, 1.0, radius, radius);
            obj.AddVertexAt(1, pt2, 1.0, radius, radius);
            obj.AddVertexAt(2, pt1, 0.0, radius, radius);
            obj.XAddNew(tr);
            return obj;
        }

        public Entity Line(Transaction tr, double tPntX, double tPntY, double tPntX2, double tPntY2, int tTypeLine = 0,
                                 short tColor = Colors.ByLayer, double tRotation = 0, Point2d? tPntRotate = null)
        {
            var obj = new Line(new Point3d(tPntX, tPntY, 0), new Point3d(tPntX2, tPntY2, 0))
                          {
                              Color = Colors.GetColorFromIndex(tColor),
                              LinetypeId = LinetypeIdCurrent,
                              LinetypeScale = LinetypeScaleCurrent
                          };
            obj.SetDatabaseDefaults();
            if (tRotation > 0.0)
            {
                tPntRotate = tPntRotate ?? new Point2d(tPntX, tPntY);
                obj.XRotate(tRotation, new Point3d(((Point2d)tPntRotate).X, ((Point2d)tPntRotate).Y, 0));
            }

            //ObjectIds.Add(obj.ObjectId);

            return obj;
        } // Line

        public Entity Polyline(Transaction tr, Extents3d tExtents3D, string layer = "0", int colorIndex = Colors.ByLayer,
                                      double width = 0.0, byte close = 1, Point3d? basePointTransform = null, double tRotation = 0.0, double scaleFactor = 0.0, Vector3d? displacement = null)
        {
            return Polyline(tr, tExtents3D.MinPoint, tExtents3D.MaxPoint, layer, colorIndex, width, close,
                                            basePointTransform, tRotation, scaleFactor, displacement);
        }
        public Entity Polyline(Transaction tr, Point3d tPointStart, Point3d tPointEnd, string layer = "0", int colorIndex = Colors.ByLayer,
                                      double tWidth = 0.0, byte close = 1, Point3d? basePointTransform = null, double rotation = 0.0, double scaleFactor = 0.0, Vector3d? displacement = null)
        {
            return Polyline(tr, new Point3dCollection { tPointStart, new Point3d(tPointEnd.X, tPointStart.Y, 0), tPointEnd, new Point3d(tPointStart.X, tPointEnd.Y, 0) },
                                            layer, colorIndex, tWidth, close, basePointTransform, rotation, scaleFactor, displacement);
        }

        public Entity Polyline(Transaction tr, Point3dCollection points, string layer = "0", int color = Colors.ByLayer,
                                      double width = 0.0, byte close = 1, Point3d? basePointTransform = null,
                                      double rotation = 0.0, double scaleFactor = 0.0,
                                      Vector3d? tDisplacement = null)
        {
            var obj = new Polyline
            {
                Layer = layer,
                ColorIndex = color,
                Closed = close > 0,
                LinetypeId = LinetypeIdCurrent,
                LinetypeScale = LinetypeScaleCurrent
            };
            obj.SetDatabaseDefaults();

            for (var i = 0; i <= points.Count - 1; i++)
            {
                obj.AddVertexAt(i, new Point2d(points[i].X, points[i].Y), 0, 0, 0);
                if (width <= 0.0001) continue;
                obj.SetStartWidthAt(i, width);
                obj.SetEndWidthAt(i, width);
            }

            //Mode that it not enable to draw on space model
            if (!IsVirtualMode) obj.XAddNew(tr);

            if (basePointTransform == null && (scaleFactor > 0.0 | tDisplacement != null | rotation > 0.0))
            {
                var p3D = new Polyline3d(Poly3dType.SimplePoly, points, true);
                if (p3D.Bounds.HasValue) basePointTransform = p3D.Bounds.Value.MinPoint;
            }

            if (scaleFactor > 0.0 && basePointTransform != null) obj.XScale(scaleFactor, (Point3d)basePointTransform);

            if (tDisplacement != null) obj.TransformBy(Matrix3d.Displacement((Vector3d)tDisplacement));

            if (rotation > 0.0 && basePointTransform != null) obj.XRotate(rotation, (Point3d)basePointTransform);

            return obj;
        }

        public Entity Pline(Transaction tr, double[] x, double[] y, short tColor = Colors.ByLayer, double tWidth = 0.0,
                                      byte tClose = 0, double tPntXRotate = Double.MinValue, double tPntYRotate = Double.MinValue, double tRotation = 0.0)
        {
            var obj = new Polyline() { LinetypeId = LinetypeIdCurrent, LinetypeScale = LinetypeScaleCurrent }; ;
            obj.SetDatabaseDefaults();
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
            obj.XAddNew(tr);

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

        public object Solid(Transaction tr, double[] x, double[] y, short tColor = Colors.ByLayer)
        {
            var obj = new Solid(new Point3d(x[0], y[0], 0), new Point3d(x[1], y[1], 0),
                                new Point3d(x[2], y[2], 0), new Point3d(x[3], y[3], 0))
                                {
                                    Color = Colors.GetColorFromIndex(tColor),
                                    LinetypeId = LinetypeIdCurrent,
                                    LinetypeScale = LinetypeScaleCurrent
                                };
            obj.SetDatabaseDefaults();
            obj.XAddNew(tr);
            //ObjectIds.Add(obj.ObjectId);
            return obj;
        } // Solid

        public Entity Point(Transaction tr, double tPntX, double tPntY, short tColor = Colors.ByLayer,
                                   int tFmod = 0, int tPntMod = 34, double tPntSize = 5.0)
        {
            var currentObject = new DBPoint(new Point3d(tPntX, tPntY, 0));

            var obj = (DBPoint)currentObject;
            obj.SetDatabaseDefaults();
            if (tFmod > 0)
            {
                _operationService.Db.Pdmode = tPntMod;
                _operationService.Db.Pdsize = tPntSize;
            }
            obj.Color = Colors.GetColorFromIndex(tColor);
            obj.XAddNew(tr);
            //ObjectIds.Add(obj.ObjectId);

            return currentObject;
        } // DBPoint

        public Entity Text(Transaction tr, double tPntX, double tPntY, string tStr, OptionsAlignment tFlgAlignment = OptionsAlignment.ByCenterHorizontalMode,
                                 short tColor = Colors.ByLayer, double tRotation = 0.0,
                                 double? tPntXRotate = null, double? tPntYRotate = null,
                                 double tHeight = 8.0)
        {
            var obj = new DBText
                          {
                              Position = new Point3d(tPntX, tPntY, 0),
                              Height = (tHeight < 8 ? 8 : tHeight),
                              TextString = tStr
                          };

            switch (tFlgAlignment)
            {
                case OptionsAlignment.ByCenterHorizontalMode:
                    obj.HorizontalMode = TextHorizontalMode.TextCenter;
                    obj.AlignmentPoint = new Point3d(tPntX, tPntY, 0);
                    tRotation = 0;
                    break;
                case OptionsAlignment.ByCenterVerticalMode:
                    obj.HorizontalMode = TextHorizontalMode.TextCenter;
                    obj.VerticalMode = TextVerticalMode.TextVerticalMid;
                    obj.AlignmentPoint = new Point3d(tPntX, tPntY, 0);
                    break;
                case OptionsAlignment.ByLeft:
                    obj.HorizontalMode = TextHorizontalMode.TextLeft;
                    break;
            }

            obj.Color = Colors.GetColorFromIndex(tColor);
            obj.SetDatabaseDefaults();
            if (tRotation > 0.0)
            {
                if (tPntXRotate == null)
                {
                    tPntXRotate = obj.Position.X;
                    tPntYRotate = obj.Position.Y;
                }
                if (tPntYRotate != null)
                    obj.XRotate(tRotation, new Point3d((double)tPntXRotate, (double)tPntYRotate, 0));
            }
            obj.XAddNew(tr);
            //ObjectIds.Add(obj.ObjectId);

            return obj;
        } // DBText

        public Entity RecHat(Transaction tr, double[] x, double[] y, short tColor = Colors.ByLayer)
        {
            var obj = new Hatch();
            try
            {
                var bt = tr.GetObject(_operationService.Db.BlockTableId, OpenMode.ForRead) as BlockTable;
                // Open the Block table for read
                var btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                var acObjIn = Pline(tr, x, y);
                var acObjIds = new ObjectIdCollection { ((Polyline)acObjIn).ObjectId };

                obj.SetDatabaseDefaults();
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
            catch (Exception ex)
            {
                Log.Add(ex);
            }

            return obj;
        } // Hatch

        #endregion

        #region "Drawing: 3d (Model space)"

        public Entity Solid3D(Transaction tr, object contourColl, double height, int colorIndex)
        {
            var obj = new Solid3d() { LinetypeId = LinetypeIdCurrent, LinetypeScale = LinetypeScaleCurrent };
            try
            {
                var objRegion = CreateRegions(contourColl);
                obj.Extrude(objRegion, height, 0);
                obj.ColorIndex = colorIndex;
                obj.XAddNew(tr);
                //ObjectIds.Add(obj.ObjectId);
                _commandLineService.ViewIsometric(7);
            }
            catch (Exception ex)
            {
                Log.Add(ex);
            }
            return obj;
        }

        public Entity Solid3D(Transaction tr, double[] x, double[] y, double[] z, double height, int colorIndex)
        {
            var resultObjectColl = new List<Entity>();
            var point3DColl = new Point3dCollection { new Point3d(x[0], y[0], z[0]) };
            for (var i = 1; i <= x.Length - 1; i++)
            {
                point3DColl.Add(new Point3d(x[i], y[i], z[i]));
                if (!point3DColl[point3DColl.Count - 1].Equals(point3DColl[0])) continue;
                resultObjectColl.Add(new Polyline3d(Poly3dType.SimplePoly, point3DColl, true));
                point3DColl.Clear();
                if (i == x.Length - 1) continue;
                i = i + 1;
                point3DColl.Add(new Point3d(x[i], y[i], z[i]));
            }
            return Solid3D(tr, resultObjectColl, height, colorIndex);
        }

        public Entity Solid3D(Transaction tr, IEnumerable<Entity> entities, BooleanOperationType booleanOperationType, string layer = "0", int colorIndex = Colors.ByLayer)
        {
            var dbObjects = new DBObjectCollection();
            var solid3D = new Solid3d() { LinetypeId = LinetypeIdCurrent, LinetypeScale = LinetypeScaleCurrent };

            entities.ToList().ForEach(x => { dbObjects.Add(x); x.XRemove(); });

            dbObjects = Region.CreateFromCurves(dbObjects);
            if (dbObjects.Count > 1)
            {
                for (int i = 1; i <= dbObjects.Count - 1; i++)
                {
                    ((Region)dbObjects[0]).BooleanOperation(booleanOperationType, (Region)dbObjects[i]);
                    dbObjects[i].Dispose();
                }
            }
            var acRegion = (Region)dbObjects[0];
            solid3D.Extrude(acRegion, 3, 0);
            solid3D.ColorIndex = colorIndex;
            solid3D.Layer = layer;
            solid3D.XAddNew(tr);
            return solid3D;
        }

        private Region CreateRegions(object tObjContour, bool tSort = true)
        {
            //Region acRegion = default(Region);
            var dbObjects = new DBObjectCollection();
            var resultObjectColl = new List<Entity>();

            if (tObjContour is Polyline | tObjContour is Polyline3d | tObjContour is Circle)
            {
                resultObjectColl.Add((Entity)tObjContour);
            }
            else if (tObjContour is ICollection)
            {
                foreach (Entity ent in (ICollection)tObjContour)
                {
                    if (ent.GetType().Name == "Polyline3d" | ent.GetType().Name == "Polyline" | ent.GetType().Name == "Circle")
                    {
                        resultObjectColl.Add(ent);
                    }
                }
                if (resultObjectColl.Count > 0 & tSort)
                {
                    resultObjectColl.XSortByArea();
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
            foreach (var p in resultObjectColl)
            {
                dbObjects.Add(p);
            }

            dbObjects = Region.CreateFromCurves(dbObjects);
            if (dbObjects.Count > 1)
            {
                for (int i = 1; i <= dbObjects.Count - 1; i++)
                {
                    ((Region)dbObjects[0]).BooleanOperation(BooleanOperationType.BoolSubtract, (Region)dbObjects[i]);
                    dbObjects[i].Dispose();
                }
            }
            var acRegion = (Region)dbObjects[0];
            return acRegion;
        }

        #endregion
    }
}
