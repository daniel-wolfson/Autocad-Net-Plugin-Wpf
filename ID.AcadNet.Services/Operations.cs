using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.Windows;
using ID.Infrastructure;
using Intellidesk.AcadNet.Common.Extensions;
using Intellidesk.AcadNet.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Intellidesk.AcadNet.Services
{
    public static class Operations
    {
        public delegate void ToolsReadObjectsEventHandler(ObjectId sender, ActionArguments args, bool isNested = false);
        public delegate void ToolsParseBlockEventHandler(ObjectId sender, ActionArguments args);
        public delegate List<ObjectId> ToolsReadObjectsFilterEventHandler(List<ObjectId> sender);
        public delegate bool ToolsTaskEventHandler(ActiveTaskDialog sender, TaskDialogCallbackArgs args);

        public static RegAppTableRecord RegApp { get; set; }
        public static int GroupId = 1;

        //Events of method ReadObjects for all types elements
        public static event ToolsReadObjectsEventHandler ToolsReadObjectsEvent;
        public static event ToolsParseBlockEventHandler ToolsParseBlockEvent;
        public static event ToolsReadObjectsFilterEventHandler ToolsReadObjectsFilterEvent;
        public static event ToolsReadObjectsEventHandler ToolsReadObjectsLineEvent;
        public static event ToolsReadObjectsEventHandler ToolsReadObjectsPolylineEvent;
        public static event ToolsReadObjectsEventHandler ToolsReadObjectsCircleEvent;
        public static event ToolsReadObjectsEventHandler ToolsReadObjectsEllipseEvent;
        public static event ToolsReadObjectsEventHandler ToolsReadObjectsMtextEvent;

        //public event ToolsTaskEventHandler ToolsTaskEvent;
        public static Entity CurrentObject { get; set; }

        public static ObjectId CurrentLineType
        {
            get { return Doc.Database.Celtype; }
            set { Doc.Database.Celtype = value; }
        }

        public static Editor Ed { get { return Doc.Editor; } }
        public static Database Db { get { return Doc.Database; } }
        public static Document Doc { get { return acadApp.DocumentManager.MdiActiveDocument; } }

        #region "ctor"

        static Operations()
        {
            //UnityContainer = container;
            //_pluginManager = UnityContainer.Resolve<IPluginManager>();
            //Plugin.LibName = Assembly.GetExecutingAssembly().GetName().Name;

            //var editService = unityContainer.Resolve<IEditService>();
            //editService.OperationService = this;
            ////unityContainer.BuildUp(typeof(IEditService), editService);
            //editService.AddRegAppTableRecord(ProjectManager.Name);

            //Log.WriteMessageEvent += OnWriteMessage;
        }

        #endregion

        public static void OnWriteMessage(string message, params object[] parameters)
        {
            if (!String.IsNullOrEmpty(message))
                Ed.WriteMessage(message, parameters);
            //message.XWriteMessageToCADConsole();
        }

        public static string GetLineType(string lineTypeName)
        {
            string retValue = "Continious";
            using (Doc.LockDocument())
            {
                using (var tr = Db.TransactionManager.StartTransaction())
                {
                    var lt = tr.GetObject(Db.LinetypeTableId, OpenMode.ForRead) as LinetypeTable;
                    if (lt != null)
                        if (!lt.Has(lineTypeName))
                        {
                            var path = HostApplicationServices.Current.FindFile("acad.lin", Db, FindFileHint.Default);
                            try
                            {
                                Db.LoadLineTypeFile(lineTypeName, path);
                            }
                            catch (Exception ex)
                            {
                                Plugin.Logger.Error($"{nameof(Operations)}.{nameof(GetLineType)} error: ", ex);
                                tr.Commit();
                                return "Continious";
                            }
                            tr.Commit();
                            retValue = GetLineType(lineTypeName);
                        }
                        else
                        {
                            tr.Commit();
                            retValue = lineTypeName;
                        }
                }
            }
            return retValue;
        }

        public static ObjectId GetLineTypeObjectId(string value) //???
        {
            dynamic lineTypeIDs = SymbolUtilityServices.GetLinetypeByLayerId(Db);

            foreach (var lineTypeID in lineTypeIDs)
            {
                if (((LinetypeTableRecord)lineTypeID).Name == value)
                    return ((LinetypeTableRecord)lineTypeID).ObjectId;
            }
            return ObjectId.Null;
        }

        /// <summary>
        /// Reading objects and rising events(fior example such as "event ToolsParseBlock") 
        ///  <para>and calling custom function (may be named such as OnToolsParseBlock (look to below by code) </para>
        /// <example>example: ParseObjects(CurrentObjectIds, new[] { typeof(BlockReference) }</example>
        /// <param name="readArgs"></param>
        /// <returns></returns>
        /// </summary>
        public static IEnumerable<ObjectId> ReadObjects(ActionArguments readArgs = null)
        {
            if (readArgs == null) readArgs = new ActionArguments();

            //if tSelectedObjectIds equal null then it is Selected user's objects, if it not equal null then it is All selected object from model space
            var objectIds = readArgs.SelectedObjects ?? new List<ObjectId>().XGetObjects(readArgs);

            //Objects filter handler from external project
            var handlerFilter = ToolsReadObjectsFilterEvent;
            if (handlerFilter != null)
                objectIds = handlerFilter(objectIds);

            using (Doc.TransactionManager.StartTransaction())
            {
                //using (var Tr = Db.TransactionManager.StartTransaction())
                //{
                //    args.Trans = Tr;
                foreach (var objId in objectIds)
                {
                    if (objId.IsErased) continue;

                    //if (readArgs.FilterTypesOn != null && readArgs.FilterTypesOn.Count > 0
                    //    && readArgs.FilterTypesOn.Select(RXObject.GetClass).Contains(objId.ObjectClass)) continue;

                    readArgs.ProgressIndex++;
                    //if (taskArgs != null)
                    //    taskArgs.Percent = (int)((float)args.ProgressIndex / objectIds.Count * 100);

                    var handler = ToolsReadObjectsEvent;
                    handler?.Invoke(objId, readArgs);

                    switch (objId.ObjectClass.DxfName)
                    {
                        case "INSERT":
                            var blockhandler = ToolsParseBlockEvent;
                            blockhandler?.Invoke(objId, readArgs);

                            //yield return objId;

                            break;
                        case "POLYLINE":
                            handler = ToolsReadObjectsPolylineEvent;
                            handler?.Invoke(objId, readArgs);
                            break;
                        case "LINE":
                            handler = ToolsReadObjectsLineEvent;
                            handler?.Invoke(objId, readArgs);
                            break;
                        case "CIRCLE":
                            handler = ToolsReadObjectsCircleEvent;
                            handler?.Invoke(objId, readArgs);
                            break;
                        case "ELLIPSE":
                            handler = ToolsReadObjectsEllipseEvent;
                            handler?.Invoke(objId, readArgs);
                            break;
                        case "MTEXT":
                            handler = ToolsReadObjectsMtextEvent;
                            handler?.Invoke(objId, readArgs);
                            break;
                    }

                    //if (args.ProgressMeterBar != null)
                    //    args.ProgressMeterBar.MeterProgress();

                    //if (taskArgs.Worker != null)
                    //{
                    //    taskArgs.Worker.ReportProgress(taskArgs.Percent);
                    //if (taskArgs.Worker.CancellationPending)
                    //    yield return null;
                    //}

                    //}
                    //catch (Autodesk.AutoCAD.Runtime.Exception ex)
                    //{
                    //    Log.Add(ex); //Log.Add("Error", "ReadObjects(), " + ex.Message);
                    //}
                }
            }


            //    Tr.Commit();
            //}
            //Log.Add("", "Parser: Count processed objects: {0}", objectIds.Count);
            return objectIds;
        }

        public static DBObject GetObject(string dxfTypeNamesFilter, string layerNamesFilter, string fieldTextFilter)
        {
            return GetObjects(dxfTypeNamesFilter, layerNamesFilter, fieldTextFilter).XCast<MText>().FirstOrDefault();
        }

        public static IEnumerable<ObjectId> GetObjects<T>(string layerNamesFilter, string fieldTextFilter) where T : DBObject
        {
            var typeNamesFilter = typeof(T).Name.ToUpper() == "DBTEXT" ? "TEXT" : typeof(T).Name.ToUpper();
            return GetObjects(typeNamesFilter, layerNamesFilter, fieldTextFilter);
        }

        public static IEnumerable<ObjectId> GetObjects(Type[] types, string layerNamesFilter, string fieldTextFilter)
        {
            var typeNamesFilter = String.Join(",", types.Select(x => (x.Name.ToUpper() == "DBTEXT") ? "TEXT" : x.Name.ToUpper()).ToArray());
            return GetObjects(typeNamesFilter, layerNamesFilter, fieldTextFilter);
        }

        public static IEnumerable<ObjectId> GetObjects(string dxfTypeNamesFilter, string layerNamesFilter, string fieldTextFilter)
        {
            var selFilter = new TypedValue[1 + (string.IsNullOrEmpty(layerNamesFilter) ? 0 : 1)];

            if (!string.IsNullOrEmpty(dxfTypeNamesFilter))
                selFilter.SetValue(new TypedValue((int)DxfCode.Start, dxfTypeNamesFilter), 0);

            if (!string.IsNullOrEmpty(layerNamesFilter))
                selFilter.SetValue(new TypedValue((int)DxfCode.LayerName, layerNamesFilter), 1);

            var ids = new List<ObjectId>();
            PromptSelectionResult selectedResult = Ed.SelectAll(new SelectionFilter(selFilter));

            if (selectedResult.Status != PromptStatus.Error && selectedResult.Value != null)
            {
                using (Doc.LockDocument())
                {
                    using (var transaction = Doc.Database.TransactionManager.StartTransaction())
                    {
                        foreach (var objId in selectedResult.Value.GetObjectIds())
                        {
                            var typeDxfNames = dxfTypeNamesFilter.Split(',');
                            DBObject currentObj = transaction.GetObject(objId, OpenMode.ForRead);
                            if (currentObj != null)
                            {
                                var typeName = currentObj.GetType().Name.ToUpper() == "DBTEXT" ? "TEXT" : currentObj.GetType().Name.ToUpper();
                                if (typeDxfNames.Contains(typeName))
                                {
                                    if (typeName == "MTEXT" && ((MText)currentObj).Text.Equals(fieldTextFilter))
                                        ids.Add(objId);
                                    if (typeName == "TEXT" && ((DBText)currentObj).TextString.Equals(fieldTextFilter))
                                        ids.Add(objId);
                                }
                            }
                        }
                        //transaction.Commit(); // if you change something.
                    }
                }
            }
            return ids.AsEnumerable();
        }

        public static IEnumerable<ObjectId> GetObjects(List<ObjectId> selectedObjects,
            Type[] filterTypes = null, string keyAttributeForBlockItem = "",
            Type[] filterNestedTypes = null, bool isNested = false)
        {
            List<ObjectId> ids = new List<ObjectId>();
            using (var tr = Db.TransactionManager.StartTransaction())
            {
                if (selectedObjects == null)
                {
                    var bt = (BlockTable)tr.GetObject(Db.BlockTableId, OpenMode.ForRead);
                    var btr = (BlockTableRecord)bt[acadApp.DocumentManager.GetCurrentSpace()].GetObject(OpenMode.ForRead);
                    selectedObjects = btr.Cast<ObjectId>().ToList();
                }
                if (selectedObjects.Count == 0) return selectedObjects;

                if (!isNested)
                {
                    ids.Clear();
                    if (filterTypes != null) selectedObjects = selectedObjects.XGetObjects(filterTypes);
                }
                else
                {
                    if (filterNestedTypes != null) selectedObjects = selectedObjects.XGetObjects(filterNestedTypes);
                }

                foreach (var objectId in selectedObjects)
                {
                    //if (filterTypesOn.Select(RXObject.GetClass).Contains(objectId.ObjectClass))
                    var br = (BlockReference)tr.GetObject(objectId, OpenMode.ForRead);

                    //blockItem is it contains tag = keyAttributeForBlockItem, else it is blokContainer contains others objects
                    var blockItem = br.XContainsAttributeTags(tr, keyAttributeForBlockItem);
                    if (blockItem)
                    {   //take only entities inherits(first param is true) from type filterNestedObjects (i.e. with property Area)   
                        //btr = br.XGetObjectIds(true, filterNestedObjects);
                        ids.Add(br.ObjectId);
                    }
                    else //take only filterObjects
                    {
                        var btr = br.XGetObjects(tr, filterTypes);
                        if (btr.Count != 0) GetObjects(btr, filterTypes, keyAttributeForBlockItem, filterNestedTypes, true);
                    }
                }
                tr.Commit();
            }
            return ids.AsEnumerable();
        }

        public static void ReportProgress()
        {
            //args.Worker.ReportProgress(percent > 100 ? 100 : percent); //, String.Format("{0}{1}", TaskArgs.Title, dataType.Name
        }
    }
}