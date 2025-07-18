using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Windows;
using ID.Infrastructure;
using ID.Infrastructure.Commands;
using ID.Infrastructure.Enums;
using ID.Infrastructure.Models;
using Intellidesk.AcadNet;
using Intellidesk.AcadNet.Common.Core;
using Intellidesk.AcadNet.Common.Enums;
using Intellidesk.AcadNet.Common.Extensions;
using Intellidesk.AcadNet.Common.Interfaces;
using Intellidesk.AcadNet.Common.Internal;
using Intellidesk.AcadNet.Common.Models;
using Intellidesk.AcadNet.Common.Utils;
using Intellidesk.AcadNet.Components;
using Intellidesk.AcadNet.Services;
using Intellidesk.AcadNet.Services.Core;
using Intellidesk.AcadNet.Services.Extentions;
using Intellidesk.AcadNet.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static Autodesk.AutoCAD.ApplicationServices.Core.Application;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Exception = System.Exception;
using Files = ID.Infrastructure.Files;
using Size = System.Drawing.Size;

[assembly: CommandClass(typeof(CommandMain))]
namespace Intellidesk.AcadNet
{
    /// <summary>
    /// This class is instantiated by AutoCAD for each document when a command is called by the user the first time in the context
    /// of a given document. In other words, non static data in this class is implicitly per-document!
    /// </summary>

    public class CommandMain : CommandDocumentBase
    {
        [DllImport("kernel32.dll")]
        public static extern bool SetProcessWorkingSetSize(IntPtr handle, int minimumWorkingSetSize, int maximumWorkingSetSize);

        [DllImport("user32.dll")]
        public static extern IntPtr SetFocus(IntPtr hWnd);

        [CommandMethod(CommandNames.UserGroup, CommandNames.PluginLoad, CommandFlags.Session | CommandFlags.NoHistory)]
        public void RibbonLoad()
        {
            RibbonControl rc = ComponentManager.Ribbon;
            if (rc != null && rc.IsVisible && !rc.ContainsTab(PluginSettings.Name))
            {
                EventHandler backgroundRenderStarting = null;
                EventHandler backgroundRenderFinished = null;

                var customRibbonTab = Plugin.GetService<CustomRibbonTab>();

                backgroundRenderStarting = (sender, args) =>
                {
                    Mouse.OverrideCursor = Cursors.Wait;

                    var ribbonControl = (RibbonControl)sender;
                    ribbonControl.BackgroundRenderStarting -= backgroundRenderStarting;
                    ribbonControl.Tabs.Add(customRibbonTab);
                    customRibbonTab.AddControls();

                    if (Plugin.Initilized)
                        Ed.WriteMessage("Intellix plugin initialized."); // $"{appSettings.Prompt}plugin initialized."
                    else
                        Ed.WriteMessage($"Please call to admin. Intellix plugin modules not initialized: {string.Join(",", Plugin.InitilizedmoduleTypes.Where(x => !x.Value).Select(x => x.Key))}"); // $"{appSettings.Prompt}plugin initialized."
                };

                backgroundRenderFinished = (sender, args) =>
                {
                    var ribbonControl = (RibbonControl)sender;
                    ribbonControl.BackgroundRenderFinished -= backgroundRenderFinished;
                    customRibbonTab.LoadControlData();

                    if (!Doc.IsNamedDrawing)
                    {
                        if (!Doc.Database.IsRegAppTableRecord(PluginSettings.Name))
                            Doc.AddRegAppTableRecord(PluginSettings.Name);
                    }

                    var commandArgs = CommandArgs.Get(CommandNames.PluginPreLoad);
                    if (commandArgs != null && !commandArgs.CancelToken.IsCancellationRequested)
                    {
                        var tasks = commandArgs.CommandParameter as List<Task>;
                        Task.WhenAll(tasks).ConfigureAwait(false);
                        commandArgs.Clean();
                    }

                    DocumentManager.MdiActiveDocument.UserData.Clear(); //important

                    //if (panel.LayoutItemButtons.Any())
                    //{
                    //    panel.UIControlRefresh();
                    //    //foreach (var layoutItemButton in panel.LayoutItemButtons)
                    //    //{
                    //    //    if (File.Exists(layoutItemButton.Text) && DocumentManager.GetDocument(layoutItemButton.Text) == null)
                    //    //        DocumentManager.Open(layoutItemButton.Text);
                    //    //}
                    //}

                    Autodesk.AutoCAD.Internal.Utils.PostCommandPrompt();
                    Mouse.OverrideCursor = null;

                    Notifications.DisplayNotifyMessage(NotifyStatus.Ready);
                };

                rc.BackgroundRenderStarting += backgroundRenderStarting;
                rc.BackgroundRenderFinished += backgroundRenderFinished;

                rc.StartBackgroundTabRendering();
            }
            else
            {
                var commandArgs = CommandArgs.Get(CommandNames.PluginPreLoad);
                if (commandArgs != null)
                {
                    List<Task> tasks = commandArgs.CommandParameter as List<Task>;
                    Task.WhenAll(tasks).ConfigureAwait(false);
                }
            }
        }

        /// <summary> Application Session Command with localized name </summary>
        [CommandMethod(CommandNames.UserGroup, CommandNames.Search, CommandFlags.Session)]
        public void PaletteSearchText()
        {
            ToolsManager.LoadPallete(PaletteNames.Search, null, CommandNames.Search);
        }

        [CommandMethod(CommandNames.UserGroup, CommandNames.Explorer, CommandFlags.Session)]
        public void PaletteExplorer()
        {
            var currentFolder = !string.IsNullOrEmpty(PluginSettings.CurrentFolder)
                ? PluginSettings.CurrentFolder : null;

            ToolsManager.LoadPallete(PaletteNames.ProjectExplorer,
                new CommandArgs(null, "Load", currentFolder), CommandNames.Explorer, LoadExplorerCallBack);
        }

        private static void LoadExplorerCallBack(object sender, ICommandArgs args)
        {
            //Mouse.OverrideCursor = null;
            //Mouse.OverrideCursor = Cursors.Wait;
            var folder = args as ICommandArgs;
            //if (folder == null) return;
            //folder.IsExpanded = true;
            //folder.IsSelected = true;
            //var palleteTab = Sender as ITabProjectExplorerView;
            //if (palleteTab == null) return;
            //Commands.DelayAction(500, () => { palleteTab.ExpandFolder(folder, true); });
            //Mouse.OverrideCursor = null;
        }

        [CommandMethod(CommandNames.UserGroup, CommandNames.WorkFilesOpenAll, CommandFlags.Session)]
        public void PaletteExplorerOpenAll()
        {
            if (ComponentManager.Ribbon != null && ComponentManager.Ribbon.IsVisible)
            {
                Notifications.SendNotifyMessageAsync(NotifyStatus.Working);

                IProjectExplorerPanelContext projectExplorerPanelViewModel = Plugin.GetService<IProjectExplorerPanelContext>();
                if (projectExplorerPanelViewModel.LayoutItemButtons.Any())
                {
                    projectExplorerPanelViewModel.RibbonRefresh();

                    foreach (var layoutItemButton in projectExplorerPanelViewModel.LayoutItemButtons)
                    {
                        if (File.Exists(layoutItemButton.Text) &&
                            DocumentManager.GetDocument(layoutItemButton.Text) == null)
                        {
                            try
                            {
                                DocumentManager.Open(layoutItemButton.Text);
                            }
                            catch (Exception ex)
                            {
                                if (Doc != null)
                                    Ed.WriteMessage("command PaletteExplorerOpenAll error :" + ex);
                            }

                        }
                    }
                }

                Notifications.SendNotifyMessageAsync(NotifyStatus.Ready);
            }
            else
            {
                var workItemsFolderPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Intellidesk", "WorkItems");

                string[] filePaths = Directory.GetFiles(workItemsFolderPath, "*.lnk", SearchOption.TopDirectoryOnly);

                foreach (var filePath in filePaths)
                {
                    var realFilePath = Files.GetShortcutTargetFile(filePath);
                    if (File.Exists(realFilePath) && DocumentManager.GetDocument(realFilePath) == null)
                        DocumentManager.Open(realFilePath);
                }
            }
        }

        [CommandMethod(CommandNames.MainGroup, CommandNames.SearchPanelRemove, CommandFlags.Session | CommandFlags.NoHistory)]
        public void SearchPanelRemove()
        {
            IPanelTabView palette = ToolsManager.PaletteTabs[PaletteNames.Search];
            ToolsManager.PaletteTabs.CloseTab(palette);
        }

        [CommandMethod(CommandNames.MainGroup, CommandNames.LayerQueriesPanelRemove, CommandFlags.Session | CommandFlags.NoHistory)]
        public void LayerQueriesPanelRemove()
        {
            IPanelTabView palette = ToolsManager.PaletteTabs[PaletteNames.LayerQueries];
            ToolsManager.PaletteTabs.CloseTab(palette);
        }

        [CommandMethod(CommandNames.MainGroup, CommandNames.BayQueriesPanelRemove, CommandFlags.Session | CommandFlags.NoHistory)]
        public void BayQueriesPanelRemove()
        {
            IPanelTabView palette = ToolsManager.PaletteTabs[PaletteNames.BayQueries];
            ToolsManager.PaletteTabs.CloseTab(palette);
        }

        [CommandMethod(CommandNames.MainGroup, CommandNames.ExplorerPanelRemove, CommandFlags.Session | CommandFlags.NoHistory)]
        public void ExplorerPanelRemove()
        {
            IPanelTabView palette = ToolsManager.PaletteTabs[PaletteNames.ProjectExplorer];
            ToolsManager.PaletteTabs.CloseTab(palette);
        }

        [CommandMethod(CommandNames.MainGroup, CommandNames.MapViewPanelRemove, CommandFlags.Session | CommandFlags.NoHistory)]
        public void MapViewPanelRemove()
        {
            IPanelTabView palette = ToolsManager.PaletteTabs[PaletteNames.MapView];
            ToolsManager.PaletteTabs.CloseTab(palette);
        }

        /// <summary> Application Session Command with localized name </summary>
        [CommandMethod(CommandNames.MainGroup, CommandNames.Ribbon, CommandFlags.Session | CommandFlags.NoHistory)]
        public void RibbonBar()
        {
            var rc = ComponentManager.Ribbon;
            if (rc != null && rc.IsVisible)
            {
                //IProjectExplorerPanelContext panel = Plugin.GetService<IProjectExplorerPanelContext>();

                //var ribbonTab = rc.FindTab(PluginSettings.Name) ??
                //                ComponentManager.Ribbon.InitControls(PluginSettings.Name, panel, AcadNetManager.InteractionRequestViewModel);
                //ribbonTab.IsActive = true;
            }
            else
            {
                DocumentManager.MdiActiveDocument.Editor.WriteMessage(PluginSettings.Prompt + "ComponentManager.Ribbon not loaded\n");
            }
        }

        /// <summary> Deciding of problem: Autocad main window not get focus after palette activity </summary>
        [CommandMethod(CommandNames.MainGroup, CommandNames.Regen, CommandFlags.Session | CommandFlags.NoHistory)]
        public void Regen()
        {
            //if (CurrentTaskArgs != null && CurrentTaskArgs.ProgressIndex == CurrentTaskArgs.ProgressLimit &&
            //    CurrentTaskArgs.ActionCompleted != null)
            //{
            //    CurrentTaskArgs.ActionCompleted(CurrentTaskArgs);
            //}

            // Button state refresh
            if (ComponentManager.Ribbon != null && ComponentManager.Ribbon.IsVisible)
                ComponentManager.Ribbon.Focus();
            //if (UIBuildService.CurrentPaletteSet.CurrentView != null) 
            //((ProjectExplorerView)UIBuildService.CurrentPaletteSet.CurrentView).Focus();
        }

        [CommandMethod(CommandNames.MainGroup, CommandNames.Refresh, CommandFlags.Session)]
        public void Refresh()
        {
            using (CommandContext context = new CommandContext(CommandNames.Refresh, "Refresh"))
            {
                //System.Windows.Point screenMin = Doc.Window.DeviceIndependentLocation;
                //System.Windows.Size screenMax = Doc.Window.DeviceIndependentSize;

                if (CommandContext.CurrentEntity != null)
                {
                    CommandContext.CurrentEntity.Unhighlight();
                    CommandContext.CurrentEntity = null;
                }

                Point2d screenSize = (Point2d)GetSystemVariable("SCREENSIZE"); //EXTMIN, EXTMAX
                Point3d upperLeftWorld = Ed.PointToWorld(new Point(0, 0), 0);
                Point3d lowerRightWorld = Ed.PointToWorld(new Point((int)screenSize.X, (int)screenSize.Y), 0);

                context.CommandLine.ZoomWindow(lowerRightWorld, upperLeftWorld);
                //CommandLine.SendToExecute("REGEN");
                context.CommandLine.Cancel();

                Selects.Clean();

                acadApp.SetSystemVariable("FILEDIA", 1);

                Mouse.OverrideCursor = null;
                Notifications.SendNotifyMessageAsync(new NotifyArgs(NotifyStatus.Ready));
            }
        }

        [CommandMethod(CommandNames.MainGroup, CommandNames.Ruler, CommandFlags.Session | CommandFlags.UsePickSet)]
        public void Ruler()
        {
            try
            {
                if (!Utils.IsModelSpace()) return;
                using (Doc.LockDocument())
                {

                    ProgressMeter pm = new ProgressMeter();
                    pm.Start($"Calculating objects...");
                    Thread.Sleep(1000);

                    IEnumerable<Entity> entities = Selects.GetEntities(GetSelectOptions.SelectImplied, EntityTypes.CURVE).ToList();
                    if (entities.Any())
                    {
                        pm.Start($"Calculating {entities.Count()} objects...");
                        pm.SetLimit(entities.Count());

                        double sumLength = 0;
                        Document doc = acadApp.DocumentManager.MdiActiveDocument;

                        foreach (Curve ent in entities)
                        {
                            sumLength += ent.Length();
                            doc.Editor.WriteMessage($"\n{ent.GetType().Name} length: {ent.Length().ToString("N5")}");
                            pm.MeterProgress();
                        }

                        doc.Editor.WriteMessage($"\n{CommandNames.UserGroup} total length: {sumLength.ToString("N5")}");
                        Clipboard.SetText(sumLength.ToString("N5"));
                    }

                    pm.Stop();
                    pm.Dispose();
                }
            }
            catch (Exception)
            {
                //ignore
            }
        }

        [CommandMethod(CommandNames.MainGroup, CommandNames.LoadAfter, CommandFlags.Session | CommandFlags.NoHistory)]
        public void LoadAfter()
        {
            //var task = Task<bool>.Factory.StartNew(() =>
            //{
            //    var taskResult = false;
            //    UiDispatcher.Invoke(() =>
            //    {
            //Mouse.OverrideCursor = Cursors.Wait;
            //if (!ComponentManager.Ribbon.ContainsTab(Plugin.Name))
            //{
            //    ComponentManager.Ribbon.Load(Plugin.Name);
            //    if (ComponentManager.Ribbon != null && ComponentManager.Ribbon.IsVisible) ComponentManager.Ribbon.Focus();

            //    EventAggregator.GetEvent<NotifyMessageStringEvent>().Publish("Working");
            //    taskResult = true;
            //}
            //Mouse.OverrideCursor = null;
            //    });
            //    return taskResult;
            //});

            //task.ContinueWith(t =>
            //{
            //    if (!t.Result) return;
            //    UiDispatcher.Invoke(() =>
            //    {
            //Mouse.OverrideCursor = Cursors.Wait;
            //var panel = UnityContainer.Resolve<ProjectExplorerViewModel>();

            ////interaction
            ////var interactionRequestViewModel = UnityContainer.Resolve<InteractionRequestViewModel>();
            ////buildService.InteractionRequestViewModel = interactionRequestViewModel;
            //ComponentManager.Ribbon.LoadData(panel);
            //ComponentManager.Ribbon.TabNotifySendMessage();

            //Mouse.OverrideCursor = null;
            //    });
            //});
        }

        /// <summary> Deciding of problem: Autocad main window not get focus after palette activity. </summary>
        [CommandMethod(CommandNames.MainGroup, CommandNames.Purge, CommandFlags.Session | CommandFlags.UsePickSet)]
        public void Purge()
        {
            using (var context = new CommandContext(CommandNames.Purge, "Purging"))
            {
                var ids = new List<ObjectId>().XGetObjects(new ActionArguments { FilterVisible = false });
                var erasedCount = ids.XEraseObjects();

                if (erasedCount > 0)
                    context.CommandLine.WriteMessage($"{PluginSettings.Prompt} unvisible objects count: {erasedCount}");
                else
                    context.CommandLine.WriteMessage(PluginSettings.Prompt + "unvisible objects not exist");

                var db = DocumentManager.MdiActiveDocument.Database;
                db.XClearUnrefedBlocks();
                db.XPurgeBlocks();

                context.CommandLine.SendToExecute("PURGE ");
            }
        }

        [CommandMethod(CommandNames.MainGroup, CommandNames.Undo, CommandFlags.Session | CommandFlags.NoHistory)]
        public static void Undo()
        {
            Editor ed = DocumentManager.MdiActiveDocument.Editor;
            ed.Command("UNDO", "M");
        }

        [CommandMethod(CommandNames.MainGroup, CommandNames.XCheckPalettesetClose, CommandFlags.NoHistory)]
        public void CheckPalettesetClose()
        {
            var ps = ToolsManager.PaletteTabs.RootPaletteSet;

            // If it's invisible, it has been closed
            if (ps != null && !ps.Visible)
            {
                // Set the static instance to null and fire the subscribed event
                if (ps.Size != new Size(0, 0))
                    ToolsManager.PaletteTabClose(ps, EventArgs.Empty);
            }
        }

        [CommandMethod(CommandNames.MainGroup, CommandNames.XFileOpen, CommandFlags.Session | CommandFlags.NoHistory)]
        public void FileOpen()
        {
            using (var commandArgs = CommandArgs.Get(CommandNames.XFileOpen))
            {
                if (commandArgs == null || commandArgs.CancelToken.IsCancellationRequested) return;

                string[] args = (string[])commandArgs.CommandParameter;
                DocumentManager.Open(args[0], args[1]);
                commandArgs.Clean();
            }
        }

        [CommandMethod(CommandNames.MainGroup, CommandNames.XDisplayPoint, CommandFlags.Session | CommandFlags.NoHistory)]
        public void DisplayPoint()
        {
            using (var commandArgs = CommandArgs.Get(CommandNames.XDisplayPoint))
            {
                if (commandArgs == null || commandArgs.CancelToken.IsCancellationRequested) return;

                string[] args = (string[])commandArgs.CommandParameter;

                Mouse.OverrideCursor = Cursors.Wait;
                try
                {
                    using (Doc.LockDocument())
                    {
                        var blockRefId = Db.InsertBlock("gis_point", new Point3d(double.Parse(args[0]), double.Parse(args[1]), 0),
                            new BlockOptions { Scale = 2.0 });

                        if (blockRefId != ObjectId.Null)
                        {
                            using (new SysVarOverride("VTENABLE", 7))
                            {
                                ICommandLine commandLine = Plugin.GetService<ICommandLine>();
                                commandLine.Zoom(blockRefId, 3);
                                Ed.Regen();
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    //commandArgs.ActionMessage.Append("Drawing point failed:" + ex.Message);
                    //notifyResult = new ErrorNotifyArgs("Drawing point failed: " + ex.Message);
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
                commandArgs.Clean();
            }
        }

        [CommandMethod(CommandNames.MainGroup, CommandNames.XWriteMessage, CommandFlags.Session | CommandFlags.NoHistory)]
        public void WriteMessage()
        {
            using (ICommandArgs commandArgs = CommandArgs.Get(CommandNames.XWriteMessage))
            {
                if (commandArgs == null || commandArgs.CancelToken.IsCancellationRequested) return;

                Ed.WriteMessage(commandArgs.CommandParameter.ToString());

                commandArgs.Clean();
            }
        }

        [CommandMethod(CommandNames.MainGroup, CommandNames.XIdleOnHubDisconnected, CommandFlags.Session | CommandFlags.NoHistory)]
        public async Task SignalRHostReset()
        {
            var signalRClientHost = AcadNetHttpServerHost.SignalRClientHost;
            if (!signalRClientHost.IsConnected && !signalRClientHost.IsWorking(CommandNames.XIdleOnHubDisconnected))
                await signalRClientHost.LoadHostAsync();
        }

        [CommandMethod(CommandNames.MainGroup, CommandNames.XSwitchToModelspace, CommandFlags.Session | CommandFlags.NoHistory)]
        public void SwitchToModelSpace()
        {
            Document doc = acadApp.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;

            short tilemode = (short)acadApp.GetSystemVariable("TILEMODE");
            using (DocumentLock docLock = doc.LockDocument())
            {
                if (tilemode == 1)
                    acadApp.SetSystemVariable("TILEMODE", 0);
                else if ((short)acadApp.GetSystemVariable("CVPORT") != 1)
                    ed.SwitchToPaperSpace();
                else
                {
                    try { ed.SwitchToModelSpace(); }
                    catch { acadApp.SetSystemVariable("TILEMODE", 1); }
                }
            }
        }

        [CommandMethod(CommandNames.MainGroup, CommandNames.XLayersLoadData, CommandFlags.Session | CommandFlags.NoHistory)]
        public void LayersLoadData()
        {
            Notifications.SendNotifyMessageAsync(NotifyStatus.Working);

            using (var commandArgs = CommandArgs.Get(CommandNames.XLayersLoadData))
            {
                var layerService = Plugin.GetService<ILayerService>();
                layerService.Load();

                Edit.WriteMessage($"{Plugin.Settings.Prompt}{nameof(CommandMain)} layer data loaded");

                if (commandArgs.Sender != null && commandArgs.Sender.GetType().GetInterfaces().Contains(typeof(IPanelTabView)))
                {
                    IPanelTabView panelTabView = commandArgs.Sender as IPanelTabView;
                    panelTabView.OnActivate(commandArgs);
                }
            }

            Notifications.SendNotifyMessageAsync(NotifyStatus.Ready, 500);
        }

        public void Test()
        {
            Autodesk.AutoCAD.ApplicationServices.Document doc = DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            TypedValue[] filter = { new TypedValue(0, "INSERT") };
            PromptSelectionResult psr = ed.GetSelection(new SelectionFilter(filter));
            if (psr.Status != PromptStatus.OK) return;
            PromptPointResult ppr = ed.GetPoint("\nInsertion point: ");
            if (ppr.Status != PromptStatus.OK) return;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                //System.Data.DataTable dataTable = psr.Value.GetObjectIds()
                //    .Select(id => new ID.AcadNet.Model.BlockAttribute(id.GetObject<BlockReference>()))
                //    .ToDataTable("Extraction");

                //Table tbl = dataTable.ToAcadTable(9.0, 40.0);
                //tbl.Position = ppr.Value.TransformBy(ed.CurrentUserCoordinateSystem);
                //BlockTableRecord btr = db.CurrentSpaceId.GetObject<BlockTableRecord>(OpenMode.ForWrite);
                //btr.AppendEntity(tbl);
                //Tr.AddNewlyCreatedDBObject(tbl, true);
                //try
                //{
                //    string filename = (string)App.GetSystemVariable("dwgprefix") + "Extraction.xls";
                //    dataTable.WriteXls(filename, null, true);
                //}
                //catch
                //{
                //    App.ShowAlertDialog("Failed to open Excel");
                //}
                //Tr.Commit();
            }
        }

        internal void OnObjectErased(object sender, ObjectErasedEventArgs e)
        {
            //if (e.Erased)
            //{
            //    if (e.DBObject.GetType() == typeof(Polyline))
            //    {
            //        var currentObjectIds = ((List<ObjectId>)LsdsTaskArgs.DataSource).XGetObjects(new[] { typeof(Polyline) })
            //            .Where(X => ((Entity)X.GetObject(OpenMode.ForRead, false)).Layer.Contains(".PRT"));
            //        if (!currentObjectIds.Any())
            //        {
            //        }
            //    }
            //}
        }

        /// <summary> Alternative to Compose, Loading the DataContext to ProjectExplorerViewModel from others sources of data </summary>
        public MapViewModel ComposeViewModel()
        {
            //if (Plugin.IsComposed) return PluginBuilder.ProjectExplorerViewModel;

            try
            {
                //var provider = new RepositoryProvider(new RepositoryFactories());
                //var context = new AcadNetContext();
                //panel = new ProjectExplorerViewModel(new UnitOfWork(context, provider));
                //ProjectManager.IsComposed = true;
                //Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n" + ProjectManager.NameMsg + " plugin mannualy composed\n");
            }
            catch (Exception)
            {
            }
            return null;
        }

        public static void MemoryClear()
        {
            SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
        }

        public async Task TaskRun(int num)
        {
            Ed.WriteMessage("Thread {0} - Start {1}", Thread.CurrentThread.ManagedThreadId, num);

            await Task.Run(() =>
            {

            });

            Ed.WriteMessage("Thread {0} - End {1}", Thread.CurrentThread.ManagedThreadId, num);
        }
    }
}