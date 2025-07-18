using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Internal.Reactors;
using Autodesk.AutoCAD.PlottingServices;
using Autodesk.AutoCAD.Publishing;
using Autodesk.AutoCAD.Runtime;
using ID.Infrastructure;
using ID.Infrastructure.Commands;
using ID.Infrastructure.Extensions;
using Intellidesk.AcadNet;
using Intellidesk.AcadNet.Common.Extensions;
using Intellidesk.AcadNet.Common.Interfaces;
using Intellidesk.AcadNet.Common.Internal;
using Intellidesk.AcadNet.Common.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using Exception = System.Exception;
using PlotType = Autodesk.AutoCAD.DatabaseServices.PlotType;

// This line is not mandatory, but improves loading performances
[assembly: CommandClass(typeof(CommandPlot))]

namespace Intellidesk.AcadNet
{
    public class CommandPlot : CommandLineBase
    {
        [DllImport("acad.exe", CallingConvention = CallingConvention.Cdecl, EntryPoint = "acedTrans")]
        static extern int acedTrans(double[] point, IntPtr fromRb, IntPtr toRb, int disp, double[] result);

        [DllImport("acad.exe", CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "?acedSetCurrentVPort@@YA?AW4ErrorStatus@Acad@@PBVAcDbViewport@@@Z")]
        private static extern int acedSetCurrentVPort(IntPtr AcDbVport);

        readonly List<string> _msgs = new List<string>();

        [CommandMethod(CommandNames.MainGroup, CommandNames.PlotWindow, CommandFlags.Session | CommandFlags.UsePickSet)]
        public void PlotWindow()
        {
            PlotEngine(CommandNames.PlotWindow);
        }

        [CommandMethod(CommandNames.MainGroup, CommandNames.PlotExtents, CommandFlags.Session | CommandFlags.UsePickSet)]
        public void PlotPreview()
        {
            PlotEngine(CommandNames.PlotExtents);
        }

        private void PlotEngine(string commandName, PlotType plotType = PlotType.Window)
        {
            Extents2d win = new Extents2d();
            var doc = acadApp.DocumentManager.MdiActiveDocument;

            if (PlotFactory.ProcessPlotState == ProcessPlotState.NotPlotting)
            {
                using (var context = new CommandContext(commandName))
                {
                    if (plotType == PlotType.Window)
                    {
                        PromptPointsResult promptPointsResult = Ed.GetPoints(commandName, 2, true);
                        if (promptPointsResult.Status == PromptStatus.OK)
                        {
                            var promptResults = new Extents2d(
                                promptPointsResult.Value.FirstOrDefault().ToPoint2d(),
                                promptPointsResult.Value.LastOrDefault().ToPoint2d());

                            win = promptResults.Normalize().GetViewExtents();

                            var sise =
                                $"size (width={win.MaxPoint.X - win.MinPoint.X}, height={win.MaxPoint.Y - win.MinPoint.Y})";
                            doc.Editor.WriteMessage($"\n{CommandNames.UserGroup}: selected window {sise}\n");
                        }
                    }

                    PreviewEndPlotStatus plotStatus = PreviewEndPlotStatus.Normal;
                    PlotSettingsValidator psv = PlotSettingsValidator.Current;

                    var pc = psv.MakePlotInfo(Db.CurrentSpaceId, plotType, win, new Extents2d(0, 0, 0, 0));
                    if (!string.IsNullOrEmpty(pc.Value.Value.Error))
                    {
                        acadApp.ShowAlertDialog($"{CommandNames.UserGroup}: {pc.Value.Value.Error}!");
                        context.Clean();
                        context.Cancel();
                        return;
                    }

                    var ps = pc.Value.Key.ValidatedSettings;
                    doc.Editor.WriteMessage(
                        $"\n{CommandNames.UserGroup}: Current device name: {ps.PlotConfigurationName}");
                    doc.Editor.WriteMessage($"\n{CommandNames.UserGroup}: " +
                                            $"Current plot area width: {ps.PlotWindowArea.MaxPoint.X - ps.PlotWindowArea.MinPoint.X}; " +
                                            $"height: {ps.PlotWindowArea.MaxPoint.Y - ps.PlotWindowArea.MinPoint.Y}");
                    doc.Editor.WriteMessage(
                        $"\n{CommandNames.UserGroup}: Current paper width: {pc.Value.Value.MediaWidth}, height: {pc.Value.Value.MediaHeight}");

                    PromptIntegerOptions promptIntOpts =
                        new PromptIntegerOptions(
                            $"\n{commandName}. PaperSize: {pc.Value.Value.MediaWidth}, {pc.Value.Value.MediaHeight}. Count copies: ")
                        {
                            DefaultValue = 1,
                            UseDefaultValue = true,
                            AllowNone = true,
                            AllowZero = false,
                            AllowNegative = false,
                        };

                    PromptIntegerResult promptIntResult = Ed.GetInteger(promptIntOpts);
                    if (promptIntResult.Status == PromptStatus.OK)
                    {
                        var copies = promptIntResult.Value;
                        using (PlotEngine pre = PlotFactory.CreatePreviewEngine((int)PreviewEngineFlags.Plot))
                        {
                            plotStatus = pre.PlotOrPreview(pc.Value.Key, true);
                        }

                        if (plotStatus == PreviewEndPlotStatus.Plot)
                        {
                            PlotEngine ple = PlotFactory.CreatePublishEngine();
                            plotStatus = ple.PlotOrPreview(pc.Value.Key, false, copies);
                        }
                    }

                    if (plotStatus == PreviewEndPlotStatus.Cancel || promptIntResult.Status != PromptStatus.OK)
                        Ed.WriteMessage($"\n{CommandNames.PlotWindow}: cancelled of user.");

                    context.Clean();
                    context.Cancel();
                }
            }
            else
            {
                //if (PlotFactory.ProcessPlotState == ProcessPlotState.BackgroundPlotting)
                //{
                //    if (MessageBox.Show($"\n{commandName}: another plot is in progress", "Are you cancel the process?", MessageBoxButtons.YesNo,
                //            MessageBoxIcon.Error) == DialogResult.Yes)
                //    {
                //        using (PlotEngine pre = PlotFactory.CreatePreviewEngine((int)PreviewEngineFlags.Plot))
                //        {
                //            pre.EndPlot(null);
                //            pre.Destroy();
                //        }
                //    }
                //}
                acadApp.ShowAlertDialog($"\n{commandName}: another plot is in progress!");
            }
        }

        // overload no scale factor or set the view - just make the layout
        public void LayoutAndViewport(Database db, string layoutName, out ObjectId rvpid, string deviceName, string mediaName, out ObjectId id)
        {
            // set default values
            rvpid = new ObjectId();
            bool flagVp = false; // flag to create a new floating view port
            double viewSize = (double)acadApp.GetSystemVariable("VIEWSIZE");
            double height = viewSize;
            double width = viewSize;
            Point2d loCenter = new Point2d(); // layout center point
            Point2d vpLowerCorner = new Point2d();
            Point2d vpUpperCorner = new Point2d();
            Document doc = acadApp.DocumentManager.MdiActiveDocument;
            LayoutManager lm = LayoutManager.Current;
            id = lm.CreateLayout(layoutName);

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                Layout lo = tr.GetObject(id, OpenMode.ForWrite, false) as Layout;
                if (lo != null)
                {
                    lm.CurrentLayout = lo.LayoutName; // make it current!

                    #region do some plotting settings here for the paper size...
                    ObjectId loid = lm.GetLayoutId(lo.LayoutName);

                    PlotInfo pi = new PlotInfo();
                    pi.Layout = loid;

                    PlotSettings ps = new PlotSettings(false);
                    PlotSettingsValidator psv = PlotSettingsValidator.Current;

                    psv.RefreshLists(ps);
                    psv.SetPlotConfigurationName(ps, deviceName, mediaName);
                    psv.SetPlotType(ps, PlotType.Layout);
                    psv.SetPlotPaperUnits(ps, PlotPaperUnit.Inches);
                    psv.SetUseStandardScale(ps, true);
                    psv.SetStdScaleType(ps, StdScaleType.ScaleToFit); // use this as default

                    pi.OverrideSettings = ps;

                    PlotInfoValidator piv = new PlotInfoValidator();
                    piv.Validate(pi);

                    lo.CopyFrom(ps);

                    PlotConfig pc = PlotConfigManager.CurrentConfig;
                    // returns data in millimeters...
                    MediaBounds mb = pc.GetMediaBounds(mediaName);

                    Point2d p1 = mb.LowerLeftPrintableArea;
                    Point2d p3 = mb.UpperRightPrintableArea;
                    Point2d p2 = new Point2d(p3.X, p1.Y);
                    Point2d p4 = new Point2d(p1.X, p3.Y);

                    // convert millimeters to inches
                    double mm2inch = 25.4;
                    height = p1.GetDistanceTo(p4) / mm2inch;
                    width = p1.GetDistanceTo(p2) / mm2inch;

                    vpLowerCorner = lo.PlotOrigin;
                    vpUpperCorner = new Point2d(vpLowerCorner.X + width, vpLowerCorner.Y + height);
                    LineSegment2d seg = new LineSegment2d(vpLowerCorner, vpUpperCorner);
                    loCenter = seg.MidPoint;
                    #endregion

                    if (lo.GetViewports().Count == 1) // Viewport was not created by default
                    {
                        // the create by default view ports on new layouts it 
                        // is off we need to mark a flag to generate a new one
                        // in another transaction - out of this one
                        flagVp = true;
                    }
                    else if (lo.GetViewports().Count == 2) // create Viewports by default it is on
                    {
                        // extract the last item from the collection
                        // of view ports inside of the layout
                        int i = lo.GetViewports().Count - 1;
                        ObjectId vpId = lo.GetViewports()[i];

                        if (!vpId.IsNull)
                        {
                            Viewport vp = tr.GetObject(vpId, OpenMode.ForWrite, false) as Viewport;
                            if (vp != null)
                            {
                                vp.Height = height; // change height
                                vp.Width = width; // change width
                                vp.CenterPoint = new Point3d(loCenter.X, loCenter.Y, 0.0); // change center
                                //vp.ColorIndex = 1; // debug

                                // zoom to the Viewport extents
                                Zoom(new Point3d(vpLowerCorner.X, vpLowerCorner.Y, 0.0),
                                    new Point3d(vpUpperCorner.X, vpUpperCorner.Y, 0.0), new Point3d(), 1.0);

                                rvpid = vp.ObjectId; // return the output ObjectId to out...
                            }
                        }
                    }
                }
                tr.Commit();
            } // end of transaction

            // we need another transaction to create a new paper space floating Viewport
            if (flagVp)
            {
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                    BlockTableRecord btr_ps = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.PaperSpace], OpenMode.ForWrite);

                    Viewport vp = new Viewport();
                    vp.Height = height; // set the height
                    vp.Width = width; // set the width
                    vp.CenterPoint = new Point3d(loCenter.X, loCenter.Y, 0.0); // set the center
                    //vp.ColorIndex = 2; // debug

                    btr_ps.AppendEntity(vp);
                    tr.AddNewlyCreatedDBObject(vp, true);

                    vp.On = true; // make it accessible!

                    // zoom to the Viewport extents
                    Zoom(new Point3d(vpLowerCorner.X, vpLowerCorner.Y, 0.0),
                        new Point3d(vpUpperCorner.X, vpUpperCorner.Y, 0.0), new Point3d(), 1.0);

                    rvpid = vp.ObjectId; // return the ObjectId to the out...

                    tr.Commit();
                } // end of transaction
            }
        }

        [CommandMethod(CommandNames.UserGroup, CommandNames.Snapshot, CommandFlags.Session)]
        public void SnapShot()
        {
            using (var context = new CommandContext(CommandNames.Snapshot, "SnapShot"))
            {
                if (!context.Cancellation.IsCancellationRequested)
                {
                    context.CommandLine.ZoomExtents();
                    InfraManager.DelayAction(2000, () =>
                    {
                        //Services.Files.ScreenShotToFile(acApp.DocumentManager.MdiActiveDocument.Window, "D:\\IntelliDesk\\IntelliDesk.bundle\\Contents\\Tiles\\test.png", 0, 0, 0, 0);
                        Common.Internal.Files.ScreenShotToFile(acadApp.MainWindow, 0, 0, 0, 0, "d:\\main-window.png", false);
                        Common.Internal.Files.ScreenShotToFile(acadApp.DocumentManager.MdiActiveDocument.Window, 30, 26, 10, 10,
                            "d:\\doc-window.png", false);
                    });
                }

                context.Clean();
            }
        }

        #region <Test>

        public void CreateLayout()
        {
            var doc = acadApp.DocumentManager.MdiActiveDocument;
            if (doc == null)
                return;

            var db = doc.Database;
            var ed = doc.Editor;
            var ext = new Extents2d();

            using (var tr = db.TransactionManager.StartTransaction())
            {
                // Create and select a new layout tab
                var id = LayoutManager.Current.CreateAndMakeLayoutCurrent("NewLayout");

                // Open the created layout
                var lay = (Layout)tr.GetObject(id, OpenMode.ForWrite);

                // Make some settings on the layout and get its extents
                lay.SetPlotSettings(
                  //"ISO_full_bleed_2A0_(1189.00_x_1682.00_MM)", // Try this big boy!
                  "ANSI_B_(11.00_x_17.00_Inches)",
                  "monochrome.ctb",
                  "DWF6 ePlot.pc3"
                );

                ext = lay.GetMaximumExtents();
                lay.ApplyToViewport(tr, 2, vp =>
                  {
                      // Size the viewport according to the extents calculated when
                      // we set the PlotSettings (device, page size, etc.)
                      // Use the standard 10% margin around the viewport
                      // (found by measuring pixels on screenshots of Layout1, etc.)
                      vp.ResizeViewport(ext, 0.8);

                      // Adjust the view so that the model contents fit
                      if (ValidDbExtents(db.Extmin, db.Extmax))
                      {
                          vp.FitContentToViewport(new Extents3d(db.Extmin, db.Extmax), 0.9);
                      }

                      // Finally we lock the view to prevent meddling
                      vp.Locked = true;
                  }
                );

                // Commit the transaction
                tr.Commit();
            }

            // Zoom so that we can see our new layout, again with a little padding
            ed.Command("_.ZOOM", "_E");
            ed.Command("_.ZOOM", ".7X");
            ed.Regen();
        }

        public static void PlotLayout1()
        {
            var doc = acadApp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            using (Transaction acTrans = db.TransactionManager.StartTransaction())
                try
                {
                    HostApplicationServices.WorkingDatabase = db;
                    LayoutManager acLayoutMgr = LayoutManager.Current;

                    // Get the current layout and output its name in the Command Line window
                    Layout acLayout = acTrans.GetObject(acLayoutMgr.GetLayoutId(acLayoutMgr.CurrentLayout),
                                                        OpenMode.ForRead) as Layout;

                    // Get the PlotInfo from the layout
                    using (PlotInfo acPlInfo = new PlotInfo())
                    {
                        acPlInfo.Layout = acLayout.ObjectId;

                        // Get a copy of the PlotSettings from the layout
                        using (PlotSettings acPlSet = new PlotSettings(acLayout.ModelType))
                        {
                            acPlSet.CopyFrom(acLayout);

                            // Update the PlotSettings object
                            PlotSettingsValidator acPlSetVdr = PlotSettingsValidator.Current;

                            // Set the plot type
                            acPlSetVdr.SetPlotType(acPlSet, PlotType.Extents);

                            // Set the plot scale
                            acPlSetVdr.SetUseStandardScale(acPlSet, true);
                            acPlSetVdr.SetStdScaleType(acPlSet, StdScaleType.ScaleToFit);

                            // Center the plot
                            acPlSetVdr.SetPlotCentered(acPlSet, true);

                            // Set the plot device to use
                            acPlSetVdr.SetPlotConfigurationName(acPlSet, "DWF6 ePlot.pc3", "ANSI_B_(11.00_x_17.00_Inches)");

                            // Set the plot info as an override since it will
                            // not be saved back to the layout
                            acPlInfo.OverrideSettings = acPlSet;

                            // Validate the plot info
                            using (PlotInfoValidator acPlInfoVdr = new PlotInfoValidator())
                            {
                                acPlInfoVdr.MediaMatchingPolicy = MatchingPolicy.MatchEnabled;
                                acPlInfoVdr.Validate(acPlInfo);

                                // Check to see if a plot is already in progress
                                if (PlotFactory.ProcessPlotState == ProcessPlotState.NotPlotting)
                                {
                                    using (PlotEngine acPlEng = PlotFactory.CreatePublishEngine())
                                    {
                                        // Track the plot progress with a Progress dialog
                                        using (PlotProgressDialog acPlProgDlg = new PlotProgressDialog(false, 1, true))
                                        {
                                            using ((acPlProgDlg))
                                            {
                                                // Define the status messages to display 
                                                // when plotting starts
                                                acPlProgDlg.set_PlotMsgString(PlotMessageIndex.DialogTitle, "Plot Progress");
                                                acPlProgDlg.set_PlotMsgString(PlotMessageIndex.CancelJobButtonMessage, "Cancel Job");
                                                acPlProgDlg.set_PlotMsgString(PlotMessageIndex.CancelSheetButtonMessage, "Cancel Sheet");
                                                acPlProgDlg.set_PlotMsgString(PlotMessageIndex.SheetSetProgressCaption, "Sheet Set Progress");
                                                acPlProgDlg.set_PlotMsgString(PlotMessageIndex.SheetProgressCaption, "Sheet Progress");

                                                // Set the plot progress range
                                                acPlProgDlg.LowerPlotProgressRange = 0;
                                                acPlProgDlg.UpperPlotProgressRange = 100;
                                                acPlProgDlg.PlotProgressPos = 0;

                                                // Display the Progress dialog
                                                acPlProgDlg.OnBeginPlot();
                                                acPlProgDlg.IsVisible = true;

                                                // Start to plot the layout
                                                acPlEng.BeginPlot(acPlProgDlg, null);

                                                // Define the plot output
                                                acPlEng.BeginDocument(acPlInfo, doc.Name, null, 1, true, "c:\\myplot");

                                                // Display information about the current plot
                                                acPlProgDlg.set_PlotMsgString(PlotMessageIndex.Status, "Plotting: " + doc.Name + " - " + acLayout.LayoutName);

                                                // Set the sheet progress range
                                                acPlProgDlg.OnBeginSheet();
                                                acPlProgDlg.LowerSheetProgressRange = 0;
                                                acPlProgDlg.UpperSheetProgressRange = 100;
                                                acPlProgDlg.SheetProgressPos = 0;

                                                // Plot the first sheet/layout
                                                using (PlotPageInfo acPlPageInfo = new PlotPageInfo())
                                                {
                                                    acPlEng.BeginPage(acPlPageInfo, acPlInfo, true, null);
                                                }

                                                acPlEng.BeginGenerateGraphics(null);
                                                acPlEng.EndGenerateGraphics(null);

                                                // Finish plotting the sheet/layout
                                                acPlEng.EndPage(null);
                                                acPlProgDlg.SheetProgressPos = 100;
                                                acPlProgDlg.OnEndSheet();

                                                // Finish plotting the document
                                                acPlEng.EndDocument(null);

                                                // Finish the plot
                                                acPlProgDlg.PlotProgressPos = 100;
                                                acPlProgDlg.OnEndPlot();
                                                acPlEng.EndPlot(null);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                }
                catch (System.Exception ex)
                {

                    ed.WriteMessage(ex.ToString());
                }
        }

        [CommandMethod("CreateOrEditPageSetup")]
        public static void CreateOrEditPageSetup()
        {
            // Get the current document and database, and start a transaction
            Document acDoc = acadApp.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            Editor acCurEd = acDoc.Editor;

            using (Transaction tr = acCurDb.TransactionManager.StartTransaction())
            {
                DBDictionary plSets = tr.GetObject(acCurDb.PlotSettingsDictionaryId, OpenMode.ForRead) as DBDictionary;
                DBDictionary vStyles = tr.GetObject(acCurDb.VisualStyleDictionaryId, OpenMode.ForRead) as DBDictionary;

                PlotSettings ps = default(PlotSettings);
                bool createNew = false;

                // Reference the Layout Manager
                LayoutManager acLayoutMgr = LayoutManager.Current;

                // Get the current layout and output its name in the Command Line window
                Layout acLayout = tr.GetObject(acLayoutMgr.GetLayoutId(acLayoutMgr.CurrentLayout), OpenMode.ForRead) as Layout;

                if (plSets != null && acLayout != null)
                {
                    PlotSettingsValidator psv = PlotSettingsValidator.Current;

                    // Check to see if the page setup exists
                    if (!plSets.Contains(CommandNames.UserGroup))
                    {
                        createNew = true;

                        // Create a new PlotSettings object:  True - model space, False - named layout
                        ps = new PlotSettings(acLayout.ModelType);
                        ps.CopyFrom(acLayout);
                        ps.PlotSettingsName = CommandNames.UserGroup;
                        ps.AddToPlotSettingsDictionary(acCurDb);
                        tr.AddNewlyCreatedDBObject(ps, true);

                        //int cnt = 0;
                        //Dictionary<string, string> mediaNames = new Dictionary<string, string>();
                        //foreach (string mediaName in psv.GetCanonicalMediaNameList(ps))
                        //{
                        //    // Output the names of the available media for the specified device
                        //    mediaNames.Add(mediaName, psv.GetLocaleMediaName(ps, cnt));
                        //    acDoc.Editor.WriteMessage("\n  " + mediaName + " | " + psv.GetLocaleMediaName(ps, cnt));
                        //    cnt = cnt + 1;
                        //}
                    }
                    else
                    {
                        ps = plSets.GetAt(CommandNames.UserGroup).GetObject(OpenMode.ForWrite) as PlotSettings;
                    }

                    // Update the PlotSettings object
                    try
                    {

                        StringCollection devlist = psv.GetPlotDeviceList();

                        for (int i = 0; i < devlist.Count; i++)
                        {
                            acCurEd.WriteMessage("\n{0} {1}", i + 1, devlist[i]);
                        }

                        // Set the Plotter and page size
                        psv.SetPlotConfigurationName(ps, "MLBS_Plot.pc3", null); //"Mlbs_610x914 (landscape)"
                        //psv.SetPlotConfigurationName(ps, "MLBS_Plot.pc3", "Mlbs_610x914 (landscape)");

                        StringCollection medlist = psv.GetCanonicalMediaNameList(ps);
                        for (int i = 0; i < medlist.Count; i++)
                        {
                            acCurEd.WriteMessage("\n{0} {1}", i + 1, medlist[i]);
                        }

                        psv.SetPlotConfigurationName(ps, "DWG To PDF.pc3", "ANSI_B_(11.00_x_17.00_Inches)");
                        medlist = psv.GetCanonicalMediaNameList(ps);
                        for (int i = 0; i < medlist.Count; i++)
                        {
                            acCurEd.WriteMessage("\n{0} {1}", i + 1, medlist[i]);
                        }
                        psv.RefreshLists(ps);

                        // Set to plot to the current display
                        if (acLayout.ModelType == false)
                        {
                            psv.SetPlotType(ps, PlotType.Layout);
                        }
                        else
                        {
                            psv.SetPlotType(ps, PlotType.Extents);
                            psv.SetPlotCentered(ps, true);
                        }

                        // Use SetPlotWindowArea with PlotType.Window
                        psv.SetPlotWindowArea(ps,
                                                     new Extents2d(new Point2d(acCurDb.Limmin.X, acCurDb.Limmin.Y),
                                                     new Point2d(acCurDb.Limmax.X, acCurDb.Limmax.Y)));

                        // Use SetPlotViewName with PlotType.View
                        //acPlSetVdr.SetPlotViewName(plSet, "MyView");

                        // Set the plot offset
                        psv.SetPlotOrigin(ps, new Point2d(0, 0));

                        // Set the plot scale
                        psv.SetUseStandardScale(ps, true);
                        psv.SetStdScaleType(ps, StdScaleType.StdScale1To1);
                        psv.SetPlotPaperUnits(ps, PlotPaperUnit.Millimeters);
                        ps.ScaleLineweights = true;

                        // Specify if plot styles should be displayed on the layout
                        ps.ShowPlotStyles = true;

                        // Rebuild plotter, plot style, and canonical media lists 
                        // (must be called before setting the plot style)
                        //acPlSetVdr.RefreshLists(acPlSet);

                        // Specify the shaded viewport options
                        ps.ShadePlot = PlotSettingsShadePlotType.AsDisplayed;

                        ps.ShadePlotResLevel = ShadePlotResLevel.Normal;

                        // Specify the plot options
                        ps.PrintLineweights = true;
                        ps.PlotTransparency = false;
                        ps.PlotPlotStyles = true;
                        ps.DrawViewportsFirst = true;
                        //acPlSet.CurrentStyleSheet;

                        // Use only on named layouts - Hide paperspace objects option
                        // plSet.PlotHidden = true;

                        // Specify the plot orientation
                        psv.SetPlotRotation(ps, PlotRotation.Degrees000);

                        // Set the plot style
                        psv.SetCurrentStyleSheet(ps, acCurDb.PlotStyleMode ? "acad.ctb" : "acad.stb");

                        // Zoom to show the whole paper
                        psv.SetZoomToPaperOnUpdate(ps, true);
                    }
                    catch (Autodesk.AutoCAD.Runtime.Exception es)
                    {
                        System.Windows.Forms.MessageBox.Show(es.Message);
                    }

                    // Save the changes made
                    tr.Commit();

                    if (createNew)
                    {
                        ps.Dispose();
                    }
                }
            }
        }

        public static void AssignPageSetupToLayout()
        {
            // Get the current document and database, and start a transaction
            Document acDoc = acadApp.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Reference the Layout Manager
                LayoutManager acLayoutMgr = LayoutManager.Current;

                // Get the current layout and output its name in the Command Line window
                Layout acLayout = acTrans.GetObject(acLayoutMgr.GetLayoutId(acLayoutMgr.CurrentLayout), OpenMode.ForRead) as Layout;

                DBDictionary acPlSet = acTrans.GetObject(acCurDb.PlotSettingsDictionaryId, OpenMode.ForRead) as DBDictionary;

                // Check to see if the page setup exists
                if (acPlSet != null && acPlSet.Contains(CommandNames.UserGroup))
                {
                    PlotSettings plSet = acPlSet.GetAt(CommandNames.UserGroup).GetObject(OpenMode.ForRead) as PlotSettings;

                    // Update the layout
                    if (acLayout != null)
                    {
                        acLayout.UpgradeOpen();
                        acLayout.CopyFrom(plSet);
                    }

                    // Save the new objects to the database
                    acTrans.Commit();
                }
                else
                {
                    // Ignore the changes made
                    acTrans.Abort();
                }
            }

            // Update the display
            acDoc.Editor.Regen();
        }

        [CommandMethod("ListPageSetup")]
        public static void ListPageSetup()
        {
            // Get the current document and database
            Document acDoc = acadApp.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            // Start a transaction
            using (Transaction tr = acCurDb.TransactionManager.StartTransaction())
            {
                DBDictionary plSettingDictionary = tr.GetObject(acCurDb.PlotSettingsDictionaryId, OpenMode.ForRead) as DBDictionary;
                acDoc.Editor.WriteMessage("\nPage Setups: ");

                PlotSettingsValidator acPlSetVdr = PlotSettingsValidator.Current;

                // List each named page setup
                if (plSettingDictionary != null)
                    foreach (DBDictionaryEntry item in plSettingDictionary)
                    {
                        PlotSettings plotSettings = tr.GetObject(item.Value, OpenMode.ForRead) as PlotSettings;
                        StringCollection sc = acPlSetVdr.GetCanonicalMediaNameList(plotSettings);
                        plotSettings?.PlotPaperSize.TransformBy(
                            Matrix2d.Displacement(new Point2d(910, 610).GetVectorTo(new Point2d(910, 610))));
                        acDoc.Editor.WriteMessage("\n  " + item.Key);
                    }

                // Abort the changes to the database
                tr.Abort();
            }
        }

        //[CommandMethod("CreateFloatingViewport")]
        public static void CreateFloatingViewport()
        {
            // Get the current document and database, and start a transaction
            Document acDoc = acadApp.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Paper space for write
                BlockTableRecord acBlkTblRec;
                acBlkTblRec =
                    acTrans.GetObject(acBlkTbl[BlockTableRecord.PaperSpace], OpenMode.ForWrite) as BlockTableRecord;

                // Switch to the previous Paper space layout
                acadApp.SetSystemVariable("TILEMODE", 0);
                acDoc.Editor.SwitchToPaperSpace();

                // Create a Viewport
                using (Viewport acVport = new Viewport())
                {
                    acVport.CenterPoint = new Point3d(3.25, 3, 0);
                    acVport.Width = 6;
                    acVport.Height = 5;

                    // Add the new object to the block table record and the transaction
                    acBlkTblRec.AppendEntity(acVport);
                    acTrans.AddNewlyCreatedDBObject(acVport, true);

                    // Change the view direction
                    acVport.ViewDirection = new Vector3d(1, 1, 1);

                    // Enable the viewport
                    acVport.On = true;

                    // Activate model space in the viewport
                    acDoc.Editor.SwitchToModelSpace();

                    // Set the new viewport current via an imported ObjectARX function
                    acedSetCurrentVPort(acVport.UnmanagedObject);
                }

                // Save the new objects to the database
                acTrans.Commit();
            }
        }

        //[CommandMethod("FourFloatingViewports")]
        public static void FourFloatingViewports()
        {
            // Get the current document and database, and start a transaction
            Document acDoc = acadApp.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
                    OpenMode.ForRead) as BlockTable;

                // Open the Block table record Paper space for write
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.PaperSpace],
                    OpenMode.ForWrite) as BlockTableRecord;

                // Switch to the previous Paper space layout
                acadApp.SetSystemVariable("TILEMODE", 0);
                acDoc.Editor.SwitchToPaperSpace();

                Point3dCollection acPt3dCol = new Point3dCollection();
                acPt3dCol.Add(new Point3d(2.5, 5.5, 0));
                acPt3dCol.Add(new Point3d(2.5, 2.5, 0));
                acPt3dCol.Add(new Point3d(5.5, 5.5, 0));
                acPt3dCol.Add(new Point3d(5.5, 2.5, 0));

                Vector3dCollection acVec3dCol = new Vector3dCollection();
                acVec3dCol.Add(new Vector3d(0, 0, 1));
                acVec3dCol.Add(new Vector3d(0, 1, 0));
                acVec3dCol.Add(new Vector3d(1, 0, 0));
                acVec3dCol.Add(new Vector3d(1, 1, 1));

                double dWidth = 2.5;
                double dHeight = 2.5;

                Viewport acVportLast = null;
                int nCnt = 0;

                foreach (Point3d acPt3d in acPt3dCol)
                {
                    using (Viewport acVport = new Viewport())
                    {
                        acVport.CenterPoint = acPt3d;
                        acVport.Width = dWidth;
                        acVport.Height = dHeight;

                        // Add the new object to the block table record and the transaction
                        acBlkTblRec.AppendEntity(acVport);
                        acTrans.AddNewlyCreatedDBObject(acVport, true);

                        // Change the view direction
                        acVport.ViewDirection = acVec3dCol[nCnt];

                        // Enable the viewport
                        acVport.On = true;

                        // Record the last viewport created
                        acVportLast = acVport;

                        // Increment the counter by 1
                        nCnt = nCnt + 1;
                    }
                }

                if (acVportLast != null)
                {
                    // Activate model space in the viewport
                    acDoc.Editor.SwitchToModelSpace();

                    // Set the new viewport current via an imported ObjectARX function
                    acedSetCurrentVPort(acVportLast.UnmanagedObject);
                }

                // Save the new objects to the database
                acTrans.Commit();
            }
        }

        public static void CreateNonRectangularFloatingViewport()
        {
            // Get the current document and database, and start a transaction
            Document acDoc = acadApp.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
                    OpenMode.ForRead) as BlockTable;

                // Open the Block table record Paper space for write
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.PaperSpace],
                    OpenMode.ForWrite) as BlockTableRecord;

                // Switch to the previous Paper space layout
                acadApp.SetSystemVariable("TILEMODE", 0);
                acDoc.Editor.SwitchToPaperSpace();

                // Create a Viewport
                using (Viewport acVport = new Viewport())
                {
                    acVport.CenterPoint = new Point3d(9, 6.5, 0);
                    acVport.Width = 2.5;
                    acVport.Height = 2.5;

                    // Set the scale to 1" = 8'
                    acVport.CustomScale = 96;

                    // Create a circle
                    using (Circle acCirc = new Circle())
                    {
                        acCirc.Center = acVport.CenterPoint;
                        acCirc.Radius = 1.25;

                        // Add the new object to the block table record and the transaction
                        acBlkTblRec.AppendEntity(acCirc);
                        acTrans.AddNewlyCreatedDBObject(acCirc, true);

                        // Clip the viewport using the circle  
                        acVport.NonRectClipEntityId = acCirc.ObjectId;
                        acVport.NonRectClipOn = true;
                    }

                    // Add the new object to the block table record and the transaction
                    acBlkTblRec.AppendEntity(acVport);
                    acTrans.AddNewlyCreatedDBObject(acVport, true);

                    // Change the view direction
                    acVport.ViewDirection = new Vector3d(0, 0, 1);

                    // Enable the viewport
                    acVport.On = true;

                    // Activate model space in the viewport
                    acDoc.Editor.SwitchToModelSpace();

                    // Set the new viewport current via an imported ObjectARX function
                    acedSetCurrentVPort(acVport.UnmanagedObject);
                }

                // Save the new objects to the database
                acTrans.Commit();
            }
        }

        //[CommandMethod("PlotToPdf")]
        public static void PlotToPdf()
        {
            String BlockName = "SHEETGOST";
            String PrinterName = "DWG To PDF.pc3";
            String PaperSize = "ISO_A3_(297.00_x_420.00_MM)";
            String OutPath = "c:\\plot2pdf";

            PlotBlockColToPdf(BlockName, PrinterName, OutPath, PaperSize);
        }

        //[CommandMethod(CommandNames.UserGroup, CommandNames.Snapshot, CommandFlags.Session)]
        public void NamedViewPlot()
        {
            Document doc = acadApp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            using (doc.LockDocument())
            using (Transaction tx =
                db.TransactionManager.StartTransaction())
            {
                string deviceName = "PublishToWeb PNG.pc3";
                string mediaName = "VGA (640.00 x 480.00 Pixels)";

                BlockTable blockTable = tx.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord blockTableRecord = tx.GetObject(blockTable[acadApp.DocumentManager.GetCurrentSpace()],
                    OpenMode.ForRead) as BlockTableRecord;

                Layout plotPage = tx.GetObject(blockTableRecord.LayoutId, OpenMode.ForRead) as Layout;

                PlotInfo plotInfo = new PlotInfo();
                plotInfo.Layout = plotPage.Id;

                PlotSettings ps = new PlotSettings(plotPage.ModelType);
                ps.CopyFrom(plotPage);

                PlotSettingsValidator psv = PlotSettingsValidator.Current;
                psv.SetPlotViewName(ps, "Test");
                psv.SetPlotType(ps, PlotType.View);
                psv.SetUseStandardScale(ps, true);
                psv.SetPlotConfigurationName(ps,
                    deviceName, mediaName.Replace(" ", "_"));
                psv.SetPlotCentered(ps, true);

                psv.SetStdScaleType(ps, StdScaleType.ScaleToFit);
                psv.RefreshLists(ps);

                plotInfo.OverrideSettings = ps;
                PlotInfoValidator piv = new PlotInfoValidator();
                piv.Validate(plotInfo);

                object backgroundPlot =
                    acadApp.GetSystemVariable("BACKGROUNDPLOT");
                acadApp.SetSystemVariable("BACKGROUNDPLOT", 0);

                using (var pe = PlotFactory.CreatePublishEngine())
                {
                    // Begin plotting a document.
                    pe.BeginPlot(null, null);
                    pe.BeginDocument(plotInfo,
                        doc.Name, null, 1, true, "c:\\temp\\test.png");

                    // Begin plotting the page.
                    PlotPageInfo ppi = new PlotPageInfo();
                    pe.BeginPage(ppi, plotInfo, true, null);
                    pe.BeginGenerateGraphics(null);
                    pe.EndGenerateGraphics(null);

                    // Finish the sheet
                    pe.EndPage(null);

                    // Finish the document
                    pe.EndDocument(null);

                    //// And finish the plot
                    pe.EndPlot(null);
                }
                acadApp.SetSystemVariable("BACKGROUNDPLOT",
                    backgroundPlot); //

                tx.Commit();
            }
        }

        public void NamedViewPlot1()
        {
            Document doc = acadApp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            using (doc.LockDocument())
            using (Transaction tx = db.TransactionManager.StartTransaction())
            {
                string deviceName = "PublishToWeb PNG.pc3";
                string mediaName = "VGA (640.00 x 480.00 Pixels)";

                BlockTable blockTable = tx.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord blockTableRecord = tx.GetObject(blockTable[acadApp.DocumentManager.GetCurrentSpace()],
                    OpenMode.ForRead) as BlockTableRecord;

                Layout plotPage = tx.GetObject(blockTableRecord.LayoutId, OpenMode.ForRead) as Layout;

                PlotInfo plotInfo = new PlotInfo();
                plotInfo.Layout = plotPage.Id;

                PlotSettings ps = new PlotSettings(plotPage.ModelType);
                ps.CopyFrom(plotPage);

                PlotSettingsValidator psv = PlotSettingsValidator.Current;
                psv.SetPlotViewName(ps, "Test");
                psv.SetPlotType(ps, PlotType.View);
                psv.SetUseStandardScale(ps, true);
                psv.SetPlotConfigurationName(ps, deviceName, mediaName.Replace(" ", "_"));
                psv.SetPlotCentered(ps, true);

                psv.SetStdScaleType(ps, StdScaleType.ScaleToFit);
                psv.RefreshLists(ps);

                plotInfo.OverrideSettings = ps;
                PlotInfoValidator piv = new PlotInfoValidator();
                piv.Validate(plotInfo);

                object backgroundPlot = acadApp.GetSystemVariable("BACKGROUNDPLOT");
                acadApp.SetSystemVariable("BACKGROUNDPLOT", 0);

                using (var pe = PlotFactory.CreatePublishEngine())
                {
                    // Begin plotting a document.
                    pe.BeginPlot(null, null);
                    pe.BeginDocument(plotInfo, doc.Name, null, 1, true,
                        "D:\\IntelliDesk\\IntelliDesk.bundle\\Contents\\Tiles\\test.png");

                    // Begin plotting the page.
                    PlotPageInfo ppi = new PlotPageInfo();
                    pe.BeginPage(ppi, plotInfo, true, null);
                    pe.BeginGenerateGraphics(null);
                    pe.EndGenerateGraphics(null);

                    // Finish the sheet
                    pe.EndPage(null);

                    // Finish the document
                    pe.EndDocument(null);

                    //// And finish the plot
                    pe.EndPlot(null);
                }
                acadApp.SetSystemVariable("BACKGROUNDPLOT", backgroundPlot); //
                tx.Commit();
            }
        }

        //[CommandMethod("ToggleSpace")]
        public static void ToggleSpace()
        {
            // Get the current document
            Document acDoc = acadApp.DocumentManager.MdiActiveDocument;

            // Get the current values of CVPORT and TILEMODE
            object oCvports = acadApp.GetSystemVariable("CVPORT");
            object oTilemode = acadApp.GetSystemVariable("TILEMODE");

            // Check to see if the Model layout is active, TILEMODE is 1 when
            // the Model layout is active
            if (System.Convert.ToInt16(oTilemode) == 0)
            {
                // Check to see if Model space is active in a viewport,
                // CVPORT is 2 if Model space is active 
                if (System.Convert.ToInt16(oCvports) == 2)
                {
                    acDoc.Editor.SwitchToPaperSpace();
                }
                else
                {
                    acDoc.Editor.SwitchToModelSpace();
                }
            }
            else
            {
                // Switch to the previous Paper space layout
                acadApp.SetSystemVariable("TILEMODE", 0);
            }
        }

        [CommandMethod("PublishAllLayouts")]
        public static void PublishAllLayouts()
        {
            Document doc = acadApp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            //put the plot in foreground
            short bgPlot = (short)acadApp.GetSystemVariable("BACKGROUNDPLOT");

            acadApp.SetSystemVariable("BACKGROUNDPLOT", 0);

            //get the layout ObjectId List
            List<ObjectId> layoutIds = GetLayoutIds(db);

            string dwgFileName = (string)acadApp.GetSystemVariable("DWGNAME");

            string dwgPath = (string)acadApp.GetSystemVariable("DWGPREFIX");

            using (Transaction Tx = db.TransactionManager.StartTransaction())
            {
                DsdEntryCollection collection = new DsdEntryCollection();

                foreach (ObjectId layoutId in layoutIds)
                {
                    Layout layout = Tx.GetObject(layoutId, OpenMode.ForRead) as Layout;

                    DsdEntry entry = new DsdEntry();

                    entry.DwgName = dwgPath + dwgFileName;
                    entry.Layout = layout.LayoutName;
                    entry.Title = "Layout_" + layout.LayoutName;
                    entry.NpsSourceDwg = entry.DwgName;
                    entry.Nps = "Setup1";

                    collection.Add(entry);
                }

                dwgFileName = dwgFileName.Substring(0, dwgFileName.Length - 4);

                DsdData dsdData = new DsdData();

                dsdData.SheetType = SheetType.MultiDwf; //SheetType.MultiPdf
                dsdData.ProjectPath = dwgPath;
                dsdData.DestinationName = dsdData.ProjectPath + dwgFileName + ".dwf";

                if (File.Exists(dsdData.DestinationName))
                    File.Delete(dsdData.DestinationName);

                dsdData.SetDsdEntryCollection(collection);

                string dsdFile = dsdData.ProjectPath + dwgFileName + ".dsd";

                //Workaround to avoid promp for dwf file name
                //set PromptForDwfName=FALSE in dsdData using
                //StreamReader/StreamWriter

                dsdData.WriteDsd(dsdFile);

                StreamReader sr = new StreamReader(dsdFile);

                string str = sr.ReadToEnd();
                sr.Close();

                str = str.Replace("PromptForDwfName=TRUE", "PromptForDwfName=FALSE");

                StreamWriter sw = new StreamWriter(dsdFile);

                sw.Write(str);
                sw.Close();

                dsdData.ReadDsd(dsdFile);
                File.Delete(dsdFile);

                PlotConfig plotConfig =
                    PlotConfigManager.SetCurrentConfig("DWF6 ePlot.pc3");

                //PlotConfig pc = Autodesk.AutoCAD.PlottingServices.
                //  PlotConfigManager.SetCurrentConfig("DWG To PDF.pc3");

                Publisher publisher =
                    acadApp.Publisher;

                publisher.AboutToBeginPublishing +=
                    Publisher_AboutToBeginPublishing;

                publisher.PublishExecute(dsdData, plotConfig);

                Tx.Commit();
            }

            //reset the background plot value
            acadApp.SetSystemVariable("BACKGROUNDPLOT", bgPlot);
        }

        static void Publisher_AboutToBeginPublishing(object sender, AboutToBeginPublishingEventArgs e)
        {
            acadApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nAboutToBeginPublishing!!");
        }

        //[CommandMethod("BatchPublishCmd", CommandFlags.Session)]
        public static void BatchPublishCmd()
        {
            short bgPlot = (short)acadApp.GetSystemVariable("BACKGROUNDPLOT");

            acadApp.SetSystemVariable("BACKGROUNDPLOT", 0);

            List<string> docsToPlot = new List<string>
            {
                "C:\\Temp\\Drawing1.dwg",
                "C:\\Temp\\Drawing2.dwg",
                "C:\\Temp\\Drawing3.dwg"
            };
            BatchPublish(docsToPlot);
        }

        // List all the layouts in the current drawing
        //[CommandMethod("ListLayouts")]
        public void ListLayouts()
        {
            // Get the current document and database
            Document acDoc = acadApp.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            // Get the layout dictionary of the current database
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                DBDictionary lays = acTrans.GetObject(acCurDb.LayoutDictionaryId, OpenMode.ForRead) as DBDictionary;
                acDoc.Editor.WriteMessage("\nLayouts:");

                // Step through and list each named layout and Model
                foreach (DBDictionaryEntry item in lays)
                {
                    acDoc.Editor.WriteMessage("\n  " + item.Key);
                }

                // Abort the changes to the database
                acTrans.Abort();
            }
        }

        [CommandMethod("simplot")]
        public static void SimplePlot()
        {
            Document doc = acadApp.DocumentManager.MdiActiveDocument;

            string cmd = "-PlotStamp On Fields Yes No Yes No No No No /n";
            doc.SendStringToExecute(cmd, true, false, false);

            //Retreive the Drawings File Path
            HostApplicationServices hs = HostApplicationServices.Current;
            string path = hs.FindFile(doc.Name, doc.Database, FindFileHint.Default);

            Editor ed = doc.Editor;
            Database db = doc.Database;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                // We'll be plotting the current layout
                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForRead);
                Layout lo = (Layout)tr.GetObject(btr.LayoutId, OpenMode.ForRead);

                // We need a PlotInfo object
                // linked to the layout
                PlotInfo pi = new PlotInfo();
                pi.Layout = btr.LayoutId;

                // We need a PlotSettings object
                // based on the layout settings
                // which we then customize
                PlotSettings ps = new PlotSettings(lo.ModelType);
                ps.CopyFrom(lo);

                // The PlotSettingsValidator helps
                // create a valid PlotSettings object
                PlotSettingsValidator psv = PlotSettingsValidator.Current;

                // We'll plot the extents, centered and
                // scaled to fit
                psv.SetPlotType(ps, PlotType.Extents);
                psv.SetUseStandardScale(ps, true);
                psv.SetStdScaleType(ps, StdScaleType.ScaleToFit);
                psv.SetPlotCentered(ps, true);

                // We'll use the standard DWF PC3, as
                // for today we're just plotting to file
                psv.SetPlotConfigurationName(ps, "DWG to PDF.pc3", "ANSI_A_(11.00_x_8.50_Inches)");

                // We need to link the PlotInfo to the
                // PlotSettings and then validate it
                pi.OverrideSettings = ps;
                PlotInfoValidator piv = new PlotInfoValidator();
                piv.MediaMatchingPolicy = MatchingPolicy.MatchEnabled;
                piv.Validate(pi);

                // A PlotEngine does the actual plotting
                // (can also create one for Preview)
                if (PlotFactory.ProcessPlotState == ProcessPlotState.NotPlotting)
                {
                    PlotEngine pe = PlotFactory.CreatePublishEngine();
                    using (pe)
                    {
                        // Create a Progress Dialog to provide info
                        // and allow thej user to cancel
                        PlotProgressDialog ppd = new PlotProgressDialog(false, 1, true);
                        using (ppd)
                        {
                            ppd.set_PlotMsgString(PlotMessageIndex.DialogTitle, "Custom Plot Progress");
                            ppd.set_PlotMsgString(PlotMessageIndex.CancelJobButtonMessage, "Cancel Job");
                            ppd.set_PlotMsgString(PlotMessageIndex.CancelSheetButtonMessage, "Cancel Sheet");
                            ppd.set_PlotMsgString(PlotMessageIndex.SheetSetProgressCaption, "Sheet Set Progress");
                            ppd.set_PlotMsgString(PlotMessageIndex.SheetProgressCaption, "Sheet Progress");

                            ppd.LowerPlotProgressRange = 0;
                            ppd.UpperPlotProgressRange = 100;
                            ppd.PlotProgressPos = 0;

                            // Let's start the plot, at last
                            ppd.OnBeginPlot();
                            ppd.IsVisible = true;
                            pe.BeginPlot(ppd, null);

                            // We'll be plotting a single document
                            pe.BeginDocument(pi, doc.Name, null, 1, true, // Let's plot to file
                                path + ".pdf"
                            );

                            // Which contains a single sheet
                            ppd.OnBeginSheet();
                            ppd.LowerSheetProgressRange = 0;
                            ppd.UpperSheetProgressRange = 100;
                            ppd.SheetProgressPos = 0;
                            PlotPageInfo ppi = new PlotPageInfo();
                            pe.BeginPage(ppi, pi, true, null);
                            pe.BeginGenerateGraphics(null);
                            pe.EndGenerateGraphics(null);

                            // Finish the sheet
                            pe.EndPage(null);
                            ppd.SheetProgressPos = 100;
                            ppd.OnEndSheet();

                            // Finish the document
                            pe.EndDocument(null);

                            // And finish the plot
                            ppd.PlotProgressPos = 100;
                            ppd.OnEndPlot();
                            pe.EndPlot(null);
                        }
                    }
                }
                else
                {
                    ed.WriteMessage("\nAnother plot is in progress.");
                }
            }
        }

        public class Blk2Plt
        {
            public BlockReference BlockRef;
            public Layout LayoutObj;
        }

        public void Terminate()
        {
            ApplicationEventManager cadWinEvnts = ApplicationEventManager.Instance();
            _msgs.Sort();
            Editor ed = acadApp.DocumentManager.MdiActiveDocument.Editor;
            foreach (string msg in _msgs)
            {
                ed.WriteMessage(msg);
            }
        }

        public static void CreateLayout(string layName)
        {
            Editor ed = acadApp.DocumentManager.MdiActiveDocument.Editor;
            Database db = HostApplicationServices.WorkingDatabase;
            using (var tr = db.TransactionManager.StartTransaction())
            {
                DBDictionary layouts = tr.GetObject(db.LayoutDictionaryId, OpenMode.ForWrite) as DBDictionary;

                var i = 0;
                var layoutName = layName;
                foreach (var layout in layouts)
                {
                    if (layout.Key.Contains(layName))
                    {
                        layoutName = layName + Interlocked.Increment(ref i);
                    }
                }

                //assumes we already created a layout with the ObjectId ltid
                ObjectId ltid = LayoutManager.Current.CreateLayout(layoutName);
                Layout lt = (Layout)tr.GetObject(ltid, OpenMode.ForWrite);
                {
                    lt.UpgradeOpen();
                    lt.Initialize();
                    //change to the one we need.
                    LayoutManager lytmgr = LayoutManager.Current;
                    lytmgr.CurrentLayout = layoutName;
                    //begin configuring plot settings
                    PlotInfo plotinf = new PlotInfo();
                    plotinf.Layout = lt.ObjectId;
                    PlotSettings plotset = new PlotSettings(lt.ModelType);
                    plotset.CopyFrom(lt);
                    PlotSettingsValidator plotsetvdr = PlotSettingsValidator.Current;
                    PlotInfoValidator plotvdr = new PlotInfoValidator();
                    var Plotterused = "MyPrinter.pc3"; //for test purposes only
                    var Papersize = "A3"; //for test purposes only
                    plotsetvdr.SetPlotConfigurationName(plotset, Plotterused, Papersize);
                    plotsetvdr.SetPlotType(plotset, PlotType.Extents);
                    plotsetvdr.SetUseStandardScale(plotset, true);
                    plotsetvdr.SetPlotRotation(plotset, PlotRotation.Degrees090);
                    plotsetvdr.SetPlotPaperUnits(plotset, PlotPaperUnit.Millimeters);
                    //if we centre the plot here,  even without then changing the origin, the resultant layout still isn't centred correctly. 
                    plotsetvdr.SetPlotCentered(plotset, true);
                    //commented these lines out for now as they have no overall effect on the centring of the subsequent layout.
                    //Point2d offset = new Point2d(0, 0);
                    //plotsetvdr.SetPlotOrigin(plotset, offset);
                    //override the current settings?
                    plotinf.OverrideSettings = plotset;
                    //validate the new settings
                    plotvdr.Validate(plotinf);
                    //sets the layout plot settings
                    lt.CopyFrom(plotset);
                    lt.TabOrder = 1;
                    lt.DowngradeOpen();
                    ed.Regen();
                }
            }
        }

        void PlotPng240X240(string outputFolder)
        {
            string fileName = "";
            Editor ed = null;

            try
            {
                ((dynamic)Application.AcadApplication).ZoomExtents(); //AcadApplication

                Document openDoc = acadApp.DocumentManager.MdiActiveDocument;
                fileName = openDoc.Name;
                Document dwg = acadApp.DocumentManager.MdiActiveDocument;
                ed = dwg.Editor;
                ed.WriteMessage($"\nProcessing {fileName}\n");

                Database db = dwg.Database;
                Autodesk.AutoCAD.DatabaseServices.TransactionManager transManager = dwg.TransactionManager;
                var trans = transManager.StartTransaction();

                // We'll be plotting the ModelSpace layout
                BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = (BlockTableRecord)trans.GetObject(bt[acadApp.DocumentManager.GetCurrentSpace()], OpenMode.ForRead);
                Layout lo = (Layout)trans.GetObject(btr.LayoutId, OpenMode.ForRead);

                // We need a PlotInfo object linked to the layout
                PlotInfo pi = new PlotInfo();
                pi.Layout = btr.LayoutId;

                // We need a PlotSettings object based on the layout settings which we then customize
                PlotSettings ps = new PlotSettings(lo.ModelType);
                ps.CopyFrom(lo);
                ps.PlotHidden = true;

                ps.ShadePlot = PlotSettingsShadePlotType.Rendered;
                ps.ShadePlotResLevel = ShadePlotResLevel.Normal;

                // The PlotSettingsValidator helps create a valid PlotSettings object
                PlotSettingsValidator psv = PlotSettingsValidator.Current;
                psv.SetPlotType(ps, PlotType.Extents);
                psv.SetStdScaleType(ps, StdScaleType.ScaleToFit);
                psv.SetUseStandardScale(ps, true);
                psv.SetPlotCentered(ps, true);
                psv.SetPlotRotation(ps, PlotRotation.Degrees000);
                psv.SetPlotConfigurationName(ps, "PublishToFSL240x240 PNG.pc3", null);
                psv.RefreshLists(ps);

                // must call this before setting current style sheet
                StringCollection medialist = psv.GetCanonicalMediaNameList(ps);
                psv.SetCanonicalMediaName(ps, medialist[0]);

                // choose the default media
                psv.SetCurrentStyleSheet(ps, "Grayscale.ctb");

                // We need to link the PlotInfo to the PlotSettings and then validate it
                pi.OverrideSettings = ps;
                PlotInfoValidator piv = new PlotInfoValidator { MediaMatchingPolicy = MatchingPolicy.MatchEnabled };
                piv.Validate(pi);

                if (PlotFactory.ProcessPlotState == ProcessPlotState.NotPlotting)
                {
                    PlotEngine pe = PlotFactory.CreatePublishEngine();

                    using (pe)
                    {
                        pe.BeginPlot(null, null);

                        // We'll be plotting a single document
                        pe.BeginDocument(pi, openDoc.Name, null, 1, true, outputFolder + "\\" + Path.GetFileNameWithoutExtension(fileName) + ".png");

                        PlotPageInfo ppi = new PlotPageInfo();
                        pe.BeginPage(ppi, pi, true, null);
                        pe.BeginGenerateGraphics(null);
                        pe.EndGenerateGraphics(null);

                        // Finish the sheet
                        pe.EndPage(null);

                        // Finish the document
                        pe.EndDocument(null);

                        // And finish the plot
                        pe.EndPlot(null);
                        ppi.Dispose();
                    }
                    pe.Destroy();
                    pe.Dispose();
                }
                piv.Dispose();
                psv.Dispose();
                ps.Dispose();
                pi.Dispose();
                trans.Commit();
                trans.Dispose();
                transManager.Dispose();
            }
            catch (Autodesk.AutoCAD.Runtime.Exception exAcad)
            {
                Logger.Error($"PlotPNG - Error plotting: {Path.GetFileNameWithoutExtension(fileName)}");
                Logger.Error($"{exAcad.Message}");

                MessageBox.Show(exAcad.Message, "STCPlotToPNG", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception exSystem)
            {
                Logger.Error($"PlotPNG - Error plotting: {Path.GetFileNameWithoutExtension(fileName)}");
                Logger.Error($"{exSystem.Message}");

                MessageBox.Show(exSystem.Message, "STCPlotToPNG", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
            }
        }

        //public static void PrintTo()
        //{
        //    WGhostScript gs = new PDFPrinter.WGhostScript();
        //    gs.AddParam("-sDEVICE=jpeg");
        //    gs.AddParam("-dJPEGQ=100");
        //    gs.AddParam("-dNOPAUSE");
        //    gs.AddParam("-dBATCH");
        //    gs.AddParam("-dSAFER");
        //    gs.AddParam("-r300");
        //    string outfile = txtOutFolderLoc.Text + txtFileName.Text + ".jpg";
        //    gs.AddParam(@"-sOutputFile=" + outfile);
        //    gs.AddParam(psFilePath);
        //    gs.Execute();
        //    acadApp.Exit();
        //}

        [CommandMethod("PublishViews2MultiSheet")]
        public static void PublishViews2MultiSheet()
        {
            //pwdWindow = new PasswordWindow();
            //pwdWindow.ShowDialog();
            Document doc = acadApp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            StringCollection viewsToPlot = new StringCollection();
            viewsToPlot.Add("Test1");
            viewsToPlot.Add("Test2");
            using (Transaction Tx = db.TransactionManager.StartTransaction())
            {
                ObjectId layoutId = LayoutManager.Current.GetLayoutId(LayoutManager.Current.CurrentLayout);
                Layout layout = Tx.GetObject(layoutId, OpenMode.ForWrite) as Layout;
                foreach (String viewName in viewsToPlot)
                {
                    PlotSettings plotSettings = new PlotSettings(layout.ModelType);
                    plotSettings.CopyFrom(layout);
                    PlotSettingsValidator psv = PlotSettingsValidator.Current;
                    psv.SetPlotConfigurationName(plotSettings, "DWF6 ePlot.pc3", "ANSI_A_(8.50_x_11.00_Inches)");
                    psv.RefreshLists(plotSettings);
                    psv.SetPlotViewName(plotSettings, viewName);
                    psv.SetPlotType(plotSettings, PlotType.View);
                    psv.SetUseStandardScale(plotSettings, true);
                    psv.SetStdScaleType(plotSettings, StdScaleType.ScaleToFit);
                    psv.SetPlotCentered(plotSettings, true);
                    psv.SetPlotRotation(plotSettings, PlotRotation.Degrees000);
                    psv.SetPlotPaperUnits(plotSettings, PlotPaperUnit.Inches);
                    plotSettings.PlotSettingsName = String.Format("{0}{1}", viewName, "PS");
                    plotSettings.PrintLineweights = true;
                    plotSettings.AddToPlotSettingsDictionary(db);
                    Tx.AddNewlyCreatedDBObject(plotSettings, true);
                    psv.RefreshLists(plotSettings);
                    layout.CopyFrom(plotSettings);
                }
                Tx.Commit();
            }
            short bgPlot = (short)acadApp.GetSystemVariable("BACKGROUNDPLOT");
            acadApp.SetSystemVariable("BACKGROUNDPLOT", 0);
            string dwgFileName = acadApp.GetSystemVariable("DWGNAME") as string;
            string dwgPath = acadApp.GetSystemVariable("DWGPREFIX") as string;
            using (Transaction Tx = db.TransactionManager.StartTransaction())
            {
                DsdEntryCollection collection = new DsdEntryCollection();
                ObjectId activeLayoutId = LayoutManager.Current.GetLayoutId(LayoutManager.Current.CurrentLayout);
                foreach (String viewName in viewsToPlot)
                {
                    Layout layout = Tx.GetObject(activeLayoutId, OpenMode.ForRead) as Layout;
                    DsdEntry entry = new DsdEntry();
                    entry.DwgName = dwgPath + dwgFileName;
                    entry.Layout = layout.LayoutName;
                    entry.Title = viewName;
                    entry.NpsSourceDwg = entry.DwgName;
                    entry.Nps = String.Format("{0}{1}", viewName, "PS");
                    collection.Add(entry);
                }
                dwgFileName = dwgFileName.Substring(0, dwgFileName.Length - 4);
                DsdData dsdData = new DsdData();
                dsdData.SheetType = SheetType.MultiDwf;
                dsdData.ProjectPath = dwgPath;
                dsdData.DestinationName = dsdData.ProjectPath + dwgFileName + ".dwf";
                /*Hardcode password*/
                dsdData.Password = "123456";
                if (File.Exists(dsdData.DestinationName)) File.Delete(dsdData.DestinationName);
                dsdData.SetDsdEntryCollection(collection);

                /*DsdFile */
                string dsdFile = dsdData.ProjectPath + dwgFileName + ".dsd";
                dsdData.WriteDsd(dsdFile);
                StreamReader sr = new StreamReader(dsdFile);
                string str = sr.ReadToEnd();
                sr.Close();
                str = str.Replace("PromptForDwfName=TRUE", "PromptForDwfName=FALSE");
                str = str.Replace("PwdProtectPublishedDWF=FALSE", "PwdProtectPublishedDWF=TRUE");
                int occ = 0;
                int index = str.IndexOf("Setup=");
                int startIndex = 0;
                StringBuilder dsdText = new StringBuilder();
                while (index != -1)
                {
                    String str1 = str.Substring(startIndex, index + 6 - startIndex);
                    dsdText.Append(str1);
                    dsdText.Append(String.Format("{0}{1}", viewsToPlot[occ], "PS"));
                    startIndex = index + 6;
                    index = str.IndexOf("Setup=", index + 6);
                    if (index == -1)
                    {
                        dsdText.Append(str.Substring(startIndex, str.Length - startIndex));
                    }
                    occ++;
                }
                StreamWriter sw = new StreamWriter(dsdFile);
                sw.Write(dsdText.ToString());
                sw.Close();
                dsdData.ReadDsd(dsdFile);
                File.Delete(dsdFile);
                PlotConfig plotConfig = PlotConfigManager.SetCurrentConfig("DWF6 ePlot.pc3");
                Publisher publisher = acadApp.Publisher;
                publisher.PublishExecute(dsdData, plotConfig);
                Tx.Commit();
            }
            acadApp.SetSystemVariable("BACKGROUNDPLOT", bgPlot);
        }

        #endregion

        #region <Private methods>

        private static void GetBlocksToPlotCollection(Database db, Editor ed, Transaction tr, string blockName, ref List<Blk2Plt> blockCol)
        {
            BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
            foreach (ObjectId btrId in bt)
            {
                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(btrId, OpenMode.ForRead);
                if (btr.IsLayout)
                {
                    Layout lo = (Layout)tr.GetObject(btr.LayoutId, OpenMode.ForRead);
                    BlockTableRecord ms = (BlockTableRecord)tr.GetObject(lo.BlockTableRecordId, OpenMode.ForRead);
                    foreach (ObjectId objId in ms)
                    {
                        Entity ent = (Entity)tr.GetObject(objId, OpenMode.ForRead);
                        if (!ent.GetType().ToString().Contains("BlockReference")) continue;
                        BlockReference blk = (BlockReference)ent;

                        if (!blk.XGetEffectiveName().ToUpper().Contains(blockName.ToUpper())) continue;

                        Blk2Plt theBlk = new Blk2Plt
                        {
                            BlockRef = blk,
                            LayoutObj = lo
                        };
                        blockCol.Add(theBlk);
                    }
                }
            }
        }

        private static List<ObjectId> GetLayoutIds(Database db)
        {
            List<ObjectId> layoutIds =
                new List<ObjectId>();

            using (Transaction Tx = db.TransactionManager.StartTransaction())
            {
                DBDictionary layoutDic = Tx.GetObject(
                        db.LayoutDictionaryId,
                        OpenMode.ForRead, false)
                    as DBDictionary;

                foreach (DBDictionaryEntry entry in layoutDic)
                {
                    layoutIds.Add(entry.Value);
                }
            }

            return layoutIds;
        }

        private static void PlotBlockColToPdf(string blockName, string printerName, string outPath, string paperSize = "ISO_A4_(297.00_x_210.00_MM)")
        {
            Document doc = acadApp.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;
            Transaction tr = db.TransactionManager.StartTransaction();
            Object sysVarBackPlot = acadApp.GetSystemVariable("BACKGROUNDPLOT");
            acadApp.SetSystemVariable("BACKGROUNDPLOT", 0);
            using (tr)
            {
                List<Blk2Plt> blocksToPlot = new List<Blk2Plt>();
                GetBlocksToPlotCollection(db, ed, tr, blockName, ref blocksToPlot);//Getting collection of blocks 
                ed.WriteMessage("\nThe number of blocks found: " + blocksToPlot.Count + "\n\n");

                if (blocksToPlot.Count < 1) return;

                if (PlotFactory.ProcessPlotState == ProcessPlotState.NotPlotting)
                {
                    PlotEngine pe = PlotFactory.CreatePublishEngine();
                    using (pe)
                    {
                        PlotProgressDialog ppd = new PlotProgressDialog(false, blocksToPlot.Count, true);
                        using (ppd)
                        {
                            int numSheet = 1;
                            // Setting up the PlotProgress dialog 
                            ppd.set_PlotMsgString(PlotMessageIndex.DialogTitle, "Custom Plot Progress");
                            ppd.set_PlotMsgString(PlotMessageIndex.CancelJobButtonMessage, "Cancel Job");
                            ppd.set_PlotMsgString(PlotMessageIndex.CancelSheetButtonMessage, "Cancel Sheet");
                            ppd.set_PlotMsgString(PlotMessageIndex.SheetSetProgressCaption, "Sheet Set Progress");
                            ppd.set_PlotMsgString(PlotMessageIndex.SheetProgressCaption, "Sheet Progress");
                            ppd.LowerPlotProgressRange = 0;
                            ppd.UpperPlotProgressRange = 100;
                            ppd.PlotProgressPos = 0;
                            ppd.OnBeginPlot();
                            ppd.IsVisible = true;

                            pe.BeginPlot(ppd, null);

                            foreach (Blk2Plt gblk in blocksToPlot)
                            {
                                // Starting new page 
                                ppd.StatusMsgString = "Plotting block " + numSheet + " of " + blocksToPlot.Count;
                                ppd.OnBeginSheet();
                                ppd.LowerSheetProgressRange = 0;
                                ppd.UpperSheetProgressRange = 100;
                                ppd.SheetProgressPos = 0;

                                PlotInfoValidator piv = new PlotInfoValidator();
                                piv.MediaMatchingPolicy = MatchingPolicy.MatchEnabled;
                                PlotPageInfo ppi = new PlotPageInfo();
                                PlotInfo pi = new PlotInfo();
                                BlockReference blk = gblk.BlockRef;
                                Layout lo = gblk.LayoutObj;

                                // Getting coodinates of window to plot 
                                Extents3d ext = (Extents3d)blk.Bounds;
                                Point3d first = ext.MaxPoint;
                                Point3d second = ext.MinPoint;
                                ResultBuffer rbFrom = new ResultBuffer(new TypedValue(5003, 1)), rbTo = new ResultBuffer(new TypedValue(5003, 2));
                                double[] firres = { 0, 0, 0 };
                                double[] secres = { 0, 0, 0 };
                                acedTrans(first.ToArray(), rbFrom.UnmanagedObject, rbTo.UnmanagedObject, 0, firres);
                                acedTrans(second.ToArray(), rbFrom.UnmanagedObject, rbTo.UnmanagedObject, 0, secres);
                                Extents2d window = new Extents2d(firres[0], firres[1], secres[0], secres[1]);

                                // We need a PlotSettings object based on the layout settings which we then customize 
                                PlotSettings ps = new PlotSettings(lo.ModelType);
                                LayoutManager.Current.CurrentLayout = lo.LayoutName;
                                pi.Layout = lo.Id;
                                ps.CopyFrom(lo);

                                // The PlotSettingsValidator helps create a valid PlotSettings object 
                                PlotSettingsValidator psv = PlotSettingsValidator.Current;
                                psv.SetPlotWindowArea(ps, window);
                                psv.SetPlotType(ps, PlotType.Window);
                                psv.SetUseStandardScale(ps, true);
                                psv.SetStdScaleType(ps, StdScaleType.ScaleToFit);
                                psv.SetPlotCentered(ps, true);
                                psv.SetPlotConfigurationName(ps, printerName, paperSize);

                                pi.OverrideSettings = ps;
                                piv.Validate(pi);

                                if (numSheet == 1) pe.BeginDocument(pi, doc.Name, null, 1, true, outPath); // Create document for the first page 

                                // Plot the window 
                                pe.BeginPage(ppi, pi, (numSheet == blocksToPlot.Count), null);
                                pe.BeginGenerateGraphics(null);
                                ppd.SheetProgressPos = 50;
                                pe.EndGenerateGraphics(null);

                                // Finish the sheet 
                                pe.EndPage(null);
                                ppd.SheetProgressPos = 100;
                                ppd.PlotProgressPos += Convert.ToInt32(100 / blocksToPlot.Count);
                                ppd.OnEndSheet();
                                numSheet++;
                            }
                            // Finish the document and finish the plot 
                            pe.EndDocument(null);
                            ppd.PlotProgressPos = 100;
                            ppd.OnEndPlot();
                            pe.EndPlot(null);
                            ed.WriteMessage("\nPlot completed successfully!\n\n");
                        }
                    }
                }
                else
                {
                    ed.WriteMessage("\nAnother plot is in progress.\n\n");
                }
                tr.Commit();
            }
            acadApp.SetSystemVariable("BACKGROUNDPLOT", sysVarBackPlot);
        }

        private static void BatchPublish(List<string> docsToPlot)
        {
            DsdEntryCollection collection = new DsdEntryCollection();
            Document doc = acadApp.DocumentManager.MdiActiveDocument;

            foreach (string filename in docsToPlot)
            {
                using (DocumentLock doclock = doc.LockDocument())
                {
                    Database db = new Database(false, true);
                    db.ReadDwgFile(filename, FileShare.Read, true, "");

                    FileInfo fi = new FileInfo(filename);
                    string docName = fi.Name.Substring(0, fi.Name.Length - 4);

                    using (Transaction tr = db.TransactionManager.StartTransaction())
                    {
                        foreach (ObjectId layoutId in GetLayoutIds(db))
                        {
                            Layout layout = tr.GetObject(layoutId, OpenMode.ForRead) as Layout;

                            DsdEntry entry = new DsdEntry
                            {
                                DwgName = filename,
                                Layout = layout.LayoutName,
                                Title = docName + "_" + layout.LayoutName
                            };
                            entry.NpsSourceDwg = entry.DwgName;
                            entry.Nps = "Setup1";

                            collection.Add(entry);
                        }
                        tr.Commit();
                    }
                }
            }

            DsdData dsdData = new DsdData
            {
                SheetType = SheetType.SingleDwf,
                ProjectPath = "C:\\Temp"
            };

            //Not used for "SheetType.SingleDwf"
            //dsdData.DestinationName = dsdData.ProjectPath + "\\output.dwf";

            dsdData.SetDsdEntryCollection(collection);
            string dsdFile = dsdData.ProjectPath + "\\dsdData.dsd";

            //Workaround to avoid promp for dwf file name
            //set PromptForDwfName=FALSE in dsdData
            //using StreamReader/StreamWriter

            if (File.Exists(dsdFile))
                File.Delete(dsdFile);

            dsdData.WriteDsd(dsdFile);

            StreamReader sr = new StreamReader(dsdFile);
            string str = sr.ReadToEnd();
            sr.Close();

            str = str.Replace("PromptForDwfName=TRUE", "PromptForDwfName=FALSE");

            StreamWriter sw = new StreamWriter(dsdFile);
            sw.Write(str);
            sw.Close();

            dsdData.ReadDsd(dsdFile);
            File.Delete(dsdFile);

            PlotConfig plotConfig = PlotConfigManager.SetCurrentConfig("DWF6 ePlot.pc3");
            Publisher publisher = acadApp.Publisher;

            publisher.PublishExecute(dsdData, plotConfig);
        }

        private bool SendToMapHttpClient(string commnadName, string requestUrl, CommandCancellationTokenSource cts)
        {
            bool actionResult = true;
            try
            {
                Ed.WriteMessage(PluginSettings.Prompt + commnadName + " get " + requestUrl + "...");
                HttpClient client = new HttpClient();
                HttpResponseMessage wcfResponse = client.GetAsync(requestUrl).GetAwaiter().GetResult();

                if ((int)wcfResponse.StatusCode >= 500)
                {
                    cts.OperationCanceledException = new OperationCanceledException(
                        $"Command {commnadName} response.StatusCode: {wcfResponse.StatusCode}");
                    cts.Cancel(false);
                    Ed.WriteMessage(PluginSettings.Prompt + cts.OperationCanceledException.Message);
                    actionResult = false;
                }

                if (!cts.IsCancellationRequested)
                {
                    HttpContent stream = wcfResponse.Content;
                    var data = stream.ReadAsStringAsync();
                    Ed.WriteMessage(PluginSettings.Prompt + data.Result);
                    cts.Cancel(false);
                }
            }
            catch (Exception ex)
            {
                ICommandLine commandLine = Plugin.GetService<ICommandLine>();
                commandLine.Error(commnadName, ex.ToStringMessage());
                cts.Cancel();
                actionResult = false;
            }
            return actionResult;
        }

        // Returns whether the provided DB extents - retrieved from
        // Database.Extmin/max - are "valid" or whether they are the default
        // invalid values (where the min's coordinates are positive and the
        // max coordinates are negative)
        private bool ValidDbExtents(Point3d min, Point3d max)
        {
            return !(min.X > 0 && min.Y > 0 && min.Z > 0 &&
                     max.X < 0 && max.Y < 0 && max.Z < 0);
        }

        private void Zoom(Point3d pMin, Point3d pMax, Point3d pCenter, double dFactor)
        {
            // Get the current document and database
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            int nCurVport = System.Convert.ToInt32(acadApp.GetSystemVariable("CVPORT"));

            // Get the extents of the current space when no points 
            // or only a center point is provided
            // Check to see if Model space is current
            if (acCurDb.TileMode == true)
            {
                if (pMin.Equals(new Point3d()) == true &&
                    pMax.Equals(new Point3d()) == true)
                {
                    pMin = acCurDb.Extmin;
                    pMax = acCurDb.Extmax;
                }
            }
            else
            {
                // Check to see if Paper space is current
                if (nCurVport == 1)
                {
                    // Get the extents of Paper space
                    if (pMin.Equals(new Point3d()) == true &&
                        pMax.Equals(new Point3d()) == true)
                    {
                        pMin = acCurDb.Pextmin;
                        pMax = acCurDb.Pextmax;
                    }
                }
                else
                {
                    // Get the extents of Model space
                    if (pMin.Equals(new Point3d()) == true &&
                        pMax.Equals(new Point3d()) == true)
                    {
                        pMin = acCurDb.Extmin;
                        pMax = acCurDb.Extmax;
                    }
                }
            }

            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Get the current view
                using (ViewTableRecord acView = acDoc.Editor.GetCurrentView())
                {
                    Extents3d eExtents;

                    // Translate WCS coordinates to DCS
                    Matrix3d matWCS2DCS;
                    matWCS2DCS = Matrix3d.PlaneToWorld(acView.ViewDirection);
                    matWCS2DCS = Matrix3d.Displacement(acView.Target - Point3d.Origin) * matWCS2DCS;
                    matWCS2DCS = Matrix3d.Rotation(-acView.ViewTwist,
                                     acView.ViewDirection,
                                     acView.Target) * matWCS2DCS;

                    // If a center point is specified, define the min and max 
                    // point of the extents
                    // for Center and Scale modes
                    if (pCenter.DistanceTo(Point3d.Origin) != 0)
                    {
                        pMin = new Point3d(pCenter.X - (acView.Width / 2),
                            pCenter.Y - (acView.Height / 2), 0);

                        pMax = new Point3d((acView.Width / 2) + pCenter.X,
                            (acView.Height / 2) + pCenter.Y, 0);
                    }

                    // Create an extents object using a line
                    using (Line acLine = new Line(pMin, pMax))
                    {
                        eExtents = new Extents3d(acLine.Bounds.Value.MinPoint,
                            acLine.Bounds.Value.MaxPoint);
                    }

                    // Calculate the ratio between the width and height of the current view
                    double dViewRatio;
                    dViewRatio = (acView.Width / acView.Height);

                    // Tranform the extents of the view
                    matWCS2DCS = matWCS2DCS.Inverse();
                    eExtents.TransformBy(matWCS2DCS);

                    double dWidth;
                    double dHeight;
                    Point2d pNewCentPt;

                    // Check to see if a center point was provided (Center and Scale modes)
                    if (pCenter.DistanceTo(Point3d.Origin) != 0)
                    {
                        dWidth = acView.Width;
                        dHeight = acView.Height;

                        if (dFactor == 0)
                        {
                            pCenter = pCenter.TransformBy(matWCS2DCS);
                        }

                        pNewCentPt = new Point2d(pCenter.X, pCenter.Y);
                    }
                    else // Working in Window, Extents and Limits mode
                    {
                        // Calculate the new width and height of the current view
                        dWidth = eExtents.MaxPoint.X - eExtents.MinPoint.X;
                        dHeight = eExtents.MaxPoint.Y - eExtents.MinPoint.Y;

                        // Get the center of the view
                        pNewCentPt = new Point2d(((eExtents.MaxPoint.X + eExtents.MinPoint.X) * 0.5),
                            ((eExtents.MaxPoint.Y + eExtents.MinPoint.Y) * 0.5));
                    }

                    // Check to see if the new width fits in current window
                    if (dWidth > (dHeight * dViewRatio)) dHeight = dWidth / dViewRatio;

                    // Resize and scale the view
                    if (dFactor != 0)
                    {
                        acView.Height = dHeight * dFactor;
                        acView.Width = dWidth * dFactor;
                    }

                    // Set the center of the view
                    acView.CenterPoint = pNewCentPt;

                    // Set the current view
                    acDoc.Editor.SetCurrentView(acView);
                }

                // Commit the changes
                acTrans.Commit();
            }
        }

        #endregion
    }
}
