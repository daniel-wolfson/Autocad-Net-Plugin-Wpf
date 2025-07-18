using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
using Microsoft.Practices.Unity;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Windows;
using Autodesk.AutoCAD.Geometry;
using Autodesk.Windows;

using Intellidesk.AcadNet.Services.Interfaces;
using Intellidesk.Infrastructure;

using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Database = Autodesk.AutoCAD.DatabaseServices.Database;
using Exception = System.Exception;
using OpenFileDialog = Autodesk.AutoCAD.Windows.OpenFileDialog;
using SaveFileDialog = Autodesk.AutoCAD.Windows.SaveFileDialog;

using System.Collections;
using System.Diagnostics;

//using Excel = Microsoft.Office.Interop.Excel;
//using Microsoft.Office.Interop;

namespace Intellidesk.AcadNet.Services
{
    //public class Tuple<T1, T2> 
    //{
    //    public Tuple(T1 item1)
    //    {
    //        Item1 = item1;
    //    }

    //    public T1 Item1 { get; private set; }
    //    public T2 Item2 { get; private set; }
    //    // implementation details 
    //} 

    // partialpublic delegate string[] LayerFilterAction(int num);
    public delegate void ToolsReadObjectsEventHandler(ObjectId sender, ActionArguments args, bool isNested = false);
    public delegate void ToolsParseBlockEventHandler(ObjectId sender, ActionArguments args);
    public delegate List<ObjectId> ToolsReadObjectsFilterEventHandler(List<ObjectId> sender);
    public delegate bool ToolsTaskEventHandler(ActiveTaskDialog sender, TaskDialogCallbackArgs args);

    public class ToolsDataObject
    {
        public ObjectId ObjectId { get; set; }
        public ActionArguments Args { get; set; }
    }

    public static class OperationServiceExt
    {

    }

    public partial class OperationService : IOperationService
    {
        private IUnityContainer _unityContainer;
        private IPluginManager _pluginManager;

        public static int instCount = 0;
        //Objects for future operations, for example: Remove objects, Undo
        public List<ObjectId> CurrentObjects = new List<ObjectId>();
        //private readonly List<ObjectId> Ids = new List<ObjectId>();
        //Objects for future drawing
        public List<Entity> DraftObjects = new List<Entity>();
        public RegAppTableRecord RegApp { get; set; }
        public int GroupId = 1;

        //Events of method ReadObjects for all types elements
        public event ToolsReadObjectsEventHandler ToolsReadObjectsEvent;
        public event ToolsParseBlockEventHandler ToolsParseBlockEvent;
        public event ToolsReadObjectsFilterEventHandler ToolsReadObjectsFilterEvent;
        public event ToolsReadObjectsEventHandler ToolsReadObjectsLineEvent;
        public event ToolsReadObjectsEventHandler ToolsReadObjectsPolylineEvent;
        public event ToolsReadObjectsEventHandler ToolsReadObjectsCircleEvent;
        public event ToolsReadObjectsEventHandler ToolsReadObjectsEllipseEvent;
        public event ToolsReadObjectsEventHandler ToolsReadObjectsMtextEvent;

        //public event ToolsTaskEventHandler ToolsTaskEvent;
        public Entity CurrentObject { get; set; }

        public ObjectId CurrentLineType
        {
            get { return Doc.Database.Celtype; }
            set { Doc.Database.Celtype = value; }
        }

        public Editor Ed { get { return Doc.Editor; } }

        public Database Db { get { return Doc.Database; } }

        public Document Doc { get { return Application.DocumentManager.MdiActiveDocument; } }

        #region "ctor"

        public OperationService(IUnityContainer container)
        {
            _unityContainer = container;
            instCount += 1;
            _pluginManager = _unityContainer.Resolve<IPluginManager>();
            _pluginManager.LibName = Assembly.GetExecutingAssembly().GetName().Name;

            //var editService = unityContainer.Resolve<IEditService>();
            //editService.OperationService = this;
            ////unityContainer.BuildUp(typeof(IEditService), editService);
            //editService.AddRegAppTableRecord(ProjectManager.Name);

            //Log.WriteMessageEvent += OnWriteMessage;
        }

        #endregion

        public void OnWriteMessage(string message, params object[] parameters)
        {
            if (!String.IsNullOrEmpty(message))
                Ed.WriteMessage(message, parameters);
            //message.XWriteMessageToCADConsole();
        }

        public string GetLineType(string lineTypeName)
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
                                Log.Add(ex);
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

        public ObjectId GetLineTypeObjectId(string value) //???
        {
            dynamic lineTypeIDs = SymbolUtilityServices.GetLinetypeByLayerId(Db);

            foreach (var lineTypeID in lineTypeIDs)
            {
                if (((LinetypeTableRecord)lineTypeID).Name == value)
                    return ((LinetypeTableRecord)lineTypeID).ObjectId;
            }
            return ObjectId.Null;
        }

        #region "Document operations: Open, Close, Find, Dialog"

        // * Operations with Documents
        //-- Current-Object-Set for the implementation of subsequent operations on the objects in the set
        //-- public List<Polyline> CurrentObjectSet = new List<Polyline>();
        public string BrowserInvoke(string url, OptionsDocument tOptions)
        {
            var result = "";
            try
            {
                //https://msdn.microsoft.com/en-us/library/aa752084(v=vs.85).aspx
                //SHDocVw.InternetExplorer IE = new SHDocVw.InternetExplorer();

                var iExplorerInstances = new SHDocVw.ShellWindows();
                if (iExplorerInstances.Count > 0)
                {
                    IEnumerator enumerator = iExplorerInstances.GetEnumerator();
                    enumerator.MoveNext();
                    var iExplorer = (SHDocVw.InternetExplorer)enumerator.Current;
                    iExplorer.Navigate(url, 0x800); //0x800 means new tab
                }
                else
                {
                    Process.Start("iexplore", url);
                    //No iexplore running, use your processinfo method
                }
            }
            catch (Exception ex)
            {
                result = ex.ParseExceptionMessage();
            }
            return result;
        }

        /// <summary> Operations with Documents </summary>
        public bool DocumentAction(string tFileName, OptionsDocument tOptions)
        {
            var actionResult = true;
            var docs = Application.DocumentManager;

            if (tOptions >= OptionsDocument.SaveAll)
            {
                foreach (Document d in docs)
                {
                    switch (tOptions)
                    {
                        case OptionsDocument.CloseAllAndSaveAll:
                            d.CloseAndSave(d.Name);
                            break;
                        case OptionsDocument.CloseAll:
                            d.CloseAndDiscard();
                            break;
                        case OptionsDocument.SaveAll:
                            break;
                        case OptionsDocument.WindowsStateNormalAll:
                            d.Window.WindowState = Window.State.Normal;
                            break;
                        case OptionsDocument.WindowsStateMinimizeAll:
                            d.Window.WindowState = Window.State.Minimized;
                            break;
                        case OptionsDocument.WindowsStateMaximizeAll:
                            d.Window.WindowState = Window.State.Maximized;
                            break;
                    }
                }
                return actionResult;
            }

            //DocumentFind(tFileName);
            var doc = Application.DocumentManager.Cast<Document>().FirstOrDefault(x => x.Name.ToLower() == tFileName.ToLower());
            if (doc == null)
            {
                if (tOptions == OptionsDocument.Open | tOptions == OptionsDocument.OpenAndActive | tOptions == OptionsDocument.Load)
                {
                    try
                    {
                        doc = docs.Open(tFileName, false);
                        //-if (tOptions != OptionsDocument.Load) ConfigManager.AppSettingSetKey("Doc", tFileName);
                    }
                    catch (Exception ex)
                    {
                        Log.Add(ex);
                        actionResult = false;
                    }
                }
            }
            else
            {
                switch (tOptions)
                {
                    case OptionsDocument.CloseAndSave:
                        Doc.CloseAndSave(tFileName);
                        break;
                    case OptionsDocument.CloseAndDiscard:
                        Doc.CloseAndDiscard();
                        break;
                    case OptionsDocument.WindowsStateNormal:
                        Doc.Window.WindowState = Window.State.Normal;
                        break;
                    case OptionsDocument.WindowsStateMaximize:
                        Doc.Window.WindowState = Window.State.Maximized;
                        break;
                    case OptionsDocument.WindowsStateMinimize:
                        Doc.Window.WindowState = Window.State.Minimized;
                        break;
                }
            }

            if (doc != null & (tOptions == OptionsDocument.Active | tOptions == OptionsDocument.OpenAndActive | tOptions == OptionsDocument.Load))
            {
                docs.MdiActiveDocument = doc;
            }
            return actionResult;
        }

        /// <summary> Find and Return Document </summary>
        public Document DocumentFind(string tFullPathName)
        {
            return Application.DocumentManager.Cast<Document>()
                .FirstOrDefault(doc => tFullPathName.ToLower() == Doc.Name.ToLower());
        }

        /// <summary> Dialog Open file </summary>
        public string DocumenOpenDataDialog(string title, string tDataType, string dialogName)
        {
            string filename = null;
            var dlg = new Microsoft.Win32.OpenFileDialog { DefaultExt = ".dwg", Filter = "dwg documents (.dwg)|*.dwg" };
            var result = dlg.ShowDialog();
            if (result == true)
            {
                filename = dlg.FileName;
            }
            return filename;

            var openFileDialog = new OpenFileDialog(title, null, tDataType, dialogName, OpenFileDialog.OpenFileDialogFlags.DefaultIsFolder);

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    switch (tDataType)
                    {
                        case "Xml":
                            //LoadDataFromXml(openFileDialog1.Filename());
                            return null;
                        case "mdb":
                            return null;
                        case "dwg":
                        case "tab":
                            return openFileDialog.Filename;
                        case "dll":
                            return openFileDialog.Filename;
                    }
                }
                catch (Autodesk.AutoCAD.Runtime.Exception ex)
                {
                    Log.Add(ex);
                    Application.ShowAlertDialog("Cannot read file from disk. Original error: " + ex.Message);
                }
            }
            return null;
        }

        /// <summary> Prompt Open FileDialog </summary>
        public string PromptOpenFileDialog(string tDataType)
        {
            var pfo = new PromptOpenFileOptions("Select Points file to load") { Filter = "Points (*.csv, *.txt)|*.csv;*.txt" };
            var pr = Ed.GetFileNameForOpen(pfo);
            return "";
        }


        /// <summary> Dialog Save file </summary>
        public void DocumenSaveDataDialog(string tDataType)
        {
            var saveFileDialog1 = new SaveFileDialog("Save As file " + tDataType, Doc.Name, tDataType, "X, Intel", SaveFileDialog.SaveFileDialogFlags.DefaultIsFolder);
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    switch (tDataType)
                    {
                        case "Xml":
                            WriteDataToXml(saveFileDialog1.Filename);
                            break;
                        case "mdb":
                            break;
                        case "dwg":
                            break;
                    }

                }
                catch (Autodesk.AutoCAD.Runtime.Exception ex)
                {
                    Log.Add(ex);
                    Application.ShowAlertDialog("Cannot save file on disk. Original error: " + ex.Message);
                }
            }
        }


        /// <summary> On Documen OpenDialog </summary>
        //public void OnDocumenOpenDialog(object sender, UIControlEventArgs e)
        //{
        //    var result = DocumenOpenDataDialog("Open file dwg", "dwg", ProjectManager.Name);
        //    if (result != null) DocumentAction(result, OptionsDocument.OpenAndActive);
        //}

        /// <summary> On Documen Activated </summary>
        public void OnDocumentActivated(object sender, DocumentCollectionEventArgs e)
        {
            //Doc = Application.DocumentManager.MdiActiveDocument;
            //Db = HostApplicationServices.WorkingDatabase;
            //Ed = Doc.Editor;
            //if (Doc != null) Log.Add("", "Current document is '{0}'\n", Doc.Name);
        }

        //public void OnDocumentActivationChanged(object sender, DocumentActivationChangedEventArgs e)
        //{
        //    //if (Doc == Application.DocumentManager.MdiActiveDocument) return;
        //    //Doc = Application.DocumentManager.MdiActiveDocument;
        //}
        //public void OnDocumentToBeDeactivated(object sender, DocumentCollectionEventArgs e)
        //{
        //    //UIManager.ShowAlertDialog("OnDocumentToBeDeactivated");
        //}

        #endregion

        #region "Processes: ReadObjects, ..."

        public Dictionary<ObjectId, string> ReadObjectsDynamic(string dxfTypeNamesFilter, string layerNamesFilter, string fieldTextFilter)
        {
            var db = HostApplicationServices.WorkingDatabase;
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            var ids = new Dictionary<ObjectId, string>();

            dynamic msID = SymbolUtilityServices.GetBlockModelSpaceId(db);
            foreach (ObjectId entID in msID)
            {
                var typeName = entID.ObjectClass.DxfName;
                if (entID.ObjectClass != null && dxfTypeNamesFilter.Contains(typeName))
                {
                    var ent = (Entity)entID.Open(OpenMode.ForRead);
                    if (typeName == "MTEXT" && ((MText)ent).Text.Equals(fieldTextFilter))
                        ids.Add(entID, ((MText)ent).Text);
                    if (typeName == "TEXT" && ((DBText)ent).TextString.Equals(fieldTextFilter))
                        ids.Add(entID, ((DBText)ent).TextString);
                }
            }

                //if (ent.ColorIndex != 40) 
                //        entID.ColorIndex = 40;
            return ids;
        }

        /// <summary>
        /// Reading objects and rising events(fior example such as "event ToolsParseBlock") 
        ///  <para>and calling custom function (may be named such as OnToolsParseBlock (look to below by code) </para>
        /// <example>example: ParseObjects(CurrentObjectIds, new[] { typeof(BlockReference) }</example>
        /// <param name="readArgs"></param>
        /// <returns></returns>
        /// </summary>
        public IEnumerable<ObjectId> ReadObjects(ActionArguments readArgs = null)
        {
            if (readArgs == null) readArgs = new ActionArguments();

            //if tSelectedObjectIds equal null then it is Selected user's objects, if it not equal null then it is All selected object from model space
            var objectIds = readArgs.SelectedObjects ?? new List<ObjectId>().XGetObjects(readArgs);

            //Objects filter handler from external project
            var handlerFilter = ToolsReadObjectsFilterEvent;
            if (handlerFilter != null)
                objectIds = handlerFilter(objectIds);

            if (readArgs.Trans != null)
                //using (var tr = Db.TransactionManager.StartTransaction())
                //{
                //    args.Trans = tr;
                foreach (var objId in objectIds)
                {
                    if (objId.IsErased) continue;

                    //if (readArgs.FilterTypesOn != null && readArgs.FilterTypesOn.Count > 0
                    //    && readArgs.FilterTypesOn.Select(RXObject.GetClass).Contains(objId.ObjectClass)) continue;

                    readArgs.ProgressIndex++;
                    //if (taskArgs != null)
                    //    taskArgs.Percent = (int)((float)args.ProgressIndex / objectIds.Count * 100);

                    var handler = ToolsReadObjectsEvent;
                    if (handler != null) handler(objId, readArgs);

                    switch (objId.ObjectClass.DxfName)
                    {
                        case "INSERT":
                            var blockhandler = ToolsParseBlockEvent;
                            if (blockhandler != null)
                                blockhandler(objId, readArgs);

                            //yield return objId;

                            break;
                        case "POLYLINE":
                            handler = ToolsReadObjectsPolylineEvent;
                            if (handler != null) handler(objId, readArgs);
                            break;
                        case "LINE":
                            handler = ToolsReadObjectsLineEvent;
                            if (handler != null) handler(objId, readArgs);
                            break;
                        case "CIRCLE":
                            handler = ToolsReadObjectsCircleEvent;
                            if (handler != null) handler(objId, readArgs);
                            break;
                        case "ELLIPSE":
                            handler = ToolsReadObjectsEllipseEvent;
                            if (handler != null) handler(objId, readArgs);
                            break;
                        case "MTEXT":
                            handler = ToolsReadObjectsMtextEvent;
                            if (handler != null) handler(objId, readArgs);
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
            //    tr.Commit();
            //}
            //Log.Add("", "Parser: Count processed objects: {0}", objectIds.Count);
            return objectIds;
        }

        public DBObject GetObject(string dxfTypeNamesFilter, string layerNamesFilter, string fieldTextFilter)
        {
            return GetObjects(dxfTypeNamesFilter, layerNamesFilter, fieldTextFilter).XCast<MText>().FirstOrDefault();
        }
        public IEnumerable<ObjectId> GetObjects<T>(string layerNamesFilter, string fieldTextFilter) where T : DBObject
        {
            var typeNamesFilter = typeof(T).Name.ToUpper() == "DBTEXT" ? "TEXT" : typeof(T).Name.ToUpper();
            return GetObjects(typeNamesFilter, layerNamesFilter, fieldTextFilter);
        }

        public IEnumerable<ObjectId> GetObjects(Type[] types, string layerNamesFilter, string fieldTextFilter)
        {
            var typeNamesFilter = String.Join(",", types.Select(x => (x.Name.ToUpper() == "DBTEXT") ? "TEXT" : x.Name.ToUpper()).ToArray());
            return GetObjects(typeNamesFilter, layerNamesFilter, fieldTextFilter);
        }

        public IEnumerable<ObjectId> GetObjects(string dxfTypeNamesFilter, string layerNamesFilter, string fieldTextFilter)
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

        public IEnumerable<ObjectId> GetObjects(List<ObjectId> selectedObjects,
                                                    Type[] filterTypes = null, string keyAttributeForBlockItem = "",
                                                    Type[] filterNestedTypes = null, bool isNested = false)
        {
            List<ObjectId> ids = new List<ObjectId>();
            using (var tr = Db.TransactionManager.StartTransaction())
            {
                if (selectedObjects == null)
                {
                    var bt = (BlockTable)(tr.GetObject(Db.BlockTableId, OpenMode.ForRead));
                    var btr = (BlockTableRecord)bt[BlockTableRecord.ModelSpace].GetObject(OpenMode.ForRead);
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
        public void ReportProgress()
        {
            //args.Worker.ReportProgress(percent > 100 ? 100 : percent); //, String.Format("{0}{1}", TaskArgs.Title, dataType.Name
        }
        #endregion

        #region "External operations: Xml, Dwg, Db"

        //LoadDataFromDwg
        public bool LoadDataFromDwg(string tBlockName = "")
        {
            //Handles cb_blk_attrObj1.LoadAttrFromDwg_Event
            var blockRefs = new List<ObjectId>();
            try
            {
                using (var tr = Db.TransactionManager.StartTransaction())
                {
                    var pntXcoll = new List<double>();
                    var pntYcoll = new List<double>();
                    var bt = (BlockTable)tr.GetObject(Db.BlockTableId, OpenMode.ForRead);

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
                        Doc.Editor.WriteMessage("LoadDataFromDwg : Block not found! Loaded Data default");
                        //LoadDataDefault();
                        //isBlockFound = False
                        return false;
                    }
                    tr.Commit();
                }

            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                Log.Add(ex); //Doc.Editor.WriteMessage("LoadDataFromDwg" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
            return false;
        }

        //LoadDataFromDwg
        public void ReadDataFromXml(string tName = "")
        {
            //Throw New NotImplementedException("Xml Data not readed")
            try
            {
                //LoadDataDefault();
                var rootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().FullName);
                var mySettings = new XmlReaderSettings { IgnoreComments = true, IgnoreWhitespace = true };
                using (var reader = XmlReader.Create((String.IsNullOrEmpty(tName) ? rootPath + "UtilDat.xml" : tName), mySettings))
                {
                    reader.MoveToContent();
                    reader.Read();
                    //for (int fieldId = 0; fieldId <= TagNames.Count - 1; fieldId++)
                    //{
                    //    if (reader.LocalName == TagNames(fieldId))
                    //    {
                    //        TagValues(fieldId) = reader.ReadString();
                    //    }
                    //    reader.Read();
                    //}
                }

            }
            catch (Exception ex)
            {
                Log.Add(ex); //Doc.Editor.WriteMessage("LoadDataFromXml " + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public object LoadDataFromMdb(string tName = "", string tFilter = "", string tSort = "")
        {
            try
            {
                //const string connectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + "RootPath" + "QW.MDB";
                //using (var connection = new System.Data.OleDb.OleDbConnection(connectionString))
                //{
                //    var command = new System.Data.OleDb.OleDbCommand("Select * from Table1" + (string.IsNullOrEmpty(tFilter) ? "" : " Where " + tFilter) + (string.IsNullOrEmpty(tSort) ? "" : " By " + tSort), connection);
                //    connection.Open();
                //    var reader = command.ExecuteReader();
                //    while (reader != null && reader.Read())
                //    {
                //        Console.WriteLine(reader[0].ToString());
                //    }
                //    if (reader != null) reader.Close();
                //}
                return true;

            }
            catch (Exception ex)
            {
                Log.Add(ex);
                MessageBox.Show(ex.Message);
                return false;
            }
        } //LoadDataFromMdb

        public string GetValueValid(string tVal, string tValDefault)
        {
            //    'Throw New NotImplementedException
            //    Dim dblVal As Double = Nothing
            //    Try
            //        dblVal = CType(tVal, Double)
            //        tVal = tVal
            //    Catch ex As InvalidCastException
            //        tVal = tValDefault
            //    End Try
            return null;
        }

        // Write Data from DataSet To Xml file "UtilDat.xml"
        public void WriteDataToXml(string tName = "")
        {
            try
            {
                var rootPath = _pluginManager.RootPath;//@"C:\Users\dimitryx\My Projects\X\Projects\"
                var settings = new XmlWriterSettings { Indent = true, IndentChars = "    " };
                using (var writer = XmlWriter.Create((String.IsNullOrEmpty(tName) ? rootPath + "MYIL.xml" : tName), settings))
                {
                    var fileName = Path.GetFileName("Block_Name_U5");
                    if (fileName != "")
                        writer.WriteStartElement(fileName.Replace(" ", "_"));
                    for (var fieldId = 0; fieldId <= CurrentObjects.Count - 1; fieldId++)
                    {
                        var extents3D = ((Entity)CurrentObjects[fieldId].GetObject(OpenMode.ForRead)).GeometricExtents;
                        writer.WriteElementString("Xmin", extents3D.MinPoint.X.ToString(CultureInfo.InvariantCulture));
                        writer.WriteElementString("Ymin", extents3D.MinPoint.Y.ToString(CultureInfo.InvariantCulture));
                        writer.WriteElementString("Xmax", extents3D.MaxPoint.X.ToString(CultureInfo.InvariantCulture));
                        writer.WriteElementString("Ymax", extents3D.MaxPoint.Y.ToString(CultureInfo.InvariantCulture));
                    }
                    writer.WriteEndElement();
                    writer.Flush();
                }

            }
            catch (Exception ex)
            {
                Log.Add(ex); //Log.Add("Error", ProjectManager.NameMsg + MethodBase.GetCurrentMethod().Name, ex.Message, ex.Source);
            }
        }

        public void LoadDataFromXml(string fileName)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load("test-doc.xml");

            //string nodepath = "/SubmissionTypes/SubmissionType[@typename='" + SubmissionType + "']";
            //XmlNode node = xmlDoc.SelectSingleNode(nodepath); 

            XmlNodeList userNodes = xmlDoc.SelectNodes("//users/user");
            if (userNodes != null)
                foreach (XmlNode userNode in userNodes)
                {
                    if (userNode.Attributes != null)
                    {
                        var age = Int32.Parse(userNode.Attributes["Xmin"].Value);
                        userNode.Attributes["Xmin"].Value = Convert.ToString(age + 1);
                    }
                }
            xmlDoc.Save("test-doc.xml");
        }

        #endregion
    }

    public partial class OperationService //TEST
    {
        public void EntityFilter()
        {
            var values = new[]{
                        new TypedValue(-4,"<or"),
                        new TypedValue(-4,"<and"),
                        new TypedValue(0,"CIRCLE"),
                        new TypedValue((int)DxfCode.Real,1.0),
                        new TypedValue(-4,"and>"),
                        new TypedValue(-4,"<and"),
                        new TypedValue(0,"Line"),
                        new TypedValue((int)DxfCode.LayerName, "ABC"),
                        new TypedValue(-4,"and>"),
                        new TypedValue(-4,"or>")
    };
            //adds objects to a selection set
            //by prompting the user to select ones to add.

            //int n = Tools.GetSelection(values).Count;
            //Tools.Editor.WriteMessage(n.ToString());
        }

        public void MakeGroup()
        {
            var res = Ed.GetSelection();//Selects objects 
            var ss = res.Value; //Gets a selection set 

            //Gets an array of object IDs
            //representing the selected objects
            var ids = ss.GetObjectIds();

            //Creates a Group object named grouptest.
            var gp = new Group("grouptest", true);
            // Appends the objects whose objectIds are in 
            //the ids array to the group.
            gp.Append(new ObjectIdCollection(ids));

            //Tools.AddDictionaryObject("ASDK_GROUPTEST", gp, Tools.Database.GroupDictionaryId);

            //Use the AddDictionaryObject() function to add the 
            //group object to AutoCAD database.
        }

        public void MakeXRecord()
        {
            var rec = new Xrecord
            {
                Data = new ResultBuffer(
                    new TypedValue((int)DxfCode.Text,
                                   "This is a test"),
                    new TypedValue((int)DxfCode.Int8, 0),
                    new TypedValue((int)DxfCode.UcsOrg,
                                   new Point3d(0, 0, 0))
                    )
            }; //Creates a Xrecord object
            //Use the Data property of Xreord to 
            //set the contents of the Xrecord object.

            //Tools.AddDictionaryObject("test", rec, Tools.Database.NamedObjectsDictionaryId);

            //Use the AddDictionaryObject() function to add the 
            //Xrcord object to AutoCAD database.

            //list the entries we just added
            foreach (TypedValue rb in rec.Data)
            {
                //Tools.Editor.WriteMessage(string.Format("TypedCode={0}, Value={1}\n", rb.TypeCode, rb.Value));
            }
        }

        public void MakeXData()
        {
            //Lines line = new Lines(new Point3d(0, 0, 0),
            //                 new Point3d(100, 100, 0));
            ////Creates a line using the Lines 
            ////class in DOTNETARX
            //RegAppTableRecord app = new RegAppTableRecord();
            ////add some xdata on the line, first 
            ////have to register an app
            //Tools.Name = "MyApp";
            //Tools.AddSymbolTableRecord(app,
            //     Tools.Database.RegAppTableId);
            ////Use the AddSymbolTableRecord function 
            ////to add the app to the RegAppTable.

            //line.XData = new ResultBuffer(new
            //   TypedValue((int)DxfCode.ExtendedDataRegAppName,
            //                                         "MyApp"),
            //   new TypedValue((int)DxfCode.ExtendedDataAsciiString,
            //                          "This is some xdata string"));
            ////Set the XData property of the line
            //Tools.AddEntity(line);//Adds the line 
            ////to database

            //// list the entries we just added
            //foreach (TypedValue rb in line.XData)
            //{
            //    Tools.Editor.WriteMessage(string.Format("TypedCode={0}, Value={1}\n", rb.TypeCode, rb.Value));
            //}
        }

        public double YMax(BlockTableRecord btr)
        {
            return btr.Cast<ObjectId>()
                    .Select(n => n.GetObject(OpenMode.ForRead) as Entity)
                    .Where(n => n != null && !(n is AttributeDefinition))
                    .Max(n => n.GeometricExtents.MaxPoint.Y);
        }

        // Adds a custom dictionary in the extension dictionary of
        // selected objects, but this time using dynamic capabilities
        // with LINQ. Conceptually simpler, but with some performance
        // overhead, as we're using two queries: one to get entities
        // without extension dictionaries (and then add them) and the
        // other to get entities with extension dictionaries.

        public void AddMyDict2()
        {
            PromptSelectionResult res = Application.DocumentManager.MdiActiveDocument.Editor.GetSelection();
            if (res.Status != PromptStatus.OK)
                return;

            // Query for ents in selset without ExtensionDictionaries
            var noExtDicts =
              from ent in res.Value.GetObjectIds().Cast<dynamic>()
              where ent.ExtensionDictionary == ObjectId.Null
              select ent;

            // Add extension dictionaries
            foreach (dynamic ent in noExtDicts)
                ent.CreateExtensionDictionary();

            // Now we've added the ext dicts, we add our dict to each

            var noMyDicts =
              from ent in res.Value.GetObjectIds().Cast<dynamic>()
              where !ent.ExtensionDictionary.Contains("MyDict")
              select ent.ExtensionDictionary;

            foreach (dynamic dict in noMyDicts)
                dict.SetAt("MyDict", new DBDictionary());
        }

        public void InfoUsingLINQ()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            dynamic bt = db.BlockTableId;
            // Dynamic .NET loop iteration

            ed.WriteMessage("\n*** BlockTableRecords in this DWG ***");
            foreach (dynamic btr in bt)
                ed.WriteMessage("\n" + btr.Name);

            // LINQ query - returns startpoints of all lines
            ed.WriteMessage("\n\n*** StartPoints of Lines in ModelSpace ***");

            dynamic ms = SymbolUtilityServices.GetBlockModelSpaceId(db);
            var lineStartPoints =
              from ent in (IEnumerable<dynamic>)ms
              where ent.IsKindOf(typeof(Line))
              select ent.StartPoint;

            foreach (Point3d start in lineStartPoints)
                ed.WriteMessage("\n" + start.ToString());

            // LINQ query - all entities on layer '0'
            ed.WriteMessage("\n\n*** Entities on Layer 0 ***");

            var entsOnLayer0 =
              from ent in (IEnumerable<dynamic>)ms
              where ent.Layer == "0"
              select ent;

            foreach (dynamic e in entsOnLayer0)
                ed.WriteMessage("\nHandle=" + e.Handle.ToString() + ", ObjectId=" +
                    ((ObjectId)e).ToString() + ", Class=" + e.ToString());

            ed.WriteMessage("\n\n");
            // Using LINQ with selection sets

            PromptSelectionResult res = ed.GetSelection();
            if (res.Status != PromptStatus.OK)
                return;

            // Select all entities in selection set that have an object
            // called "MyDict" in their extension dictionary

            var extDicts =
              from ent in res.Value.GetObjectIds().Cast<dynamic>()
              where ent.ExtensionDictionary != ObjectId.Null &&
                ent.ExtensionDictionary.Contains("MyDict")
              select ent.ExtensionDictionary.Item("MyDict");

            // Erase our dictionary
            foreach (dynamic myDict in extDicts)
                myDict.Erase();
        }
    }

}
