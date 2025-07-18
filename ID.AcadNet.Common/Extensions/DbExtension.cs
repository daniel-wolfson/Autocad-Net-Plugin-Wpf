using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using ID.Infrastructure;
using ID.Infrastructure.Interfaces;
using Intellidesk.AcadNet.Common.CompBuilder;
using Intellidesk.AcadNet.Common.Enums;
using Intellidesk.AcadNet.Common.Interfaces;
using Intellidesk.AcadNet.Common.Models;
using Intellidesk.AcadNet.Common.Utils;
using Intellidesk.Data.Models.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization.Formatters.Binary;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Exception = Autodesk.AutoCAD.Runtime.Exception;
using GeoExtensions = Intellidesk.AcadNet.Common.Extentions.GeoExtensions;
using Transaction = Autodesk.AutoCAD.DatabaseServices.Transaction;
using TransactionManager = Autodesk.AutoCAD.DatabaseServices.TransactionManager;

namespace Intellidesk.AcadNet.Common.Extensions
{
    public static class DbExtensions
    {
        #region <Transaction>
        public static Transaction OpenTransaction(this TransactionManager trm)
        {
            return trm.TopTransaction ?? trm.StartTransaction();
        }

        public static TransactionScope TransactionScope(this TransactionManager trm, OpenMode openMode = OpenMode.ForRead)
        {
            return new TransactionScope(HostApplicationServices.WorkingDatabase, openMode);
        }

        public static TransactionManager TransactionLongScope(this TransactionManager trm, OpenMode openMode = OpenMode.ForRead)
        {
            trm = new TransactionManagerScope(trm);
            return trm;
        }

        public static T TransactionScope<T>(this Database db, Func<Transaction, T> action) where T : class
        {
            var doc = acadApp.DocumentManager.GetDocument(db);
            bool existTopTransaction = db.TransactionManager.TopTransaction != null;
            Transaction tr = db.TransactionManager.TopTransaction ?? db.TransactionManager.StartTransaction();

            T result;
            using (doc.LockDocument())
            {
                result = action(tr);
            }

            if (!existTopTransaction)
            {
                tr.Commit();
                tr.Dispose();
            }

            return result;
        }

        #endregion

        #region <Get>

        public static IEnumerable<ObjectId> GetBlockReferenceIds(this Database db, string blockname)
        {
            using (var tr = acadApp.DocumentManager.MdiActiveDocument.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead, false, true);

                ObjectId btrId = bt[blockname];
                if (btrId == ObjectId.Null || btrId.IsEffectivelyErased) return null;

                var btr = (BlockTableRecord)tr.GetObject(btrId, OpenMode.ForRead, false, true);
                ObjectIdCollection brefIds = btr.GetBlockReferenceIds(true, false);
                return brefIds.Cast<ObjectId>();
            }
        }

        public static IEnumerable<T> GetObjectsDynamic<T>(this Database db) where T : Entity
        {
            dynamic ids = acadApp.DocumentManager.GetCurrentSpaceId(db);
            return ((IEnumerable<dynamic>)ids).Select(x => x.ObjectId).OfType<T>();
            //if (ent.ColorIndex != 40) 
            //        entID.ColorIndex = 40;
        }

        public static Dictionary<ObjectId, string> GetObjectsDynamic(this Database db, string dxfTypeNamesFilter,
            string layerNamesFilter, string seachText)
        {
            var ids = new Dictionary<ObjectId, string>();

            dynamic msID = acadApp.DocumentManager.GetCurrentSpaceId(db);
            foreach (ObjectId entID in msID)
            {
                var typeName = entID.ObjectClass.DxfName;
                if (entID.ObjectClass != null && dxfTypeNamesFilter.Contains(typeName) &&
                    (string.IsNullOrEmpty(layerNamesFilter) ||
                     (!string.IsNullOrEmpty(layerNamesFilter) && entID.XGetLayerName() == layerNamesFilter)))
                {
                    var ent = (Entity)entID.GetObject(OpenMode.ForRead);
                    if (typeName == "MTEXT" && ((MText)ent).Text.Equals(seachText))
                        ids.Add(entID, ((MText)ent).Text);
                    if (typeName == "TEXT" && ((DBText)ent).TextString.Equals(seachText))
                        ids.Add(entID, ((DBText)ent).TextString);
                }
            }
            return ids;
        }

        //GetObject from Handle
        public static DBObject XGetObject(this Database db, string handle, OpenMode mode = OpenMode.ForRead, Action<DBObject> action = null)
        {
            if (string.IsNullOrEmpty(handle))
                return null;

            ObjectId id;

            if (!db.TryGetObjectId(new Handle(Convert.ToInt64(handle, 16)), out id))
                return null;

            if (id.IsErased)
                return null;

            using (var tr = db.TransactionManager.StartTransaction()) //new TransactionScope(db))
            {
                var dbObject = tr.GetObject(id, mode);
                if (action != null) action(dbObject);
                return dbObject;
            }
        }

        public static DBObject XGetObject(this Database db, ObjectId id, OpenMode mode = OpenMode.ForRead, Action<DBObject> action = null)
        {
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var dbObject = tr.GetObject(id, mode);
                if (action != null) action(dbObject);
                tr.Commit();
                return dbObject;
            }
        }

        // GetObject from Handle Long
        public static DBObject XGetObject(this Database db, long handle)
        {
            DBObject result = null;
            ObjectId id;
            if (db.TryGetObjectId(new Handle(handle), out id))
            {
                using (var tr = db.TransactionManager.StartTransaction())
                {
                    result = tr.GetObject(id, OpenMode.ForRead);
                }
            }
            else
            {
                Plugin.Logger.Error($"{nameof(DbExtensions)}.{nameof(XGetObject)} error: ", new SystemException("... Entity handle" + Convert.ToString(handle) + " not found"));
            }
            return result;
        }

        public static List<ObjectIdItem> XGetObjectDisplayItems(this Database db, string handle)
        {
            List<ObjectIdItem> displayObjectList = new List<ObjectIdItem>();
            ObjectId objectId = db.XGetObjectId(handle);

            objectId.XOpenForRead(ent =>
            {
                IPaletteElement element = ent.XGetDataObject();
                var items = new[] { ent.Handle.ToString() }.Concat(element.Items);

                foreach (var itemHandle in items)
                {
                    DBObject dbObject = db.XGetObject(itemHandle);
                    if (dbObject != null)
                        displayObjectList.Add(dbObject.XGetDisplayItem());
                }
            });

            return displayObjectList;
        }

        public static ObjectId[] XGetObjectIdItems(this IPaletteElement element, params Type[] includeFilterTypes)
        {
            var db = HostApplicationServices.WorkingDatabase;
            ObjectId[] ids =
                element.Items.Select(db.XGetObjectId).Concat(new[] { db.XGetObjectId(element.Handle) }).ToArray();
            if (includeFilterTypes != null && includeFilterTypes.Any())
                return ids.Where(x => includeFilterTypes.Contains(x.ObjectClass.GetRuntimeType())).ToArray();
            return ids;
        }

        //GetObjectId from Handle
        public static ObjectId XGetObjectId(this Database db, string handle)
        {
            db.TryGetObjectId(new Handle(Convert.ToInt64(handle, 16)), out ObjectId id);
            return id;
        }

        /// <summary> Gets the ObjectId that corresponds to the handle. </summary>
        public static ObjectId XGetObjectId(this Database db, Handle handle)
        {
            ObjectId id = ObjectId.Null;
            try
            {
                id = db.GetObjectId(false, handle, 0);
            }
            catch (Exception x)
            {
                if (x.ErrorStatus != ErrorStatus.UnknownHandle)
                {
                    throw x;
                }
            }
            return id;
        }

        public static object XGetXDataObject(this Database db, string handle, Type type)
        {
            DBObject dbObject = db.XGetObject(handle);
            return dbObject.XGetXDataObject(type);
        }

        public static IPaletteElement XGetXDataObject(this Database db, string handle)
        {
            var dbObject = db.XGetObject(handle);
            var xDataType = dbObject.XGetData(TypeCodes.TypeName);
            Type type = Type.GetType($"Intellidesk.AcadNet.Common.Model.{xDataType},{typeof(AcadCable).Assembly.FullName}");
            var xData = dbObject.XGetData(TypeCodes.Object);
            IPaletteElement element = (IPaletteElement)JsonConvert.DeserializeObject(xData, type);
            return element;
        }

        /// <summary> Get objects derived from DBObject </summary>
        /// <param name="db">current db</param>
        /// <param name="status">DBObjectStatus (Any/Erased/NotErased)</param>
        /// <returns>ObjectId[]</returns>
        public static ObjectId[] XGetAllData(this Database db, eDBObjectStatus status)
        {
            bool? a = null;
            switch (status)
            {
                case eDBObjectStatus.Erased:
                    a = true;
                    break;
                case eDBObjectStatus.NotErased:
                    a = false;
                    break;
                case eDBObjectStatus.Any:
                    break;
            }
            return XGetData(db, (n, x) => !a.HasValue || x.IsErased == a, (n, m) => m);
        }

        //Db.ObjectId[] selectionIds = Db.GetData<Db.ObjectId>(PlinesWithSomeVertexFilter, (t, X) => X);
        //Boolean PlinesWithSomeVertexFilter(Transaction Tr, Db.ObjectId id) {

        /// <summary> Get data from db </summary>
        /// <typeparam name="T">type of objects, that array returning</typeparam>
        /// <param name="db">current db</param>
        /// <param name="requirement">Condition of choce objects</param>
        /// <param name="result">result object</param>
        /// <returns>return array objects of T</returns>
        public static T[] XGetData<T>(this Database db, Func<Transaction, ObjectId, bool> requirement,
            Func<Transaction, ObjectId, T> result)
        {
            List<T> primitives = new List<T>();
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                for (long i = db.BlockTableId.Handle.Value; i < db.Handseed.Value; i++)
                {
                    ObjectId id = ObjectId.Null;
                    Handle h = new Handle(i);
                    if (TryGetValidObjectId(db, h, ref id) && requirement(tr, id))
                        primitives.Add(result(tr, id));
                }
            }
            return primitives.ToArray();
        }

        /// <summary> GetByTypes </summary>
        public static Dictionary<string, List<ObjectId>> XGetByTypes(this Database db,
            Func<Transaction, ObjectId, bool> requirement)
        {
            Dictionary<string, List<ObjectId>> dict = new Dictionary<string, List<ObjectId>>();
            using (Transaction t = db.TransactionManager.StartTransaction())
            {
                for (long i = db.BlockTableId.Handle.Value;
                    i < db.Handseed.Value;
                    i++)
                {
                    ObjectId id = ObjectId.Null;
                    Handle h = new Handle(i);
                    if (TryGetValidObjectId(db, h, ref id) && requirement(t, id))
                    {
                        string type = id.ObjectClass.Name;
                        if (!dict.ContainsKey(type))
                            dict.Add(type, new List<ObjectId>());
                        dict[type].Add(id);
                    }
                }
            }
            return dict;
        }

        /// <summary> GroupByTypes </summary>
        public static Dictionary<string, List<ObjectId>> XGetGroupByTypes(this Database db, ObjectId[] ids)
        {
            Dictionary<string, List<ObjectId>> dict = new Dictionary<string, List<ObjectId>>();
            foreach (ObjectId id in ids)
            {
                string type = id.ObjectClass.Name;
                if (!dict.ContainsKey(type))
                    dict.Add(type, new List<ObjectId>());
                dict[type].Add(id);
            }
            return dict;
        }

        public static ObjectId[] XGetGroupEntities(this Database db, ObjectId groupId)
        {
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                Group @group = tr.GetObject(groupId, OpenMode.ForRead) as Group;
                if (@group != null)
                    return @group.GetAllEntityIds();
                //Tr.Commit();
            }
            return null;
        }

        public static void XGetProjectInfo(this Database db)
        {
            var doc = acadApp.DocumentManager.GetDocument(db);
            //IDictionary<string, object> data = new Dictionary<string, object>();
            try
            {
                using (doc.LockDocument())
                {
                    using (Transaction trans = db.TransactionManager.StartTransaction())
                    {
                        // Find the NOD in the database
                        DBDictionary nod =
                            (DBDictionary)trans.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForWrite);

                        //resultDataInfo = new FileDataInfo()
                        //{


                        //};

                        IPluginSettings appSettings = Plugin.GetService<IPluginSettings>();
                        if (nod.Contains(appSettings.Name))
                        {
                            // Now let's read the data back and print them out
                            //  to the Visual Studio's Output window
                            ObjectId myDataId = nod.GetAt(appSettings.Name);
                            Xrecord readBack = (Xrecord)trans.GetObject(myDataId, OpenMode.ForRead);

                            //foreach (TypedValue value in readBack.Data)
                            //{
                            //    System.Diagnostics.Debug.Print("===== OUR DATA: " + value.TypeCode + ". " + value.Value);
                            //}
                            TypedValue[] data = readBack.Data.AsArray();

                            Dictionary<string, string> dic = new Dictionary<string, string>();

                            for (int i = 1; i < data.Length - 1; i++)
                            {
                                var key = data[i].Value.ToString();
                                i++;
                                var value = data[i].Value.ToString();

                                dic.Add(key, value);
                            }

                            //if (dic.ContainsKey("CoordSystem"))
                            //CoordSystem = (CoordSystem)Enum.Parse(typeof(CoordSystem), dic["CoordSystem"]);
                            //Type = "Autocad Drawing";
                        }
                        else
                        {
                            //SetProjectInfo(doc, GetTypedValues());
                        }
                        trans.Commit();
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.Print(e.ToString());
            }
            finally
            {
                db.Dispose();
            }
        }

        public static void XGetProjectInfo(this Database db, string fileFullPath, bool isBackgroundLoad = false)
        {
            //, ProjectTypedValues typedValue
            //{
            if (!string.IsNullOrEmpty(fileFullPath) && !File.Exists(fileFullPath)) return;

            var doc = acadApp.DocumentManager.GetDocument(db);
            //var doc = Documents.DocumentFind(fileFullPath);

            if (doc != null)
            {
                db = doc.Database;
                //db.GetProjectInfo(doc);
            }
            else if (isBackgroundLoad)
            {
                db = new Database();
                db.ReadDwgFile(fileFullPath, System.IO.FileShare.ReadWrite, false, null);
                db.SaveAs(db.OriginalFileName, DwgVersion.Current);
            }
        }

        #endregion

        #region <Set>

        public static void XSetGridAndSnap(this Database db, int gridSpace = 1, int snapSpace = 1)
        {
            // Get the current database
            Document acDoc = acadApp.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the active viewport
                ViewportTableRecord acVportTblRec;
                acVportTblRec = acTrans.GetObject(acDoc.Editor.ActiveViewportId,
                    OpenMode.ForWrite) as ViewportTableRecord;

                // Turn on the grid for the active viewport
                acVportTblRec.GridEnabled = true;

                // Adjust the spacing of the grid to 1, 1
                acVportTblRec.GridIncrements = new Point2d(gridSpace, gridSpace);

                // Turn on the snap mode for the active viewport
                acVportTblRec.SnapEnabled = true;

                // Adjust the snap spacing to 0.5, 0.5
                acVportTblRec.SnapIncrements = new Point2d(snapSpace, snapSpace);

                // Change the snap base point to 1, 1
                acVportTblRec.SnapBase = new Point2d(1, 1);

                // Change the snap rotation angle to 30 degrees (0.524 radians)
                acVportTblRec.SnapAngle = 0.524;

                // Update the display of the tiled viewport
                acDoc.Editor.UpdateTiledViewportsFromDatabase();

                // Commit the changes and dispose of the transaction
                acTrans.Commit();
            }
        }

        internal static void SetDatabaseDefaultSettings(Database db)
        {
            if (db == null)
                throw new ArgumentNullException("db");
            if (db.IsDisposed)
                throw new ArgumentException("Db.IsDisposed == true");

            db.Measurement = MeasurementValue.Metric;
            db.Insunits = UnitsValue.Millimeters;
            db.WorldUcsBaseOrigin(OrthographicView.TopView);
        }

        #endregion

        #region <Read and Validation>

        public static IEnumerable<T> XReadObjectsDynamic<T>(this Database db, string layerPattern = "", string entityPattern = "")
            where T : Entity
        {
            dynamic ms = acadApp.DocumentManager.GetCurrentSpaceId(db);
            var ids = (IEnumerable<dynamic>)ms;

            if (typeof(T) != typeof(Entity))
                ids = ids.Where(ent => ent.IsKindOf(typeof(T)));

            if (typeof(T) == typeof(DBText) && !string.IsNullOrEmpty(entityPattern) && entityPattern != "*")
            {
                ids = ids.Where(ent =>
                {
                    var pattern = entityPattern.Contains("*") ? entityPattern.Replace("*", "") : $@"(\w*{entityPattern}\w*)";  //$"\b()\b";
                    return ((string)ent.TextString).FindMatch(pattern);
                });
            }

            //.Where(id => id.ObjectClass == RXObject.GetClass(typeof(T))).ToList();

            if (!string.IsNullOrEmpty(layerPattern) && !layerPattern.Contains("*"))
                ids = ids.Where(ent => ent.Layer == layerPattern);
            else if (!string.IsNullOrEmpty(layerPattern) && layerPattern.Contains("*") && layerPattern.Length > 1)
                ids = ids.Where(ent => ent.Layer.Contains(layerPattern));

            List<ObjectId> idResults = ids.Select(x => x.ObjectId).Cast<ObjectId>().ToList();

            using (var tr = db.TransactionManager.StartTransaction())
            {
                foreach (var id in idResults)
                    yield return (T)tr.GetObject(id, OpenMode.ForRead);
            }
        }

        public static IEnumerable<DBObject> XReadElementsDynamic<T>(this Database db, string layerNameFilter = "")
            where T : IPaletteElement
        {
            dynamic ms = acadApp.DocumentManager.GetCurrentSpaceId(db);
            var ids = (IEnumerable<dynamic>)ms;

            ids = ids.Where(id => db.XGetObject((ObjectId)id).XGetXDataObjectType() == typeof(T));

            List<ObjectId> idResults = ids.Select(x => x.ObjectId).Cast<ObjectId>().ToList();

            using (var tr = db.TransactionManager.StartTransaction())
            {
                foreach (var objId in idResults)
                    yield return tr.GetObject(objId, OpenMode.ForRead);
            }
        }

        public static IEnumerable<T> XReadLayoutObjects<T>(this Database db, string layoutName) where T : DBObject
        {
            using (var tr = db.TransactionManager.StartTransaction())
            {
                //if (scope.tr == null) throw new Exception(ErrorStatus.NotTopTransaction);

                IEnumerable<DBDictionaryEntry> layouts =
                    ((DBDictionary)tr.GetObject(db.LayoutDictionaryId, OpenMode.ForRead))
                        .Cast<DBDictionaryEntry>().Where(entry => entry.Key == layoutName); //"Model"

                foreach (DBDictionaryEntry entry in layouts)
                {
                    Layout lay = (Layout)tr.GetObject(entry.Value, OpenMode.ForRead);
                    BlockTableRecord btr = (BlockTableRecord)tr.GetObject(lay.BlockTableRecordId, OpenMode.ForRead);

                    foreach (ObjectId id in btr)
                    {
                        if (id.ObjectClass == RXClass.GetClass(typeof(T)))
                            yield return (T)tr.GetObject(id, OpenMode.ForRead);
                    }
                }
            }
        }

        public static IEnumerable<T> XReadObjects<T>(this Database db, string layerName = "", string entityPattern = "") where T : Entity
        {
            Expression<Func<DBObject, bool>> dbObjectPredicate = obj => true;
            var objectClass = RXObject.GetClass(typeof(T));


            //if (!string.IsNullOrEmpty(layerName) && layerName != "*" 
            //    && !string.IsNullOrEmpty(layerName.Replace("*", ""))
            //    && !string.IsNullOrEmpty(entityPattern))
            //{
            //    dbObjectPredicate = obj => layerName.Contains("*")
            //            ? ((Entity)obj).Layer.Contains(layerName.Replace("*", ""))
            //            : ((Entity)obj).Layer == layerName
            //            && ((DBText)obj).TextString.Contains(entityPattern.Replace("*", ""));
            //}
            if (!string.IsNullOrEmpty(layerName) && layerName != "*"
                && !string.IsNullOrEmpty(layerName.Replace("*", "")))
            {
                dbObjectPredicate = obj => layerName.Contains("*")
                        ? ((Entity)obj).Layer.Contains(layerName.Replace("*", ""))
                        : ((Entity)obj).Layer == layerName;
            }

            if (typeof(T) == typeof(DBText) && entityPattern != "")
            {
                Expression<Func<DBObject, bool>> newPred =
                    obj => (obj as DBText).TextString.Contains(entityPattern.Replace("*", ""));

                //dbObjectPredicate = dbObjectPredicate.AndAlso(newPred);
                //dbObjectPredicate = Expression.Lambda<Func<DBObject, bool>>(
                //    Expression.AndAlso(dbObjectPredicate.Body, newPred.Body), dbObjectPredicate.Parameters);

                dbObjectPredicate = Expression.Lambda<Func<DBObject, bool>>(
                    Expression.AndAlso(
                        dbObjectPredicate.Body,
                        new ExpressionParameterReplacer(newPred.Parameters, dbObjectPredicate.Parameters).Visit(newPred.Body)),
                    dbObjectPredicate.Parameters);

                //var parameter = Expression.Parameter(typeof(DBText), "x");
                //var castparameter = Expression.TypeAs(parameter, typeof(DBText));
                //var member = Expression.Property(castparameter, "TextString");
                //var constant = Expression.Constant(entityPattern);
                //var body = Expression.Equal(member, constant);
                //var dbObjectPredicate1 = Expression.Lambda<Func<DBText, bool>>(body, parameter);
                //var f1 = dbObjectPredicate1.Compile();
            }

            return db.XReadObjects<T>(id => id.ObjectClass == objectClass, dbObjectPredicate.Compile());
        }

        public static IEnumerable<T> XReadObjects<T>(this Database db, string layerName = "", bool isParent = false) where T : Entity
        {
            Func<ObjectId, bool> objectIdPredicate = null;
            Func<DBObject, bool> dbObjectPredicate = null;
            var objectClass = RXObject.GetClass(typeof(T));

            if (isParent)
                objectIdPredicate = id => id.ObjectClass.MyParent == objectClass;
            else
                objectIdPredicate = id => id.ObjectClass == objectClass;

            if (!string.IsNullOrEmpty(layerName) && layerName != "*" && !string.IsNullOrEmpty(layerName.Replace("*", "")))
            {
                dbObjectPredicate = obj => layerName.Contains("*")
                        ? ((Entity)obj).Layer.Contains(layerName.Replace("*", ""))
                        : ((Entity)obj).Layer == layerName;
            }

            return db.XReadObjects<T>(objectIdPredicate, dbObjectPredicate);
        }

        public static IEnumerable<T> XReadObjects<T>(this Database db,
            Func<ObjectId, bool> objectIdPredicate,
            Func<DBObject, bool> dbObjectPredicate = null) where T : Entity
        {
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var modelSpaceId = acadApp.DocumentManager.GetCurrentSpaceId(db);
                var btr = (BlockTableRecord)tr.GetObject(modelSpaceId, OpenMode.ForRead);

                var results = btr
                    .Cast<ObjectId>()
                    .Where(objectIdPredicate)
                    .Select(id => tr.GetObject(id, OpenMode.ForRead))
                    .Where(x => !x.IsErased
                        && (dbObjectPredicate != null ? dbObjectPredicate(x) : true))
                    .Cast<T>();

                foreach (var br in results.ToList())
                    yield return br;
            }
        }

        public static IEnumerable<T> XReadObjects<T>(this Database db,
            Type[] entityTypes, string layerName = "") where T : Entity
        {
            Func<Type, RXClass> getClass = RXObject.GetClass;

            // You can set this anywhere
            var acceptableTypes = new HashSet<RXClass>();
            foreach (var entityType in entityTypes)
            {
                acceptableTypes.Add(getClass(entityType));
            }

            TransactionScope scope;
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var modelSpaceId = acadApp.DocumentManager.GetCurrentSpaceId(db);
                var btr = (BlockTableRecord)tr.GetObject(modelSpaceId, OpenMode.ForRead);

                var blocks = btr
                    .Cast<ObjectId>()
                    .Where(id => acceptableTypes.Contains(id.ObjectClass))
                    .Select(id => tr.GetObject(id, OpenMode.ForRead))
                    .Where(x => !x.IsErased)
                    .Cast<T>();

                if (!string.IsNullOrEmpty(layerName) && layerName != "*" && !string.IsNullOrEmpty(layerName.Replace("*", "")))
                {
                    blocks = blocks.Where(br =>
                        layerName.Contains("*")
                            ? br.Layer.Contains(layerName.Replace("*", "")) : br.Layer == layerName);
                }

                foreach (var br in blocks.ToList())
                    yield return br;
            }
        }

        public static IEnumerable<T> XReadBlocks<T>(this Database db,
            string layerPattern = null, string blockNamePattern = null,
            string attrTagPattern = null, string attrValuePattern = null,
            bool includeNested = false)
        where T : BlockReference
        {
            using (var tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                RXClass rxc = RXObject.GetClass(typeof(T));

                foreach (ObjectId btrId in bt)
                {
                    BlockTableRecord btr = (BlockTableRecord)tr.GetObject(btrId, OpenMode.ForRead);
                    if (btr.IsLayout && !btr.Name.Contains("Model_Space") || btr.IsErased) continue;

                    foreach (ObjectId id in btr.Cast<ObjectId>().Where(x => x.ObjectClass == rxc))
                    {
                        if (includeNested)
                        {
                            var nestedBlockReferences = btr.XReadBlockReferences<T>(blockNamePattern, layerPattern, attrTagPattern, attrValuePattern);
                            foreach (var br in nestedBlockReferences)
                                yield return br;
                        }
                        else
                        {
                            var br = tr.GetObject(id, OpenMode.ForRead, false, false) as T;
                            if (br != null && !br.BlockTableRecord.IsNull)
                            {
                                var ltr = br.LayerId.XAsLayer();
                                if (ltr != null && ltr.XValidate(layerPattern) && br.XValidate(blockNamePattern, attrTagPattern, attrValuePattern))
                                    yield return br;
                            }
                        }
                    }
                }
            }
        }

        public static IEnumerable<T> XReadBlocksFromLayouts<T>(this Database db,
            string blockNamePattern = "*", string layerPattern = "*", string attrTagPattern = "*",
            bool includeNestedObjects = true) where T : BlockReference
        {
            using (var tr = db.TransactionManager.StartTransaction())
            {
                DBDictionary layouts = (DBDictionary)tr.GetObject(db.LayoutDictionaryId, OpenMode.ForRead);
                RXClass rxc = RXObject.GetClass(typeof(BlockReference));

                foreach (var entry in layouts)
                {
                    if (entry.Key != "Model")
                    {
                        Layout lay = (Layout)tr.GetObject(entry.Value, OpenMode.ForRead);
                        BlockTableRecord btr = (BlockTableRecord)tr.GetObject(lay.BlockTableRecordId, OpenMode.ForRead);

                        //BlockReference br1 = (BlockReference)scope.tr.GetObject(id, OpenMode.ForRead);
                        foreach (ObjectId id in btr)
                        {
                            if (id.ObjectClass == rxc)
                            {
                                BlockReference br = (BlockReference)tr.GetObject(id, OpenMode.ForRead) as BlockReference;
                                acadApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage($"\nBlock name: {br.Name}");

                                if (!br.IsErased)
                                    //layer pattern
                                    if (layerPattern == "*" || !string.IsNullOrEmpty(layerPattern.Replace("*", "")) &&
                                        br.Layer.Contains(layerPattern))
                                    {
                                        //blockName pattern
                                        if (blockNamePattern == "*" || blockNamePattern != null && br.Name == blockNamePattern)
                                        {
                                            //attr pattern
                                            if (attrTagPattern == "*" || attrTagPattern != null && br.AttributeCollection != null && br.AttributeCollection.Count > 0)
                                            {
                                                var attrs = br.XGetAttributes(new[] { attrTagPattern });
                                                if (attrs.Any())
                                                    yield return br as T;
                                            }
                                        }

                                        //var btrId = br.IsDynamicBlock ? br.DynamicBlockTableRecord : br.BlockTableRecord;
                                        //BlockTableRecord btrNested = (BlockTableRecord)scope.tr.GetObject(btrId, OpenMode.ForRead);

                                        //foreach (var brNested in btrNested.GetBlockReferencesByModelSpace("Model")) //btr.GetBlockReferenceIds(true, true))
                                        //{
                                        //    //if (!br.IsErased &&
                                        //    //    (layerName == "*" || !string.IsNullOrEmpty(layerName.Replace("*", "")) &&
                                        //    //    br.Layer.Contains(layerName)) &&
                                        //    //    br.Name == "EB")
                                        //    //    yield return br as T;
                                        //}

                                        //yield return (T)scope.tr.GetObject(id, OpenMode.ForRead) as T;
                                    }
                            }
                        }
                    }
                }
            }
        }

        public static IEnumerable<T> XReadBlockTables<T>(this Database db,
            string layerName = "*", string name = "*", bool includeNestedObjects = true) where T : BlockReference
        {
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var objs = db.XReadObjects<T>(layerName, "");

                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                foreach (ObjectId btId in bt)
                {
                    BlockTableRecord btr = (BlockTableRecord)tr.GetObject(btId, OpenMode.ForRead);

                    if (!btr.IsLayout && !btr.IsErased)
                    {
                        bool isMacth = name == "*" ||
                            name.Contains("*") ?
                                btr.Name.Contains(name.Replace("*", "")) : btr.Name == name;
                        if (!isMacth) continue;

                        acadApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage($"\nBlock name: {btr.Name}");

                        if (includeNestedObjects)
                        {
                            foreach (var br in btr.XGetBlockReferences(null, "ENTITY_CODE")) //btr.GetBlockReferenceIds(true, true))
                            {
                                if (!br.Value.IsErased &&
                                    (layerName == "*" ||
                                    !string.IsNullOrEmpty(layerName.Replace("*", "")) && br.Value.Layer.Contains(layerName))
                                    && br.Value.Name == "EB")
                                    yield return br as T;
                            }
                        }
                        else
                        {
                            var br = btr.Cast<ObjectId>().Select(x => x.XGetEntity<BlockReference>());
                            yield return br as T;
                        }
                    }
                }
                //}
            }
        }

        public static T XReadObject<T>(this Database db, string handleString) where T : Entity
        {
            var doc = acadApp.DocumentManager.MdiActiveDocument;
            long ln = Convert.ToInt64(handleString, 16);
            Handle hn = new Handle(ln);
            ObjectId id = db.GetObjectId(false, hn, 0);

            using (Transaction tr = doc.TransactionManager.StartTransaction())
            {
                return tr.GetObject(id, OpenMode.ForRead) as T;
            }
        }

        public static IEnumerable<ObjectIdItem> XReadElements(this Database db, Type type, string title, string layerNameFilter = "")
        {
            //if (type == typeof (AcadCabinet)) if (type == typeof(AcadCable)) (type == typeof(AcadClosure))
            //{
            //    return db.ReadElements<AcadCabinet>(title, layerNameFilter);
            //    return db.ReadElements<AcadCable>(title, layerNameFilter);
            //    return db.ReadElements<AcadClosure>(title, layerNameFilter);
            //}

            var method = typeof(DbExtensions).GetMethods().FirstOrDefault(m => m.Name == "ReadElements" && m.IsGenericMethod);
            if (method != null)
                return method.MakeGenericMethod(type)
                    .Invoke(null, new object[] { db, title, layerNameFilter }) as IEnumerable<ObjectIdItem>;

            return null;
        }

        public static IEnumerable<ObjectIdItem> XReadElements<T>(this Database db, string title, string layerNameFilter = "All")
            where T : IPaletteElement
        {
            List<ObjectIdItem> results = new List<ObjectIdItem>();

            using (var tr = db.TransactionManager.StartTransaction())
            {
                var modelSpaceId = acadApp.DocumentManager.GetCurrentSpaceId(db);
                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(modelSpaceId, OpenMode.ForRead);

                foreach (ObjectId id in btr)
                {
                    DBObject dbObject = HostApplicationServices.WorkingDatabase.XGetObject(id);

                    if (!string.IsNullOrEmpty(layerNameFilter) && layerNameFilter != "All")
                    {
                        var entity = dbObject as Entity;
                        if (entity != null && entity.Layer != layerNameFilter)
                            continue;
                    }

                    if (dbObject.XData == null) continue;

                    Type type = dbObject.XGetXDataObjectType();
                    if (type == null) continue;

                    PaletteElement element = null;
                    if (type == typeof(AcadTitle))
                    {
                        element = dbObject.XGetXDataObject<AcadTitle>();
                        DBObject ownerDbObject = db.XGetObject(element.OwnerHandle);

                        type = ownerDbObject.XGetXDataObjectType();
                        if (type == null) continue;

                        bool condition = title.Contains("*") ? element.Title.Contains(title.Replace("*", "")) : element.Title == title;
                        if (type == typeof(T) && (condition || title == "*"))
                        {
                            var elementType = ((Entity)dbObject).GetType().Name.ToLower().Replace("db", "");
                            results.Add(new ObjectIdItem(dbObject.ObjectId, $"{element.Title} ({elementType})"));
                        }
                        else
                        {
                            results.RemoveAll(x => x.ObjectId == ownerDbObject.ObjectId);
                            type = null;
                        }
                    }

                    if (type != null && type == typeof(T))
                    {
                        if (type == typeof(AcadCable))
                        {
                            element = dbObject.XGetXDataObject<AcadCable>();
                        }
                        else if (type == typeof(AcadClosure))
                        {
                            element = dbObject.XGetXDataObject<AcadClosure>();
                        }
                        else if (type == typeof(AcadCabinet))
                        {
                            element = dbObject.XGetXDataObject<AcadCabinet>();
                        }
                        if (element != null && results.All(x => x.ObjectId.Handle.ToString() != element.Handle))
                        {
                            var elementType = ((Entity)dbObject).GetType().Name.ToLower();
                            results.Add(new ObjectIdItem(dbObject.ObjectId, $"{element.Title} ({elementType})"));
                        }
                    }

                    //if (id.ObjectClass == RXClass.GetClass(typeof(T)))
                    //    yield return (T)scope.tr.GetObject(id, OpenMode.ForRead);
                }
            }
            return results.AsEnumerable();
        }

        public static bool TryGetValidObjectId(this Database db, Handle hnd, ref ObjectId id)
        {
            bool result = db.TryGetObjectId(hnd, out id);
            if (result && id.IsValid)
                return result;

            id = ObjectId.Null;
            return false;
        }

        public static ObjectIdCollection GetAllBlockReferencesByNames(this Database db, string pattern)
        {
            //Database db = HostApplicationServices.WorkingDatabase;
            ObjectIdCollection result = new ObjectIdCollection();
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                foreach (ObjectId id in bt)
                {
                    BlockTableRecord btr = (BlockTableRecord)id.GetObject(OpenMode.ForRead);
                    if (btr.Name != "") //Utils.WcMatch(btr.Name, pattern)
                    {
                        foreach (ObjectId refId in btr.GetBlockReferenceIds(true, false))
                        {
                            GetNestedReferences(refId, refId, ref result);
                        }
                        foreach (ObjectId anonId in btr.GetAnonymousBlockIds())
                        {
                            BlockTableRecord anonBTR = (BlockTableRecord)tr.GetObject(anonId, OpenMode.ForRead);
                            foreach (ObjectId refId in anonBTR.GetBlockReferenceIds(true, false))
                            {
                                GetNestedReferences(refId, refId, ref result);
                            }
                        }
                    }
                }
                tr.Commit();
            }
            return result;
        }

        public static List<BlockReference> GetDynamicBlockReferences(this Database db)
        {
            Editor ed = acadApp.DocumentManager.MdiActiveDocument.Editor;
            List<BlockReference> dynBlockRefs = new List<BlockReference>();

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                //get the blockTable and iterate through all blockDef
                BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                foreach (ObjectId btrId in bt)
                {
                    //get the blockDef and check if is anonymous
                    BlockTableRecord btr = (BlockTableRecord)trans.GetObject(btrId, OpenMode.ForRead);
                    if (btr.IsDynamicBlock)
                    {
                        //get all anonymous blocks from this dynamic block
                        ObjectIdCollection anonymousIds = btr.GetAnonymousBlockIds();
                        //ObjectIdCollection dynBlockRefs = new ObjectIdCollection();


                        foreach (ObjectId anonymousBtrId in anonymousIds)
                        {
                            //get the anonymous block
                            BlockTableRecord anonymousBtr = (BlockTableRecord)trans.GetObject(anonymousBtrId, OpenMode.ForRead);

                            //and all references to this block
                            ObjectIdCollection blockRefIds = anonymousBtr.GetBlockReferenceIds(true, true);
                            ILayerService layerService = Plugin.GetService<ILayerService>();
                            foreach (ObjectId id in blockRefIds)
                            {
                                var layer = id.XAsLayer();
                                if (layer != null)
                                {
                                    BlockReference br = (BlockReference)id.GetObject(OpenMode.ForRead);
                                    dynBlockRefs.Add(br);
                                }
                            }
                        }

                        //Do something with the collection we created
                        //ed.WriteMessage(String.Format("Dynamic block \"{0}\" found with {1} anonymous block and {2} block references\n", btr.Name, anonymousIds.Count, dynBlockRefs.Count));
                    }
                    else
                    {

                    }
                }
                return dynBlockRefs;
            }
        }

        #endregion

        #region <Remove>

        //Remove object by Handle
        public static void XRemoveObject(this Database db, string handle)
        {
            db.XRemoveObject(db.XGetObjectId(handle));
        }

        //Remove object by ObjectId
        public static void XRemoveObject(this Database db, ObjectId objectId)
        {
            using (var tr = db.TransactionManager.StartTransaction())
            {
                try
                {
                    var dbObject = tr.GetObject(objectId, OpenMode.ForWrite, true);
                    dbObject.Erase();
                    dbObject.Dispose();
                }
                catch (Exception ex)
                {
                    Plugin.Logger.Error($"{nameof(DbExtensions)}.{nameof(XRemoveObject)} error: ", ex);
                }
            }
        }

        public static void XRemoveObjects(this Database db, IPaletteElement element, params Type[] exceptFilterTypes)
        {
            ObjectId[] objectIds = element.Items.Select(db.XGetObjectId).Concat(new[] { db.XGetObjectId(element.Handle) }).ToArray();
            db.XRemoveObjects(objectIds, exceptFilterTypes);
        }

        public static void XRemoveObjects(this Database db, ObjectId[] objectIds, params Type[] exceptFilterTypes)
        {
            using (var tr = db.TransactionManager.StartTransaction())
            {
                try
                {
                    foreach (var objectId in objectIds)
                    {
                        if (exceptFilterTypes != null && exceptFilterTypes.Contains(objectId.ObjectClass.GetRuntimeType())) continue;

                        var dbObject = tr.GetObject(objectId, OpenMode.ForWrite, true);
                        dbObject.Erase();
                        dbObject.Dispose();

                    }
                }
                catch (Exception ex)
                {
                    Plugin.Logger.Error($"{nameof(DbExtensions)}.{nameof(XRemoveObjects)} error: ", ex);
                }
            }
        }

        public static void XRemoveObjects(this Database db, string[] handles)
        {
            foreach (var handle in handles)
            {
                db.XRemoveObject(db.XGetObjectId(handle));
            }
        }

        //Remove objects by Handle
        public static void XRemoveObjects(this Database db, string objectName, string[] layers = null)
        {
            var doc = acadApp.DocumentManager.GetDocument(db);
            using (doc.LockDocument())
            {
                using (var tr = db.TransactionManager.StartTransaction())
                {
                    var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                    var btr = (BlockTableRecord)tr.GetObject(bt[objectName ?? acadApp.DocumentManager.GetCurrentSpace()], OpenMode.ForRead);

                    if (objectName != null)
                    {
                        var ids = btr.GetBlockReferenceIds(true, true);
                        foreach (ObjectId id in ids)
                        {
                            BlockReference blkRef = (BlockReference)tr.GetObject(id, OpenMode.ForRead);
                            ids.Remove(id);
                            blkRef.UpgradeOpen();
                            blkRef.Erase();
                        }
                        btr.UpgradeOpen();
                        btr.Erase(true);
                    }
                    else
                        foreach (ObjectId entId in btr)
                        {
                            var ent = (Entity)tr.GetObject(entId, OpenMode.ForWrite);
                            if (layers != null && layers.Length != 0)
                            {
                                if (layers.Contains(ent.Layer))
                                    ent.Erase(true);
                            }
                            else
                            {
                                ent.Erase(true);
                            }
                        }
                }
            }
        }

        //Remove object by Handle
        public static void XEraseObjectTest(this Database db)
        {
            // Start a transaction
            using (var tr = db.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                var acBlkTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                var acBlkTblRec = tr.GetObject(acBlkTbl[acadApp.DocumentManager.GetCurrentSpace()], OpenMode.ForWrite) as BlockTableRecord;

                // Create a lightweight polyline
                var acPoly = new Polyline();
                acPoly.AddVertexAt(0, new Point2d(2, 4), 0, 0, 0);
                acPoly.AddVertexAt(1, new Point2d(4, 2), 0, 0, 0);
                acPoly.AddVertexAt(2, new Point2d(6, 4), 0, 0, 0);

                // Add the new object to the block table record and the transaction
                if (acBlkTblRec != null) acBlkTblRec.AppendEntity(acPoly);
                tr.AddNewlyCreatedDBObject(acPoly, true);

                // Update the display and display an alert message
                var doc = acadApp.DocumentManager.GetDocument(db);
                doc.Editor.Regen();
                acadApp.ShowAlertDialog("Erase the newly added polyline.");

                // Erase the polyline from the drawing
                acPoly.Erase(true);

                // Save the new object to the database
                tr.Commit();
            }
        }

        public static void XRemoveXrecord(this Database db, ObjectId dictId, string xRecordName)
        {
            //ObjectId dictId = obj.ExtensionDictionary;
            if (dictId == ObjectId.Null) return;

            using (var tr = db.TransactionManager.StartTransaction())
            {
                using (DBDictionary xdict = (DBDictionary)tr.GetObject(dictId, OpenMode.ForRead))
                {
                    //var xRecordName = obj.Handle.ToString();
                    if (xdict.Contains(xRecordName))
                    {
                        ObjectId xrecordId = xdict.GetAt(xRecordName);
                        xdict.UpgradeOpen();
                        xdict.Remove(xRecordName);

                        using (Xrecord xrecord = (Xrecord)xrecordId.GetObject(OpenMode.ForWrite))
                        {
                            xrecord.Erase();
                        }
                    }
                }
            }
            //obj.ReleaseExtensionDictionary();
        }

        public static ObjectId GetObjectId(this Database db, long handleValue)
        {
            Handle handle = new Handle(handleValue);
            ObjectId id = db.GetObjectId(false, handle, 0);
            return id;
        }

        #endregion

        #region <Update>

        //Update BlockAttribute by BlockName
        public static void UpdateBlockAttributeByName(this Database db, string tBlockName, string tAttrName,
            string tNewValue)
        {
            try
            {
                using (var tr = db.TransactionManager.StartTransaction())
                {
                    var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                    if (bt.Has(tBlockName))
                    {
                        var btr = (BlockTableRecord)bt[tBlockName].GetObject(OpenMode.ForRead);
                        foreach (ObjectId objId in btr.GetBlockReferenceIds(true, true))
                        {
                            //var br = (BlockReference)Tr.GetObject(objId, OpenMode.ForRead);
                            db.UpdateBlockAttributeByObjectId(objId, tAttrName, tNewValue);
                        }
                    }
                    else
                    {
                        acadApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("Block not found!");
                    }
                    tr.Commit();
                }
                //For Each ObjId As ObjectId In BlkRefs
                //    UpdateAttributesInBlock(ObjId, attbName, attbValue)
                //Next
            }
            catch (Exception ex)
            {
                Plugin.Logger.Error($"{nameof(DbExtensions)}.{nameof(UpdateBlockAttributeByName)} error: ", ex);
            }
        } //UpdateBlockAttributeByName

        //Update BlockAttribute by BlockId
        private static void UpdateBlockAttributeByObjectId(this Database db, ObjectId tObjId, string tAttrName,
            string tNewValue)
        {
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var br = (BlockReference)tr.GetObject(tObjId, OpenMode.ForRead);
                foreach (ObjectId arId in br.AttributeCollection)
                {
                    var ar = (AttributeReference)tr.GetObject(arId, OpenMode.ForRead);
                    if (ar.Tag.ToUpper() != tAttrName.ToUpper()) continue;

                    using (acadApp.DocumentManager.MdiActiveDocument.LockDocument())
                    {
                        ar.UpgradeOpen();
                        ar.ColorIndex = 1;
                        ar.TextString = tNewValue;
                        ar.DowngradeOpen();
                    }
                }
                tr.Commit();

                acadApp.DocumentManager.MdiActiveDocument.Editor.Regen();
                //Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(Convert.ToChar(13).ToString(CultureInfo.InvariantCulture));
            }
        }

        #endregion

        #region <Actions>

        /// <summary> Action </summary>
        public static void Action(this Database db, ObjectId[] ids, Action<Transaction, ObjectId> action)
        {
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                foreach (ObjectId item in ids)
                    action(tr, item);
                tr.Commit();
            }
        }

        /// <summary> ExtrudePline </summary>
        public static void ExtrudePline(this Database db)
        {
            Document doc = acadApp.DocumentManager.GetDocument(db);

            // Always dispose DocumentLock
            using (doc.LockDocument())
            // Always dispose Transaction
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                // No need to dispose a DBObject opened from a transaction
                BlockTableRecord currentSpace =
                    (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);

                // Always dispose a new DBObject which may not be added to the database
                using (Polyline pline = new Polyline())
                {
                    pline.AddVertexAt(0, Point2d.Origin, 0.0, 0.0, 0.0);
                    pline.AddVertexAt(1, new Point2d(10.0, 0.0), 0.0, 0.0, 0.0);
                    pline.AddVertexAt(2, new Point2d(10.0, 10.0), 1.0, 0.0, 0.0);
                    pline.AddVertexAt(3, new Point2d(0.0, 10.0), 0.0, 0.0, 0.0);
                    pline.Closed = true;

                    // Dispose DBObjectCollection in case there're some objects left 
                    // for which no managed wrapper have been created
                    using (DBObjectCollection plineCollection = new DBObjectCollection())
                    {
                        plineCollection.Add(pline);

                        // Dispose DBObjectCollection in case there're some objects left 
                        // for which no managed wrapper have been created
                        using (DBObjectCollection regionCollection =
                            Region.CreateFromCurves(plineCollection))
                        {
                            // Always dispose an object contained in a DBObjectCollection
                            // for which a managed wrapper is created and isn't added to the database
                            using (Region region = (Region)regionCollection[0])
                            {
                                // Use Dispose to insure the new DBObject will be disposed 
                                // if an exception occurs before it is added to the Database
                                using (Solid3d solid = new Solid3d())
                                {
                                    solid.Extrude(region, 30.0, 0.0);
                                    currentSpace.AppendEntity(solid);
                                    tr.AddNewlyCreatedDBObject(solid, true);
                                }
                            }
                        }
                    }
                    if ((short)acadApp.GetSystemVariable("DELOBJ") == 0)
                    {
                        currentSpace.AppendEntity(pline);
                        tr.AddNewlyCreatedDBObject(pline, true);
                    }
                }
                tr.Commit();
            }
        }

        /// <summary> CreateGroup </summary>
        public static DBObject CreateGroup(this Database db, PaletteElement element, ObjectId[] ids)
        {
            var groupName = db.GroupNameExist(element.Title)
                ? element.Title + "(" + DateTime.Now.ToString().GetHashCode().ToString("x") + ")"
                : element.Title;

            return db.CreateGroup(groupName, element.ElementName + "-" + element.Title, ids);
        }

        public static bool GroupNameExist(this Database db, string groupName)
        {
            using (var tr = db.TransactionManager.StartTransaction())
            {
                DBDictionary gd = (DBDictionary)tr.GetObject(db.GroupDictionaryId, OpenMode.ForRead);
                return gd.Contains(groupName);
            }
        }

        /// <summary> CreateGroup </summary>
        public static DBObject CreateGroup(this Database db, string groupName, string groupDescription, ObjectId[] ids)
        {
            Group grp = new Group(groupDescription, false);

            using (var tr = db.TransactionManager.StartTransaction())
            {
                DBDictionary gd = (DBDictionary)tr.GetObject(db.GroupDictionaryId, OpenMode.ForRead);
                gd.UpgradeOpen();
                ObjectId grpId = gd.SetAt(groupName, grp);
                tr.AddNewlyCreatedDBObject(grp, true);

                grp.InsertAt(0, new ObjectIdCollection(ids));
            }

            Document doc = acadApp.DocumentManager.GetDocument(db);
            doc.Editor.WriteMessage("\nCreated group named \"{0}\" containing {1} entities.", groupName, ids.Length);

            return grp;
        }

        //LoadDataFromDwg
        public static bool LoadDataFromDwg(this Database db, string tBlockName = "")
        {
            //Handles cb_blk_attrObj1.LoadAttrFromDwg_Event
            var blockRefs = new List<ObjectId>();
            try
            {
                using (var tr = db.TransactionManager.StartTransaction())
                {
                    var pntXcoll = new List<double>();
                    var pntYcoll = new List<double>();
                    var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);

                    if (bt.Has(tBlockName))
                    {
                        var btr = (BlockTableRecord)bt[tBlockName].GetObject(OpenMode.ForRead);
                        blockRefs.Clear();
                        foreach (ObjectId objId in btr.GetBlockReferenceIds(true, true))
                        {
                            blockRefs.Add(objId);

                            var br = (BlockReference)tr.GetObject(objId, OpenMode.ForRead);

                            if (btr.HasAttributeDefinitions) // & FlgAttrRead
                            {
                                //AttributeReference
                                foreach (ObjectId arId in br.AttributeCollection)
                                {
                                    var obj = tr.GetObject(arId, OpenMode.ForRead);
                                    var ar = (AttributeReference)obj;
                                    //Add(ar.Tag.ToString().Trim(), ar.TextString.ToString().Trim(), obj.ObjectId);
                                }
                                //FlgAttrRead = false;
                            }
                            pntXcoll.Add(br.Position.X);
                            pntYcoll.Add(br.Position.Y);
                        }

                        //isBlockFound = True
                    }
                    else
                    {
                        acadApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("LoadDataFromDwg : Block not found! Loaded Data default");
                        //LoadDataDefault();
                        //isBlockFound = False
                        return false;
                    }
                    tr.Commit();
                }

            }
            catch (Exception ex)
            {
                Plugin.Logger.Error($"{nameof(DbExtensions)}.{nameof(LoadDataFromDwg)} error: ", ex);
            }
            return false;
        }

        public static void XAddDBEvents()
        {
            // get the current working database
            Database curDwg = acadApp.DocumentManager.MdiActiveDocument.Database;

            // add a handlers for what we need
            curDwg.ObjectAppended += new ObjectEventHandler(callback_ObjectAppended);
            curDwg.ObjectErased += new ObjectErasedEventHandler(callback_ObjectErased);
            curDwg.ObjectReappended += new ObjectEventHandler(callback_ObjectReappended);
            curDwg.ObjectUnappended += new ObjectEventHandler(callback_ObjectUnappended);
        }

        private static void callback_ObjectAppended(object sender, ObjectEventArgs e)
        {

            // add the class name of the object to the tree view
            //System.Windows.Forms.TreeNode newNode = myPalette.treeView1.Nodes.Add(e.DBObject.GetType().ToString());

            // we need to record its id for recognition later
            //newNode.Tag = e.DBObject.ObjectId.ToString();
        }

        private static void callback_ObjectErased(object sender, ObjectErasedEventArgs e)
        {

            // if the object was erased
            if (e.Erased)
            {

                // find the object in the treeview control so we can remove it
                //foreach (System.Windows.Forms.TreeNode node in myPalette.treeView1.Nodes)
                //{
                //    // is this the one we want
                //    if (node.Tag.ToString() == e.DBObject.ObjectId.ToString())
                //    {
                //        node.Remove();
                //        break;
                //    }
                //}
            }
            else
            {
                // if the object was unerased
                // add the class name of the object to the tree view
                //System.Windows.Forms.TreeNode newNode = myPalette.treeView1.Nodes.Add(e.DBObject.GetType().ToString());

                //// we need to record its id for recognition later
                //newNode.Tag = e.DBObject.ObjectId.ToString();
            }
        }

        private static void callback_ObjectReappended(object sender, ObjectEventArgs e)
        {

            //// add the class name of the object to the tree view
            //System.Windows.Forms.TreeNode newNode = myPalette.treeView1.Nodes.Add(e.DBObject.GetType().ToString());

            //// we need to record its id for recognition later
            //newNode.Tag = e.DBObject.ObjectId.ToString();
        }

        private static void callback_ObjectUnappended(object sender, ObjectEventArgs e)
        {

            // find the object in the treeview control so we can remove it 
            //foreach (System.Windows.Forms.TreeNode node in myPalette.treeView1.Nodes)
            //{
            //    // is this the one we want
            //    if (node.Tag.ToString() == e.DBObject.ObjectId.ToString())
            //    {
            //        node.Remove();
            //        break;
            //    }
            //}
        }

        #endregion

        #region <private methods>

        private static bool IsPrimary(ObjectId id, out BlockTableRecord owner)
        {
            BlockReference br = (BlockReference)id.GetObject(OpenMode.ForRead);
            owner = (BlockTableRecord)br.OwnerId.GetObject(OpenMode.ForRead);
            return owner.IsLayout;
        }

        private static void GetNestedReferences(ObjectId curId, ObjectId refId, ref ObjectIdCollection result)
        {
            BlockTableRecord owner;
            if (IsPrimary(curId, out owner))
                result.Add(refId);
            else
                foreach (ObjectId id in owner.GetBlockReferenceIds(true, false))
                    GetNestedReferences(id, refId, ref result);
        }

        #endregion

        #region <Ucs-Axis>

        /// <summary>
        /// Figure out the current UCS matrix for the given database.  If
        /// PaperSpace is active, it will return the UCS for PaperSpace.
        /// Otherwise, it will return the UCS for the current viewport in 
        /// ModelSpace.
        /// </summary>
        /// <returns>UCS Matrix for the specified database</returns>

        public static Matrix3d GetUcsMatrix(this Database db)
        {
            Debug.Assert(db != null);

            Point3d origin;
            Vector3d xAxis, yAxis, zAxis;

            if (IsPaperSpace(db))
            {
                origin = db.Pucsorg;
                xAxis = db.Pucsxdir;
                yAxis = db.Pucsydir;
            }
            else
            {
                origin = db.Ucsorg;
                xAxis = db.Ucsxdir;
                yAxis = db.Ucsydir;
            }

            zAxis = xAxis.CrossProduct(yAxis);

            return Matrix3d.AlignCoordinateSystem(Ge.kOrigin, Ge.kXAxis, Ge.kYAxis, Ge.kZAxis, origin, xAxis, yAxis, zAxis);
        }

        /// <summary> Get the UCS Z Axis for the given database </summary>
        public static Vector3d GetUcsZAxis(this Database db)
        {
            Matrix3d m = GetUcsMatrix(db);

            return m.CoordinateSystem3d.Zaxis;
        }

        /// <summary> GetUcsXAxis </summary>
        public static Vector3d GetUcsXAxis(this Database db)
        {
            Matrix3d m = GetUcsMatrix(db);
            return m.CoordinateSystem3d.Xaxis;
        }

        /// <summary> GetUcsYAxis </summary>
        public static Vector3d GetUcsYAxis(this Database db)
        {
            Matrix3d m = GetUcsMatrix(db);
            return m.CoordinateSystem3d.Yaxis;
        }

        /// <summary> Get the Plane that is defined by the current UCS </summary>
        public static Plane GetUcsPlane(this Database db)
        {
            Matrix3d m = GetUcsMatrix(db);
            CoordinateSystem3d coordSys = m.CoordinateSystem3d;
            return new Plane(coordSys.Origin, coordSys.Xaxis, coordSys.Yaxis);
        }

        /// <summary>
        /// Get the Matrix that is the Xform between UCS and WCS Origin.  This is useful
        /// for operations like creating a block definition.  For those cases you want the
        /// origin of the block to be in a reasonable spot.
        /// </summary>
        /// <param name="wcsBasePt">Base point to use as the origin</param>
        /// <param name="db">Specific database to use</param>
        /// <returns>Xform between UCS and WCS Origin</returns>

        public static Matrix3d GetUcsToWcsOriginMatrix(this Database db, Point3d wcsBasePt)
        {
            Matrix3d m = GetUcsMatrix(db);
            Point3d origin = m.CoordinateSystem3d.Origin;
            origin += wcsBasePt.GetAsVector();
            m = Matrix3d.AlignCoordinateSystem(origin,
                m.CoordinateSystem3d.Xaxis,
                m.CoordinateSystem3d.Yaxis,
                m.CoordinateSystem3d.Zaxis,
                Ge.kOrigin, Ge.kXAxis, Ge.kYAxis, Ge.kZAxis);
            return m;
        }

        /// <summary>
        /// Get the X-Axis relative to an entities ECS (In other words, what it considers the
        /// X-Axis.  This is crucial for Entities like Dimensions and DBPoints.  The X-Axis is
        /// determined by the Arbitrary Axis algorithm.
        /// </summary>
        /// <param name="ecsZAxis">The normal vector of the entity</param>
        /// <returns>The X-Axis for this ECS</returns>

        public static Vector3d GetEcsXAxis(this Database db, Vector3d ecsZAxis)
        {
            Matrix3d arbMat = GeoExtensions.GetEcsToWcsMatrix(Ge.kOrigin, ecsZAxis);
            return arbMat.CoordinateSystem3d.Xaxis;
        }

        /// <summary> Transform an Entity from UCS to WCS </summary>
        /// <param name="ent">Entity to transform</param>
        /// <param name="db">Database the entity belongs to (or will belong to)</param>

        public static void TransformToWcs(this Database db, Entity ent)
        {
            Debug.Assert(ent != null);
            Debug.Assert(db != null);
            Debug.Assert(ent.IsWriteEnabled);

            Matrix3d m = GetUcsMatrix(db);
            ent.TransformBy(m);
        }

        /// <summary>
        /// Transform a collection of Entities from UCS to WCS
        /// </summary>
        /// <param name="ents">Entities to transform</param>
        /// <param name="db">Database the entities belong to (or will belong to)</param>

        public static void TransformToWcs(this Database db, DBObjectCollection ents)
        {
            Debug.Assert(ents != null);
            Debug.Assert(db != null);
            Matrix3d m = GetUcsMatrix(db);

            foreach (Entity tmpEnt in ents)
            {
                Debug.Assert(tmpEnt.IsWriteEnabled);
                tmpEnt.TransformBy(m);
            }
        }

        #endregion <Ucs-Axis>

        #region <others methods>

        public static void ExplodeBlock(this Database db, string blockName)
        {
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead, false, true);
                if (bt.Has(blockName))
                {
                    var btrid = bt[blockName];
                    if (!btrid.IsEffectivelyErased)
                    {
                        var btr = (BlockTableRecord)tr.GetObject(btrid, OpenMode.ForRead, false, true);

                        var brefIds = btr.GetBlockReferenceIds(true, false);
                        var oids = new ObjectId[brefIds.Count];
                        brefIds.CopyTo(oids, 0);
                        foreach (ObjectId objid in brefIds)
                        {
                            var br = (BlockReference)tr.GetObject(objid, OpenMode.ForWrite);
                            br.ExplodeToOwnerSpace();
                        }
                    }
                }
            }
        }

        /// <summary> Is Paper Space active in the given database? </summary>
        public static bool IsPaperSpace(this Database db)
        {
            if (db.TileMode) return false;

            Editor ed = acadApp.DocumentManager.MdiActiveDocument.Editor;
            if (db.PaperSpaceVportId == ed.CurrentViewportObjectId)
                return true;

            return false;
        }

        public static bool IsModelSpace(this Database db, bool isDisplayAlert = true)
        {
            if (!db.TileMode) return true;

            Editor ed = acadApp.DocumentManager.MdiActiveDocument.Editor;
            if (db.PaperSpaceVportId != ed.CurrentViewportObjectId)
                return true;

            if (isDisplayAlert)
                acadApp.ShowAlertDialog("Command working in ModelSpace only!");

            return false;
        }



        public static void CloneAndXformObjects(this Database db, ObjectIdCollection entsToClone,
                    ObjectId ownerBlockId, Matrix3d xformMat)
        {
            using (TransactionHelper trHlp = new TransactionHelper(db))
            {
                trHlp.Start();

                IdMapping idMap = new IdMapping();
                db.DeepCloneObjects(entsToClone, ownerBlockId, idMap, false);

                // walk through all the cloned objects and Xform any of the entities
                foreach (IdPair idpair in idMap)
                {
                    if (idpair.IsCloned)
                    {
                        DBObject clonedObj = trHlp.Transaction.GetObject(idpair.Value, OpenMode.ForWrite);
                        Entity clonedEnt = clonedObj as Entity;
                        if (clonedEnt != null)
                            clonedEnt.TransformBy(xformMat);
                    }
                }

                trHlp.Commit();
            }
        }

        public static void MakePointEnt(this Database db, Point3d pt, int colorIndex, Transaction tr)
        {
            short mode = (short)acadApp.GetSystemVariable("pdmode");
            if (mode == 0)
                acadApp.SetSystemVariable("pdmode", 99);

            using (DBPoint dbPt = new DBPoint(pt))
            {
                dbPt.ColorIndex = colorIndex;
                SymTbl.AddToCurrentSpace(dbPt, db, tr);
            }
        }

        public static void MakeRayEnt(this Database db, Point3d basePt, Vector3d unitDir, int colorIndex, Transaction tr)
        {
            if (unitDir.IsZeroLength())
            {
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
                ed.WriteMessage("\nSkipping zero length vector (colorIndex = {0:d})", colorIndex);
                return;
            }

            using (Ray ray = new Ray())
            {
                ray.ColorIndex = colorIndex;
                ray.BasePoint = basePt;
                ray.UnitDir = unitDir;
                SymTbl.AddToCurrentSpace(ray, db, tr);
            }
        }

        public static void MakeExtentsBlock(this Database db, BoundBlock3d ext, int colorIndex)
        {
            Point3d minPt = ext.GetMinimumPoint();
            Point3d maxPt = ext.GetMaximumPoint();

            double deltaX = Math.Abs(maxPt.X - minPt.X);
            double deltaY = Math.Abs(maxPt.Y - minPt.Y);
            double deltaZ = Math.Abs(maxPt.Z - minPt.Z);

            Point3d[] pts = new Point3d[8];

            pts[0] = minPt;
            pts[6] = maxPt;

            // make bottom face
            pts[1] = new Point3d(pts[0].X + deltaX, pts[0].Y, pts[0].Z);
            pts[2] = new Point3d(pts[1].X, pts[1].Y + deltaY, pts[1].Z);
            pts[3] = new Point3d(pts[0].X, pts[0].Y + deltaY, pts[0].Z);

            // project up by Z
            pts[4] = new Point3d(pts[0].X, pts[0].Y, pts[0].Z + deltaZ);
            pts[5] = new Point3d(pts[1].X, pts[1].Y, pts[1].Z + deltaZ);
            pts[7] = new Point3d(pts[3].X, pts[3].Y, pts[3].Z + deltaZ);

            Vector3d offset = minPt.GetAsVector();

            // move points so that they are centered at WCS origin
            // for block creation.  Express everything in WCS since
            // that is what Entity.Extents works in.
            for (int i = 0; i < pts.Length; i++)
            {
                pts[i] -= offset;
            }

            DBObjectCollection faceEnts = new DBObjectCollection();
            faceEnts.Add(new Face(pts[0], pts[1], pts[2], pts[3], true, true, true, true));  // bottom face
            faceEnts.Add(new Face(pts[4], pts[5], pts[6], pts[7], true, true, true, true));  // top face
            faceEnts.Add(new Face(pts[0], pts[1], pts[5], pts[4], true, true, true, true));  // front face
            faceEnts.Add(new Face(pts[1], pts[2], pts[6], pts[5], true, true, true, true));  // right side face
            faceEnts.Add(new Face(pts[2], pts[3], pts[7], pts[6], true, true, true, true));  // back side face
            faceEnts.Add(new Face(pts[3], pts[0], pts[4], pts[7], true, true, true, true));  // left side face

            CompBldrAnonBlkDef compBldr = new CompBldrAnonBlkDef(db);
            compBldr.Start();

            foreach (Entity ent in faceEnts)
            {
                compBldr.SetToDefaultProps(ent);
                compBldr.AddToDb(ent);
            }

            compBldr.Commit();

            BlockReference blkRef = new BlockReference(minPt, compBldr.BlockDefId);
            blkRef.ColorIndex = colorIndex;
            SymTbl.AddToCurrentSpaceAndClose(blkRef, compBldr.Database);
        }

        /// <summary> Converts a string value to a Handle object. </summary>
        public static Handle StringToHandle(this Database db, string strHandle)
        {
            Handle handle = new Handle();
            try
            {
                Int64 nHandle = Convert.ToInt64(strHandle, 16);
                handle = new Handle(nHandle);
            }
            catch (System.FormatException)
            {
            }
            return handle;
        }

        public static void BinarySave(this Database db, Entity entityToWriteTo, object objToSerialize, string key = "default")
        {
            var doc = acadApp.DocumentManager.GetDocument(db);
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter serializer = new BinaryFormatter();
                serializer.Serialize(stream, objToSerialize); stream.Position = 0;
                ResultBuffer data = new ResultBuffer();

                /*Code to get binary serialization into result buffer*/
                using (Transaction tr = db.TransactionManager.StartTransaction())
                using (DocumentLock docLock = doc.LockDocument())
                {
                    if (!entityToWriteTo.IsWriteEnabled)
                    {
                        entityToWriteTo = tr.GetObject(entityToWriteTo.Id, OpenMode.ForWrite) as Entity;
                    }
                    if (entityToWriteTo.ExtensionDictionary == ObjectId.Null)
                    {
                        entityToWriteTo.CreateExtensionDictionary();
                    }
                    using (DBDictionary dict = tr.GetObject(entityToWriteTo.ExtensionDictionary, OpenMode.ForWrite, false) as DBDictionary)
                    {
                        Xrecord xrec;
                        if (dict.Contains(key))
                        {
                            xrec = tr.GetObject(dict.GetAt(key), OpenMode.ForWrite) as Xrecord;
                            xrec.Data = data;
                        }
                        else
                        {
                            xrec = new Xrecord { Data = data };
                            dict.SetAt(key, xrec);
                            tr.AddNewlyCreatedDBObject(xrec, true);
                        }
                        xrec.Dispose();
                    }
                    tr.Commit();
                }
                data.Dispose();
            }
        }

        public static ObjectIdCollection ImportSymbolTableRecords<T>(
            this Database targetDb, string sourceFile, params string[] recordNames)
            where T : SymbolTable
        {
            using (Database sourceDb = new Database(false, true))
            {
                sourceDb.ReadDwgFile(sourceFile, FileOpenMode.OpenForReadAndAllShare, false, "");
                ObjectId sourceTableId, targetTableId;
                switch (typeof(T).Name)
                {
                    case "BlockTable":
                        sourceTableId = sourceDb.BlockTableId;
                        targetTableId = targetDb.BlockTableId;
                        break;
                    case "DimStyleTable":
                        sourceTableId = sourceDb.DimStyleTableId;
                        targetTableId = targetDb.DimStyleTableId;
                        break;
                    case "LayerTable":
                        sourceTableId = sourceDb.LayerTableId;
                        targetTableId = targetDb.LayerTableId;
                        break;
                    case "LinetypeTable":
                        sourceTableId = sourceDb.LinetypeTableId;
                        targetTableId = targetDb.LinetypeTableId;
                        break;
                    case "RegAppTable":
                        sourceTableId = sourceDb.RegAppTableId;
                        targetTableId = targetDb.RegAppTableId;
                        break;
                    case "TextStyleTable":
                        sourceTableId = sourceDb.TextStyleTableId;
                        targetTableId = targetDb.TextStyleTableId;
                        break;
                    case "UcsTable":
                        sourceTableId = sourceDb.UcsTableId;
                        targetTableId = targetDb.UcsTableId;
                        break;
                    case "ViewTable":
                        sourceTableId = sourceDb.ViewportTableId;
                        targetTableId = targetDb.ViewportTableId;
                        break;
                    case "ViewportTable":
                        sourceTableId = sourceDb.ViewportTableId;
                        targetTableId = targetDb.ViewportTableId;
                        break;
                    default:
                        throw new ArgumentException("Requires a concrete type derived from SymbolTable");
                }

                using (Transaction tr = sourceDb.TransactionManager.StartTransaction())
                {
                    T sourceTable = (T)tr.GetObject(sourceTableId, OpenMode.ForRead);
                    ObjectIdCollection idCol = new ObjectIdCollection();
                    foreach (string name in recordNames)
                    {
                        if (sourceTable.Has(name))
                        {
                            idCol.Add(sourceTable[name]);
                        }
                    }
                    if (idCol.Count == 0)
                        return null;
                    IdMapping idMap = new IdMapping();
                    sourceDb.WblockCloneObjects(
                        idCol, targetTableId, idMap, DuplicateRecordCloning.Replace, false);
                    tr.Commit();
                    ObjectIdCollection retVal = new ObjectIdCollection();
                    foreach (ObjectId id in idCol)
                    {
                        if (idMap[id].IsCloned)
                        {
                            retVal.Add(idMap[id].Value);
                        }
                    }
                    return retVal.Count == 0 ? null : retVal;
                }
            }
        }
        //ObjectIdCollection result =db.ImportSymbolTableRecords<TextStyleTable>(@"F:\gile\Templates\Standard2010.dwt", "Arial_Std", "Romans_Std");

        #endregion
    }
    public class ExpressionParameterReplacer : ExpressionVisitor
    {
        public ExpressionParameterReplacer(IList<ParameterExpression> fromParameters, IList<ParameterExpression> toParameters)
        {
            ParameterReplacements = new Dictionary<ParameterExpression, ParameterExpression>();
            for (int i = 0; i != fromParameters.Count && i != toParameters.Count; i++)
                ParameterReplacements.Add(fromParameters[i], toParameters[i]);
        }
        private IDictionary<ParameterExpression, ParameterExpression> ParameterReplacements
        {
            get;
            set;
        }
        protected override Expression VisitParameter(ParameterExpression node)
        {
            ParameterExpression replacement;
            if (ParameterReplacements.TryGetValue(node, out replacement))
                node = replacement;
            return base.VisitParameter(node);
        }
    }
}


//Sample GetData
//BlockReference[] selectionIds = Db.GetData<BlockReference>((Tr, id) =>
//{
//    if (Tr == null || Tr.IsDisposed || id == ObjectId.Null || !id.IsValid || id.IsErased)
//        return false;

//    Database _db = id.Database;
//    DBObject item = Tr.GetObject(id, OpenMode.ForRead);

//    bool result = item is BlockReference;
//    if (!result) return false;

//    ObjectId ownerId = _db.CurrentSpaceId;
//    Entity entity = item as Entity;

//    if (entity.BlockId != ownerId) return false;

//    BlockReference br = (BlockReference)item;
//    if (br.Position != new Point3d(5.0, 5.0, 0.0))
//    {
//        return true;
//    }
//    return false;
//},
//(Tr, X) => (BlockReference)Tr.GetObject(X, OpenMode.ForRead));