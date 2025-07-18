using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Windows;
using ID.Infrastructure;
using ID.Infrastructure.Extensions;
using ID.Infrastructure.Interfaces;
using Intellidesk.AcadNet.Common.Enums;
using Intellidesk.AcadNet.Common.Extensions;
using Intellidesk.AcadNet.Services.Extentions;
using Prism.Events;
using SHDocVw;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Threading;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Exception = System.Exception;
using SaveFileDialog = Autodesk.AutoCAD.Windows.SaveFileDialog;

namespace Intellidesk.AcadNet.Common.Internal
{
    public static class Documents
    {
        private static Dispatcher UiDispatcher { get; set; }
        private static Dispatcher BackgroundDispatcher { get; set; }

        public static IPluginSettings PluginSettings
        {
            get { return Plugin.GetService<IPluginSettings>(); }
        }

        private static readonly IEventAggregator _eventAggregator;

        private static Editor Ed { get { return Doc.Editor; } }
        private static Database Db { get { return Doc.Database; } }
        private static Document Doc { get { return acadApp.DocumentManager.MdiActiveDocument; } }

        public static ObjectId CurrentLineType
        {
            get { return Doc.Database.Celtype; }
            set { Doc.Database.Celtype = value; }
        }

        static Documents()
        {
            _eventAggregator = Plugin.GetService<IEventAggregator>();

            UiDispatcher = Dispatcher.CurrentDispatcher;
            new Thread(() =>
            {
                BackgroundDispatcher = Dispatcher.CurrentDispatcher;
                Dispatcher.Run();
            }).Start();
        }

        public static void OnWriteMessage(string message, params object[] parameters)
        {
            if (!string.IsNullOrEmpty(message))
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
                                Plugin.Logger.Error($"{nameof(Documents)}.{nameof(GetLineType)} error: ", ex);
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

        #region "Document operations: Open, Close, Find, Dialog"

        // * Operations with Documents
        //-- Current-Object-Set for the implementation of subsequent operations on the objects in the set
        //-- public List<Polyline> CurrentObjectSet = new List<Polyline>();
        public static string BrowserInvoke(string url, DocumentOptions tOptions)
        {
            var result = "";
            try
            {
                //https://msdn.microsoft.com/en-us/library/aa752084(v=vs.85).aspx
                //SHDocVw.InternetExplorer IE = new SHDocVw.InternetExplorer();

                var iExplorerInstances = new ShellWindows();
                if (iExplorerInstances.Count > 0)
                {
                    IEnumerator enumerator = iExplorerInstances.GetEnumerator();
                    enumerator.MoveNext();
                    var iExplorer = (InternetExplorer)enumerator.Current;
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
                result = ex.ToStringMessage();
            }
            return result;
        }

        /// <summary> Operations with Documents </summary>
        public static bool DocumentAction(string tFileName, DocumentOptions tOptions)
        {
            var actionResult = true;

            if (tOptions >= DocumentOptions.SaveAll)
            {
                foreach (Document doc in acadApp.DocumentManager)
                {
                    switch (tOptions)
                    {
                        case DocumentOptions.CloseAllAndSaveAll:
                            doc.CloseAndSave(doc.Name);
                            break;
                        case DocumentOptions.CloseAll:
                            doc.CloseAndDiscard();
                            break;
                        case DocumentOptions.SaveAll:
                            break;
                        case DocumentOptions.WindowsStateNormalAll:
                            doc.Window.WindowState = Window.State.Normal;
                            break;
                        case DocumentOptions.WindowsStateMinimizeAll:
                            doc.Window.WindowState = Window.State.Minimized;
                            break;
                        case DocumentOptions.WindowsStateMaximizeAll:
                            doc.Window.WindowState = Window.State.Maximized;
                            break;
                    }
                }
                return actionResult;
            }

            //DocumentFind(tFileName);
            var activeDocument = acadApp.DocumentManager.Cast<Document>().FirstOrDefault(x => x.Name.ToLower() == tFileName.ToLower());
            if (activeDocument == null)
            {
                if (tOptions == DocumentOptions.Open | tOptions == DocumentOptions.OpenAndActive | tOptions == DocumentOptions.Load)
                {
                    try
                    {
                        activeDocument = acadApp.DocumentManager.Open(tFileName, false);
                        //-if (tOptions != OptionsDocument.Load) ConfigManager.AppSettingSetKey("Doc", tFileName);
                    }
                    catch (Exception ex)
                    {
                        Plugin.Logger.Error($"{nameof(Documents)}.{nameof(DocumentAction)} error: ", ex);
                        actionResult = false;
                    }
                }
            }
            else
            {
                switch (tOptions)
                {
                    case DocumentOptions.CloseAndSave:
                        activeDocument.CloseAndSave(tFileName);
                        break;
                    case DocumentOptions.CloseAndDiscard:
                        activeDocument.CloseAndDiscard();
                        break;
                    case DocumentOptions.WindowsStateNormal:
                        activeDocument.Window.WindowState = Window.State.Normal;
                        break;
                    case DocumentOptions.WindowsStateMaximize:
                        activeDocument.Window.WindowState = Window.State.Maximized;
                        break;
                    case DocumentOptions.WindowsStateMinimize:
                        activeDocument.Window.WindowState = Window.State.Minimized;
                        break;
                }
            }

            if (activeDocument != null & (tOptions == DocumentOptions.Active | tOptions == DocumentOptions.OpenAndActive | tOptions == DocumentOptions.Load))
            {
                acadApp.DocumentManager.MdiActiveDocument = activeDocument;
            }
            return actionResult;
        }

        /// <summary> Find and Return Document </summary>
        public static Document DocumentFind(string tFullPathName)
        {
            return acadApp.DocumentManager.Cast<Document>()
                .FirstOrDefault(doc => doc.Name.ToLower() == tFullPathName.ToLower());
        }

        /// <summary> Dialog Open file </summary>
        public static string DocumenOpenDataDialog(string title, string tDataType, string dialogName)
        {
            string filename = null;
            var dlg = new Microsoft.Win32.OpenFileDialog { DefaultExt = ".dwg", Filter = "dwg documents (.dwg)|*.dwg" };
            var result = dlg.ShowDialog();
            if (result == true)
            {
                filename = dlg.FileName;
            }
            return filename;

            //var openFileDialog = new OpenFileDialog(title, null, tDataType, dialogName, OpenFileDialog.OpenFileDialogFlags.DefaultIsFolder);
            //if (openFileDialog.ShowDialog() == DialogResult.OK)
            //{
            //    try
            //    {
            //        switch (tDataType)
            //        {
            //            case "Xml":
            //                //LoadDataFromXml(openFileDialog1.Filename());
            //                return null;
            //            case "mdb":
            //                return null;
            //            case "dwg":
            //            case "tab":
            //                return openFileDialog.Filename;
            //            case "dll":
            //                return openFileDialog.Filename;
            //        }
            //    }
            //    catch (Autodesk.AutoCAD.Runtime.Exception ex)
            //    {
            //        Plugin.Logger.Error($"{nameof(Documents)}.{nameof(DocumenOpenDataDialog)} error: ", ex);
            //        acadApp.ShowAlertDialog("Cannot read file from disk. Original error: " + ex.Message);
            //    }
            //}
            //return null;
        }

        /// <summary> Prompt Open FileDialog </summary>
        public static string PromptOpenFileDialog(string tDataType)
        {
            var pfo = new PromptOpenFileOptions("Select Points file to load") { Filter = "Points (*.csv, *.txt)|*.csv;*.txt" };
            var pr = Ed.GetFileNameForOpen(pfo);
            return "";
        }

        /// <summary> Dialog Save file </summary>
        public static void DocumenSaveDataDialog(string tDataType)
        {
            var saveFileDialog1 = new SaveFileDialog("Save As file " + tDataType, Doc.Name, tDataType, "X, Intel", SaveFileDialog.SaveFileDialogFlags.DefaultIsFolder);
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    switch (tDataType)
                    {
                        case "Xml":
                            Files.WriteDataToXml(saveFileDialog1.Filename);
                            break;
                        case "mdb":
                            break;
                        case "dwg":
                            break;
                    }

                }
                catch (Autodesk.AutoCAD.Runtime.Exception ex)
                {
                    Plugin.Logger.Error($"{nameof(Documents)}.{nameof(DocumenSaveDataDialog)} error: ", ex);
                    acadApp.ShowAlertDialog("Cannot save file on disk. Original error: " + ex.Message);
                }
            }
        }

        public static bool IsDwgCompatible(Type[] typeFilterOn = null, string[] attributePatternOn = null) //Rule.LsdsTypeFilterOn, Rule.LsdsAttributePatternOn
        {
            //Get implied entities //LsdsCommands.CurrentObjectIds
            var currentObjectIds = new List<ObjectId>(); // SelectManager.GetImplied();

            //Get entities from drawing space model
            currentObjectIds = currentObjectIds.XGetObjects(typeFilterOn, attributePatternOn);

            return currentObjectIds.Count != 0;
        }

        #endregion

        public static void AddProjectInfoToFile(string fileFullPath)
        {
            Database db;
            if (!File.Exists(fileFullPath)) return;

            var doc = DocumentFind(fileFullPath);
            if (doc != null)
            {
                db = doc.Database;
            }
            else
            {
                db = new Database();
                db.ReadDwgFile(fileFullPath, FileShare.ReadWrite, false, null);
            }
            AddProjectInfo(db);

            db.SaveAs(fileFullPath, DwgVersion.Current);
        }

        public static void AddProjectInfo(Database db)
        {
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                // Find the NOD in the database
                DBDictionary nod = (DBDictionary)trans.GetObject(
                            db.NamedObjectsDictionaryId, OpenMode.ForWrite);

                // We use Xrecord class to store data in Dictionaries
                Xrecord myXrecord = new Xrecord();
                myXrecord.Data = new ResultBuffer(
                        new TypedValue((int)DxfCode.Int16, ProjectTypedValues.CoordSystem),
                        new TypedValue((int)DxfCode.Text, "ITM"));

                // Create the entry in the Named Object Dictionary
                nod.SetAt(PluginSettings.Name, myXrecord);
                trans.AddNewlyCreatedDBObject(myXrecord, true);

                // Now let's read the data back and print them out
                //  to the Visual Studio's Output window
                ObjectId myDataId = nod.GetAt(PluginSettings.Name);
                Xrecord readBack = (Xrecord)trans.GetObject(myDataId, OpenMode.ForRead);

                foreach (TypedValue value in readBack.Data)
                    Debug.Print("===== OUR DATA: " + value.TypeCode.ToString()
                              + ". " + value.Value.ToString());

                trans.Commit();
            }
        }

        public static bool isNODExist(Database db)
        {
            bool valueResult = false;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                DBDictionary nod = (DBDictionary)trans.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForRead);
                valueResult = nod.Contains(PluginSettings.Name);
            }
            return valueResult;
        }

        public static bool IsDocumentLoaded(this string fullname)
        {
            if (string.IsNullOrEmpty(fullname)) return false;

            return acadApp.DocumentManager.Cast<Document>()
                .Select(doc => doc.Name.ToLower())
                .Contains(fullname.ToLower());
        }

        public static Document GetDocument(this string fullname)
        {
            if (string.IsNullOrEmpty(fullname)) return null;

            return acadApp.DocumentManager.Cast<Document>()
                .FirstOrDefault(d => string.Equals(d.Name, fullname, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}