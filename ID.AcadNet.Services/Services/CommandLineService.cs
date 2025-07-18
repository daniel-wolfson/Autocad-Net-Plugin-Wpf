using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Input;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Customization;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Intellidesk.AcadNet.Services.Interfaces;
using Intellidesk.Infrastructure;
using Intellidesk.Infrastructure.Interfaces;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Unity;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Exception = System.Exception;

namespace Intellidesk.AcadNet.Services
{
    /// <summary>
    /// This class is instantiated by AutoCAD for each document when
    /// a command is called by the user the first time in the context
    /// of a given document. In other words, non data in this class
    /// is implicitly per-document!
    /// </summary>
    /// 
    public struct CommandArguments1
    {
        public string Command { get; set; }
        public object Arguments { get; set; }
    }

    public class CommandLineService : ICommandLineService //ServiceBase<CommandLineService>, 
    {
        private IOperationService _operationService;
        private IGeomService _geomService;
        private IEditService _editService;

        public ICommand AcadRoutedCommand;
        public ITaskArguments CommandArguments;

        public CompositeCommand CommandLineProxy { get { return CommandLine.ChainCompositeCommands; } }

        private readonly CompositeCommand _saveAllCommand = new CompositeCommand();
        private IPluginManager _pluginManager;
        public ICommand SaveAllCommand { get { return _saveAllCommand; } }

        public Editor Ed { get { return Doc.Editor; } }

        public Database Db { get { return Doc.Database; } }

        public Document Doc { get { return Application.DocumentManager.MdiActiveDocument; } }

        //private IOperationService OperationService
        //{
        //    get
        //    {
        //        if (_operationService == null)
        //            _operationService = ContainerAccessor.Resolve<OperationService>();
        //        return _operationService;
        //    }
        //}
        //private IGeomService GeomService
        //{
        //    get
        //    {
        //        if (_geomService == null)
        //            _geomService = ContainerAccessor.Resolve<GeomService>();
        //        return _geomService;
        //    }
        //}
        //private IEditService EditService
        //{
        //    get
        //    {
        //        if (_editService == null)
        //            _editService = ContainerAccessor.Resolve<EditService>();
        //        return _editService;
        //    }
        //}

        //CommandBinding customCommandBinding = new CommandBinding(AcadRoutedCommand, ExecutedCustomCommand, CanExecuteCustomCommand);

        #region "ctor"

        public CommandLineService(IUnityContainer container)
        {
            _operationService = container.Resolve<IOperationService>();
            _geomService = container.Resolve<IGeomService>();
            _editService = container.Resolve<IEditService>();
            _pluginManager = container.Resolve<IPluginManager>();
            _editService.AddRegAppTableRecord(_pluginManager.Name);

            //CommandArguments = container.Resolve<ITaskArguments>();
            CommandLineProxy.RegisterCommand(SaveAllCommand);
        }

        #endregion

        #region Private Methods


        #endregion

        #region "Commands Lisp"

        public void Cancel()
        {
            Application.DocumentManager.MdiActiveDocument.SendStringToExecute(Convert.ToString(Convert.ToChar(27)), true, false, false);
        }

        public void Enter()
        {
            Application.DocumentManager.MdiActiveDocument.SendStringToExecute(Convert.ToString(Convert.ToChar(13)), true, false, false);
        }

        public void SendToExecute(string cmd, ITaskArguments args)
        {

            CommandArguments = args;
            SendToExecute(cmd);
        }

        [CommandMethod("Partner")]
        public void SendToExecute(string cmd, string cmdArg = "", int tCmdEcho = 0, int tFileDia = 0,
            bool tActivate = true, bool tWrapUpInactiveDoc = false, bool tEchoCommand = false)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Document doc = Application.DocumentManager.GetDocument(db);

            using (new SysVarOverride("CMDECHO", tCmdEcho)) //var oldCmdEcho = Application.GetSystemVariable("CMDECHO");
            {
                using (new SysVarOverride("FILEDIA", tFileDia)) //var oldFileDia = Application.GetSystemVariable("FILEDIA");
                {
                    //var manualResetEvent = new ManualResetEvent(false);
                    //var mdi = Autodesk.AutoCAD.ApplicationServices.Application.
                    //Application.DocumentManager.MdiActiveDocument;

                    if (cmdArg == "")
                    {
                        //CommandEventHandler handlerStart = (sender, e) => 
                        //    mdi.SendStringToExecute(cmd.TrimEnd() + " ", tActivate, tWrapUpInactiveDoc, tEchoCommand); 

                        //CommandEventHandler handlerEnd = null;
                        //handlerEnd = (sender, e) =>
                        //{
                        //    mdi.CommandWillStart -= handlerStart;
                        //    mdi.CommandCancelled -= handlerEnd;
                        //    mdi.CommandEnded -= handlerEnd;
                        //    manualResetEvent.Set();
                        //};
                        //mdi.CommandWillStart += handlerStart;
                        //mdi.CommandCancelled += handlerEnd;
                        //mdi.CommandEnded += handlerEnd;

                        doc.SendStringToExecute(cmd.TrimEnd() + " ", tActivate, tWrapUpInactiveDoc, tEchoCommand);
                    }
                    else
                    {
                        //LispWillStartEventHandler handlerStart = (sender, e) => SendToExecute(cmd, cmdArg);
                        //EventHandler handlerEnd = null;
                        //handlerEnd = (sender, e) =>
                        //{
                        //    mdi.LispWillStart -= handlerStart;
                        //    mdi.LispCancelled -= handlerEnd;
                        //    mdi.LispEnded -= handlerEnd;
                        //    manualResetEvent.Set();
                        //};
                        //mdi.LispWillStart += handlerStart;
                        //mdi.LispEnded += handlerEnd;
                        //mdi.LispCancelled += handlerEnd;

                        doc.SendStringToExecute("(command \"" + cmd + "\" \" " + cmdArg + "\") ", tActivate, tWrapUpInactiveDoc, tEchoCommand);

                        //manualResetEvent.WaitOne();
                    }
                }
            }
        }

        //[LispFunction("PaletteStatus", "PaletteStatusLocal")]
        public void PaletteStatus(ResultBuffer args)
        {
            if (args != null)
            {
                var strVal1 = "";
                var strVal2 = "";

                var nCnt = 0;
                foreach (var rb in args)
                {
                    if (rb.TypeCode == (int)LispDataType.Text)
                    {
                        switch (nCnt)
                        {
                            case 0:
                                strVal1 = rb.Value.ToString();
                                break;
                            case 1:
                                strVal2 = rb.Value.ToString();
                                break;
                        }

                        nCnt = nCnt + 1;
                    }
                }
                _operationService.Ed.WriteMessage("\nName: " + strVal1 + " " + strVal2);
            }

            //Application.Invoke():
            args = new ResultBuffer(
                new TypedValue((int)LispDataType.Text, "lispTest"),
                new TypedValue((int)LispDataType.Text, "3/16\"=1'-0\"\""));
            ResultBuffer retVal = Application.Invoke(args);

        }

        //[CommandMethod("reloadLinetype", CommandFlags.Session)]
        public void ReloadLinetype()
        {
            //var docManager = Application.DocumentManager;
            //var db = docManager.MdiActiveDocument.Database;

            var trans = Db.TransactionManager.StartTransaction();
            var bReload = false;

            using (trans)
            {
                var table = trans.GetObject(Db.LinetypeTableId, OpenMode.ForRead) as LinetypeTable;
                if (table != null && table.Has("CENTER")) bReload = true;
            }

            var fileDia = (Int16)
                Application.GetSystemVariable("FILEDIA");
            Application.SetSystemVariable("FILEDIA", 0);

            //reload using linetype command...
            var acadObject = Autodesk.AutoCAD.ApplicationServices.Application.AcadApplication;
            var activeDocument = acadObject.GetType().InvokeMember("ActiveDocument", BindingFlags.GetProperty, null, acadObject, null);

            var dataArry = new object[1];
            if (bReload)
                dataArry[0] = "-linetype Load CENTER\nacad.lin\nYes\n ";
            else
                dataArry[0] = "-linetype Load CENTER\nacad.lin\n ";

            activeDocument.GetType().InvokeMember("SendCommand", BindingFlags.InvokeMethod, null, activeDocument, dataArry);
            Application.SetSystemVariable("FILEDIA", fileDia);
        }

        // P/Invoke acedInvoke:
        //[System.Security.SuppressUnmanagedCodeSecurity]
        //[DllImport("acad.exe", CallingConvention = CallingConvention.Cdecl)]
        //extern private int acedInvoke(IntPtr rbIn, out IntPtr rbOut);

        public ResultBuffer InvokeLisp(ResultBuffer args)
        {
            //IntPtr rb = IntPtr.Zero;
            //ResultBuffer stat = Application.Invoke(args); //(args.UnmanagedObject, out rb)
            //if (stat == (int)PromptStatus.OK && rb != IntPtr.Zero)
            //    return (ResultBuffer)DisposableWrapper.Create(typeof(Resu​ltBuffer), rb, true);
            return null;
        }

        [CommandMethod("Bar")]
        public void Bar()
        {
            var args = new ResultBuffer(
                new TypedValue((int)LispDataType.Text, "lispTest"),
                new TypedValue((int)LispDataType.Text, "3/16\"=1'-0\"\""));
            var retVal = Application.Invoke(args);
            _operationService.Doc.Editor.WriteMessage("\nretVal:" + retVal.AsArray()[0].Value);
        }

        public void Regen()
        {
            // Redraw the drawing
            Application.UpdateScreen();
            Application.DocumentManager.MdiActiveDocument.Editor.UpdateScreen();

            // Regenerate the drawing
            Application.DocumentManager.MdiActiveDocument.Editor.Regen();
        }

        public void WriteMessage(string s)
        {
            // A simple helper to write to the command-line
            var doc = Application.DocumentManager.MdiActiveDocument;
            doc.Editor.WriteMessage(s);
        }

        public void Alert(string alert)
        {
            Application.ShowAlertDialog(alert);
        }

        #endregion

        #region "Commands Menu"

        public void LoadCuixMenu(ref Assembly assem, string sectionName = "")
        {
            if (String.IsNullOrEmpty(sectionName))
                sectionName = Path.GetFileNameWithoutExtension(assem.Location);
            var myCuiFile = (Path.GetDirectoryName(assem.Location) + "\\" + sectionName + ".cuix").Replace("\\", "\\\\");
            var myCuiFileToSend = sectionName + ".cuix";
            //Dim currentWorkSpace As String = CType(Application.GetSystemVariable("WSCURRENT"), String)
            var mainCui = Application.GetSystemVariable("MENUNAME") + ".cuix";
            var cs = new CustomizationSection(mainCui);

            PartialCuiFileCollection pcfc = cs.PartialCuiFiles;
            if (pcfc.Contains(myCuiFile) | pcfc.Contains(myCuiFileToSend.ToLower()))
            {
                _operationService.Ed.WriteMessage(Convert.ToChar(10).ToString(CultureInfo.InvariantCulture) + "Customization file \"" + myCuiFileToSend + "\" already loaded.");
            }
            else
            {
                if (File.Exists(sectionName + ".cuix"))
                {
                    try
                    {
                        this.SendToExecute("_CUILOAD", myCuiFileToSend);
                        //LoadLispCmd("_WSCURRENT", "AutoCAD Classic")
                    }
                    catch (Exception ex)
                    {
                        Log.Add(ex); //Log.Add("Error: " + MethodBase.GetCurrentMethod().GetFullNameBundle(), "\nCustomization file \"" + myCuiFile + "\" error.");
                    }
                }
                else
                {
                    var msg = "Error: " + MethodBase.GetCurrentMethod().GetFullNameBundle() + "\nCustomization file \"" + myCuiFile + "\" does not exist - building it.";
                    Log.Add(new Exception(msg)); //Log.Add("Error: " + MethodBase.GetCurrentMethod().GetFullNameBundle(), "\nCustomization file \"" + myCuiFile + "\" does not exist - building it.");
                }
            }
        }

        #endregion

        #region "Commands View, Zoom"

        //Zoom All
        public void Zoom()
        {
            SendToExecute("._zoom _all ");
        }

        //Zoom Window
        public void Zoom(Point3d[] points)
        {
            var min2D = new Point2d(points[0].X - (points[1].X - points[0].X) / 5, points[0].Y);
            var max2D = new Point2d(points[1].X + (points[1].X - points[0].X) / 5, points[1].Y);
            var view = new ViewTableRecord
            {

                CenterPoint = new Point2d(min2D.X + (max2D.X - min2D.X) / 2, min2D.Y + (max2D.Y - min2D.Y) / 2),
                Height = max2D.Y - min2D.Y,
                Width = max2D.X - min2D.X
            };
            Application.DocumentManager.MdiActiveDocument.Editor.SetCurrentView(view);
        }

        //Zoom Window
        public void Zoom(double tXmin, double tYmin, double tXmax, double tYmax)
        {
            var min2D = new Point2d(tXmin - (tXmax - tXmin) / 5, tYmin);
            var max2D = new Point2d(tXmax + (tXmax - tXmin) / 5, tYmax);
            var view = new ViewTableRecord
            {
                CenterPoint = new Point2d(min2D.X + (max2D.X - min2D.X) / 2, min2D.Y + (max2D.Y - min2D.Y) / 2),
                Height = max2D.Y - min2D.Y,
                Width = max2D.X - min2D.X
            };

            Application.DocumentManager.MdiActiveDocument.Editor.SetCurrentView(view); Thread.Sleep(500);
        }

        //Zoom List of entity
        public void Zoom(List<Entity> tObj)
        {
            Zoom(new ObjectIdCollection(tObj.Select(x => x.ObjectId).ToArray()));
        }
        public void Zoom(ObjectId objectId)
        {
            Zoom(new ObjectIdCollection { objectId });
        }
        public void Zoom(ObjectIdCollection tObjIds)
        {
            var db = HostApplicationServices.WorkingDatabase;
            var doc = Application.DocumentManager.MdiActiveDocument; //.GetDocument(db);
            var pntMin = Point3d.Origin;
            var pntMax = Point3d.Origin;

            if (tObjIds.Count > 0)
            {
                try
                {
                    using (DocumentLock l = doc.LockDocument())
                    {
                        using (var tr = db.TransactionManager.StartTransaction())
                        {
                            var flg = true;
                            foreach (ObjectId objId in tObjIds)
                            {
                                if (objId != ObjectId.Null)
                                {
                                    var obj = tr.GetObject(objId, OpenMode.ForRead);
                                    if (flg)
                                    {
                                        if (obj.Bounds != null)
                                        {
                                            pntMin = obj.Bounds.Value.MinPoint;
                                            pntMax = obj.Bounds.Value.MaxPoint;
                                        }
                                        flg = false;
                                    }
                                    if (obj.Bounds != null)
                                    {
                                        pntMin = new Point3d(Math.Min(obj.Bounds.Value.MinPoint.X, pntMin.X),
                                            Math.Min(obj.Bounds.Value.MinPoint.Y, pntMin.Y), 0);
                                        pntMax = new Point3d(Math.Max(obj.Bounds.Value.MaxPoint.X, pntMax.X),
                                            Math.Max(obj.Bounds.Value.MaxPoint.Y, pntMax.Y), 0);
                                    }
                                }
                            }

                        }
                    }

                    doc.SendToExecute("._zoom _w " +
                                      Convert.ToString(pntMin.X - (pntMax.X - pntMin.X) / 5) + "," +
                                      Convert.ToString(pntMin.Y - (pntMax.Y - pntMin.Y) / 5) + " " +
                                      Convert.ToString(pntMax.X + (pntMax.X - pntMin.X) / 5) + "," +
                                      Convert.ToString(pntMax.Y + (pntMax.Y - pntMin.Y) / 5) + " ");

                    //Thread.Sleep(500);
                    //doc.Editor.Regen();
                    //Application.DocumentManager.MdiActiveDocument.SendStringToExecute(".redraw ", true, false, false);


                }
                catch (Exception ex)
                {
                    Log.Add(ex);
                    //Log.Add("Error", ProjectManager.NameMsg + "<XAddNew> {0}, {1}", ex.Message, ex.Source);
                }
            }


        }

        public void ViewIsometric(int iJob)
        {
            var optionsViews = Enum.GetNames(typeof(OptionsView)).ToList();
            if (Application.Version.Major > 14)
            {
                if (iJob <= optionsViews.Count())
                {
                    Application.DocumentManager.MdiActiveDocument.SendStringToExecute("_regenauto" + "\rON" + Convert.ToChar(13).ToString(CultureInfo.InvariantCulture), false, false, false);
                    // IM, 2007.02.27
                    Application.DocumentManager.MdiActiveDocument.SendStringToExecute("_-view" + "\r_" + optionsViews[iJob].ToLower() + "\r", false, false, false);
                }
            }
        }

        public void Viewport(Vector3d vec)
        {
            using (var tr = Db.TransactionManager.StartTransaction())
            {
                using (Doc.LockDocument())
                {
                    // Открываем активное видовое окно
                    var acVportTblRec = tr.GetObject(_operationService.Ed.ActiveViewportId, OpenMode.ForWrite) as ViewportTableRecord;

                    // Вращаем направление вида текущего видового экрана
                    if (acVportTblRec != null) acVportTblRec.ViewDirection = vec; // new Vector3d(-1, -1, 1);
                    _operationService.Ed.UpdateTiledViewportsFromDatabase();
                }
            }
        }

        public void UpdateScreen()
        {
            Application.UpdateScreen();
        }

        #endregion

        #region "Commands from VeiwModels"

        private void CanExecuteCustomCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            var target = e.Source as Control;
            e.CanExecute = target != null;
        }

        private void ExecutedCustomCommand(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("Custom Command Executed");
        }

        //[CommandMethod("BX")]
        public void BindXrefs()
        {
            //var doc = Application.DocumentManager.MdiActiveDocument;
            //var db = doc.Database;
            var xrefCollection = new ObjectIdCollection();

            using (var xg = Db.GetHostDwgXrefGraph(false))
            {
                int numOfNodes = xg.NumNodes;
                for (int cnt = 0; cnt < xg.NumNodes; cnt++)
                {
                    var xNode = xg.GetXrefNode(cnt);
                    if (!xNode.Database.Filename.Equals(Db.Filename))
                    {
                        if (xNode.XrefStatus == XrefStatus.Resolved)
                        {
                            xrefCollection.Add(xNode.BlockTableRecordId);
                        }
                    }
                }
            }
            if (xrefCollection.Count != 0)
                Db.BindXrefs(xrefCollection, true);
        }

        //[CommandMethod("MyGroup", "AppInters", "AppIntersLocal", CommandFlags.UsePickSet)]
        public void AppInters() // This method can have any name
        {
            _geomService.GetIntersect();
        }

        public void ExecuteOpenDwg(string filename, string searchPath)
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            var fullPath = filename.XFindFullPath(searchPath);
            if (fullPath != "" && fullPath.IsFileLoaded())
            {
                var f = Application.DocumentManager.Cast<Document>().ToList();
                //var doc = Application.DocumentManager.Cast<Document>().FirstOrDefault(x => x.Name.ToLower() == fullPath.ToLower());
                //if (doc != null) Application.DocumentManager.MdiActiveDocument = doc;
            }
            else if (fullPath == "")
            {
                var td = new Autodesk.Windows.TaskDialog
                {
                    WindowTitle = _pluginManager.Name,
                    MainInstruction = "File not found!",
                    ContentText = "Change name or path to file using the explorer and try again!",
                    MainIcon = Autodesk.Windows.TaskDialogIcon.Shield,
                    CommonButtons = Autodesk.Windows.TaskDialogCommonButtons.Ok
                };
                td.Show();
            }
            else
            {
                _operationService.DocumentAction(fullPath, OptionsDocument.OpenAndActive);
            }

            Mouse.OverrideCursor = null;
        }

        #endregion

        #region "AutoCAD defaults wizard's command"

        // Modal Command with pickfirst selection

        //[CommandMethod("MyGroup", "WizardPickFirst", "WizardPickFirstLocal", CommandFlags.Modal | CommandFlags.UsePickSet)]
        public void MyPickFirst() // This method can have any name
        {
            var result = Application.DocumentManager.MdiActiveDocument.Editor.GetSelection();
            if (result.Status == PromptStatus.OK)
            {
                //There are selected entities
                //Put your command using pickfirst set code here
            }
        }

        // Application Session Command with localized name

        //[CommandMethod("MyGroup", "WizardSessionCmd", "WizardSessionCmdLocal", CommandFlags.Modal | CommandFlags.Session)]
        public void MySessionCmd() // This method can have any name
        {
            // Put your command code here
        }

        //LispFunction is similar to CommandMethod but it creates a lisp 
        //callable function. Many return types are supported not just string
        //or integer.

        //[LispFunction("WizardLispFunction", "WizardLispFunctionLocal")]
        public int MyLispFunction(ResultBuffer args) // This method can have any name
        {
            // Put your command code here

            // Return a value to the AutoCAD Lisp Interpreter
            return 1;
        }

        #endregion
    }

    internal static class DocExtensions
    {
        public static void SendToExecute(this Document doc, string cmd, string cmdArg = "", int tCmdEcho = 0, int tFileDia = 0,
            bool tActivate = true, bool tWrapUpInactiveDoc = false, bool tEchoCommand = false)
        {
            using (new SysVarOverride("CMDECHO", tCmdEcho))
            {
                using (new SysVarOverride("FILEDIA", tFileDia))
                {
                    using (DocumentLock l = doc.LockDocument())
                    {
                        if (cmdArg == "")
                        {
                            doc.SendStringToExecute(cmd.TrimEnd() + " ", tActivate, tWrapUpInactiveDoc, tEchoCommand);
                        }
                        else
                        {
                            doc.SendStringToExecute("(command \"" + cmd + "\" \" " + cmdArg + "\") ", tActivate,
                                tWrapUpInactiveDoc, tEchoCommand);
                        }
                    }
                }
            }
        }
    }
}