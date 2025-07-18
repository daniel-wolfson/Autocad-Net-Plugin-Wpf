using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Intellidesk.AcadNet.Common.Internal;
using Intellidesk.AcadNet.Common.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Intellidesk.AcadNet.Common.Extensions
{
    public static class ObjectIdExtensions
    {
        #region <Get>
        public static bool IsGroup(this ObjectId id)
        {
            return id.ObjectClass.DxfName.ToLower().Contains("group");
        }

        /// <summary> cast to layer </summary>
        public static LayerTableRecord XAsLayer(this ObjectId layetId)
        {
            var db = acadApp.DocumentManager.MdiActiveDocument.Database;
            using (var tr = db.TransactionManager.StartTransaction())
            {
                return tr.GetObject(layetId, OpenMode.ForRead) as LayerTableRecord;
            }
        }

        public static bool XValidate(this LayerTableRecord ltr, string layerPattern = null)
        {
            return !ltr.IsOff && !ltr.IsFrozen && !ltr.IsLocked && !ltr.IsHidden &&
                (string.IsNullOrEmpty(layerPattern) || layerPattern == "*" ||
                 layerPattern.IsSearchPatternValid(() => ltr.Name.Contains(layerPattern)));
        }

        public static LayerTableRecord XGetLayer(this ObjectId entId)
        {
            var doc = acadApp.DocumentManager.MdiActiveDocument;
            var db = doc.Database;

            using (var tr = db.TransactionManager.StartTransaction())
            {
                var layerName = ((Entity)entId.GetObject(OpenMode.ForRead, false, true)).Layer;
                var lt = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
                if (lt.Has(layerName))
                {
                    return (LayerTableRecord)tr.GetObject(lt[layerName], OpenMode.ForRead);
                }
            }
            return null;
        }

        public static string XGetLayerName(this ObjectId entId)
        {
            string result;
            using (var tr = acadApp.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction())
            {
                result = ((Entity)entId.GetObject(OpenMode.ForRead, false, true)).Layer;
            }
            return result; //((Entity)id.Open(OpenMode.ForRead)).Layer;
        }

        public static string XGetLayerByIdDynamic(this ObjectId id)
        {
            var result = "";
            dynamic layers = HostApplicationServices.WorkingDatabase.LayerTableId;
            foreach (dynamic l in layers)
                if (l.ObjectId == id)
                    result = l.Name;
            return result;
        }

        public static Extents3d GetExtents(this ObjectId objId)
        {
            return Geoms.GetExtents(new[] { objId });
        }

        public static T XCast<T>(this ObjectId id) where T : Entity
        {
            if (id == ObjectId.Null || (id != ObjectId.Null && id.Database == null))
                return null;

            var db = acadApp.DocumentManager.MdiActiveDocument.Database;
            using (var tr = db.TransactionManager.StartTransaction())
            {
                return tr.GetObject(id, OpenMode.ForRead) as T;
            }
        }

        public static Entity XGetEntity(this ObjectId id)
        {
            if (id == ObjectId.Null) return null;

            var db = id.Database ?? HostApplicationServices.WorkingDatabase;
            using (var tr = db.TransactionManager.StartTransaction())
            {
                return tr.GetObject(id, OpenMode.ForRead) as Entity;
            }
        }

        public static Task<Entity> XGetEntityAsync(this ObjectId id)
        {
            if (id == ObjectId.Null) return null;

            var db = id.Database ?? HostApplicationServices.WorkingDatabase;
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var ent = tr.GetObject(id, OpenMode.ForRead) as Entity;
                return Task.FromResult(ent);
            }
        }

        // Opens a DBObject in ForRead mode (kaefer @ TheSwamp)
        public static T GetObject<T>(this ObjectId id) where T : DBObject
        {
            return id.GetObject<T>(OpenMode.ForRead);
        }

        // Opens a DBObject
        public static T GetObject<T>(this ObjectId id, OpenMode mode) where T : DBObject
        {
            return id.GetObject(mode) as T;
        }

        /// <summary>  Creates a temporary UCS which is aligned to the specified entity. </summary>
        private static void CreateUCSAlignedToEntity(this ObjectId id, Editor editor)
        {
            var db = id.Database ?? HostApplicationServices.WorkingDatabase;

            // We're going to make an inquiry into the drawing DB so start a transaction
            var tr = db.TransactionManager.StartTransaction();
            try
            {
                // Change the Current UCS to that of the 'surface' we want to draw the panels on.
                Entity entity = (Entity)tr.GetObject(id, OpenMode.ForRead);

                // Gather the necessary data that defines the entity's unique coordinate system
                Point3d entityOrigin = entity.GeometricExtents.MinPoint;
                Point3d entityUR = entity.GeometricExtents.MaxPoint;
                Vector3d entityXaxis = entity.Ecs.CoordinateSystem3d.Xaxis;

                Point3d entityUL = new Point3d(entityOrigin.X, entityUR.Y, entityUR.Z);  // Derive the upper-left corner from the coords of the origin & upper-right corner
                Vector3d entityYaxis = new Vector3d(0, entityUL.Y - entityOrigin.Y, entityUL.Z - entityOrigin.Z);  // Calculate the 3D Y-axis of the entity

                // Get the Active Viewport
                ViewportTableRecord viewportTableRecord = (ViewportTableRecord)tr.GetObject(editor.ActiveViewportId, OpenMode.ForWrite);
                viewportTableRecord.IconAtOrigin = true;
                viewportTableRecord.IconEnabled = true;

                // Set the CurrentUCS to that of the Entity's UCS
                editor.CurrentUserCoordinateSystem = Matrix3d.AlignCoordinateSystem(Point3d.Origin, Vector3d.XAxis, Vector3d.YAxis, Vector3d.ZAxis, entityOrigin, entityXaxis, entityYaxis, entityXaxis.CrossProduct(entityYaxis));

                viewportTableRecord.SetUcs(entityOrigin, entityXaxis, entityYaxis);
                editor.UpdateTiledViewportsFromDatabase();

                tr.Commit();
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                editor.WriteMessage("\nError: " + ex.Message);
            }
            finally
            {
                tr.Dispose();
            }
        }

        // Opens a collection of DBObject in ForRead mode (kaefer @ TheSwamp)       
        public static IEnumerable<T> GetObjects<T>(this IEnumerable ids) where T : DBObject
        {
            return ids.GetObjects<T>(OpenMode.ForRead);
        }

        // Opens a collection of DBObject in the given mode (kaefer @ TheSwamp)
        public static IEnumerable<T> GetObjects<T>(this IEnumerable ids, OpenMode mode) where T : DBObject
        {
            return ids
                .Cast<ObjectId>()
                .Select(id => id.GetObject<T>(mode))
                .Where(res => res != null);
        }

        // Applies the given Action to each element of the collection (mimics the F# Seq.iter function).
        public static void Iterate<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (T item in collection) action(item);
        }

        // Applies the given Action to each element of the collection (mimics the F# Seq.iteri function).
        // The integer passed to the Action indicates the index of element.
        public static void Iterate<T>(this IEnumerable<T> collection, Action<T, int> action)
        {
            int i = 0;
            foreach (T item in collection) action(item, i++);
        }

        // Creates a System.Data.DataTable from a BlockAttribute collection.
        public static System.Data.DataTable ToDataTable(this IEnumerable<BlockAttribute> blockAtts, string name)
        {
            System.Data.DataTable dTable = new System.Data.DataTable(name);
            dTable.Columns.Add("Name", typeof(string));
            dTable.Columns.Add("Quantity", typeof(int));

            blockAtts
                .GroupBy(blk => blk, (blk, blks) => new { Block = blk, Count = blks.Count() }, new BlockAttributeEqualityComparer())
                .Iterate(row =>
                {
                    System.Data.DataRow dRow = dTable.Rows.Add(row.Block.Name, row.Count);
                    row.Block.Attributes.Iterate(att =>
                    {
                        if (!dTable.Columns.Contains(att.Key))
                            dTable.Columns.Add(att.Key);
                        dRow[att.Key] = att.Value;
                    });
                });
            return dTable;
        }

        #endregion <Get>

        #region <OpenForWrite>

        public static DBObject XOpenForWrite(this Database db, ObjectId id, Action<DBObject> action = null)
        {
            using (acadApp.DocumentManager.GetDocument(db).LockDocument())
            using (var tr = db.TransactionManager.StartTransaction())
            {
                Entity ent = (Entity)id.GetObject(OpenMode.ForRead);
                var obj = tr.GetObject(id, OpenMode.ForRead);
                action?.Invoke(obj);
                return obj;
            }
        }

        public static DBObject XOpenForWrite(this Database db, ObjectId id, IEnumerable<ObjectIdItem> items)
        {
            var doc = acadApp.DocumentManager.GetDocument(db);
            using (doc.LockDocument())
            using (var tr = db.TransactionManager.StartTransaction())
            {
                //ObjectId id1 = db.GetObjectId(false, new Handle(Convert.ToInt64(handle, 16)), 0);
                //db.TryGetObjectId(new Handle(Convert.ToInt64(handle, 16)), out ObjectId id);
                DBObject obj = tr.GetObject(id, OpenMode.ForRead);
                obj.XUpdateDataObject(items);

                return obj;
            }
        }

        public static void XOpenForWrite(this Entity obj, Action<DBObject> action = null)
        {
            var db = obj.Database ?? HostApplicationServices.WorkingDatabase;
            var doc = acadApp.DocumentManager.GetDocument(db);

            using (doc.LockDocument())
            using (var tr = db.TransactionManager.StartTransaction())
            {
                if (action != null)
                {
                    if (!obj.IsNewObject)
                    {
                        obj.UpgradeOpen();
                        action(obj);
                    }
                    else
                    {
                        action(obj);
                    }
                }

                tr.Commit();
            }
        }

        /// <summary> Opens object for write. </summary>
        public static void XOpenForWrite(this ObjectId id, Action<DBObject> action = null)
        {
            var db = id.Database ?? HostApplicationServices.WorkingDatabase;
            using (acadApp.DocumentManager.GetDocument(db).LockDocument())
            using (var tr = db.TransactionManager.StartTransaction()) //.StartTransaction())
            {
                if (action != null)
                    action(tr.GetObject(id, OpenMode.ForWrite));

                tr.Commit();
            }
        }

        /// <summary> Opens object for write. </summary>
        public static void XOpenForWrite<T>(this ObjectId id, Action<T> action) where T : DBObject // newly 20130411
        {
            var db = id.Database ?? HostApplicationServices.WorkingDatabase;
            using (acadApp.DocumentManager.GetDocument(db).LockDocument())
            using (var tr = db.TransactionManager.StartTransaction())
            {
                if (action != null)
                    action(tr.GetObject(id, OpenMode.ForWrite) as T);

                tr.Commit();
            }
        }

        /// <summary> Opens object for write. </summary>
        public static void XOpenForWrite<T>(this ObjectId id, Func<T, DBObject[]> action) where T : DBObject
        {
            var db = id.Database ?? HostApplicationServices.WorkingDatabase;
            using (acadApp.DocumentManager.GetDocument(db).LockDocument())
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var newObjects = action(tr.GetObject(id, OpenMode.ForWrite) as T).ToList();
                newObjects.ForEach(newObject => tr.AddNewlyCreatedDBObject(newObject, true));
                tr.Commit();
            }
        }

        /// <summary> Opens objects for write. </summary>
        public static void XOpenForWrite(this IEnumerable<ObjectId> ids, Action<DBObject[]> action) // newly 20120908
        {
            var db = DbHelper.GetDatabase(ids) ?? HostApplicationServices.WorkingDatabase;
            using (acadApp.DocumentManager.GetDocument(db).LockDocument())
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var list = ids.Select(id => tr.GetObject(id, OpenMode.ForWrite)).ToArray();
                action(list);
                tr.Commit();
            }
        }

        /// <summary> Opens objects for write. </summary>
        public static void XOpenForWrite<T>(this IEnumerable<ObjectId> ids, Action<T[]> action) where T : DBObject // newly 20130411
        {
            var db = DbHelper.GetDatabase(ids) ?? HostApplicationServices.WorkingDatabase;
            using (acadApp.DocumentManager.GetDocument(db).LockDocument())
            using (var trans = db.TransactionManager.StartTransaction())
            {
                var list = ids.Select(id => trans.GetObject(id, OpenMode.ForWrite) as T).ToArray();
                action(list);
                trans.Commit();
            }
        }

        /// <summary> Opens objects for write. </summary>
        public static void XOpenForWrite<T>(this IEnumerable<ObjectId> ids, Func<T[], DBObject[]> action) where T : DBObject
        {
            var db = DbHelper.GetDatabase(ids) ?? HostApplicationServices.WorkingDatabase;
            using (acadApp.DocumentManager.GetDocument(db).LockDocument())
            using (var trans = db.TransactionManager.StartTransaction())
            {
                var list = ids.Select(id => trans.GetObject(id, OpenMode.ForWrite) as T).ToArray();
                var newObjects = action(list).ToList();
                newObjects.ForEach(newObject => trans.AddNewlyCreatedDBObject(newObject, true));
                trans.Commit();
            }
        }

        /// <summary> Opens objects for write. </summary>
        public static void XForEach(this IEnumerable<ObjectId> ids, Action<DBObject> action)
        {
            var db = DbHelper.GetDatabase(ids) ?? HostApplicationServices.WorkingDatabase;
            using (acadApp.DocumentManager.GetDocument(db).LockDocument())
            using (var trans = db.TransactionManager.StartTransaction())
            {
                ids.Select(id => trans.GetObject(id, OpenMode.ForWrite)).ToList().ForEach(action);
                trans.Commit();
            }
        }

        /// <summary> Opens objects for write. </summary>
        public static void XForEach<T>(this IEnumerable<ObjectId> ids, Action<T> action) where T : DBObject // newly 20130520
        {
            var db = DbHelper.GetDatabase(ids) ?? HostApplicationServices.WorkingDatabase;
            using (acadApp.DocumentManager.GetDocument(db).LockDocument())
            using (var trans = db.TransactionManager.StartTransaction())
            {
                ids.Select(id => trans.GetObject(id, OpenMode.ForWrite) as T).ToList().ForEach(action);
                trans.Commit();
            }
        }

        #endregion

        #region <OpenForRead>

        /// <summary>
        /// Opens object for read.
        /// </summary>
        /// <param name="id">The object ID.</param>
        /// <returns>The opened object.</returns>
        public static DBObject XOpenForRead(this ObjectId id, Action<DBObject> action = null)
        {
            using (var tr = id.Database.TransactionManager.StartTransaction())
            {
                var obj = tr.GetObject(id, OpenMode.ForRead);
                action?.Invoke(obj);
                return obj;
            }
        }

        public static T XOpenForRead<T>(this ObjectId id, Action<T> action = null) where T : Entity
        {
            using (var tr = id.Database.TransactionManager.StartTransaction())
            {
                T obj = (T)tr.GetObject(id, OpenMode.ForRead);
                action?.Invoke(obj);
                return obj;
            }
        }

        /// <summary>
        /// Opens object for read.
        /// </summary>
        /// <typeparam name="T">The type of object.</typeparam>
        /// <param name="id">The object ID.</param>
        /// <returns>The opened object.</returns>
        public static T XOpenForRead<T>(this ObjectId id) where T : DBObject // newly 20130122
        {
            return id.XOpenForRead() as T;
        }

        /// <summary>
        /// Opens objects for read.
        /// </summary>
        /// <param name="ids">The object IDs.</param>
        /// <returns>The opened object.</returns>
        public static DBObject[] XOpenForRead(this IEnumerable<ObjectId> ids) // newly 20120915
        {
            using (var trans = DbHelper.GetDatabase(ids).TransactionManager.StartTransaction())
            {
                return ids.Select(id => trans.GetObject(id, OpenMode.ForRead)).ToArray();
            }
        }

        /// <summary>
        /// Opens objects for read.
        /// </summary>
        /// <typeparam name="T">The type of object.</typeparam>
        /// <param name="ids">The object IDs.</param>
        /// <returns>The opened object.</returns>
        public static T[] XOpenForRead<T>(this IEnumerable<ObjectId> ids) where T : DBObject // newly 20130122
        {
            var db = DbHelper.GetDatabase(ids) ?? HostApplicationServices.WorkingDatabase;
            using (var tr = db.TransactionManager.StartTransaction())
            {
                return ids.Select(id => tr.GetObject(id, OpenMode.ForRead) as T).ToArray();
            }
        }

        #endregion
    }
}