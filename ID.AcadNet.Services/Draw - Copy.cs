using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using ID.Infrastructure;
using ID.Infrastructure.Enums;
using ID.Infrastructure.Extensions;
using Intellidesk.AcadNet.Common.Core;
using Intellidesk.AcadNet.Common.Enums;
using Intellidesk.AcadNet.Common.Extensions;
using Intellidesk.AcadNet.Common.Interfaces;
using Intellidesk.AcadNet.Common.Jig;
using Intellidesk.AcadNet.Common.Model;
using Intellidesk.Data.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using AcadTextHorizontalMode = Autodesk.AutoCAD.DatabaseServices.TextHorizontalMode;
using Color = Autodesk.AutoCAD.Colors.Color;
using Exception = Autodesk.AutoCAD.Runtime.Exception;
using Polyline = Autodesk.AutoCAD.DatabaseServices.Polyline;

namespace Intellidesk.AcadNet.Services
{
    //public delegate void AcadEntityEventHandler(object sender, EventArgs e);

    /// <summary> DrawService (version 3.0) </summary>
    public class Draw : BaseService, IDraw
    {
        private static Draw _instance { get; set; }

        public string CurrentLayerName { get; set; } = "0";

        public static double LinetypeScaleCurrent;
        public static bool isBisy = false;

        private Draw()
        {
        }

        static Draw()
        {
            acadApp.SetSystemVariable("SNAPMODE", 1);
        }

        public static IDraw GetInstance()
        {
            return _instance ?? (_instance = new Draw());
        }

        public Entity Rectangle(double width, double height)
        {
            Point3d p1 = Point3d.Origin;
            Point3d p2 = Point3d.Origin + new Vector3d(0, 0, 0);

            //if (p1.X == p2.X || p1.Y == p2.Y)
            //{
            //    Doc.Editor.WriteMessage("\nInvalid coordinate specification");
            //    return;
            //}

            Doc.Editor.DrawVector(p1, p2, 1, true);

            double len = height; //p1.DistanceTo(p2);
            double wid = width;

            Doc.Editor.WriteMessage("\n\tLength:\t{0:f3}\tWidth:{1:f3}\n", len, wid);

            Plane plan = new Plane(Point3d.Origin, Vector3d.ZAxis);
            double ang = p1.GetVectorTo(p2).AngleOnPlane(plan);
            Point3dCollection pts = new Point3dCollection();

            Point3d c1 = p1.XGetPolarPoint(ang - Math.PI / 2, wid);
            Point3d c4 = p1.XGetPolarPoint(ang + Math.PI / 2, wid);
            Point3d c2 = c1.XGetPolarPoint(ang, len);
            Point3d c3 = c4.XGetPolarPoint(ang, len);

            pts.Add(c1);
            pts.Add(c2);
            pts.Add(c3);
            pts.Add(c4);

            Polyline poly = new Polyline();
            int idx = 0;
            foreach (Point3d p in pts)
            {
                poly.AddVertexAt(idx, p.XGetPoint2d(), 0, 0, 0);
                idx += 1;
            }
            poly.Closed = true;
            return poly;
        }

        public Entity Rectangle(Point3dCollection corners)
        {
            Polyline ent = new Polyline();
            ent.SetDatabaseDefaults();
            for (int i = 0; i < corners.Count; i++)
            {
                Point3d pt3d = corners[i];
                Point2d pt2d = new Point2d(pt3d.X, pt3d.Y);
                ent.AddVertexAt(i, pt2d, 0, Db.Plinewid, Db.Plinewid);
            };
            ent.Closed = true;
            ent.TransformBy(UCS);
            return ent;
        }

        public Circle Circle(double tPntX, double tPntY, double tRadius, short tColor = Colors.ByLayer)
        {
            var obj = new Circle() { LinetypeScale = 1 };
            obj.SetDatabaseDefaults();
            obj.LinetypeId = Db.LinetypeTableId;
            obj.Center = new Point3d(tPntX, tPntY, 0);
            obj.Radius = tRadius;
            obj.Color = Colors.GetColorFromIndex(tColor);
            return obj;
        }

        /// <summary> Line </summary>
        public static Entity Line(double tPntX, double tPntY, double tPntX2, double tPntY2, int tTypeLine = 0,
            short tColor = Colors.ByLayer, double tRotation = 0, Point2d? tPntRotate = null)
        {
            var obj = new Line(new Point3d(tPntX, tPntY, 0), new Point3d(tPntX2, tPntY2, 0))
            {
                Color = Colors.GetColorFromIndex(tColor),
                LinetypeScale = LinetypeScaleCurrent
            };
            obj.SetDatabaseDefaults();
            obj.LinetypeId = Db.LinetypeTableId;

            if (tRotation > 0.0)
            {
                tPntRotate = tPntRotate ?? new Point2d(tPntX, tPntY);
                obj.XRotate(tRotation, new Point3d(((Point2d)tPntRotate).X, ((Point2d)tPntRotate).Y, 0));
            }
            return obj;
        }

        /// <summary> Solid </summary>
        public object Solid(double[] x, double[] y, short tColor = Colors.ByLayer)
        {
            var obj = new Solid(new Point3d(x[0], y[0], 0), new Point3d(x[1], y[1], 0),
                new Point3d(x[2], y[2], 0), new Point3d(x[3], y[3], 0))
            {
                Color = Colors.GetColorFromIndex(tColor),
                LinetypeScale = LinetypeScaleCurrent
            };
            obj.SetDatabaseDefaults();
            obj.LinetypeId = Db.LinetypeTableId;
            return obj;
        }

        /// <summary> Point </summary>
        public Entity Point(double tPntX, double tPntY, short tColor = Colors.ByLayer,
            int tFmod = 0, int tPntMod = 34, double tPntSize = 5.0)
        {
            var currentObject = new DBPoint(new Point3d(tPntX, tPntY, 0));
            var obj = (DBPoint)currentObject;
            obj.SetDatabaseDefaults();

            if (tFmod > 0)
            {
                Db.Pdmode = tPntMod;
                Db.Pdsize = tPntSize;
            }
            obj.Color = Colors.GetColorFromIndex(tColor);

            return currentObject;
        }


        public DBText Text(AcadNet.Common.Model.DbTextArgs args)

        {
            DBText ent = new DBText
            {
                Position = new Point3d(args.Position.X, args.Position.Y, 0),
                Height = args.Height < 0.8 ? 0.8 : args.Height,
                TextString = args.TextString,
                Layer = CurrentLayerName
            };

            switch (args.Alignment)
            {
                case AlignmentOptions.ByCenterHorizontalMode:
                    ent.HorizontalMode = AcadTextHorizontalMode.TextCenter;
                    ent.AlignmentPoint = new Point3d(args.Position.X, args.Position.Y, 0);
                    args.Rotation = 0;
                    break;
                case AlignmentOptions.ByCenterVerticalMode:
                    ent.HorizontalMode = AcadTextHorizontalMode.TextCenter;
                    ent.VerticalMode = TextVerticalMode.TextVerticalMid;
                    ent.AlignmentPoint = new Point3d(args.Position.X, args.Position.Y, 0);
                    break;
                case AlignmentOptions.ByLeft:
                    ent.HorizontalMode = AcadTextHorizontalMode.TextLeft;
                    break;
            }

            ent.Color = Color.FromColorIndex(ColorMethod.ByAci, (short)args.ColorIndex);
            ent.SetDatabaseDefaults();

            if (args.Rotation > 0.0 && args.otherPointRotate == null)
                ent.XRotate(args.Rotation, new Point3d(ent.Position.X, ent.Position.Y, 0));

            return ent;
        }

        /// <summary> RecHat </summary>
        public Entity RecHat(double[] x, double[] y, short tColor = Colors.ByLayer)
        {
            var obj = new Hatch();
            try
            {
                obj.SetDatabaseDefaults();
                using (var tr = Db.TransactionManager.StartTransaction())
                {
                    var bt = tr.GetObject(Db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    // Open the Block table for read
                    var btr = tr.GetObject(bt[acadApp.DocumentManager.GetCurrentSpace()], OpenMode.ForWrite) as BlockTableRecord;
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
                Plugin.Logger.Error($"{nameof(InsertJig)}.{nameof(RecHat)} error: ", ex);
            }
            return obj;
        }

        /// <summary> AttachRasterImage </summary>
        public void AttachRasterImage1(Database db, Point3d pnt)
        {
            using (Transaction acTrans = db.TransactionManager.StartTransaction())
            {
                // Define the name and image to use
                string strImgName = "GisPoint";
                string strFileName = @"C:\Projects\ID\ID.AcadNet\Resources\gis-point.png";

                RasterImageDef acRasterDef;
                bool bRasterDefCreated = false;
                ObjectId acImgDefId;

                // Get the image dictionary
                ObjectId acImgDctID = RasterImageDef.GetImageDictionary(db);

                // Check to see if the dictionary does not exist, it not then create it
                if (acImgDctID.IsNull)
                {
                    acImgDctID = RasterImageDef.CreateImageDictionary(db);
                }

                // Open the image dictionary
                DBDictionary acImgDict = acTrans.GetObject(acImgDctID, OpenMode.ForRead) as DBDictionary;

                // Check to see if the image definition already exists
                if (acImgDict.Contains(strImgName))
                {
                    acImgDefId = acImgDict.GetAt(strImgName);
                    acRasterDef = acTrans.GetObject(acImgDefId, OpenMode.ForWrite) as RasterImageDef;
                }
                else
                {
                    // Create a raster image definition
                    RasterImageDef acRasterDefNew = new RasterImageDef();

                    // Set the source for the image file
                    acRasterDefNew.SourceFileName = strFileName;

                    // Load the image into memory
                    acRasterDefNew.Load();

                    // Add the image definition to the dictionary
                    acImgDict.UpgradeOpen();
                    acImgDefId = acImgDict.SetAt(strImgName, acRasterDefNew);

                    acTrans.AddNewlyCreatedDBObject(acRasterDefNew, true);

                    acRasterDef = acRasterDefNew;

                    bRasterDefCreated = true;
                }

                // Open the Block table for read
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[acadApp.DocumentManager.GetCurrentSpace()],
                    OpenMode.ForWrite) as BlockTableRecord;

                // Create the new image and assign it the image definition
                using (RasterImage acRaster = new RasterImage())
                {
                    acRaster.ImageDefId = acImgDefId;

                    // Use ImageWidth and ImageHeight to get the size of the image in pixels (1024 X 768).
                    // Use ResolutionMMPerPixel to determine the number of millimeters in a pixel so you 
                    // can convert the size of the drawing into other units or millimeters based on the 
                    // drawing units used in the current drawing.

                    // Define the width and height of the image
                    Vector3d width;
                    Vector3d height;

                    // Check to see if the measurement is set to English (Imperial) or Metric units
                    if (db.Measurement == MeasurementValue.English)
                    {
                        width = new Vector3d((acRasterDef.ResolutionMMPerPixel.X * acRaster.ImageWidth) / 25.4, 0, 0);
                        height = new Vector3d(0, (acRasterDef.ResolutionMMPerPixel.Y * acRaster.ImageHeight) / 25.4, 0);
                    }
                    else
                    {
                        width = new Vector3d(acRasterDef.ResolutionMMPerPixel.X * acRaster.ImageWidth, 0, 0);
                        height = new Vector3d(0, acRasterDef.ResolutionMMPerPixel.Y * acRaster.ImageHeight, 0);
                    }

                    // Define the position for the image 
                    Point3d insPt = pnt; //new Point3d(5.0, 5.0, 0.0);

                    // Define and assign a coordinate system for the image's orientation
                    CoordinateSystem3d coordinateSystem = new CoordinateSystem3d(insPt, width * 2, height * 2);
                    acRaster.Orientation = coordinateSystem;

                    // Set the rotation angle for the image
                    acRaster.Rotation = 0;

                    // Add the new object to the block table record and the transaction
                    acBlkTblRec.AppendEntity(acRaster);
                    acTrans.AddNewlyCreatedDBObject(acRaster, true);

                    // Connect the raster definition and image together so the definition
                    // does not appear as "unreferenced" in the External References palette.
                    RasterImage.EnableReactors(true);
                    acRaster.AssociateRasterDef(acRasterDef);

                    if (bRasterDefCreated)
                    {
                        acRasterDef.Dispose();
                    }
                }

                // Save the new object to the database
                acTrans.Commit();

                // Dispose of the transaction
            }
        }

        /// <summary> AttachRasterImage </summary>
        public void AttachRasterImage(Database db, Point3d pnt)
        {
            RasterImageDef imageDef;
            ObjectId imageDefId;
            string dictName = "MY_IMAGE_NAME";

            using (var tr = db.TransactionManager.StartTransaction())
            {
                ObjectId imageDictId = RasterImageDef.GetImageDictionary(db);
                if (imageDictId == ObjectId.Null)
                    imageDictId = RasterImageDef.CreateImageDictionary(db);

                DBDictionary imageDict = (DBDictionary)tr.GetObject(imageDictId, OpenMode.ForRead);

                if (imageDict.Contains(dictName))
                {
                    imageDefId = imageDict.GetAt(dictName);
                    imageDef = (RasterImageDef)tr.GetObject(imageDefId, OpenMode.ForWrite);
                }
                else
                {
                    imageDef = new RasterImageDef();
                    imageDef.SourceFileName = @"C:\Projects\ID\ID.AcadNet\Resources\gis-point.png";
                    imageDef.Load();
                    imageDict.UpgradeOpen();
                    imageDefId = imageDict.SetAt(dictName, imageDef);
                    tr.AddNewlyCreatedDBObject(imageDef, true);
                }

                RasterImage image = new RasterImage();
                image.ImageDefId = imageDefId;
                Vector3d uCorner = new Vector3d(1.5, 0, 0);
                Vector3d vOnPlane = new Vector3d(0, 1, 0);
                CoordinateSystem3d coordinateSystem = new CoordinateSystem3d(Point3d.Origin, uCorner, vOnPlane);
                image.Orientation = coordinateSystem;
                image.ImageTransparency = true;
                image.ShowImage = true;

                BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                // Open the Block table for read
                var msBtr = (BlockTableRecord)tr.GetObject(bt[acadApp.DocumentManager.GetCurrentSpace()], OpenMode.ForWrite);

                //BlockTable bt = (BlockTable)Tr.GetObject(Db.BlockTableId, OpenMode.ForRead);
                //BlockTableRecord msBtr = Tr.GetObject(bt(BlockTableRecord.ModelSpace), OpenMode.ForWrite);
                msBtr.AppendEntity(image);
                tr.AddNewlyCreatedDBObject(image, true);

                RasterImage.EnableReactors(true);
                image.AssociateRasterDef(imageDef);
            }
        }

        /// <summary> Set curent layer </summary>
        public string SetLayer(string layerName)
        {
            try
            {
                ILayerService layerService = Plugin.GetService<ILayerService>();
                if (!layerService.Contains(layerName))
                    layerService.Add(layerName);
            }
            catch (System.Exception)
            {
                return "0";
            }
            return layerName;
        }
    }
}


//using (Doc.LockDocument())
//using (Transaction Tr = Db.TransactionManager.StartTransaction())
//{
//    DBObject dbObj = Tr.GetObject(ent.ObjectId, OpenMode.ForRead);
//    dbObj.SetXrecord(ent.Handle.ToString(), 
//        new TypedValue((int)DxfCode.Text, "Cable"),
//        new TypedValue((int)DxfCode.Handle, ent.Handle),
//        new TypedValue((int)DxfCode.LayerName, "0"),
//        new TypedValue((int)DxfCode.Color, 255),
//        new TypedValue((int)DxfCode.ExtendedDataAsciiString, JsonConvert.SerializeObject(cable))
//        //new TypedValue((int)LispDataType.Point2d, ent.XGetBasePoint().XtoPoint2D())
//        );
//    Tr.Commit();
//}