using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using ID.Infrastructure;
using ID.Infrastructure.Enums;
using Intellidesk.AcadNet.Common.Models;
using Intellidesk.Data.Models.Entities;
using Intellidesk.Data.Repositories.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Group = Autodesk.AutoCAD.DatabaseServices.Group;

namespace Intellidesk.AcadNet.Common.Extensions
{
    /// <summary> DBObjectExtensions </summary>
    // ReSharper disable once InconsistentNaming
    public static class DBObjectExtensions
    {
        public static Document Doc => acadApp.DocumentManager.MdiActiveDocument; //acadApp.DocumentManager.GetDocument(db);
        public static Database Db => HostApplicationServices.WorkingDatabase;

        //public static ObjectIdItem GetListItem(this DBObject obj, AcadCable element)
        //{
        //    if (obj is DBText)
        //    {
        //        DBText title = (DBText)obj;
        //        return new ObjectIdItem(title.ObjectId, $"Title: {title.TextString}");
        //    }
        //    if (obj is Polyline)
        //    {
        //        Polyline pline = (Polyline)obj;
        //        return new ObjectIdItem(pline.ObjectId,
        //            $"Cable (vertex: {pline.NumberOfVertices}; length: {pline.Length():F4}, l={pline.Length:F4})");
        //    }
        //    return null;
        //}

        //public static ObjectIdItem GetListItem(this DBObject obj, AcadClosure element)
        //{
        //    if (obj is DBText)
        //    {
        //        DBText title = (DBText)obj;
        //        return new ObjectIdItem(title.ObjectId, $"Title: {title.TextString}");
        //    }
        //    else if (obj is Polyline)
        //    {
        //        Polyline pline = (Polyline)obj;
        //        return new ObjectIdItem(pline.ObjectId, $"Closure (type: {element.TypeName})");
        //    }
        //    else if (obj is Circle)
        //    {
        //        Circle circle = (Circle)obj;
        //        return new ObjectIdItem(circle.ObjectId, $"Cable (circle radius: {circle.Radius.ToString("N:2")})");
        //    }
        //    return null;
        //}

        //public static ObjectIdItem GetListItem(this DBObject obj, AcadTitle element)
        //{
        //    if (obj is DBText)
        //    {
        //        DBText title = (DBText)obj;
        //        return new ObjectIdItem(title.ObjectId, $"Title: {title.TextString}");
        //    }
        //    return null;
        //}

        #region <Get>

        public static ObjectIdItem XGetDisplayItem(this DBObject obj)
        {
            var element = obj.XGetDataObject();
            return obj.XGetDisplayItem(element);
        }

        public static ObjectIdItem XGetDisplayItem(this DBObject obj, IPaletteElement element)
        {
            string bodySubType = "";
            string entityInfo = "";


            var bodyType = Enum.GetName(typeof(eBodyType), element.BodyType);

            if (obj is DBText)
            {
                entityInfo = $" (text: {((DBText)obj).TextString})";
            }
            else if (obj is Polyline && bodyType != eBodyType.Marker.ToString())
            {
                Polyline pline = (Polyline)obj;
                entityInfo = $" (vertex: {pline.NumberOfVertices}; length: {pline.Length:F4})";
            }
            else if (obj is Circle)
            {
                Circle circle = (Circle)obj;
                entityInfo = $" (radius: {circle.Radius})";
            }
            else if (obj is Group)
            {
                Group group = (Group)obj;
                Type type = group.XGetXDataObjectType();

                if (type == typeof(AcadClosure))
                    entityInfo = group.XGetXDataObject<AcadClosure>().Title;
                else if (type == typeof(AcadCabinet))
                    entityInfo = group.XGetXDataObject<AcadCabinet>().Title;
            }

            if (!string.IsNullOrEmpty(element.TypeCodeFullName) && element.TypeCodeFullName == typeof(eOpenCloseType).FullName)
            {
                bodySubType = " " + Enum.GetName(typeof(eOpenCloseType), element.TypeCode).ToLower();
            }

            var displayItem = $"{bodyType}{bodySubType}{entityInfo}";

            return !string.IsNullOrEmpty(displayItem) ? new ObjectIdItem(obj.ObjectId, displayItem) : null;
        }

        public static Task<TResult> XGetDisplayItem<TEntity, TResult>(this TEntity obj,
            CancellationToken token,
            string[] nameParams = null, string[] valueParams = null)
            where TEntity : Entity
            where TResult : ObjectIdItem
        {
            TResult objectIdItem = null;

            if (token.IsCancellationRequested)
                Task.FromResult(objectIdItem);

            if (obj.GetType() == typeof(DBText))
            {
                var entity = obj as DBText;
                if (entity != null)
                    objectIdItem = (TResult)Activator.CreateInstance(typeof(TResult), obj.ObjectId, entity.TextString);
            }
            else if (obj.GetType() == typeof(Curve))
            {
                Polyline entity = obj as Polyline;
                if (entity != null)
                    objectIdItem = Activator.CreateInstance(typeof(TResult), obj.ObjectId,
                        $"Polyline length: {entity.Length:N4}, vertices: {entity.NumberOfVertices}") as TResult;
                Line line = obj as Line;
                if (line != null)
                    objectIdItem = Activator.CreateInstance(typeof(TResult), obj.ObjectId,
                        $"Line length: {Math.Round(line.Length, 4)}") as TResult;
                Circle circle = obj as Circle;
                if (circle != null)
                    objectIdItem = Activator.CreateInstance(typeof(TResult), obj.ObjectId,
                        $"Circle Radius: {Math.Round(circle.Radius, 4)}") as TResult;
            }
            else if (obj.GetType() == typeof(Polyline))
            {
                Polyline entity = obj as Polyline;
                if (entity != null)
                    objectIdItem = Activator.CreateInstance(typeof(TResult), obj.ObjectId,
                        $"Polyline length: {entity.Length:N4}, vertices: {entity.NumberOfVertices}") as TResult;
            }
            else if (obj.GetType() == typeof(Line))
            {
                Line entity = obj as Line;
                if (entity != null)
                    objectIdItem = Activator.CreateInstance(typeof(TResult), obj.ObjectId,
                        $"Line length: {Math.Round(entity.Length, 4)}") as TResult;
            }
            else if (obj.GetType() == typeof(BlockReference))
            {
                Database db = obj.Database;
                BlockReference br = obj as BlockReference;
                if (br != null && !br.IsErased)
                {
                    string attrsString = "";


                    br.ObjectId.XOpenForRead((ent) =>
                    {
                        IEnumerable<AttributeReference> attrList = br.XGetAttributes(nameParams, valueParams).ToList();

                        if (attrList.Any())
                            attrsString = string.Join(", ", attrList.Select(x => x.Tag + "=" + x.TextString).ToArray());

                        var btr = db.XGetObject(br.IsDynamicBlock ? br.DynamicBlockTableRecord : br.BlockTableRecord, OpenMode.ForRead) as BlockTableRecord;

                        objectIdItem = Activator.CreateInstance(typeof(TResult), obj.ObjectId,
                            $"Block {btr?.Name}, {attrsString}") as TResult;

                            //if (typeof(TResult) == typeof(ObjectIdItemAttr))
                            //{
                            //    ObjectIdItemAttr objectIdItemAttr = objectIdItem as ObjectIdItemAttr;
                            //    if (objectIdItemAttr != null)
                            //    {
                            //        objectIdItemAttr.Attrs = new Dictionary<string, string>();
                            //        if (attrTagNames != null)
                            //            foreach (var filterParam in attrTagNames)
                            //            {
                            //                var filter = filterParam == "*" ? ".*" : filterParam;
                            //                var attr = attrList.FirstOrDefault(x => Regex.IsMatch(x.Tag, filter));
                            //                if (attr != null && !objectIdItemAttr.Attrs.ContainsKey(attr.Tag))
                            //                    objectIdItemAttr.Attrs.Add(filterParam, attr.TextString);
                            //            }
                            //        objectIdItem = objectIdItemAttr as TResult;
                            //    }
                            //}
                        });
                }
            }

            if (objectIdItem == null)
                objectIdItem = Activator.CreateInstance(typeof(TResult)) as TResult;

            return Task.FromResult(objectIdItem);
        }

        public static DBText XGetPrototype(this DBText entity)
        {
            return new DBText
            {
                TextString = entity.TextString,
                Position = entity.Position,
                HorizontalMode = entity.HorizontalMode,
                Layer = entity.Layer,
                ColorIndex = entity.ColorIndex,
                Height = entity.Height,
                Rotation = entity.Rotation
            };
        }

        // Evalue si un point est à l'intérieur de la boite (BoundingBox).
        // Evaluates if the point is within the extents (BoundingBox).
        private static bool IsInside(this Point3d pt, Extents3d extents)
        {
            return
                pt.X >= extents.MinPoint.X &&
                pt.Y >= extents.MinPoint.Y &&
                pt.Z >= extents.MinPoint.Z &&
                pt.X <= extents.MaxPoint.X &&
                pt.Y <= extents.MaxPoint.Y &&
                pt.Z <= extents.MaxPoint.Z;
        }

        // Retourne l'objet MText contenu dans la cellule (ou null s'il nexiste pas).
        // Returns the MText object contained in the cell (or null if it don't exist).
        public static MText GetMtext(this Cell cell)
        {
            Table table = cell.ParentTable;
            Extents3d extents = cell.GetExtents().ToExtents3d();
            extents.TransformBy(table.BlockTransform.Inverse());
            BlockTableRecord btr = (BlockTableRecord)table.BlockTableRecord.GetObject(OpenMode.ForRead);
            RXClass mtextClass = RXClass.GetClass(typeof(MText));
            foreach (ObjectId id in btr)
            {
                if (id.ObjectClass == mtextClass)
                {
                    MText mtext = (MText)id.GetObject(OpenMode.ForRead);
                    if (mtext.Location.IsInside(extents))
                        return mtext;
                }
            }
            return null;
        }

        #endregion

        #region <Update>

        /// <summary> Remove entity </summary>
        public static void XErase(this DBObject ent)
        {
            using (Doc.LockDocument())
            using (var tr = Db.TransactionManager.StartTransaction())
            {
                ent = tr.GetObject(ent.ObjectId, OpenMode.ForWrite); //ent.UpgradeOpen();
                ent.Erase();
                tr.Commit();
            }
        }

        public static void XUpgradeOpen(this DBObject dbObject, IPaletteElement elementArgs, ObjectState objectState = ObjectState.Unchanged)
        {
            try
            {
                if (dbObject.GetType() != typeof(DBText) && dbObject.GetType() != typeof(Circle) && dbObject.GetType() != typeof(Polyline))
                    return;

                IPaletteElement element = dbObject.XGetDataObject();
                element.Update(elementArgs, objectState);

                dbObject.ObjectId.XOpenForWrite<Entity>(ent =>
                {
                    if (ent.GetType() == typeof(DBText))
                        ((DBText)ent).TextString = elementArgs.Title;

                    if (elementArgs.ColorIndex.HasValue)
                        ent.Color = Color.FromColorIndex(ColorMethod.ByAci, ((short?)elementArgs.ColorIndex) ?? 0);

                    if (ent.Layer != elementArgs.LayerName)
                        ent.Layer = elementArgs.LayerName;

                    ent.XAddData(element);
                });

                if (element.Items.Length > 0)
                {
                    foreach (var itemHandle in element.Items)
                    {
                        var itemDbObject = Db.XGetObject(itemHandle) as Entity;
                        var itemElement = itemDbObject.XGetDataObject();

                        if (itemElement != null)
                        {
                            itemElement.Update(elementArgs);
                            itemDbObject.XUpgradeOpen(itemElement, objectState);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Plugin.Logger.Error(ex, nameof(XUpgradeOpen));
            }
        }

        private static void XSaveXRecord(this DBObject obj, string key, Transaction trans, ResultBuffer buffer)
        {
            if (obj.ExtensionDictionary == ObjectId.Null)
                obj.CreateExtensionDictionary();

            using (DBDictionary dict = trans.GetObject(obj.ExtensionDictionary, OpenMode.ForWrite, false) as DBDictionary)
            {
                if (dict == null) return;

                // check to see if dictionary contains XRecord
                // if so, update the data - important for Undo Operations
                if (dict.Contains(key))
                {
                    Xrecord xRecord = (Xrecord)trans.GetObject(dict.GetAt(key),
                    OpenMode.ForWrite);
                    xRecord.Data = buffer;
                }
                else
                {
                    Xrecord xRecord = new Xrecord { Data = buffer };
                    dict.SetAt(key, xRecord);
                    trans.AddNewlyCreatedDBObject(xRecord, true);
                }
            }
        }

        // Retourne la boite (BoundingBox) définie par la collection de points.
        // Returns the extents (BoundingBox) defined by the points collection
        private static Extents3d ToExtents3d(this Point3dCollection pts)
        {
            return pts.Cast<Point3d>()
                .Aggregate(new Extents3d(pts[0], pts[0]), (ext, pt) =>
                { ext.AddPoint(pt); return ext; });
        }

        /// <summary>
        /// Returns the transformation matrix from the ViewportTableRecord DCS to WCS.
        /// </summary>
        /// <param name="view">The ViewportTableRecord instance this method applies to.</param>
        /// <returns>The DCS to WCS transformation matrix.</returns>
        public static Matrix3d EyeToWorld(this ViewportTableRecord view)
        {
            return
                Matrix3d.Rotation(-view.ViewTwist, view.ViewDirection, view.Target) *
                Matrix3d.Displacement(view.Target - Point3d.Origin) *
                Matrix3d.PlaneToWorld(view.ViewDirection);
        }

        /// <summary>
        /// Returns the transformation matrix from the ViewportTableRecord WCS to DCS.
        /// </summary>
        /// <param name="view">The ViewportTableRecord instance this method applies to.</param>
        /// <returns>The WCS to DCS transformation matrix.</returns>
        public static Matrix3d WorldToEye(this ViewportTableRecord view)
        {
            return view.EyeToWorld().Inverse();
        }

        #endregion
    }
}