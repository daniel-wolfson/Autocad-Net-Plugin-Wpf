using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using ID.Infrastructure;
using Intellidesk.AcadNet.Common.Core;
using Intellidesk.AcadNet.Common.Enums;
using Intellidesk.AcadNet.Common.Internal;
using Intellidesk.AcadNet.Common.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Exception = System.Exception;

namespace Intellidesk.AcadNet.Common.Extensions
{
    /// <summary> Collections Extensions </summary>
    public static class CollectionsExtensions
    {
        private static Editor Ed => Doc.Editor;
        private static Database Db => Doc.Database;
        private static Document Doc => acadApp.DocumentManager.MdiActiveDocument;

        /// <summary> Cast ObjectId collection to <T> collection </summary>
        public static List<T> XCast<T>(this IEnumerable<ObjectId> ids)
        {
            return ids.Select(id => id.GetObject(OpenMode.ForRead)).Cast<T>().ToList();
        }

        public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
        {
            if (collection == null) throw new ArgumentNullException("collection");

            foreach (var i in items) collection.Add(i);
        }

        public static List<T> XCast<T>(this IEnumerable<ObjectId> ids, Transaction tr)
        {
            return ids.Select(id => tr.GetObject(id, OpenMode.ForRead)).Cast<T>().ToList();
        }

        /// <summary> Parse to ObjectIdCollection </summary>
        public static ObjectIdCollection ParseToIds(List<Entity> objIds)
        {
            if (objIds.Count > 0)
            {
                var arrobjIds = objIds.Select(x => x.ObjectId);
                return new ObjectIdCollection(arrobjIds.ToArray());
            }
            return null;
        }

        /// <summary> Write message To CADConsole </summary>
        public static void XWriteMessageToCadConsole(this string msg)
        {
            acadApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage(msg); //Tools.Ed.WriteMessage(msgList.Last().ToString());
        }

        /// <summary> Convert to Lisp format String  </summary>
        public static string XToLispString(this Point3d tPoint3D)
        {
            return Convert.ToString(tPoint3D.X) + "," + Convert.ToString(tPoint3D.Y) + ",0 ";
        }

        #region "Get objects"

        /// <summary> Get objects of users by Rules, and by Filter of TypeEntity or by Filter of AttributeName </summary>
        public static List<ObjectId> XGetObjects(this List<ObjectId> ids, RuleCollection rules)
        {
            var readArgs = new ActionArguments
            {
                FilterTypesOn = rules.TypeFilterOn,
                FilterAttributesOn = rules.AttributePatternOn
            };

            return ids.XGetObjects(readArgs);
            //return ids.XGetObjects(rules.FilterModeEnable ? rules.XGetSingleFilterTypesOn() : rules.XGetFilterTypesOn(),
            //                       rules.FilterModeEnable ? rules.XGetSingleFilterBlockAttributesOn() : rules.XGetFilterAttributesOn());
        }

        /// <summary> Get the Objects </summary>
        public static List<ObjectId> XGetObjects(this List<ObjectId> ids, Type[] filterTypesOn = null, string[] filterBlockAttributesOn = null, bool isParentType = false)
        {
            var toolReadArgs = new ActionArguments
            {
                FilterTypesOn = filterTypesOn,
                FilterAttributesOn = filterBlockAttributesOn
            };
            return ids.XGetObjects(toolReadArgs);
        }

        /// <summary> XGetObjects </summary>
        public static List<ObjectId> XGetObjects(this List<ObjectId> ids, ActionArguments readArgs)
        {
            var db = acadApp.DocumentManager.MdiActiveDocument.Database;
            using (var tr = db.TransactionManager.StartTransaction())
            {
                if (ids.Count == 0)
                {
                    var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                    var btr = (BlockTableRecord)bt[BlockTableRecord.ModelSpace].GetObject(OpenMode.ForRead);
                    ids = btr.Cast<ObjectId>().ToList();
                }
                if (ids.Count == 0) return ids;

                ids = ids.Where(objId =>
                {
                    var ent = (Entity)tr.GetObject(objId, OpenMode.ForRead);
                    return ent.Visible == readArgs.FilterVisible;
                }).ToList();

                if ((readArgs.FilterTypesOn == null || readArgs.FilterTypesOn.Count == 0 || !readArgs.FilterTypesOn.Contains(typeof(BlockReference)))
                    && readArgs.FilterAttributesOn != null && readArgs.FilterAttributesOn.Length != 0)
                {
                    //Array.Resize(ref readArgs.FilterTypesOn, ((readArgs.FilterTypesOn == null) ? 0 : readArgs.FilterTypesOn.Count) + 1);
                    //readArgs.FilterTypesOn[readArgs.FilterTypesOn.Count - 1] = typeof(BlockReference);
                    if (readArgs.FilterTypesOn != null) readArgs.FilterTypesOn.Add(typeof(BlockReference));
                }

                if (readArgs.FilterTypesOn != null && readArgs.FilterTypesOn.Count != 0)
                {
                    var rxClasses = readArgs.FilterTypesOn.Select(RXObject.GetClass).ToList();
                    ids = ids.Where(x => rxClasses.Contains(readArgs.IsParentFilterTypes ? x.ObjectClass.MyParent : x.ObjectClass)).ToList();
                }

                if (readArgs.FilterAttributesOn != null && readArgs.FilterAttributesOn.Length != 0)
                {
                    var containers = ids.Select(x => (BlockReference)tr.GetObject(x, OpenMode.ForRead))
                        .Where(x => x.Visible == readArgs.FilterVisible && !x.XContainsAttributeTags(tr, readArgs.FilterAttributesOn))
                        .ToList();

                    foreach (var br in containers)
                    {
                        var objectIds = br.XGetObjects(tr, new[] { typeof(BlockReference) });
                        if (objectIds.Any())
                        {
                            objectIds = objectIds.Where(x => x.XContainsAttributeTags(tr, readArgs.FilterAttributesOn)).ToList();
                            ids.AddRange(objectIds);
                        }
                    }

                    //containers.ForEach(br => ids.AddRange(br.XGetObjects(Tr)
                    //                              .XGetObjects(new[] { typeof(BlockReference) })
                    //                              .Where(X => X.XContainsAttributeTags(Tr, readArgs.FilterBlockAttributesOn))
                    //                              .ToList())); //.XAddXData(br)

                    containers.ForEach(br => ids.Remove(br.ObjectId));

                    //var test = ids.Select(X => (BlockReference)X.GetObject(OpenMode.ForRead)).ToList();
                    //containers.ForEach(br => ids.XAddXData(((BlockReference)br.GetObject(OpenMode.ForRead)).Position));
                }
                tr.Commit();
            }
            return ids;
        }

        /// <summary> Dynamic Get the Objects </summary>
        public static List<dynamic> XGetObjectsDynamic(this List<dynamic> ids, Type[] filterTypesOn = null, string[] filterBlockAttributesOn = null, bool isParentType = false)
        {
            var toolReadArgs = new ActionArguments
            {
                FilterTypesOn = filterTypesOn,
                FilterAttributesOn = filterBlockAttributesOn
            };
            return ids.XGetObjectsDynamic(toolReadArgs);
        }

        /// <summary> XGetObjects </summary>
        public static List<dynamic> XGetObjectsDynamic(this List<dynamic> ids, ActionArguments readArgs)
        {
            var db = acadApp.DocumentManager.MdiActiveDocument.Database;
            if (!ids.Any())
            {
                //var bt = (BlockTable)(Tr.GetObject(Db.BlockTableId, OpenMode.ForRead));
                //var btr = (BlockTableRecord)bt[BlockTableRecord.ModelSpace].GetObject(OpenMode.ForRead);
                dynamic btr = acadApp.DocumentManager.GetCurrentSpaceId(db);
                ids = ((IEnumerable<dynamic>)btr).ToList();
            }

            if (!ids.Any()) return ids;

            if ((readArgs.FilterTypesOn == null || readArgs.FilterTypesOn.Count == 0 || !readArgs.FilterTypesOn.Contains(typeof(BlockReference)))
                && readArgs.FilterAttributesOn != null && readArgs.FilterAttributesOn.Length != 0)
            {
                //Array.Resize(ref readArgs.FilterTypesOn, ((readArgs.FilterTypesOn == null) ? 0 : readArgs.FilterTypesOn.Count) + 1);
                //readArgs.FilterTypesOn[readArgs.FilterTypesOn.Count - 1] = typeof(BlockReference);
                if (readArgs.FilterTypesOn != null) readArgs.FilterTypesOn.Add(typeof(BlockReference));
            }

            if (readArgs.FilterTypesOn != null && readArgs.FilterTypesOn.Count != 0)
            {
                var rxClasses = readArgs.FilterTypesOn.Select(RXObject.GetClass).ToList();
                ids = ids.Where(x => rxClasses.Contains(readArgs.IsParentFilterTypes ? x.ObjectClass.MyParent : x.ObjectClass)).ToList();
            }

            if (readArgs.FilterAttributesOn != null && readArgs.FilterAttributesOn.Length != 0)
            {
                var containers = ids.Select(x => x.GetObject(OpenMode.ForRead) as BlockReference)
                    .Where(x => !x.DynamicXContainsAttributeTags(readArgs.FilterAttributesOn))
                    .ToList();

                //var localIds = ids.ToList();

                containers.ForEach(br => ids.AddRange(br.XGetObjectsDynamic()
                    .XGetObjectsDynamic(new[] { typeof(BlockReference) })
                    .Where(x => ((ObjectId)x).DynamicXContainsAttributeTags(readArgs.FilterAttributesOn))
                    .ToList() //.XAddXData(br)
                    ));
                containers.ForEach(br => ids.ToList().Remove(br.ObjectId));
            }
            return ids;
        }

        /// <summary> Erasing Objects 
        /// <returns>, returning count erased objects </returns>
        /// </summary>
        public static int XEraseObjects(this List<ObjectId> ids)
        {
            var retValue = 0;
            try
            {
                var doc = acadApp.DocumentManager.MdiActiveDocument;
                using (doc.LockDocument())
                {
                    using (var tr = doc.Database.TransactionManager.StartTransaction())
                    {
                        foreach (var id in ids)
                        {
                            var obj = id.GetObject(OpenMode.ForWrite);
                            obj.Erase();
                        }
                        tr.Commit();
                        retValue = ids.Count();
                    }
                }
            }
            catch (Exception)
            {
                retValue = 0;
            }

            return retValue;
        }

        public static IEnumerable<Entity> XGetEntities(this ObjectId[] ids, params string[] typeFilter)
        {
            var ents = new List<Entity>();
            if (ids != null && ids.Length > 0)
            {
                using (var tr = Db.TransactionManager.StartTransaction())
                {
                    foreach (var objId in ids)
                    {
                        var ent = (Entity)tr.GetObject(objId, OpenMode.ForRead);
                        if (typeFilter.Length == 0 || typeFilter.Length > 0 && new List<string>(typeFilter).Contains(ent.GetType().Name))
                        {
                            ents.Add(ent);
                        }
                    }
                }
            }
            return ents;
        }

        #endregion

        #region "Nested objects"

        /// <summary> Drop nested polylines </summary>
        public static void XDropByNesting(this List<Polyline> tObjects, ListSortDirection sortDirection = ListSortDirection.Ascending)
        {
            if (tObjects.Count <= 1) return;
            tObjects.Sort((p1, p2) =>
            {
                var result1 = false;
                var result2 = false;
                if (p2.Bounds != null && p1.Bounds != null)
                {
                    result1 = Math.Abs(p1.Bounds.Value.MinPoint.X) <= Math.Abs(p2.Bounds.Value.MinPoint.X) &&
                              Math.Abs(p1.Bounds.Value.MinPoint.Y) <= Math.Abs(p2.Bounds.Value.MinPoint.Y);

                    result2 = Math.Abs(p1.Bounds.Value.MaxPoint.X) <= Math.Abs(p2.Bounds.Value.MaxPoint.X) &&
                              Math.Abs(p1.Bounds.Value.MaxPoint.Y) <= Math.Abs(p2.Bounds.Value.MaxPoint.Y);
                }
                if (result1 && result2)
                {
                    p1.Erase();
                }
                return sortDirection == ListSortDirection.Ascending ? result1.CompareTo(result2) : result2.CompareTo(result1);
            });
            //tObjContour.Sort((p1, p2) => (p1.XGetBasePoint() - P2.XGetBasePoint()).Length <= Tolerance.Global.EqualPoint);
        }

        /// <summary> Erase nested elements for each polyline of List of Polyline </summary>
        public static List<Entity> XForEachEraseNested(this List<Entity> tObjSet)
        {
            IEnumerator<Entity> iterator = tObjSet.GetEnumerator();
            while (iterator.MoveNext())
            {
                var objectSetForErase = tObjSet.XGetNestedFor(iterator.Current);
                if (objectSetForErase.Count > 0)
                {
                    objectSetForErase.ForEach(plnErase =>
                    {
                        tObjSet.Remove(plnErase);
                        plnErase.XErase();
                    });
                    iterator = tObjSet.GetEnumerator();
                    iterator.MoveNext();
                }
            }
            return tObjSet;
        }

        /// <summary> Erase nested elements for each polyline of List Point3dCollection </summary>
        public static List<Point3dCollection> XForEachEraseNested(this List<Point3dCollection> tObjSet)
        {
            IEnumerator<Point3dCollection> iterator = tObjSet.GetEnumerator();
            while (iterator.MoveNext())
            {
                var objectSetForErase = tObjSet.XGetNestedFor(iterator.Current);
                if (objectSetForErase.Count > 0)
                {
                    objectSetForErase.ForEach(plnErase => tObjSet.Remove(plnErase));

                    iterator = tObjSet.GetEnumerator();
                    iterator.MoveNext();
                }
            }
            return tObjSet;
        }

        ///<summary> Get all nested objets into Countour from collection (tObjSet) </summary>
        public static List<Point3dCollection> XGetNestedFor(this List<Point3dCollection> tObjects, Point3dCollection extCountour)
        {
            var nestedObjects = new List<Point3dCollection>();
            if (tObjects.Count <= 0) return nestedObjects;

            foreach (var testObj in tObjects)
            {
                if (!testObj.Equals(extCountour)) //((new Polyline().XInit(extCountour).Area - new Polyline().XInit(testObj).Area > 0) ||
                {
                    if (testObj.XIsPointsInside(extCountour)) nestedObjects.Add(testObj);
                }
            }
            return nestedObjects;
        }

        #endregion

        #region "Groups"

        /// <summary> Add to Group </summary>
        public static Group XToGroup(this List<Entity> tObjSet, Transaction tr, string tGroupName = "NONAME")
        {
            var grp = new Group(); //tGroupName, true
            //using (Tools.Doc.LockDocument())
            var gd = (DBDictionary)tr.GetObject(Db.GroupDictionaryId, OpenMode.ForRead);
            if (gd.Contains(tGroupName))
            {
                Ed.WriteMessage("\nA group with this name already exists.");
                return null;
            }

            gd.UpgradeOpen();
            gd.SetAt(tGroupName, grp);
            tr.AddNewlyCreatedDBObject(grp, true);

            var ids = new ObjectIdCollection(tObjSet.Select(obj => obj.ObjectId).ToArray());
            grp.InsertAt(0, ids);

            Plugin.Logger.Information($"{nameof(CollectionsExtensions)}.{nameof(XToGroup)}", "ok", "Created group named \"{0}\" containing {1} entities.", tGroupName, tObjSet.Count);
            return grp;
        }

        /// <summary> Add List of ObjectId into group </summary>
        public static Group XToGroup(this List<ObjectId> objIds, Transaction tr, string tGroupName = "NONAME")
        {
            Group grp;
            var gd = (DBDictionary)tr.GetObject(Db.GroupDictionaryId, OpenMode.ForRead);
            if (!gd.Contains(tGroupName))
            {
                grp = new Group(tGroupName, false);
                gd.UpgradeOpen();
                gd.SetAt(tGroupName, grp);
                tr.AddNewlyCreatedDBObject(grp, true);
                var ids = new ObjectIdCollection(objIds.ToArray());
                grp.InsertAt(0, ids);
            }
            else
            {
                grp = (Group)tr.GetObject(gd.GetAt(tGroupName), OpenMode.ForRead);
                grp.UpgradeOpen();
                gd.SetAt(tGroupName, grp);
                var ids = new ObjectIdCollection(objIds.ToArray());
                grp.Append(ids);
            }

            Plugin.Logger.Information($"{nameof(CollectionsExtensions)}.{nameof(XToGroup)}", "ok", "Created group named \"{0}\" containing {1} entities.", tGroupName, objIds.Count);
            return grp;
        }

        /// <summary> Add objectIdCollection into group </summary>
        public static Group XAddGroup(this Group grp, Transaction tr, ObjectIdCollection ids)
        {
            try
            {
                //using (var Tr = AcadToolsManager.Db.TransactionManager.StartTransaction())
                using (Doc.LockDocument())
                {
                    var gd = (DBDictionary)tr.GetObject(Db.GroupDictionaryId, OpenMode.ForRead);
                    gd.UpgradeOpen();
                    gd.SetAt(grp.Description, grp);
                    grp.InsertAt(0, ids);
                    tr.AddNewlyCreatedDBObject(grp, true);
                }
                //Log.Add("ok", "Created group named \"{0}\" containing {1} entities.", grp.Description, ids.Count);
            }
            catch (Exception ex)
            {
                Plugin.Logger.Error($"{nameof(CollectionsExtensions)}.{nameof(XAddGroup)} Progress started", ex);
            }
            return grp;
        }

        /// <summary> Add objectIdCollection into group </summary>
        public static Group XAddGroupDynamic(this Group grp, ObjectIdCollection ids)
        {
            try
            {
                //using (var Tr = AcadToolsManager.Db.TransactionManager.StartTransaction())
                using (Doc.LockDocument())
                {
                    var gd = (DBDictionary)Db.GroupDictionaryId.GetObject(OpenMode.ForRead);
                    gd.UpgradeOpen();
                    gd.SetAt(grp.Description, grp);
                    grp.InsertAt(0, ids);

                    using (Doc.LockDocument())
                    {
                        using (var tr = Db.TransactionManager.StartTransaction())
                        {
                            tr.AddNewlyCreatedDBObject(grp, true);
                        }
                    }
                }
                //Log.Add("ok", "Created group named \"{0}\" containing {1} entities.", grp.Description, ids.Count);
            }
            catch (Exception ex)
            {
                Plugin.Logger.Error($"{nameof(CollectionsExtensions)}.{nameof(XAddGroupDynamic)}", ex);
            }
            return grp;
        }

        #endregion

        #region "Transformations"

        /// <summary> Transformation: Displacement </summary>
        public static void XMove(this List<ObjectId> ids, Transaction tr, Point3d destPt)
        {
            ids.XUpdate(tr, Matrix3d.Displacement(destPt.GetAsVector()));
        }

        /// <summary> Transformation: Displacement </summary>
        public static void XMoveDynamic(this List<ObjectId> ids, Point3d destPt)
        {
            ids.XUpdateDynamic(Matrix3d.Displacement(destPt.GetAsVector()));
        }

        /// <summary> Transformation: Scaling </summary>
        public static void XScale(this List<ObjectId> ids, Transaction tr, double scalefactor, Point3d centerPt)
        {
            ids.XUpdate(tr, Matrix3d.Scaling(scalefactor, centerPt));
        }

        /// <summary> Dynamic transformation: Scaling </summary>
        public static void XScaleDynamic(this List<ObjectId> ids, double scalefactor, Point3d centerPt)
        {
            ids.XUpdateDynamic(Matrix3d.Scaling(scalefactor, centerPt));
        }

        /// <summary> Transformation: Rotation </summary>
        public static void XRotate(this List<ObjectId> ids, Transaction tr, double angle, Point3d centerPt)
        {
            ids.XUpdate(tr, Matrix3d.Rotation(angle, Ed.CurrentUserCoordinateSystem.CoordinateSystem3d.Zaxis, centerPt));
        }

        /// <summary> Dynamic transformation: Rotation </summary>
        public static void XRotateDynamic(this List<ObjectId> ids, double angle, Point3d centerPt)
        {
            ids.XUpdateDynamic(Matrix3d.Rotation(angle, Ed.CurrentUserCoordinateSystem.CoordinateSystem3d.Zaxis, centerPt));
        }

        /// <summary> Write transformations of entity to Db </summary>
        public static void XUpdate(this List<ObjectId> ids, Transaction tr, Matrix3d mat)
        {
            using (Doc.LockDocument())
            {
                ids.ForEach(x => ((Entity)tr.GetObject(x, OpenMode.ForWrite)).TransformBy(mat));
            }
        }

        /// <summary> Dynamic Write transformations of entity to Db </summary>
        public static void XUpdateDynamic(this List<ObjectId> ids, Matrix3d mat)
        {
            using (Doc.LockDocument())
            {
                using (var tr = Db.TransactionManager.StartTransaction())
                {
                    ids.ForEach(x => ((Entity)tr.GetObject(x, OpenMode.ForWrite)).TransformBy(mat));
                }
            }
        }

        /// <summary> Transformations through commands </summary>
        public static void XTransformCommands(this List<ObjectId> ids, BlockReference br, UpdateOptions options)
        {
            if (ids.Count == 0) return;
            Ed.SelectLast();
            switch (options)
            {
                case UpdateOptions.Scale:
                    Doc.SendStringToExecute("_SCALE " + ids.XGetPoint3DMin().XToLispString() +
                                            Convert.ToString(br.ScaleFactors.X) + " ", true, false, true);
                    break;
                case UpdateOptions.Rotate:
                    Doc.SendStringToExecute("ROTATE L  " + ids.XGetPoint3DMin().XToLispString() +
                                            Convert.ToString(Geoms.RadianToDegree(br.Rotation)) + " ", false, false, true);
                    break;
            }
            //CmdManager.LoadLispCmd("SCALE " + grpIds.XGetPoint3DMin().XToLispString() + " " + Convert.ToString(br.ScaleFactors.X) + " ");
        }

        #endregion

        #region "Sorts"

        /// <summary> Sort By Area </summary>
        public static List<ObjectId> XSortByArea(this List<ObjectId> ids, ListSortDirection sortDirection = ListSortDirection.Ascending)
        {
            if (ids.Count <= 1) return ids;
            ids.Select(obj => obj.GetObject(OpenMode.ForRead)).ToList().Sort((ent1, ent2) =>
            {
                var result1 = ReferenceEquals(ent1.GetType(), typeof(Line)) ? ((Line)ent1).Length : ((Curve)ent1).Area;
                var result2 = ReferenceEquals(ent2.GetType(), typeof(Line)) ? ((Line)ent2).Length : ((Curve)ent2).Area;
                return sortDirection == ListSortDirection.Ascending ? result1.CompareTo(result2) : result2.CompareTo(result1);
            });
            return ids;
        }

        /// <summary> Sort By Area </summary>
        public static List<Entity> XSortByArea(this List<Entity> objects, ListSortDirection sortDirection = ListSortDirection.Ascending)
        {
            if (objects.Count <= 1) return objects;
            objects.Sort((ent1, ent2) =>
            {
                var result1 = ReferenceEquals(ent1.GetType(), typeof(Line)) ? ((Line)ent1).Length : ((Curve)ent1).Area;
                var result2 = ReferenceEquals(ent2.GetType(), typeof(Line)) ? ((Line)ent2).Length : ((Curve)ent2).Area;
                return sortDirection == ListSortDirection.Ascending ? result1.CompareTo(result2) : result2.CompareTo(result1);
            });
            return objects;
        }

        /// <summary> Sort By Area </summary>
        public static List<Polyline> XSortByArea(this List<Polyline> objects, ListSortDirection sortDirection = ListSortDirection.Ascending)
        {
            if (objects.Count <= 1) return objects;
            objects.Sort((p1, p2) => sortDirection == ListSortDirection.Ascending ? p1.Area.CompareTo(p2.Area) : p2.Area.CompareTo(p1.Area));
            return objects;
        }

        /// <summary> Sort By Area </summary>
        public static List<Point3dCollection> XSortByArea(this List<Point3dCollection> objects, ListSortDirection sortDirection = ListSortDirection.Ascending)
        {
            if (objects.Count <= 1) return objects;
            objects.Sort((ent1, ent2) =>
            {
                var result1 = new Polyline().XCreateVertices(ent1).Area;
                var result2 = new Polyline().XCreateVertices(ent2).Area;
                //var result3 =  (new Polyline3d(Poly3dType.SimplePoly, ent2, true)).Area;
                return sortDirection == ListSortDirection.Ascending ? result1.CompareTo(result2) : result2.CompareTo(result1);
            });
            return objects;
        }

        /// <summary> Sort By X </summary>
        public static void XSortByX(this Point3dCollection points, ListSortDirection sortDirection = ListSortDirection.Ascending)
        {
            if (points.Count <= 1) return;
            points.Cast<Point3d>().ToList().Sort((p1, p2) =>
                sortDirection == ListSortDirection.Ascending
                    ? p1.X.CompareTo(p2.X)
                    : p2.X.CompareTo(p1.X));
        }

        /// <summary> Sort By X </summary>
        public static void XSortByX(this List<Point3dCollection> listPoints, ListSortDirection sortDirection = ListSortDirection.Ascending)
        {
            if (listPoints.Count <= 1) return;

            //var raw3D = new Point3d[listPoints.Count];
            //listPoints.CopyTo(raw3D, 0);
            //Array.Sort(raw3D, new sort3dByX());
            //Point3dCollection sorted3d = new Point3dCollection(raw3D);

            var temp = new List<List<Point3d>>();
            listPoints.ForEach(x => temp.Add(new List<Point3d>(x.Cast<Point3d>())));
            temp.ForEach(lst => lst.Sort((p1, p2) =>
                sortDirection == ListSortDirection.Ascending
                    ? p1.X.CompareTo(p2.X)
                    : p2.X.CompareTo(p1.X)));
            listPoints.Clear();
            temp.ForEach(x => listPoints.Add(new Point3dCollection(x.ToArray())));
        }

        internal class Sort3dByX : IComparer<Point3d>
        {
            public static bool IsZero(double a)
            {
                return Math.Abs(a) < Tolerance.Global.EqualPoint;
            }

            public static bool IsEqual(double a, double b)
            {
                return IsZero(b - a);
            }

            public int Compare(Point3d a, Point3d b)
            {
                if (IsEqual(a.X, b.X)) return 0; // ==
                if (a.X < b.X) return -1; // <
                return 1; // >
            }
        }

        #endregion
    }
}