using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using ID.Infrastructure;
using Intellidesk.AcadNet.Common.Extensions;
using Intellidesk.AcadNet.Common.Interfaces;
using System;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Intellidesk.AcadNet.Services
{
    public delegate void AcadEntityEventHandler(object sender, EventArgs e);

    /// <summary> DrawService (version 3.0) </summary>
    public class DrawService : BaseService, IDrawService
    {
        private static DrawService _instance { get; set; }
        public string CurrentLayerName { get; set; } = "0";
        public static double LinetypeScaleCurrent;
        public static bool isBisy = false;

        private DrawService()
        {
        }

        static DrawService()
        {
            acadApp.SetSystemVariable("SNAPMODE", 1);
        }

        public static IDrawService GetInstance()
        {
            return _instance ?? (_instance = new DrawService());
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