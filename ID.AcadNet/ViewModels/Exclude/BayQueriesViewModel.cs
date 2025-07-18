//extern alias Newtonsoft10;
//extern alias Newtonsoft6;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using ID.Infrastructure;
using ID.Infrastructure.Commands;
using ID.Infrastructure.Enums;
using ID.Infrastructure.Extensions;
using ID.Infrastructure.Interfaces;
using ID.Infrastructure.Models;
using Intellidesk.AcadNet.Common;
using Intellidesk.AcadNet.Common.Commands;
using Intellidesk.AcadNet.Common.Core;
using Intellidesk.AcadNet.Common.Extensions;
using Intellidesk.AcadNet.Common.Interfaces;
using Intellidesk.AcadNet.Common.Model;
using Intellidesk.AcadNet.Interfaces;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Cursors = System.Windows.Input.Cursors;

namespace Intellidesk.AcadNet.ViewModels
{
    public class Bay
    {
        public string Name { get; set; }
        public ObjectId ObjectId { get; set; }
        public Dictionary<string, string> Attrs { get; set; }
        public Point3dCollection Vertices { get; set; }
        public Extents2d Extends => Vertices.XGetPointsExtents2d();

        public Bay(string name, ObjectId objectId, Dictionary<string, string> attrs, Point3dCollection vertices)
        {
            Name = name;
            ObjectId = objectId;
            Attrs = attrs;
            Vertices = vertices;
        }
    }

    public class BayQueriesViewModel : BaseViewModel, IBayQueriesViewModel, IExecutablePanelViewModel
    {
        private IPluginSettings _appSettings;
        public Action<object> Load { get; set; }

        #region <ctor>
        public BayQueriesViewModel()
        {
            ProgressBarMinimum = 0;
            ProgressBarMaximum = 100;

            RunCommand = new DelegateCommand<string>(ExecuteRunCommand, CanExecuteRunCommand);
            StopCommand = new DelegateCommand<object>(ExecuteStopCommand);
            ResetCommand = new DelegateCommand<object>(ExecuteResetCommand);
            CloseCommand = new DelegateCommand<object>(ExecuteCloseCommand);
            RefreshCommand = new DelegateCommand<object>(ExecuteRefreshCommand);
            SelectSetCommand = new DelegateCommand<object>(ExecuteSelectSetCommand);
            ExportCommand = new DelegateCommand<object>(ExecuteExportCommand);
            ClearCommand = new DelegateCommand<object>(ExecuteProgressResetCommand);

            _appSettings = Plugin.GetService<IPluginSettings>();

            Load = (x) =>
            {
                Thread.Sleep(500);
                UiDispatcher.BeginInvoke(new Action(() =>
                {
                    Bays = LoadBays();
                    //CurrentBay = Bays.FirstOrDefault().ObjectId;
                }));

            };
        }
        #endregion

        #region <Properties>

        private ObjectIdItem _selectedItem;
        public ObjectIdItem SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                Entity ent;
                if (_selectedItem != null && value != null)
                {
                    ent = _selectedItem.ObjectId.XCast<BlockReference>();
                    if (ent != null) ent.Unhighlight();
                }

                _selectedItem = value;

                if (value != null)
                {
                    CommandLine.Zoom(_selectedItem.ObjectId, CurrentZoomDisplayFactor);
                    ent = _selectedItem.ObjectId.XCast<BlockReference>();

                    if (ent != null) ent.Highlight();
                    CommandContext.CurrentEntity = ent;
                }
                Thread.Sleep(200);
            }
        }

        public int CurrentZoomDisplayFactor
        {
            get { return _appSettings.ZoomDisplayFactor; }
            set
            {
                UiDispatcher.BeginInvoke(new Action(() =>
                {
                    _appSettings.ZoomDisplayFactor = value;
                    //Plugin.Container.RegisterInstance<IPluginSettings>(_appSettings);
                }));

                OnPropertyChanged();
            }
        }

        public double ProgressBarMinimum { get; set; }

        /// <summary> Gets or sets the progress bar's maximum value.</summary>
        private double _progressBarMax;

        public double ProgressBarMaximum
        {
            get { return _progressBarMax; }
            set
            {
                _progressBarMax = value;
                OnPropertyChanged();
            }
        }

        /// <summary> Gets or sets the progress bar's current value. </summary>
        private double _progressBarValue;

        public double ProgressBarValue
        {
            get { return _progressBarValue; }
            set
            {
                _progressBarValue = value;
                OnPropertyChanged();
            }
        }

        private double _progressStateValue;

        public double ProgressStateValue
        {
            get { return _progressStateValue; }
            set
            {
                _progressStateValue = value;
                OnPropertyChanged();
            }
        }

        private ObjectId _currentBay = ObjectId.Null;
        public ObjectId CurrentBay
        {
            get => _currentBay;
            set
            {
                if (!_currentBay.IsNull)
                {
                    var oldEntity = _currentBay.XCast<Entity>();
                    oldEntity.Unhighlight();
                }

                _currentBay = value;
                OnPropertyChanged();

                var ent = value.XCast<Entity>();
                if (ent != null)
                {
                    ent.Highlight();
                    CommandLine.ZoomWindow(ent.GeometricExtents.MinPoint, ent.GeometricExtents.MaxPoint);
                }
            }
        }

        private List<Bay> _bays = new List<Bay>();
        public List<Bay> Bays
        {
            get => _bays;
            set
            {
                if (value != null && value.Any())
                {
                    _bays = value;
                    if (_bays.Any())
                        _currentBay = _bays.First().ObjectId;
                    OnPropertyChanged();
                }
            }
        }

        private ObservableRangeCollection<ObjectIdItemAttr> _existListItems = new ObservableRangeCollection<ObjectIdItemAttr>();
        public ObservableRangeCollection<ObjectIdItemAttr> ExistListItems
        {
            get { return _existListItems; }
            set
            {
                _existListItems = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region <Commands>

        public ICommand AddCommand { get; set; }
        public ICommand RunCommand { get; set; }
        public ICommand StopCommand { get; set; }
        public ICommand ResetCommand { get; set; }
        public ICommand CloseCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand SelectSetCommand { get; set; }
        public ICommand ExportCommand { get; set; }
        public ICommand ClearCommand { get; set; }

        private void ExecuteCloseCommand(object commandParameter)
        {
            var commandLine = Plugin.GetService<ICommandLine>();
            commandLine.SendToExecute(CommandNames.SearchPanelRemove);
        }
        public void ExecuteRefreshCommand(object commandParameter)
        {
            UiDispatcher.BeginInvoke(new Action(() =>
            {
                Mouse.OverrideCursor = Cursors.Wait;
                Thread.Sleep(500);

                Bays = LoadBays();

                Mouse.OverrideCursor = null;
            }));
        }
        private void ExecuteStopCommand(object commandParameter)
        {
            IsCanceled = true;
        }
        private void ExecuteResetCommand(object commandParameter)
        {

        }
        private void ExecuteRunCommand(string commandParameter)
        {
            Autodesk.AutoCAD.Internal.Utils.PostCommandPrompt();
            Notifications.DisplayNotifyMessageAsync(NotifyStatus.Working);
            Mouse.OverrideCursor = Cursors.Wait;

            ExistListItems.Clear();
            ProgressMeter pm = new ProgressMeter();

            var doc = acadApp.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;

            using (Doc.LockDocument())
            {
                CommandLine.Cancel();

                ////List<string> brefs = Db.ReadObjects<BlockReference>()
                ////    .Where(x => x.Layer != "Bay side")
                ////    .Select(x => x.XGetTrueName()).ToList();

                List<KeyValuePair<string, BlockReference>> items = new List<KeyValuePair<string, BlockReference>>();
                Bay bay = Bays.FirstOrDefault(x => x.ObjectId == CurrentBay);
                string attrTags = "ENTITY_CODE,CEID,NC/CU";

                using (var scope = db.TransactionManager.TransactionScope())
                {
                    BlockTable bt = (BlockTable)scope.tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                    int blockCount = bt.Cast<ObjectId>().Count();

                    pm.Start($"Scanning for {blockCount} bloks ({string.Join(",", attrTags)})...");
                    pm.SetLimit(blockCount);

                    IEnumerable<KeyValuePair<string, BlockReference>> blockReferenceItems;
                    foreach (ObjectId btrId in bt)
                    {
                        BlockTableRecord btr = (BlockTableRecord)scope.tr.GetObject(btrId, OpenMode.ForRead);
                        if (!btr.IsLayout && !btr.IsErased)
                        {
                            blockReferenceItems = btr.XGetBlockReferences(null, attrTags);
                            if (blockReferenceItems != null)
                                items.AddRange(blockReferenceItems.ToList());
                        }
                        pm.MeterProgress();
                    }

                    if (items.Any())
                    {
                        if (bay != null && bay.Name == "All")
                        {
                            foreach (var b in Bays.Where(x => x.Name != "All"))
                            {
                                ExtractEntities(items, b, attrTags.Split(','));
                            }
                        }
                        else
                        {
                            ExtractEntities(items, bay, attrTags.Split(','));
                        }
                    }
                }

                //objectIds = ids.SelectMany(brBtr => 
                //    (brBtr.IsDynamicBlock ? brBtr.DynamicBlockTableRecord : brBtr.BlockTableRecord).
                //    GetObject<BlockTableRecord>()
                //    .GetBlockReferenceIds(true, true)
                //    .Cast<ObjectId>()
                //.Select(br => (BlockReference)br.XGetEntity<BlockReference>()))
                //.ToList();
            }

            pm.Stop();
            pm.Dispose();

            Notifications.DisplayNotifyMessageAsync(NotifyStatus.Ready);
            Mouse.OverrideCursor = null;
            Autodesk.AutoCAD.Internal.Utils.PostCommandPrompt();
        }
        private void ExecuteXRefBuinAllCommand(string commandParameter)
        {
            Autodesk.AutoCAD.Internal.Utils.PostCommandPrompt();
            Notifications.DisplayNotifyMessageAsync(NotifyStatus.Working);
            Mouse.OverrideCursor = Cursors.Wait;
            ExistListItems.Clear();
            ProgressMeter pm = new ProgressMeter();

            var doc = acadApp.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;

            List<KeyValuePair<BlockTableRecord, XrefGraphNode>> xrefs = db.GetAllXref()
                .Where(xrNode => xrNode.Value.XrefStatus == XrefStatus.Resolved).ToList();

            var xrefCount = xrefs.Count();

            List<BlockReference> objectIds;
            if (xrefCount < 0)
            {
                pm.Start($"Loading {xrefCount} xref's ...");

                pm.SetLimit((xrefs.Count() + 1) * 100000);
                Thread.Sleep(500);

                foreach (var xrNode in xrefs)
                {
                    var status = xrNode.Value.XrefStatus == XrefStatus.Resolved
                        ? "Xref" : Enum.GetName(typeof(XrefStatus), xrNode.Value.XrefStatus);
                    ed.WriteMessage($"\n{CommandNames.UserGroup} add {status}: {xrNode.Key.Name}");
                    for (int i = 0; i < 100000; i++)
                        pm.MeterProgress();
                    pm.Start($"Loading {xrefCount} xref {xrNode.Key.Name}");
                    System.Windows.Forms.Application.DoEvents();
                    Thread.Sleep(2000);
                }

                pm.Start($"Loading {xrefCount} xref's ...");
                System.Windows.Forms.Application.DoEvents();
                Thread.Sleep(3000);

                Db.BindAllXref(new ObjectIdCollection(xrefs.Select(x => x.Key.ObjectId).ToArray()));

                for (int i = 0; i < 100000; i++)
                    pm.MeterProgress();

                System.Windows.Forms.Application.DoEvents();
                Thread.Sleep(20);

                Mouse.OverrideCursor = null;
                Notifications.DisplayNotifyMessageAsync(NotifyStatus.Ready);

                pm.Stop();

                objectIds = xrefs.SelectMany(x =>
                        x.Key.GetBlockReferenceIds(true, true)
                            .Cast<ObjectId>()
                            .Select(br => (BlockReference)br.XGetEntity<BlockReference>()))
                    .ToList();
            }

            //pm.SetLimit(objectIds.Count);
            //pm.Start($"Define nested blocks from {objectIds.Count} ...");

            //var objectIdItemResults = new List<ObjectIdItem>();
            ////var explodeObjectList = new List<ObjectId>();
            //using (Doc.LockDocument())
            //{
            //    using (var scope = Db.TransactionManager.TransactionScope())
            //    {
            //        foreach (var bref in objectIds)
            //        {
            //            //ObjectEventHandler handler =
            //            //        (s, e) =>
            //            //        {
            //            //            explodeObjectList.Add(e.DBObject.ObjectId);
            //            //        };
            //            //Db.ObjectAppended += handler;
            //            //bref.ExplodeToOwnerSpace();
            //            //foreach (Point3d point in points)
            //            //    brefItem.Extends.Add(point);
            //            //Db.ObjectAppended -= handler;
            //            var position = bref.Position.XIsPointInside(CurrentBay);
            //        }
            //    }
            //}

            //pm.Stop();
            //pm.SetLimit(objectIdItemResults.Count);
            //pm.Start($"Sending {objectIdItemResults.Count} blocks ...");

            pm.Stop();
            pm.Dispose();

            Notifications.DisplayNotifyMessageAsync(NotifyStatus.Ready);
            Mouse.OverrideCursor = null;
            Autodesk.AutoCAD.Internal.Utils.PostCommandPrompt();
        }
        private void ExecuteSelectSetCommand(object commandParameter)
        {
            Ed.SetImpliedSelection(ExistListItems.Select(x => x.ObjectId).ToArray());
            //CommandMain.SetFocus(Application.DocumentManager.MdiActiveDocument.Window.Handle);
            Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView();
        }
        private void ExecuteExportCommand(object commandParameter)
        {
            Notifications.DisplayNotifyMessageAsync(NotifyStatus.Working);
            Mouse.OverrideCursor = Cursors.Wait;
            ProgressMeter pm = new ProgressMeter();

            pm.Start("Export to desktop excel file (export_data.csv) ...");
            pm.SetLimit(ExistListItems.Count);
            Thread.Sleep(500);

            List<string> listItems = ExistListItems.Select(x => x.DisplayName).ToList();
            //Aggregator.GetEvent<ObjectIdMessageEvent>().Publish(ids);
            //GeoDataManager.ExecuteSaveAsCommand(ids);

            for (int i = 0; i <= ExistListItems.Count; i++)
                pm.MeterProgress();

            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            listItems.XWriteCsv(path + @"\export_data.Csv");
            Thread.Sleep(2000);

            for (int i = 0; i <= 100000; i++)
                pm.MeterProgress();

            pm.Stop();
            pm.Dispose();

            //acadApp.DocumentManager.ExecuteInCommandContextAsync(async data =>
            //{
            //    acadApp.DocumentManager.CurrentDocument.UserData.XAdd(CommandNames.ExportToExcelFile, (List<string>)data);
            //    Editor.CommandResult cmdResult = Ed.CommandAsync("." + CommandNames.ExportToExcelFile);
            //    cmdResult.OnCompleted(() => { });
            //}, ExistListItems.Select(x => x.DisplayName).ToList());

            Mouse.OverrideCursor = null;
            Notifications.DisplayNotifyMessageAsync(NotifyStatus.Ready);
        }


        public bool CanExecuteRunCommand(object parameter)
        {
            return true; // !string.IsNullOrEmpty(SelectedText);
        }

        #endregion "Commands"

        #region <Methods>

        private void ExtractEntities(IEnumerable<KeyValuePair<string, BlockReference>> ids, Bay bay,
            string[] withAttrTags)
        {
            var relevantIds = ids.Where(br =>
                br.Value.Position.X >= bay.Extends.MinPoint.X &&
                br.Value.Position.Y >= bay.Extends.MinPoint.Y &&
                br.Value.Position.X <= bay.Extends.MaxPoint.X &&
                br.Value.Position.Y <= bay.Extends.MaxPoint.Y &&
                br.Value.Layer != "Bay side").ToList();

            if (!relevantIds.Any()) return;

            var invokeMethod = new Action<CancellationToken>((token) =>
            {
                relevantIds.ForEach(async ent =>
                {
                    if (!IsCanceled)
                    {
                        ObjectIdItemAttr objectIdItem =
                            await ent.Value.XGetDisplayItem<BlockReference, ObjectIdItemAttr>(token, withAttrTags);

                        if (objectIdItem != null)
                        {
                            objectIdItem.DisplayName = bay.Name + "," + objectIdItem.DisplayName;
                            ProgressBarValue = ProgressStateValue += 1;
                            ExistListItems.Add(objectIdItem);
                        }
                    }
                }
                );
            });

            UiDispatcher.Invoke(invokeMethod, CancelToken);

            UiDispatcher.Invoke(() =>
            {
                if (ExistListItems.Any())
                {
                    SelectedItem = ExistListItems.FirstOrDefault();
                    if (SelectedItem != null)
                    {
                        CommandLine.Zoom(SelectedItem.ObjectId, CurrentZoomDisplayFactor);
                        var ent = SelectedItem.ObjectId.XCast<BlockReference>();
                        if (ent != null) ent.Highlight();
                    }
                }
                ProgressBarValue = 1;
                ProgressBarValue = 0;
            });

        }

        public void ExecuteProgressResetCommand(object commandParameter)
        {
            UiDispatcher.Invoke(() =>
            {
                ProgressStateValue = 0;
                ProgressBarValue = 0;
                ProgressBarMaximum = commandParameter != null ? Convert.ToInt32(commandParameter) : 0;
                ExistListItems.Clear();
                CommandLine.Cancel();
            });
        }

        private int RampToInt(string v)
        {
            int destValue;
            bool success = int.TryParse(v, out destValue);
            return success ? destValue : 0;
        }

        private List<Bay> LoadBays()
        {
            Notifications.DisplayNotifyMessageAsync(NotifyStatus.Working);
            Mouse.OverrideCursor = Cursors.Wait;
            ProgressMeter pm = new ProgressMeter();

            List<BlockReference> brefs = Db.XReadObjects<BlockReference>("Bay side", false).ToList();

            pm.Start($"Loading {brefs.Count} bays ...");
            pm.SetLimit(brefs.Count);
            Thread.Sleep(500);

            List<Bay> brefItems = new List<Bay> { new Bay("All", ObjectId.Null, null, null) };

            using (Doc.LockDocument())
            {
                using (var scope = Db.TransactionManager.TransactionScope())
                {
                    brefs.ForEach(bref =>
                    {
                        if (bref.AttributeCollection.Count > 0)
                        {
                            var attrs = bref.XGetAttributes().ToDictionary(attr => attr.Tag, attr => attr.TextString);

                            Point3dCollection points = new Point3dCollection();
                            Extents3d extents = bref.GeometricExtents;
                            points.Add(extents.MinPoint);
                            points.Add(extents.MaxPoint);

                            brefItems.Add(new Bay(
                                $"name: {attrs.FirstOrDefault(x => x.Key == "BAY_NAME").Value}; " +
                                $"side: {attrs.FirstOrDefault(x => x.Key == "BAY_SIDE").Value}; " +
                                $"part: {attrs.FirstOrDefault(x => x.Key == "BAY_PART").Value}",
                                bref.ObjectId,
                                bref.XGetAttributes().ToDictionary(attr => attr.Tag, attr => attr.TextString),
                                points));
                        }
                        pm.MeterProgress();
                    });
                    scope.tr.Commit();
                }

                Ed.WriteMessage(brefItems.Any()
                    ? $"{CommandNames.UserGroup} : {brefItems.Count} blocks have been found!"
                    : $"{CommandNames.UserGroup} : blocks not found!");

                pm.Stop();
                pm.Dispose();

                Mouse.OverrideCursor = null;
                Notifications.DisplayNotifyMessageAsync(NotifyStatus.Ready);
            }
            return brefItems;
        }

        public bool OnFindAction(ITaskArgs args)
        {
            return true;
        }

        public override void OnDocumentActivated(object sender, DocumentCollectionEventArgs e)
        {
            UiDispatcher.InvokeAsync(() =>
            {
                this.ExecuteResetCommand((object)0);
            });
        }

        public override void OnDocumentToBeDestroyed(object sender, DocumentCollectionEventArgs e)
        {
        }

        public Task<ICommandArgs> ExecuteCommand(ICommandArgs command)
        {
            return null;
        }

        public override event PropertyChangedEventHandler PropertyChanged;
        public event PaletteExecuteStartEventhandler PaletteExecuteStarted;
        public event PaletteExecuteCompleteEventhandler PaletteExecuteCompleted;

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        internal virtual bool IsFileLockedOrReadOnly(FileInfo fi)
        {
            FileStream fs = null;
            try
            {
                fs = fi.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (System.Exception ex)
            {
                if (ex is IOException || ex is UnauthorizedAccessException)
                {
                    return true;
                }
            }
            finally
            {
                if (fs != null)
                    fs.Close();
            }

            // File is accessible
            return false;
        }
        #endregion
    }
}

//_machineNames = machineMatchesLayers.Select(x => x.Substring(0, x.IndexOf(x.Split('-')[3])-1)).Distinct().ToList();
//_machineScopes = machineMatchesLayers.Select(x => x.Split('-')[3]).Distinct().ToList();
//_machineRamps = machineMatchesLayers.Select(x => x.Split('-')[5]).Distinct()
//    .ToDictionary(k => k, v =>
//    {
//        int destValue;
//        bool success = int.TryParse(v, out destValue);
//        return success ? destValue : 0;
//    })
//    .OrderBy(item => item.Value)
//    .Select(k => k.Key).ToList();