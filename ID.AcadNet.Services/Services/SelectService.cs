using System;
using System.Linq;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Intellidesk.AcadNet.Services.Interfaces;
using Intellidesk.Infrastructure;

using Microsoft.Practices.Unity;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Intellidesk.AcadNet.Services
{
    public partial class SelectService : ISelectService
    {
        private readonly IUnityContainer _unityContainer;
        private readonly ICommandLineService _commandLineService;
        private readonly IOperationService _operationService;

        public SelectService(IUnityContainer unityContainer)
        {
            _unityContainer = unityContainer;
            _commandLineService = _unityContainer.Resolve<ICommandLineService>();
            _operationService = _unityContainer.Resolve<IOperationService>(); ;
        }
        // Get SelectionSet per options: new selection, implied selection(include already selected objects)
        public SelectionSet GetSelectionSet()
        {
            var nAttempt = 0;
            SelectionSet curSet = null;
            _operationService.Ed.SetImpliedSelection(new ObjectId[] { }); //new ObjectId[0]; new ObjectId[] {ent.ObjectId}
            do
            {
                var psr = _operationService.Ed.GetSelection();
                if (psr.Status == PromptStatus.OK)
                {
                    curSet = psr.Value;
                    var objectIds = new ArrayList();
                    foreach (var ent in curSet.XGetEntities())
                    {
                        objectIds.Add(ent.ObjectId);
                    }
                    _operationService.Ed.WriteMessage("... Objects selected: " + curSet.Count.ToString(CultureInfo.InvariantCulture));
                    break; // TODO: might not be correct. Was : Exit Do
                }
                nAttempt = nAttempt + 1;
                if (nAttempt == 2)
                {
                    break; // TODO: might not be correct. Was : Exit Do
                }
                _operationService.Ed.WriteMessage("... Objects selected: 0");
            } while (true);
            return curSet;
        } //GetSelectionSet

        // get count Point or listPoints with promptMessage to user
        public List<Point3d> GetPromptPoints(int tPromptCount, OptionsGetPoints tOption)
        {
            var pointsList = new List<Point3d>();
            PromptPointResult ppr = null;
            var ppo = new PromptPointOptions("");
            for (var i = 0; i <= tPromptCount - 1; i++)
            {
                if (i == 0)
                {
                    // Prompt for the start point
                    ppo.Message = "\nEnter the start point of the construction: ";
                    ppr = _operationService.Ed.GetPoint(ppo);
                    // Exit if the user presses ESC or cancels the command
                    if (ppr.Status == PromptStatus.Cancel)
                        return null;
                    ppo.UseBasePoint = true;
                    ppo.BasePoint = ppr.Value;
                }
                else
                {
                    //' Prompt for the end point
                    ppo.Message = (string.Format("\nEnter the point {0}: \n", i + 1));
                    ppr = _operationService.Doc.Editor.GetPoint(ppo);
                }
                pointsList.Add(new Point3d(Convert.ToInt32(ppr.Value.X), Convert.ToInt32(ppr.Value.Y), ppr.Value.Z));
            }
            _commandLineService.Cancel();

            if (ppr != null && ppr.Status == PromptStatus.OK)
            {
                if (tOption == OptionsGetPoints.PointList)
                {
                    return (from pnt in pointsList select new Point3d(pnt.X, pnt.Y, pnt.Z)).ToList();
                }
                return pointsList;
            }
            return null;
        } // GetPoints

        //Get new or implied selection
        public void SelectClear()
        {
            _operationService.Ed.SetImpliedSelection(new ObjectId[0]);
        }

        public List<ObjectId> GetSelect()
        {
            var nAttempt = 0; SelectionSet curSet = null;
            do
            {
                var psr = _operationService.Ed.GetSelection();
                if (psr.Status == PromptStatus.OK)
                {
                    //nAttempt = 0;
                    curSet = psr.Value;
                    //Dim ids As ObjectId() = New ObjectId(ss.GetObjectIds.Count - 1) {}
                    //ss.GetObjectIds.CopyTo(ids, 0)
                    //Ed.SetImpliedSelection(ids)
                    _operationService.Ed.WriteMessage("Objects selected: " + Convert.ToString(curSet.Count));
                    break;
                }
                nAttempt = nAttempt + 1;
                if (nAttempt == 3)
                {
                    break;
                }
                _operationService.Ed.WriteMessage("Objects selected: 0");
            } while (true);
            return curSet != null ? curSet.GetObjectIds().ToList() : null;
        }

        /// <summary> Get implied objects </summary>
        public List<ObjectId> GetSelectImplied(List<ObjectId> objectIds = null)
        {
            var nAttempt = 0; SelectionSet curSet = null;
            if (objectIds != null)
                _operationService.Ed.SetImpliedSelection(objectIds.ToArray());
            else
                _operationService.Ed.SelectImplied();
            do
            {
                var psr = _operationService.Ed.GetSelection();
                if (psr.Status == PromptStatus.OK)
                {
                    //nAttempt = 0;
                    curSet = psr.Value;
                    //Dim ids As ObjectId() = New ObjectId(ss.GetObjectIds.Count - 1) {}
                    //ss.GetObjectIds.CopyTo(ids, 0)
                    //Ed.SetImpliedSelection(ids)
                    _operationService.Ed.WriteMessage("Objects selected: " + Convert.ToString(curSet.Count));
                    break;
                }
                nAttempt = nAttempt + 1;
                if (nAttempt == 3)
                {
                    break;
                }
                _operationService.Ed.WriteMessage("Objects selected: 0");
            } while (true);
            return curSet != null ? curSet.GetObjectIds().ToList() : null;
        }

        /// <summary> Get implied objects </summary>
        public List<ObjectId> GetImplied()
        {
            var result = new List<ObjectId>();
            
            var psr = Application.DocumentManager.MdiActiveDocument.Editor.SelectImplied();
            if (psr.Status == PromptStatus.OK)
                result = psr.Value.GetObjectIds().ToList();
            
            return result;
        }

        // Get entites from Model Space with: GetSelectOptions and SelectFilter
        public List<Entity> GetEntitiesOld(OptionsGetSelect tOptionPolygon, Point3dCollection tPoint3DColl,
                                                  params string[] tSelectFilter)
        {
            //"LWPOLYLINE"
            var listEntityColl = new List<Entity>();
            using (var tr = _operationService.Db.TransactionManager.StartTransaction())
            {
                PromptSelectionResult psr = null;
                switch (tOptionPolygon)
                {
                    case OptionsGetSelect.SelectWindowPolygon:
                        if (tSelectFilter.Length == 0)
                        {
                            psr = _operationService.Ed.SelectWindowPolygon(tPoint3DColl);
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
                            psr = _operationService.Ed.SelectWindowPolygon(tPoint3DColl, sf);
                        }
                        break;
                }
                if (psr != null && psr.Status == PromptStatus.OK)
                {
                    _operationService.Ed.WriteMessage("\nSelected {0} entities.", psr.Value.Count);
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
        public Entity GetEntity(OptionsGetSelect tOption = OptionsGetSelect.GetObject, string tPrompt = "",
                                       Point3dCollection tPolygonPoint3DColl = null, params string[] tEntityFilter)
        {
            return GetEntities(tOption, tPrompt, tPolygonPoint3DColl, tEntityFilter).First();
        }

        public List<Entity> GetEntities(OptionsGetSelect tOption, string tPrompt = "", Point3dCollection tPolygonPoint3DColl = null,
                                               params string[] tEntityFilter)
        {
            //"LWPOLYLINE"
            var listEntityColl = new List<Entity>();
            var sumFilter = "";
            using (var tr = _operationService.Db.TransactionManager.StartTransaction())
            {
                PromptSelectionResult psr = null;
                SelectionFilter sf = null;
                if (tEntityFilter.Length != 0)
                {
                    foreach (var flt in tEntityFilter)
                    {
                        sumFilter = sumFilter + (!string.IsNullOrEmpty(sumFilter) ? "," : "") + Convert.ToString(flt);
                    }
                    var tvs = new[] { new TypedValue(Convert.ToInt32(DxfCode.Start), sumFilter) };
                    sf = new SelectionFilter(tvs);
                }
                var isPromptDefault = !string.IsNullOrEmpty(tPrompt);
                var pso = new PromptSelectionOptions();
                switch (tOption)
                {
                    case OptionsGetSelect.AllBlocks:
                        psr = _operationService.Ed.SelectAll(sf);
                        break;
                    case OptionsGetSelect.All:
                        psr = _operationService.Ed.SelectAll(sf);
                        break;
                    case OptionsGetSelect.SelectLast:
                        psr = _operationService.Ed.SelectLast();
                        break;
                    case OptionsGetSelect.GetSelection:
                        if (isPromptDefault) pso.MessageForAdding = "Select objects ";
                        if (!string.IsNullOrEmpty(sumFilter)) pso.MessageForAdding = pso.MessageForAdding + "(only for " + sumFilter + ")";
                        psr = _operationService.Ed.GetSelection(pso, sf);
                        break;
                    case OptionsGetSelect.GetObject:
                        if (isPromptDefault) pso.MessageForAdding = "Select single object ";
                        if (!string.IsNullOrEmpty(sumFilter)) pso.MessageForAdding = pso.MessageForAdding + "(only for " + sumFilter + ")";
                        pso.SingleOnly = true;
                        psr = _operationService.Ed.GetSelection(pso, sf);
                        break;
                    case OptionsGetSelect.SelectWindowPolygon:
                        psr = _operationService.Ed.SelectWindowPolygon(tPolygonPoint3DColl);
                        break;
                }
                if (psr != null && psr.Status == PromptStatus.OK)
                {
                    _operationService.Ed.WriteMessage("\nSelected {0} objects.", psr.Value.Count);
                    foreach (var objId in psr.Value.GetObjectIds())
                    {
                        var ent = (Entity)tr.GetObject(objId, OpenMode.ForRead);
                        //If tSelectFilter(0).Contains(ent.GetType.Name.ToUpper) Then
                        listEntityColl.Add(ent);
                        ent.Highlight();
                        _operationService.Ed.WriteMessage(ent.BlockName);
                        //ent.List()
                        //ent.Dispose()
                    }
                }
                else
                {
                    listEntityColl.Add(new Line(new Point3d(0, 0, 0), new Point3d(0, 0, 0)));
                }
                tr.Commit();
                return listEntityColl;
            }
        }

        public void SelectBlockByName(string blockName)
        {
            using (var trans = _operationService.Db.TransactionManager.StartTransaction())
            {
                try
                {
                    var bt = (BlockTable)trans.GetObject(_operationService.Db.BlockTableId, OpenMode.ForRead, false, true);
                    if (bt.Has(blockName))
                    {
                        var btrid = bt[blockName];
                        if (!btrid.IsEffectivelyErased)
                        {
                            var btr = (BlockTableRecord)trans.GetObject(btrid, OpenMode.ForRead, false, true);
                            var brefIDs = btr.GetBlockReferenceIds(true, false);
                            var oids = new ObjectId[brefIDs.Count];
                            brefIDs.CopyTo(oids, 0);
                            _operationService.Ed.SetImpliedSelection(oids);
                        }
                    }
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    Log.Add(ex);
                }
            }
        }

        public List<ObjectId> SelectBlocks(string blockNamePattern = "")
        {
            //var tvs = new TypedValue[3];
            var tvs = new[] { new TypedValue(0, "INSERT") };
            if (blockNamePattern != "") tvs.SetValue(new TypedValue(1, blockNamePattern), 1);
            var sf = new SelectionFilter(tvs);
            var psr = _operationService.Ed.SelectAll(sf);
            if (psr.Status == PromptStatus.OK)
            {
                _operationService.Ed.SetImpliedSelection(psr.Value.GetObjectIds());
                _operationService.Ed.WriteMessage("Count of blocks" + Convert.ToString(psr.Value.GetObjectIds().Count()));
            }
            return (psr.Value != null ? psr.Value.GetObjectIds().ToList() : null);
        }

    }

    public partial class SelectService //TEST
        : ISelectService
    {
        public void HighlightSubEntity(Document doc, PromptNestedEntityResult rs)
        {
            //// Extract relevant information from the prompt object
            //var selId = rs.ObjectId;
            //var objIds = rs.GetContainers();
            //var len = objIds.Length;
            //// Reverse the "containers" list
            //var revIds = new ObjectId[len + 1];
            //for (var i = 0; i < len; i++)
            //{
            //    var id = (ObjectId)objIds.GetValue(len - i - 1);
            //    revIds.SetValue(id, i);
            //}
            //// Now add the selected entity to the end
            //revIds.SetValue(selId, len);
            //// Retrieve the sub-entity path for this entity
            //var subEnt = new SubentityId(SubentityType.Null, 0);
            //var path = new FullSubentityPath(revIds, subEnt);
            //// Open the outermost container, relying on the open transaction...
            //var id2 = (ObjectId)revIds.GetValue(0);
            //var ent = id2.GetObject(OpenMode.ForRead) as Entity;
            //// ... and highlight the nested entity
            //if (ent != null)
            //    ent.Highlight(path, false);
        }

        [Autodesk.AutoCAD.Runtime.CommandMethod("SubEntSelect")]
        public void SubEntSelect()
        {
            //PromptKeywordOptions pko = new PromptKeywordOptions("\nSpecify sub-entity selection type:");
            //pko.AllowNone = false;
            //pko.Keywords.Add("Block");
            //pko.Keywords.Add("BlockReference");
            //pko.Keywords.Default = "Block";
            //PromptResult pkr = WorkSpace.Ed.GetKeywords(pko);
            //if (pkr.Status != PromptStatus.OK)
            //    return;
            PromptSelectionOptions pso = new PromptSelectionOptions();
            pso.MessageForAdding = "\nSelect block "; // +pkr.StringResult + ": ";
            pso.SingleOnly = true;
            //pso.SinglePickInSpace = true;
            //pso.ForceSubSelections = true;
            PromptSelectionResult psr = _operationService.Ed.GetSelection(pso);
            if (psr.Status != PromptStatus.OK)
                return;
            SelectionSet ss = psr.Value;
            SelectedObject so = ss[0];
            if (!so.ObjectId.ObjectClass.IsDerivedFrom(
                Autodesk.AutoCAD.Runtime.RXObject.GetClass(typeof(BlockReference))))
            {
                _operationService.Ed.WriteMessage(
                    "\nYou didn't select a solid, please try again...");
                return;
            }
            using (var tr = _operationService.Db.TransactionManager.StartTransaction())
            {
                var br = tr.GetObject(so.ObjectId, OpenMode.ForRead) as BlockReference;
                SelectedSubObject[] sso = so.GetSubentities();
                //Checks that selected type matches keyword selection
                //if (subentityType != sso[0].FullSubentityPath.SubentId.Type)
                //{
                //    WorkSpace.Ed.WriteMessage("\nInvalid Subentity Type: " +
                //        sso[0].FullSubentityPath.SubentId.Type +
                //        ", please try again...");
                //    return;
                //}
                SubentityId subentityId = sso[0].FullSubentityPath.SubentId;
                //Creates subentity path to use with GetSubentity
                if (br != null)
                {
                    var subEntityPath = new FullSubentityPath(new[] { br.ObjectId }, subentityId);
                    //Returns a non-database resident entity 
                    //that represents the subentity
                    using (var entity = br.GetSubentity(subEntityPath))
                    {
                        _operationService.Ed.WriteMessage("\nSubentity Entity Type: " + entity);
                    }
                }
                //Creates entity path to generate Brep object from it
                if (br != null)
                {
                    var entityPath = new FullSubentityPath(new[] { br.ObjectId }, new SubentityId(SubentityType.Null, IntPtr.Zero));
                }
                //switch (subentityType)
                //{
                //    case SubentityType.Face:
                //        break;
                //    case SubentityType.Edge:
                //        break;
                //    case SubentityType.Vertex:
                //        break;
                //    default:
                //        break;
                //}
                tr.Commit();
            }
        }

        #region "Nested elememts"

        [Autodesk.AutoCAD.Runtime.CommandMethod("SelectNested")]
        public void SelectNested()
        {
            // Collection of our selected entities and their sub-ent paths
            ObjectIdCollection ids;
            List<FullSubentityPath> paths;

            // Start a transaction... will initially be used
            // to highlight the selected entities and then to
            // modify their layer
            Transaction tr = _operationService.Doc.TransactionManager.StartTransaction();
            using (tr)
            {
                if (SelectNestedEntities(_operationService.Ed, out ids, out paths) &&
                    ids.Count > 0)
                {
                    // Get the name of our destination later
                    //PromptResult pr = WorkSpace.Ed.GetString("\nNew layer for these objects: ");
                    //if (pr.Status == PromptStatus.OK)
                    //{
                    //    // Check that the layer exists
                    //    string newLay = pr.StringResult;
                    //    LayerTable lt =
                    //      tr.GetObject(WorkSpace.Db.LayerTableId, OpenMode.ForRead)
                    //        as LayerTable;
                    //    if (lt.Has(newLay))
                    //    {
                    //        // If so, set the layer name to be the one chosen
                    //        // on each of the selected entitires
                    //        for (int i = 0; i < ids.Count; i++)
                    //        {
                    //            Entity ent =
                    //              tr.GetObject(ids[i], OpenMode.ForWrite) as Entity;
                    //            if (ent != null)
                    //            {
                    //                ent.Layer = newLay;
                    //            }
                    //        }
                    //    }
                    //    else
                    //    {
                    //        WorkSpace.Ed.WriteMessage(
                    //          "\nLayer not found in current drawing."
                    //        );
                    //    }
                    //    UnhighlightSubEntities(paths);
                    //}
                }
                tr.Commit();
                // Regen reflects the new layer
                _operationService.Ed.Regen();
            }
        }

        private bool SelectNestedEntities(Editor ed, out ObjectIdCollection ids, out List<FullSubentityPath> paths)
        {
            ids = new ObjectIdCollection();
            paths = new List<FullSubentityPath>();
            // Loop until cancelled or completed
            PromptNestedEntityResult rs;
            do
            {
                rs = _operationService.Ed.GetNestedEntity("\nSelect nested entity: ");
                if (rs.Status == PromptStatus.OK)
                {
                    ids.Add(rs.ObjectId);
                    FullSubentityPath path = HighlightSubEntity(rs);
                    if (path != FullSubentityPath.Null)
                        paths.Add(path);
                }
            }
            while (rs.Status == PromptStatus.OK);
            // Cancel is the status when "enter" is used to
            // terminate the selection, which means we can't
            // use it to distinguish from an actual
            // cancellation.
            return (rs.Status == PromptStatus.Cancel);
        }

        // Unhighlight a set of sub-entities
        private void UnhighlightSubEntities(List<FullSubentityPath> paths)
        {
            for (int i = 0; i < paths.Count; i++)
            {
                ObjectId[] ids = paths[i].GetObjectIds();
                Entity ent = ids[0].GetObject(OpenMode.ForRead) as Entity;
                if (ent != null)
                {
                    ent.Unhighlight(paths[i], false);
                }
            }
        }

        // Highlight a sub-entity based on its nested
        // selection information.
        // Return the calculated sub-entity path, so
        // the calling application can later unhighlight.
        private FullSubentityPath HighlightSubEntity(PromptNestedEntityResult rs)
        {
            // Extract relevant information from the prompt object
            ObjectId selId = rs.ObjectId;
            var objIds = new List<ObjectId>(rs.GetContainers());
            // Reverse the "containers" list
            objIds.Reverse();
            // Now append the selected entity
            objIds.Add(selId);
            // Retrieve the sub-entity path for this entity
            var subEnt = new SubentityId(SubentityType.Null, System.IntPtr.Zero);
            var path = new FullSubentityPath(objIds.ToArray(), subEnt);
            // Open the outermost container, relying on the open
            // transaction...
            var ent = objIds[0].GetObject(OpenMode.ForRead) as Entity;
            // ... and highlight the nested entity
            if (ent == null)
                return FullSubentityPath.Null;
            ent.Highlight(path, false);
            // Return the sub-entity path for later unhighlighting
            return path;
        }

        public void GetBlockNameFromItsSubentity()
        {
            //Matrix3d ucs = WorkSpace.Ed.CurrentUserCoordinateSystem;
            //PromptNestedEntityOptions pno = new PromptNestedEntityOptions("\nSelect a Line inside the Block: ");

            //PromptNestedEntityResult nres = WorkSpace.Ed.GetNestedEntity(pno);
            //if (nres.Status != PromptStatus.OK)
            //{
            //    WorkSpace.Ed.WriteMessage("\nEntsel failed");
            //    return;
            //}

            //Transaction tr = WorkSpace.Db.TransactionManager.StartTransaction();
            //using (tr)
            //{
            //    Point3d pickPt = nres.PickedPoint.TransformBy(ucs);
            //    WorkSpace.Ed.WriteMessage("\nPicked point is {0}", pickPt);
            //    ObjectId selId = nres.ObjectId;
            //    List<ObjectId> objIds = new List<ObjectId>(nres.GetContainers());

            //    // Reverse the "containers" list
            //    // Now append the selected entity
            //    objIds.Add(selId);
            //    objIds.Reverse();

            //    // Retrieve the sub-entity path for this entity
            //    SubentityId subEnt = new SubentityId(SubentityType.Null, 0);
            //    FullSubentityPath path = new FullSubentityPath(objIds.ToArray(), subEnt);

            //    // Open the outermost container, relying on the open
            //    // transaction...

            //    Entity pSubEnt = tr.GetObject(objIds[0], OpenMode.ForRead, false) as Entity;
            //    // Output the class name of the sub entity

            //    tr.Commit();
            //}

        }

        #endregion
    }
}
