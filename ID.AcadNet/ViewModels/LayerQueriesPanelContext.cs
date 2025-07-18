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
using Intellidesk.AcadNet.Common.Core;
using Intellidesk.AcadNet.Common.Extensions;
using Intellidesk.AcadNet.Common.Interfaces;
using Intellidesk.AcadNet.Common.Models;
using Intellidesk.AcadNet.Interfaces;
using Intellidesk.AcadNet.Services;
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

    public class MachineTypedLayer
    {
        public string SourceLayer { get; set; }
        public string MachineLayer { get; set; }
        public string MachineName { get; set; }
        public string ProcessName { get; set; }
        public string MachineScope { get; set; }
        public string MachineRamp { get; set; }
        public int MachineRampNum { get; set; }
        public bool IsFreeze { get; set; }
    }

    public class LayerHistoryItem
    {
        public string ProcessName { get; set; }
        public string MachineLayer { get; set; }
        public int RampFrom { get; set; }
        public int RampTo { get; set; }

        public int[] Ramps { get; set; }
        public int[] RampsFrom { get; set; }
        public int[] RampsTo { get; set; }
    }

    public class LayerQueriesPanelContext : BaseViewModel, ILayerQueriesPanelContext, IExecutablePanelViewModel
    {
        public override event PropertyChangedEventHandler PropertyChanged;
        public event PaletteExecuteStartEventhandler PaletteExecuteStarted;
        public event PaletteExecuteCompleteEventhandler PaletteExecuteCompleted;

        private IPluginSettings _appSettings;
        public Action<object> Load { get; set; }
        private List<LayerHistoryItem> layerHistories = new List<LayerHistoryItem>();

        #region <ctor>
        public LayerQueriesPanelContext()
        {
            ProgressBarMinimum = 0;
            ProgressBarMaximum = 100;

            MachineTypedLayers = LoadMachineLayers();

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

                }));

            };
        }
        #endregion

        #region <Properties>

        private string _currentSourceLayer = "Selected layers";
        public string CurrentSourceLayer
        {
            get { return _currentSourceLayer; }
            set
            {
                _currentSourceLayer = value;
                OnPropertyChanged();
            }
        }

        private string _currentMachineLayer = "None";
        public string CurrentMachineLayer
        {
            get { return _currentMachineLayer; }
            set
            {
                _currentMachineLayer = value;
                OnPropertyChanged();
            }
        }

        private string _currentMachineName = "None";
        public string CurrentMachineName
        {
            get { return _currentMachineName; }
            set
            {
                _currentMachineName = value;
                OnPropertyChanged();
            }
        }

        private string _currentProcessName = "None";
        public string CurrentProcessName
        {
            get { return _currentProcessName; }
            set
            {
                _currentProcessName = value;
                OnPropertyChanged();
            }
        }

        private string _currentMachineScope = "None";
        public string CurrentMachineScope
        {
            get { return _currentMachineScope; }
            set
            {
                _currentMachineScope = value;
                OnPropertyChanged();
            }
        }

        private int _currentMachineRampFrom;
        public int CurrentMachineRampFrom
        {
            get { return _currentMachineRampFrom; }
            set
            {
                _currentMachineRampFrom = value;
                OnPropertyChanged();
            }
        }

        private int _currentMachineRampTo;
        public int CurrentMachineRampTo
        {
            get { return _currentMachineRampTo; }
            set
            {
                _currentMachineRampTo = value;
                OnPropertyChanged();
            }
        }

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

        private List<List<MachineTypedLayer>> _machineTypedLayersSource = new List<List<MachineTypedLayer>>();
        private List<List<MachineTypedLayer>> MachineTypedLayersSource
        {
            get { return _machineTypedLayersSource; }
            set
            {
                _machineTypedLayersSource = value;
            }
        }

        private List<MachineTypedLayer> _machineTypedLayers = new List<MachineTypedLayer>();
        private List<MachineTypedLayer> MachineTypedLayers
        {
            get => _machineTypedLayers;
            set
            {
                if (value != null && value.Any())
                {
                    _machineTypedLayers = value;
                    SourceLayers = _machineTypedLayers.Select(x => x.SourceLayer).Distinct().ToList();
                    SourceLayers.Insert(0, "Selected layers");
                    MachineLayers = _machineTypedLayers.Select(x => x.MachineLayer).Distinct().ToList();
                    MachineNames = _machineTypedLayers.Select(x => x.MachineName).Distinct().ToList();
                    ProcessNames = _machineTypedLayers.Select(x => x.ProcessName).Distinct().ToList();
                    MachineScopes = _machineTypedLayers.Select(x => x.MachineScope).Distinct().ToList();
                    MachineRamps = _machineTypedLayers.GroupBy(x => x.MachineRampNum).Select(grp => grp.First())
                        .OrderBy(x => x.MachineRampNum).ToDictionary(k => k.MachineRamp, v => v.MachineRampNum);

                    CurrentMachineLayer = MachineLayers.FirstOrDefault();
                    CurrentMachineScope = MachineScopes.FirstOrDefault();
                    CurrentProcessName = ProcessNames.FirstOrDefault();
                    CurrentMachineRampFrom = MachineRamps.FirstOrDefault().Value;
                    CurrentMachineRampTo = MachineRamps.LastOrDefault().Value;
                    CurrentMachineName = MachineNames.FirstOrDefault();
                }
                else
                {
                    MachineNames = MachineScopes = new List<string>();
                    MachineRamps = new Dictionary<string, int>();
                    CurrentMachineName = CurrentMachineScope = CurrentMachineLayer = "None";
                    CurrentMachineRampFrom = CurrentMachineRampTo = 0;
                }

                OnPropertyChanged("SourceLayers");
                OnPropertyChanged("MachineLayers");
                OnPropertyChanged("MachineNames");
                OnPropertyChanged("MachineScopes");
                OnPropertyChanged("ProcessNames");
                OnPropertyChanged("MachineRamps");
                OnPropertyChanged("CurrentMachineRampFrom");
                OnPropertyChanged("CurrentMachineRampTo");

            }
        }

        public List<string> SourceLayers { get; private set; }
        public List<string> MachineLayers { get; private set; }
        public List<string> MachineNames { get; private set; } = new List<string>();
        public List<string> ProcessNames { get; private set; } = new List<string>();
        public List<string> MachineScopes { get; private set; } = new List<string>();
        public Dictionary<string, int> MachineRamps { get; private set; } = new Dictionary<string, int>();

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
                if (_currentBay != ObjectId.Null)
                {
                    var oldEntity = value.XCast<Entity>();
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

        private ObservableRangeCollection<ObjectIdItemArgs> _existListItems = new ObservableRangeCollection<ObjectIdItemArgs>();
        public ObservableRangeCollection<ObjectIdItemArgs> ExistListItems
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
        public ICommand SendCommand { get; set; }
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

                MachineTypedLayers = LoadMachineLayers();

                Mouse.OverrideCursor = null;
            }));
        }
        private void ExecuteStopCommand(object commandParameter)
        {
            IsCanceled = true;
        }
        private void ExecuteResetCommand(object commandParameter)
        {
            if (MachineTypedLayersSource.Any())
            {
                List<MachineTypedLayer> typedLayers = MachineTypedLayersSource.LastOrDefault();
                if (typedLayers != null)
                {
                    using (Doc.LockDocument())
                    {
                        Mouse.OverrideCursor = Cursors.Wait;
                        Notifications.SendNotifyMessageAsync(NotifyStatus.Working);
                        CommandLine.Cancel();
                        ProgressMeter pm = new ProgressMeter();

                        pm.Start($"Reset for {typedLayers.Count} layers ...");
                        pm.SetLimit(typedLayers.Count);
                        Thread.Sleep(500);

                        LayerService layerService = Plugin.GetService<LayerService>();

                        foreach (var typedLayer in typedLayers)
                        {
                            layerService.Freeze(typedLayer.SourceLayer, typedLayer.IsFreeze);
                            pm.MeterProgress();
                        }

                        CommandLine.Regen();

                        if (MachineTypedLayersSource.Any())
                            MachineTypedLayersSource.Clear();

                        pm.Stop();
                        pm.Dispose();
                        Mouse.OverrideCursor = null;
                        Notifications.SendNotifyMessageAsync(NotifyStatus.Ready);
                    }
                }
            }
        }
        private void ExecuteRunCommand(string commandParameter)
        {
            IsCanceled = false;
            CommandLine.Cancel();
            Mouse.OverrideCursor = Cursors.Wait;
            Notifications.SendNotifyMessageAsync(NotifyStatus.Working);
            ExecuteProgressResetCommand(null);

            var pm = new ProgressMeter();
            pm.Start($"process: { CurrentProcessName} working...");
            pm.SetLimit(300000);

            using (Doc.LockDocument())
            {
                if (!MachineTypedLayersSource.Any())
                    MachineTypedLayersSource.Add(MachineTypedLayers);

                List<MachineTypedLayer> typedLayersFrom = MachineTypedLayers.
                    Where(x => x.MachineLayer == CurrentMachineLayer &&
                               x.ProcessName == CurrentProcessName &&
                               x.MachineRampNum < CurrentMachineRampFrom).ToList();

                List<MachineTypedLayer> typedLayersTo = MachineTypedLayers.
                    Where(x => x.MachineLayer == CurrentMachineLayer &&
                               x.ProcessName == CurrentProcessName &&
                               x.MachineRampNum > CurrentMachineRampTo).ToList();

                List<MachineTypedLayer> typedLayers = MachineTypedLayers.
                    Where(x => x.MachineLayer == CurrentMachineLayer &&
                               x.ProcessName == CurrentProcessName &&
                               x.MachineRampNum >= CurrentMachineRampFrom &&
                               x.MachineRampNum <= CurrentMachineRampTo).ToList();

                var rampsFrom = typedLayersFrom.GroupBy(x => x.MachineRampNum).Select(grp => grp.First())
                                .OrderBy(x => x.MachineRampNum).Select(x => x.MachineRampNum).ToArray();

                var rampsTo = typedLayersTo.GroupBy(x => x.MachineRampNum).Select(grp => grp.First())
                                .OrderBy(x => x.MachineRampNum).Select(x => x.MachineRampNum).ToArray();

                var ramps = typedLayers.GroupBy(x => x.MachineRampNum).Select(grp => grp.First())
                                .OrderBy(x => x.MachineRampNum).Select(x => x.MachineRampNum).ToArray();

                if (layerHistories.Any(x => x.ProcessName == CurrentProcessName))
                {
                    var layerHistory = layerHistories.First(x => x.ProcessName == CurrentProcessName);
                    int[] exceptRampNumsFrom = layerHistory.RampsFrom.Except(rampsFrom).Union(rampsFrom.Except(layerHistory.RampsFrom)).ToArray();
                    int[] exceptRampNumsTo = layerHistory.RampsTo.Except(rampsTo).Union(rampsTo.Except(layerHistory.RampsTo)).ToArray();
                    int[] exceptRampNums = layerHistory.RampsTo.Except(ramps).Union(ramps.Except(layerHistory.Ramps)).ToArray();

                    if (exceptRampNumsFrom.Length > 0 || exceptRampNumsTo.Length > 0)
                    {
                        if (exceptRampNumsFrom.Length > 0)
                        {
                            //pm.Start($"process: { layerHistory.ProcessName} working...");
                            //pm.SetLimit(200000);

                            var exceptLayersFrom = MachineTypedLayers
                                .Where(x => x.MachineLayer == layerHistory.MachineLayer &&
                                            x.ProcessName == layerHistory.ProcessName &&
                                            exceptRampNumsFrom.Contains(x.MachineRampNum)).ToList();

                            //Thread.Sleep(200);
                            //for (int i = 0; i <= 100000; i++)
                            //{
                            //    pm.MeterProgress();
                            //} // Active to RampFrom

                            SrtartQuery(exceptLayersFrom, false);
                        }
                        if (exceptRampNumsTo.Length > 0)
                        {
                            var exceptLayersTo = MachineTypedLayers
                                .Where(x => x.MachineLayer == layerHistory.MachineLayer &&
                                            x.ProcessName == layerHistory.ProcessName &&
                                            exceptRampNumsTo.Contains(x.MachineRampNum)).ToList();

                            //Thread.Sleep(200);
                            //for (int i = 0; i <= 100000; i++)
                            //{
                            //    pm.MeterProgress();
                            //} // Active to RampTo
                            SrtartQuery(exceptLayersTo, false);
                        }

                        //pm.Stop();
                        //pm.Dispose();
                    }

                    layerHistories.RemoveAll(x => x.ProcessName == layerHistory.ProcessName);
                }

                layerHistories.Add(new LayerHistoryItem()
                {
                    MachineLayer = CurrentMachineLayer,
                    ProcessName = CurrentProcessName,
                    RampFrom = CurrentMachineRampFrom,
                    RampTo = CurrentMachineRampTo,
                    RampsFrom = rampsFrom,
                    RampsTo = rampsTo,
                    Ramps = ramps
                });

                for (int i = 0; i <= 100000; i++)
                {
                    pm.MeterProgress();
                } //"Freeze to RampsFrom"
                Thread.Sleep(200);
                SrtartQuery(typedLayersFrom, true);

                for (int i = 0; i <= 100000; i++)
                {
                    pm.MeterProgress();
                } //"Freeze to RampsTo"
                Thread.Sleep(200);
                SrtartQuery(typedLayersTo, true);

                for (int i = 0; i <= 100000; i++)
                {
                    pm.MeterProgress();
                } //"Active MainRamp"
                Thread.Sleep(200);
                SrtartQuery(typedLayers, false);

                pm.Stop();
                pm.Dispose();

                ICommandLine commandLine = Plugin.GetService<ICommandLine>();
                commandLine.SendToExecute("_REGEN ");

                Mouse.OverrideCursor = null;
                Notifications.SendNotifyMessageAsync(NotifyStatus.Ready);

                //CommandLine.Regen();
            };
        }

        private void SrtartQuery(List<MachineTypedLayer> typedLayers, bool isFreeze)
        {
            LayerService layerService = Plugin.GetService<LayerService>();
            foreach (var typedLayer in typedLayers)
            {
                layerService.Freeze(typedLayer.SourceLayer, isFreeze);
            }
        }

        private void ExecuteXRefBuinAllCommand(string commandParameter)
        {
            Autodesk.AutoCAD.Internal.Utils.PostCommandPrompt();
            Notifications.SendNotifyMessageAsync(NotifyStatus.Working);
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
                Notifications.SendNotifyMessageAsync(NotifyStatus.Ready);

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

            Notifications.SendNotifyMessageAsync(NotifyStatus.Ready);
            Mouse.OverrideCursor = null;
            Autodesk.AutoCAD.Internal.Utils.PostCommandPrompt();
        }

        public void ExecuteSelectSetCommand(object commandParameter)
        {
            Ed.SetImpliedSelection(ExistListItems.Select(x => x.ObjectId).ToArray());
            //CommandMain.SetFocus(Application.DocumentManager.MdiActiveDocument.Window.Handle);
            Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView();
        }
        private void ExecuteExportCommand(object commandParameter)
        {
            Notifications.SendNotifyMessageAsync(NotifyStatus.Working);
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
            Notifications.SendNotifyMessageAsync(NotifyStatus.Ready);
        }
        public bool CanExecuteRunCommand(object parameter)
        {
            return true; // !string.IsNullOrEmpty(SelectedText);
        }

        #endregion "Commands"

        #region <Methods>

        public void ExecuteProgressResetCommand(object commandParameter)
        {
            UiDispatcher.Invoke(() =>
            {
                ProgressStateValue = 0;
                ProgressBarValue = 0;
                ProgressBarMaximum = commandParameter != null ? Convert.ToInt32(commandParameter) : 0;
                //ExistListItems.Clear();
                CommandLine.Cancel();
            });
        }

        private int RampToInt(string v)
        {
            int destValue;
            bool success = int.TryParse(v, out destValue);
            return success ? destValue : 0;
        }

        private List<MachineTypedLayer> LoadMachineLayers()
        {
            LayerService layerService = Plugin.GetService<LayerService>();
            List<string> layers = layerService.GetAll().Select(x => x.LayerName).ToList();
            layers.Remove("0");

            List<MachineTypedLayer> machineMatchesLayers =
                layers.Where(x => x.Contains("|") && x.Split('|')[1].Split('-').Length == 7)
                .Select(x => new { xrefName = x.Split('|')[0], blockName = x.Split('|')[1] })
                .Select(x =>
                    {
                        var blockNames = x.blockName.Split('-');
                        return new MachineTypedLayer()
                        {
                            SourceLayer = x.xrefName + "|" + x.blockName,
                            MachineLayer = x.blockName.Substring(0, x.blockName.IndexOf(blockNames[3]) - 1),
                            ProcessName = blockNames[3],
                            MachineName = blockNames[4],
                            MachineScope = blockNames[5],
                            MachineRamp = blockNames[6],
                            MachineRampNum = new Func<string, int>(v =>
                            {
                                if (v == "ETS") v = "200";
                                else if (v == "RTS") v = "300";

                                int destValue;
                                bool success = int.TryParse(v, out destValue);
                                return success ? destValue : 0;
                            })(blockNames[6])
                        };
                    })

                .Where(x => x.MachineRampNum != 0).ToList();

            return machineMatchesLayers;
        }

        private List<Bay> LoadBays()
        {
            Notifications.SendNotifyMessageAsync(NotifyStatus.Working);
            Mouse.OverrideCursor = Cursors.Wait;
            ProgressMeter pm = new ProgressMeter();

            List<BlockReference> brefs = Db.XReadObjects<BlockReference>("Bay side", false).ToList();

            pm.Start($"Loading {brefs.Count} bays ...");
            pm.SetLimit(brefs.Count);
            Thread.Sleep(500);

            List<Bay> brefItems = new List<Bay> { new Bay("All", ObjectId.Null, null, null) };

            using (Doc.LockDocument())
            {
                using (var tr = Db.TransactionManager.StartTransaction())
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
                    tr.Commit();
                }

                Ed.WriteMessage(brefItems.Any()
                    ? $"{CommandNames.UserGroup} : {brefItems.Count} blocks have been found!"
                    : $"{CommandNames.UserGroup} : blocks not found!");

                pm.Stop();
                pm.Dispose();

                Mouse.OverrideCursor = null;
                Notifications.SendNotifyMessageAsync(NotifyStatus.Ready);
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
                UnregisterEvents();
                this.ExecuteResetCommand((object)0);
                RegisterEvents();
            });
        }

        public override void OnDocumentToBeDestroyed(object sender, DocumentCollectionEventArgs e)
        {
        }

        public Task<ICommandArgs> ExecuteCommand(ICommandArgs command)
        {
            return null;
        }

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