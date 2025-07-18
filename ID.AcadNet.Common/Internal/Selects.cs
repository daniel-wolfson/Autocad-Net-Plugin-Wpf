using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using ID.Infrastructure;
using ID.Infrastructure.Commands;
using Intellidesk.AcadNet.Common.Enums;
using Intellidesk.AcadNet.Common.Extensions;
using Intellidesk.AcadNet.Common.Models;
using Intellidesk.Data.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Polyline = Autodesk.AutoCAD.DatabaseServices.Polyline;

namespace Intellidesk.AcadNet.Common.Internal
{
    public static class Selects
    {
        private static Document Doc => acadApp.DocumentManager.MdiActiveDocument;
        private static Editor Ed => Doc.Editor;
        public static Database Db => Doc.Database;

        private static string _lastHandle = null;
        private static ObjectId _lastSelectAddedObjectId = ObjectId.Null;
        private static ObjectId _toolTipObjectId = ObjectId.Null;
        private static ObjectId _selectedEntity;

        #region <public methods>

        public static ObjectId SelectedEntity
        {
            get { return _selectedEntity; }
            set
            {
                Entity ent;
                if (_selectedEntity != ObjectId.Null)
                {
                    ent = _selectedEntity.XCast<Entity>();
                    if (ent != null) ent.Unhighlight();
                }
                _selectedEntity = value;

                ent = _selectedEntity.XCast<Entity>();
                if (ent != null) ent.Highlight();
            }
        }

        public static int GetCountPrompt(int copies = 1)
        {
            PromptIntegerOptions promptIntOpts = new PromptIntegerOptions($"\n{CommandNames.PlotWindow}. Count copies: ")
            {
                DefaultValue = 1,
                UseDefaultValue = true,
                AllowNone = true,
                AllowZero = false,
                AllowNegative = false,
            };
            PromptIntegerResult promptIntResult = Ed.GetInteger(promptIntOpts);
            if (promptIntResult.Status == PromptStatus.OK)
                copies = promptIntResult.Value;
            return copies;
        }

        //Get new or implied selection
        public static void SelectClear()
        {
            Ed.SetImpliedSelection(new ObjectId[0]);
        }

        // Get entites from Model Space with: GetSelectOptions and SelectFilter
        public static List<Entity> GetEntitiesOld(GetSelectOptions tOptionPolygon, Point3dCollection tPoint3DColl,
            params string[] tSelectFilter)
        {
            //"LWPOLYLINE"
            var listEntityColl = new List<Entity>();
            using (var tr = Db.TransactionManager.StartTransaction())
            {
                PromptSelectionResult psr = null;
                switch (tOptionPolygon)
                {
                    case GetSelectOptions.SelectWindowPolygon:
                        if (tSelectFilter.Length == 0)
                        {
                            psr = Ed.SelectWindowPolygon(tPoint3DColl);
                        }
                        else
                        {
                            var sumFilter = "";
                            foreach (var flt in tSelectFilter)
                            {
                                sumFilter = sumFilter + (!string.IsNullOrEmpty(sumFilter) ? "," : "") + Convert.ToString(flt);
                            }
                            var tvs = new[] { new TypedValue(Convert.ToInt32(DxfCode.Start), sumFilter) };
                            var sf = new SelectionFilter(tvs);
                            psr = Ed.SelectWindowPolygon(tPoint3DColl, sf);
                        }
                        break;
                }
                if (psr != null && psr.Status == PromptStatus.OK)
                {
                    Ed.WriteMessage("\nSelected {0} entities.", psr.Value.Count);
                    foreach (var objId in psr.Value.GetObjectIds())
                    {
                        var ent = (Entity)tr.GetObject(objId, OpenMode.ForRead);
                        ent.Highlight();
                        ent.List();
                        listEntityColl.Add(ent);
                        //ent.Dispose()
                    }
                }
                tr.Commit();
                return listEntityColl;
            }
        }

        // Get entity/entites from Model Space with: GetSelectOptions and Prompt and EntityFilter
        public static Entity GetEntity(string tPrompt = "", Point3dCollection tPolygonPoint3DColl = null, params EntityTypes[] tEntityFilter)
        {
            return GetEntities(GetSelectOptions.GetObject, tPrompt, tPolygonPoint3DColl, tEntityFilter).FirstOrDefault();
        }

        public static IEnumerable<Entity> GetEntities(GetSelectOptions tOption, params EntityTypes[] tEntityFilter)
        {
            return GetEntities(tOption, null, null, tEntityFilter);
        }

        public static IEnumerable<Entity> GetEntities(GetSelectOptions tOption, Point3dCollection tPolygonPoint3DColl, params EntityTypes[] tEntityFilter)
        {
            return GetEntities(tOption, null, tPolygonPoint3DColl, tEntityFilter);
        }

        public static IEnumerable<Entity> GetEntities(GetSelectOptions tOption, string tPrompt = "",
            Point3dCollection tPolygonPoint3DColl = null, params EntityTypes[] tEntityFilter)
        {
            var sumFilter = "";
            using (var tr = Db.TransactionManager.StartTransaction())
            {
                PromptSelectionResult psr = null;
                SelectionFilter sf = null;

                if (tOption == GetSelectOptions.SelectImplied)
                {
                    var objIds = Ed.GetSelectImplied();
                    if (objIds.Any())
                    {
                        var entityFilters = tEntityFilter.Select(x => x.XToString()).ToList();
                        foreach (var objId in objIds)
                        {
                            var ent = tr.GetObject(objId, OpenMode.ForRead) as Entity;
                            if (ent != null
                                && tEntityFilter.Contains(EntityTypes.CURVE)
                                && ent.GetType().BaseType == typeof(Curve)
                                || entityFilters.Contains(ent.GetType().Name.ToUpper()))
                            {
                                yield return ent;
                            }
                        }
                        SelectedEntity = objIds.FirstOrDefault();
                    }
                    else
                    {
                        tOption = GetSelectOptions.GetObjects;
                    }
                }

                var tEntityFilterList = tEntityFilter.ToList();
                if (tEntityFilter.Contains(EntityTypes.CURVE))
                {
                    tEntityFilterList.Remove(EntityTypes.CURVE);
                    tEntityFilterList.AddRange(new[] { EntityTypes.CIRCLE, EntityTypes.LINE, EntityTypes.LWPOLYLINE, EntityTypes.POLYLINE, EntityTypes.POLYLINE2D, EntityTypes.POLYLINE3D });
                    tEntityFilter = tEntityFilterList.ToArray();
                }

                if (tEntityFilterList.Any())
                {
                    //foreach (var flt in tEntityFilter)
                    //{
                    //    sumFilter = sumFilter + (!string.IsNullOrEmpty(sumFilter) ? "," : "") + flt.XToString();
                    //}
                    sumFilter = string.Join(",", tEntityFilterList.Select(x => x.XToString()));
                    var tvs = new[] { new TypedValue(Convert.ToInt32(DxfCode.Start), sumFilter) };
                    sf = new SelectionFilter(tvs);
                }

                var isPromptDefault = string.IsNullOrEmpty(tPrompt);
                var pso = new PromptSelectionOptions();
                switch (tOption)
                {
                    case GetSelectOptions.AllBlocks:
                        psr = Ed.SelectAll(sf);
                        break;
                    case GetSelectOptions.All:
                        psr = Ed.SelectAll(sf);
                        break;
                    case GetSelectOptions.SelectLast:
                        psr = Ed.SelectLast();
                        break;
                    case GetSelectOptions.GetObjects:
                        tPrompt = string.IsNullOrEmpty(tPrompt) ? "Select objects " : tPrompt;
                        pso.MessageForAdding = tPrompt + " (only for " + sumFilter.Replace("LW", "").ToLower() + ")";
                        psr = Ed.GetSelection(pso, sf);
                        break;
                    case GetSelectOptions.GetObject:
                        tPrompt = string.IsNullOrEmpty(tPrompt) ? "Select single object " : tPrompt;
                        pso.MessageForAdding = tPrompt + " (only for " + sumFilter.ToLower() + ")";
                        pso.SingleOnly = true;
                        psr = Ed.GetSelection(pso, sf);
                        break;
                    case GetSelectOptions.SelectWindowPolygon:
                        psr = Ed.SelectWindowPolygon(tPolygonPoint3DColl);
                        break;
                }
                if (psr != null && psr.Status == PromptStatus.OK)
                {
                    Ed.WriteMessage("\nSelected {0} objects.", psr.Value.Count);
                    var ids = psr.Value.GetObjectIds();
                    if (ids != null && ids.Any())
                    {
                        foreach (var objId in ids)
                        {
                            Entity ent = tr.GetObject(objId, OpenMode.ForRead) as Entity;
                            if (ent != null)
                                yield return ent;
                        }
                        SelectedEntity = ids.FirstOrDefault();
                    }
                }
                else
                {
                    yield break;
                }
                tr.Commit();
            }
        }

        //PromptKeywordOptions pKeyOpts = new PromptKeywordOptions("\nSelect objects: ");
        //pKeyOpts.Keywords.Default = "Window";
        //pKeyOpts.Message = "\n" + Commands.Current + "Select objects [All/Window/Polygon]: ";
        //pKeyOpts.Keywords.Add("All");
        //pKeyOpts.Keywords.Add("Window");
        //pKeyOpts.Keywords.Add("Polygon");
        //pKeyOpts.AllowNone = true;
        //pKeyOpts.AppendKeywordsToMessage = true;
        //List<Entity> promptResults = Selects.GetEntities(GetSelectOptions.GetObjects);
        //context.Ed.GetEntity("Select objects: ");

        public static void SelectBlockByName(string blockName)
        {
            using (var trans = Db.TransactionManager.StartTransaction())
            {
                try
                {
                    var bt = (BlockTable)trans.GetObject(Db.BlockTableId, OpenMode.ForRead, false, true);
                    if (bt.Has(blockName))
                    {
                        var btrid = bt[blockName];
                        if (!btrid.IsEffectivelyErased)
                        {
                            var btr = (BlockTableRecord)trans.GetObject(btrid, OpenMode.ForRead, false, true);
                            var brefIDs = btr.GetBlockReferenceIds(true, false);
                            var oids = new ObjectId[brefIDs.Count];
                            brefIDs.CopyTo(oids, 0);
                            Ed.SetImpliedSelection(oids);
                        }
                    }
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    Plugin.Logger.Error($"{nameof(Selects)}.{nameof(SelectBlockByName)} error: {{0}}", ex);
                }
            }
        }

        public static List<ObjectId> SelectBlocks(string blockNamePattern = "")
        {
            //var tvs = new TypedValue[3];
            var tvs = new[] { new TypedValue(0, "INSERT") };
            if (blockNamePattern != "") tvs.SetValue(new TypedValue(1, blockNamePattern), 1);
            var sf = new SelectionFilter(tvs);
            var psr = Ed.SelectAll(sf);
            if (psr.Status == PromptStatus.OK)
            {
                Ed.SetImpliedSelection(psr.Value.GetObjectIds());
                Ed.WriteMessage("Count of blocks" + Convert.ToString(psr.Value.GetObjectIds().Count()));
            }
            return psr.Value != null ? psr.Value.GetObjectIds().ToList() : null;
        }

        public static void SelectAll()
        {
            Document curDoc = acadApp.DocumentManager.MdiActiveDocument;
            Extents3d allEntsExtents = new Extents3d();
            using (Transaction tr = curDoc.TransactionManager.StartTransaction())
            {
                BlockTable bt = tr.GetObject(curDoc.Database.BlockTableId, OpenMode.ForRead, false) as BlockTable;
                BlockTableRecord btr = tr.GetObject(bt[acadApp.DocumentManager.GetCurrentSpace()], OpenMode.ForRead, false) as BlockTableRecord;
                allEntsExtents.AddBlockExtents(btr);
                tr.Commit();
            }
            Plane plane = new Plane();
            Extents2d window = new Extents2d(
                allEntsExtents.MinPoint.Convert2d(plane),
                allEntsExtents.MaxPoint.Convert2d(plane));
        }

        public static Point3d PolarPoint(Point3d basepoint, double angle, double distance)
        {
            return new Point3d(
            basepoint.X + distance * Math.Cos(angle),
            basepoint.Y + distance * Math.Sin(angle),
            basepoint.Z);
        }

        public static void testPolygonRegular()
        {
            Document doc = acadApp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            PromptIntegerOptions pio = new PromptIntegerOptions("");
            pio.Message = "\nNumber of sides: ";

            // Restrict input to positive and non-negative values
            pio.AllowZero = false;
            pio.AllowNegative = false;
            // Add default value
            pio.DefaultValue = 3;
            pio.AllowNone = true;
            // Get the value entered by the user
            PromptIntegerResult ires = ed.GetInteger(pio);
            if (ires.Status != PromptStatus.OK) return;
            int n = ires.Value;

            PromptDoubleOptions pdo = new PromptDoubleOptions("\nEnter radius: ");
            pdo.AllowZero = false;
            pdo.AllowNegative = false;
            pdo.AllowNone = true;
            pdo.UseDefaultValue = true;
            pdo.DefaultValue = 3.0;

            PromptDoubleResult res;
            res = ed.GetDouble(pdo);
            if (res.Status != PromptStatus.OK) return;

            double rad = res.Value;

            double ang = Math.PI * 2 / n;
            Point3d cp, p1, p2;
            // Center point
            cp = new Point3d(0, 0, 0);

            Polyline poly = new Polyline(n);
            Polyline poly2 = new Polyline(n);
            for (int th = 0; th < n; th++)
            {
                // Circumscribed
                p1 = PolarPoint(cp, th * ang, rad / Math.Cos(ang / 2)).TransformBy(Matrix3d.Identity);
                poly.AddVertexAt(th, new Point2d(p1.X, p1.Y), 0, 0, 0);
                // Inscribed
                p2 = PolarPoint(cp, th * ang, rad).TransformBy(Matrix3d.Identity);
                poly2.AddVertexAt(th, new Point2d(p2.X, p2.Y), 0, 0, 0);
            }
            poly.Closed = true;
            poly.ColorIndex = 1;
            poly2.Closed = true;
            poly2.ColorIndex = 3;
            // Add circle for imagination only
            Circle circ = new Circle(new Point3d(0, 0, 0), Vector3d.ZAxis, rad);
            circ.ColorIndex = 5;
            Transaction tr = doc.TransactionManager.StartTransaction();

            using (tr)
            {
                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);
                btr.AppendEntity(poly);
                tr.AddNewlyCreatedDBObject(poly, true);
                btr.AppendEntity(poly2);
                tr.AddNewlyCreatedDBObject(poly2, true);
                btr.AppendEntity(circ);
                tr.AddNewlyCreatedDBObject(circ, true);
                tr.Commit();
            }
        }

        #endregion <public methods>

        #region <Events>

        /// <summary> OnPointMonitor </summary>
        /// <see cref="http://through-the-interface.typepad.com/through_the_interface/2009/07/providing-information-on-autocad-objects-in-a-tooltip-using-net.html"/>
        public static void OnPointMonitor(object sender, PointMonitorEventArgs e)
        {
            Editor ed = (Editor)sender;
            Document doc = ed.Document;
            List<string> curveInfo = new List<string>();

            short tilemode = (short)acadApp.GetSystemVariable("TILEMODE");
            FullSubentityPath[] paths = e.Context.GetPickedEntities();
            if (paths.Length == 0 || tilemode == 0) return;

            try
            {
                using (Transaction tr = doc.TransactionManager.StartTransaction())
                {
                    foreach (FullSubentityPath path in paths)
                    {
                        ObjectId[] ids = path.GetObjectIds();
                        if (ids.Length > 0)
                        {
                            ObjectId id = ids[ids.GetUpperBound(0)];
                            DBObject obj = tr.GetObject(id, OpenMode.ForRead);

                            if (obj.XExistData())
                            {
                                IPaletteElement element = obj.XGetDataObject();
                                Type elementType = obj.XGetXDataObjectType();

                                if (elementType.BaseType != null && elementType.BaseType.BaseType != typeof(PaletteElement))
                                    return;

                                curveInfo.Add($"  ElementType: {elementType.Name.Replace("Acad", "")}");
                                curveInfo.Add($"  TypeName: {element.ElementName}");

                                if (obj.GetType().BaseType == typeof(Curve))
                                {
                                    if (elementType == typeof(AcadCable))
                                        curveInfo.Add($"  Length: {(obj as Curve).Length():F2}");
                                    else if (elementType == typeof(AcadClosure) && obj is Circle)
                                        curveInfo.Add($"  Radius: {(obj as Circle).Radius:F2}");
                                }
                                else if (obj is DBText)
                                {
                                    curveInfo.Add($"  Text: {(obj as DBText).TextString}");
                                }
                                else if (obj is BlockReference)
                                {
                                    BlockReference bref = obj as BlockReference;
                                    BlockTableRecord bdef =
                                        (BlockTableRecord)tr.GetObject(bref.BlockTableRecord, OpenMode.ForRead);
                                    if (!bdef.HasAttributeDefinitions) return;

                                    curveInfo.Add($"  Attributes: {bref.XGetAttributesData()}");
                                }
                            }
                        }
                        tr.Commit();
                    }

                    // Add the tooltip of the lengths of the curves detected
                    if (curveInfo.Any())
                    {
                        var curveInfoString = string.Join<string>("<LineBreak/>", curveInfo);
                        e.AppendToolTipText(
                            $"]]><LineBreak/><Bold>{Plugin.Settings.Name}</Bold><LineBreak/><LineBreak/>{curveInfoString}<![CDATA[");
                    }
                }
            }
            catch
            {
                // Not sure what we might get here, but not real action
                // needed (worth adding an Exception parameter and a
                // breakpoint, in case things need investigating).
            }
        }

        public static void OnPointMonitor1(object sender, PointMonitorEventArgs e)
        {
            Editor ed = (Editor)sender;
            Document doc = ed.Document;

            try
            {
                FullSubentityPath[] paths = e.Context.GetPickedEntities();

                // Go through the results of the selection and detect the curves
                string curveInfo = "";

                Transaction tr = doc.TransactionManager.StartTransaction();
                using (tr)
                {
                    // Open each object, one by one
                    foreach (FullSubentityPath path in paths)
                    {
                        ObjectId[] ids = path.GetObjectIds();
                        if (ids.Length > 0)
                        {
                            ObjectId id = ids[ids.GetUpperBound(0)];
                            DBObject obj = tr.GetObject(id, OpenMode.ForRead);

                            // If it's a curve, get its length
                            Curve cv = obj as Curve;
                            if (cv != null)
                            {
                                double length =
                                    cv.GetDistanceAtParameter(cv.EndParam) -
                                    cv.GetDistanceAtParameter(cv.StartParam);

                                // Then add a message including the object type and its length
                                curveInfo += $"{obj.GetType().Name}'s length: {length:F}" + "\n";
                            }
                        }
                    }
                    // Cheaper than aborting
                    tr.Commit();
                }

                // Add the tooltip of the lengths of the curves detected

                if (curveInfo != "")
                    e.AppendToolTipText(curveInfo);
            }
            catch
            {
                // Not sure what we might get here, but not real action
                // needed (worth adding an Exception parameter and a
                // breakpoint, in case things need investigating).
            }

        }

        public static void OnInputMonitor2(object sender, PointMonitorEventArgs e)
        {
            if (e.Context == null) return;

            if (!e.Context.PointComputed)
                return;

            //  first lets check what is under the Cursor
            FullSubentityPath[] fullEntPath = e.Context.GetPickedEntities();
            if (fullEntPath.Length > 0)
            {
                //  start a transaction
                Transaction tr = acadApp.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction();
                using (tr)
                {
                    try
                    {
                        //  open the Entity for read, it must be derived from Curve
                        Curve ent = (Curve)tr.GetObject(fullEntPath[0].GetObjectIds()[0], OpenMode.ForRead);

                        //  ok, so if we are over something - then check to see if it has an extension dictionary
                        if (ent.ExtensionDictionary.IsValid)
                        {
                            // open it for read
                            DBDictionary extensionDict =
                                (DBDictionary)tr.GetObject(ent.ExtensionDictionary, OpenMode.ForRead);

                            // find the entry
                            ObjectId entryId = extensionDict.GetAt("MyData");

                            // if we are here, then all is ok
                            // extract the xrecord
                            Xrecord myXrecord;

                            //  read it from the extension dictionary
                            myXrecord = (Xrecord)tr.GetObject(entryId, OpenMode.ForRead);

                            // We will draw temporary graphics at certain positions along the entity
                            foreach (TypedValue myTypedVal in myXrecord.Data)
                            {
                                if (myTypedVal.TypeCode == (short)DxfCode.Real)
                                {
                                    //  To locate the temporary graphics along the Curve 
                                    // to show the distances we need to get the point along the curve.
                                    Point3d pnt = ent.GetPointAtDist((double)myTypedVal.Value);

                                    //  We need to work out how many pixels are in a unit square
                                    // so we can keep the temporary graphics a set size regardless of
                                    // the zoom scale. 
                                    Point2d pixels = e.Context.DrawContext.Viewport.GetNumPixelsInUnitSquare(pnt);

                                    //  We need some constant distances. 
                                    double xDist = 10 / pixels.X;

                                    double yDist = 10 / pixels.Y;

                                    // Draw the temporary Graphics. 
                                    Circle circle = new Circle(pnt, Vector3d.ZAxis, xDist);

                                    e.Context.DrawContext.Geometry.Draw(circle);

                                    DBText text = new DBText();

                                    // Always a good idea to set the Database defaults With things like 
                                    // text, dimensions etc. 
                                    text.SetDatabaseDefaults();

                                    // Set the position of the text to the same point as the circle, 
                                    // but offset by the radius. 
                                    text.Position = pnt + new Vector3d(xDist, yDist, 0);

                                    // Use the data from the Xrecord for the text. 
                                    text.TextString = myTypedVal.Value.ToString();

                                    text.Height = yDist;

                                    //  Use the Draw method to display the text. 
                                    e.Context.DrawContext.Geometry.Draw(text);
                                }
                            }
                        }
                        //  all ok, commit it
                        tr.Commit();
                    }
                    catch
                    {
                    }
                    finally
                    {
                        //  whatever happens we must dispose the transaction
                        tr.Dispose();
                    }
                }
            }
        }

        public static void OnInputMonitor3(object sender, PointMonitorEventArgs e)
        {
            //  first lets check what is under the Cursor
            FullSubentityPath[] fullEntPath = e.Context?.GetPickedEntities();
            if (fullEntPath?.Length > 0)
            {
                //  start a transaction
                Transaction trans = acadApp.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction();
                try
                {
                    //  open the Entity for read, it must be derived from Curve
                    Curve ent = (Curve)trans.GetObject(fullEntPath[0].GetObjectIds()[0], OpenMode.ForRead);

                    //  ok, so if we are over something - then check to see if it has an extension dictionary
                    if (ent.ExtensionDictionary.IsValid)
                    {
                        // open it for read
                        DBDictionary extensionDict = (DBDictionary)trans.GetObject(ent.ExtensionDictionary, OpenMode.ForRead);

                        // find the entry
                        ObjectId entryId = extensionDict.GetAt("MyData");

                        // if we are here, then all is ok
                        // extract the xrecord
                        Xrecord myXrecord;

                        //  read it from the extension dictionary
                        myXrecord = (Xrecord)trans.GetObject(entryId, OpenMode.ForRead);

                        // We will draw temporary graphics at certain positions along the entity
                        foreach (TypedValue myTypedVal in myXrecord.Data)
                        {
                            if (myTypedVal.TypeCode == (short)DxfCode.Real)
                            {
                                //  To locate the temporary graphics along the Curve 
                                // to show the distances we need to get the point along the curve.
                                Point3d pnt = ent.GetPointAtDist((double)myTypedVal.Value);

                                //  We need to work out how many pixels are in a unit square
                                // so we can keep the temporary graphics a set size regardless of
                                // the zoom scale. 
                                Point2d pixels = e.Context.DrawContext.Viewport.GetNumPixelsInUnitSquare(pnt);

                                //  We need some constant distances. 
                                double xDist = 10 / pixels.X;

                                double yDist = 10 / pixels.Y;

                                // Draw the temporary Graphics. 
                                Circle circle = new Circle(pnt, Vector3d.ZAxis, xDist);

                                e.Context.DrawContext.Geometry.Draw(circle);

                                DBText text = new DBText();

                                // Always a good idea to set the Database defaults With things like 
                                // text, dimensions etc. 
                                text.SetDatabaseDefaults();

                                // Set the position of the text to the same point as the circle, 
                                // but offset by the radius. 
                                text.Position = pnt + new Vector3d(xDist, yDist, 0);

                                // Use the data from the Xrecord for the text. 
                                text.TextString = myTypedVal.Value.ToString();

                                text.Height = yDist;

                                //  Use the Draw method to display the text. 
                                e.Context.DrawContext.Geometry.Draw(text);


                            }
                        }
                    }
                    //  all ok, commit it
                    trans.Commit();
                }
                catch
                {
                }
                finally
                {
                    //  whatever happens we must dispose the transaction
                    trans.Dispose();
                }
            }
        }

        public static void OnPointMonitor4(object sender, PointMonitorEventArgs e)
        {
            _toolTipObjectId = ObjectId.Null;

            if (acadApp.GetSystemVariable("RollOverTips").ToString() != "1" || !SuperToolTipSettings.EnableSuperToolTips)
                return;

            if ((e.Context.History & PointHistoryBits.FromKeyboard) == PointHistoryBits.FromKeyboard)
                return;

            FullSubentityPath[] paths = e.Context.GetPickedEntities();

            if (paths == null || paths.Length == 0)
                return;

            ObjectId[] ids = paths[0].GetObjectIds();

            if (ids == null || ids.Length == 0)
                return;

            var i = 0;

            if (!ids[i].IsValid)
                return;
            GetClass(ids, i);
        }

        public static void Clean()
        {
            _lastSelectAddedObjectId = ObjectId.Null;
        }
        private static void GetClass(ObjectId[] ids, int i)
        {
            if (ids[i].ObjectClass.Name == "AecbDbDuct" ||
                ids[i].ObjectClass.Name == "AecbDbDuctFitting" ||
                ids[i].ObjectClass.Name == "AecbDbDuctCustomFitting" ||
                ids[i].ObjectClass.Name == "AecbDbDuctFlex" ||
                ids[i].ObjectClass.Name == "AecbDbPipe" ||
                ids[i].ObjectClass.Name == "AecbDbPipeFitting" && SuperToolTipSettings.DisplayPipeFittings ||
                ids[i].ObjectClass.Name == "AecbDbPipeCustomFitting" ||
                ids[i].ObjectClass.Name == "AecbDbPipeFlex" && SuperToolTipSettings.DisplayFlexPipe ||
                ids[i].ObjectClass.Name == "AecbDbCableTray" && SuperToolTipSettings.DisplayCableTray ||
                ids[i].ObjectClass.Name == "AecbDbCableTrayFitting" ||
                ids[i].ObjectClass.Name == "AecbDbConduit" ||
                ids[i].ObjectClass.Name == "AecbDbConduitFitting" && SuperToolTipSettings.DisplayConduitFittings ||
                ids[i].ObjectClass.Name == "AecbDbSchematicPipe" ||
                ids[i].ObjectClass.Name == "AecbDbSchematicPipeFitting" ||
                ids[i].ObjectClass.Name == "AecbDbSchematic" ||
                ids[i].ObjectClass.Name == "AecbDbSchematicSymbol" ||
                ids[i].ObjectClass.Name == "AecbDbDevice" ||
                ids[i].ObjectClass.Name == "AecbDbPanel" ||
                ids[i].ObjectClass.Name == "AecbDbWire" ||
                ids[i].ObjectClass.Name == "AecbDbMvPart")
            {
                _toolTipObjectId = ids[i];
            }

            if (ids[i].ObjectClass.Name == "AcDbBlockReference")
            {
                GetClass(ids, i + 1);
            }
        }

        private class SuperToolTipSettings
        {
            public static bool DisplayPipeFittings { get; set; }
            public static bool DisplayFlexPipe { get; set; }
            public static bool DisplayCableTray { get; set; }
            public static bool DisplayConduitFittings { get; set; }
            public static bool EnableSuperToolTips { get; set; }

        }

        #endregion <Events>
    }
}