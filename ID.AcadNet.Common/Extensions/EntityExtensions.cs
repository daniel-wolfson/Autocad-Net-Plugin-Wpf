using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using ID.Infrastructure;
using Intellidesk.AcadNet.Common.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Db = Autodesk.AutoCAD.DatabaseServices;
using Ed = Autodesk.AutoCAD.EditorInput;
using Exception = System.Exception;

namespace Intellidesk.AcadNet.Common.Extensions
{
    /// <summary> Entities extensions class </summary>
    public static class EntityExtensions
    {
        public static Document Doc => acadApp.DocumentManager.MdiActiveDocument;
        public static Editor Ed => Doc.Editor;
        public static Database Db => HostApplicationServices.WorkingDatabase;


        /// <summary> XIsMacthFor </summary>
        public static bool XIsMacthFor(this Entity ent, params Type[] types)
        {
            return types.Contains(ent.GetType());  //if (ent.GetRXClass() == RXObject.GetClass(typeof(Line)))
        }

        public static Entity XSaveChanges<T>(this T ent, Action<T> action = null) where T : Entity
        {
            using (var tr = Db.TransactionManager.StartTransaction())
            {
                ent.SetDatabaseDefaults();

                if (action != null)
                {
                    action(ent);
                }

                //var bt = (BlockTable)scope.tr.GetObject(Db.BlockTableId, OpenMode.ForRead); //bt[BlockTableRecord.ModelSpace]
                var btr = (BlockTableRecord)tr.GetObject(Db.CurrentSpaceId, OpenMode.ForWrite);
                btr.AppendEntity(ent);
                tr.AddNewlyCreatedDBObject(ent, true);
                tr.Commit();
                return ent;
            }
        }

        public static Entity XSaveChanges(this Entity ent, Action<Entity> action = null)
        {
            return ent.XSaveChanges<Entity>(action);
        }

        /// <summary> Add New entity </summary>
        public static Entity XSaveChangesCommit(this Entity tEnt)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Document doc = acadApp.DocumentManager.GetDocument(db);
            try
            {
                using (doc.LockDocument())
                {
                    Transaction tr = db.TransactionManager.StartTransaction();
                    tEnt.SetDatabaseDefaults();

                    // Open the Block table for read
                    var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);

                    // Open the Block table record Model space for write
                    var btr = (BlockTableRecord)tr.GetObject(bt[acadApp.DocumentManager.GetCurrentSpace()], OpenMode.ForWrite);

                    // Add the new object to the block table record and the transaction
                    btr?.AppendEntity(tEnt);

                    // Save the new object to the database
                    tr.AddNewlyCreatedDBObject(tEnt, true);

                    tr.Commit();
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.Error($"{nameof(EntityExtensions)}.{nameof(XSaveChangesCommit)} Progress started", ex);
            }
            return tEnt;
        }

        /// <summary> Hihglight entity </summary>
        public static void XHihglight(this Entity tEnt, bool tValue)
        {
            if (tValue) tEnt.Highlight();
            else tEnt.Unhighlight();
        }

        /// <summary> Get Handle entity </summary>
        public static string XGetHandle(this Entity tEnt)
        {
            return tEnt.Handle.ToString();
        }

        /// <summary> Set Color </summary>
        public static void XSetColor(this Entity tEnt, short tValue)
        {
            using (Doc.LockDocument())
            {
                tEnt.UpgradeOpen();
                tEnt.Color = Colors.GetColorFromIndex(tValue);
            }
        }

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

        public static Entity XRotate(this Entity ent, double radianValue, Point3d basePoint)
        {
            //' Rotate 45 degrees = 0.7854, around the Z-axis of the current UCS using a base point of (tPntX1,tPntY1,0)
            ent.TransformBy(Matrix3d.Rotation(radianValue, Ed.CurrentUserCoordinateSystem.CoordinateSystem3d.Zaxis, basePoint));
            return ent;
        }

        public static Entity XRotateByPointXYZ(this Entity ent, double radianValue, params double[] arrayPointsXyz)
        {
            ent.XRotate(radianValue, (arrayPointsXyz.Length < 3 ? new Point3d(0, 0, 0) : new Point3d(arrayPointsXyz[0], arrayPointsXyz[1], arrayPointsXyz[2])));
            return ent;
        }

        public static Entity XScale(this Entity ent, double scaleFactor, Point3d basePoint)
        {
            ent.TransformBy(Matrix3d.Scaling(scaleFactor, basePoint));
            return ent;
        }

        public static Entity XMove(this Entity ent, Point3d basePoint)
        {
            ent.TransformBy(Matrix3d.Displacement(basePoint.GetAsVector()));
            return ent;
        }

        public static void SetView(Entity ent)
        {
            Document doc = acadApp.DocumentManager.MdiActiveDocument;
            // База данных чертежа (в данном случае -
            // активного документа)

            Db.Database db = doc.Database;

            // Редактор базы данных чертежа
            Ed.Editor ed = doc.Editor;

            // Если мы не в пространстве модели,
            // то переходим в него, дабы
            // затем показать наш смайлик
            if (!db.TileMode)
                acadApp.SetSystemVariable("TILEMODE", 1);

            // назначаем вид таким образом, чтобы
            // наш смайлик вписался в экран
            Db.ViewTableRecord view = new Db.ViewTableRecord();

            view.Width = ent.GeometricExtents.MaxPoint.X - ent.GeometricExtents.MinPoint.X;
            view.Height = ent.GeometricExtents.MaxPoint.Y - ent.GeometricExtents.MinPoint.Y;
            //view.CenterPoint = new Gm.Point2d(head.Center.X, head.Center.Y);
            ed.SetCurrentView(view);
        }

        public static double Length(this Curve cv)
        {
            return cv.GetDistanceAtParameter(cv.EndParam) - cv.GetDistanceAtParameter(cv.StartParam);
        }

        /// <summary> Get points extents </summary>
        public static Extents3d XGetPointsExtents(this Polyline polyline)
        {
            return polyline.Bounds.HasValue
                ? new Extents3d(polyline.Bounds.Value.MinPoint, polyline.Bounds.Value.MaxPoint)
                : new Extents3d(polyline.GeometricExtents.MinPoint, polyline.GeometricExtents.MaxPoint);
        }

        public static Point3d XGetMidPoint(this Entity entity)
        {
            return
                //polyline.Bounds.HasValue 
                //? new LineSegment3d(polyline.Bounds.Value.MinPoint, polyline.Bounds.Value.MaxPoint).MidPoint 
                //: 
                new LineSegment3d(entity.GeometricExtents.MinPoint, entity.GeometricExtents.MaxPoint).MidPoint;
        }
    }
}