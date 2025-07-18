#define INTEL

//extern alias Newtonsoft10;
//extern alias Newtonsoft6;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using ID.Infrastructure;
using ID.Infrastructure.Commands;
using ID.Infrastructure.Enums;
using ID.Infrastructure.Extensions;
using ID.Infrastructure.Helpers;
using ID.Infrastructure.Interfaces;
using ID.Infrastructure.Models;
using Intellidesk.AcadNet.Common.Core;
using Intellidesk.AcadNet.Common.Extensions;
using Intellidesk.AcadNet.Common.Interfaces;
using Intellidesk.AcadNet.Common.Internal;
using Intellidesk.AcadNet.Common.Models;
using Intellidesk.AcadNet.Interfaces;
using Intellidesk.AcadNet.Services;
using Intellidesk.AcadNet.Services.Extentions;
using Intellidesk.AcadNet.ViewModels;
using Intellidesk.Data.Models.Entities;
using Prism.Commands;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using Unity;
using Unity.Lifetime;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Cursors = System.Windows.Input.Cursors;
using ObjectIdItem = Intellidesk.AcadNet.Common.Models.ObjectIdItem;

[assembly: CommandClass(typeof(SearchTextPanelContext))]
namespace Intellidesk.AcadNet.ViewModels
{
    public class SearchTextPanelContext : BaseViewModel, ISearchTextPanelContext
    {
        private IPluginSettings _appSettings;

        public Action<object> Load { get; set; }

        private int _totalCount = 0;
        public int TotalCount
        {
            get { return _totalCount; }
            set
            {
                _totalCount = value;
                OnPropertyChanged();
            }
        }
        public int FromCount { get => ExistListItems.Count; }

        #region <ctor>
        public SearchTextPanelContext()
        {
            ProgressBarMinimum = 0;
            //ProgressBarMaximum = 100;

            var layerService = Plugin.GetService<LayerService>();
            var list = layerService.GetAll().Select(x => x.LayerName).ToList();
            list.Insert(0, "All");
            Layers = list;

            RunCommand = new DelegateCommand<string>(ExecuteRunCommand, CanExecuteRunCommand);
            StopCommand = new DelegateCommand<object>(ExecuteStopCommand);
            SelectSetCommand = new DelegateCommand<object>(ExecuteSelectSetCommand);
            ExportCommand = new DelegateCommand<Task>(ExecuteExportCommand);
            UndoCommand = new DelegateCommand<object>(ExecuteProgressResetCommand);
            CloseCommand = new DelegateCommand<object>(ExecuteCloseCommand);
            ClearCommand = new DelegateCommand<object>(ExecuteProgressResetCommand);
            RefreshCommand = new DelegateCommand<object>(ExecuteRefreshCommand);

            _appSettings = Plugin.GetService<IPluginSettings>();

            _existListItems = new ObservableRangeCollection<ObjectIdItem>();
            //_existListItems.CollectionChanged += ExistListItemsOnCollectionChanged;
            BindingOperations.EnableCollectionSynchronization(_existListItems, _lock);

        }

        private void ExistListItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(FromCount));
        }
        #endregion

        #region <Properties>
        //public IEventAggregator Aggregator => _eventAggregator ?? (_eventAggregator = Plugin.GetService<IEventAggregator>());

        private string _selectedText = "*";
        public string SelectedText
        {
            get => _selectedText;
            set
            {
                _selectedText = value;
                OnPropertyChanged();
            }
        }

        private string _runContentText = "Search";
        public string RunContentText
        {
            get => _runContentText;
            set
            {
                acadApp.DocumentManager.ExecuteInApplicationContext((v) =>
                {
                    var content = v.ToString();
                    StopButtonEnabled = content == "Working";
                    _runContentText = StopButtonEnabled ? (char)160 + content + ".." : content;

                }, value);
                OnPropertyChanged();
            }
        }

        private bool _stopButtonEnabled = false;
        public bool StopButtonEnabled
        {
            get => _stopButtonEnabled;
            set
            {
                _stopButtonEnabled = value;
                OnPropertyChanged();
            }
        }

        private string _selectedAttrName = "*";
        public string SelectedAttrName
        {
            get => _selectedAttrName;
            set
            {
                _selectedAttrName = value;
                OnPropertyChanged();
            }
        }

        private string _selectedAttrValue = "*";
        public string SelectedAttrValue
        {
            get => _selectedAttrValue;
            set
            {
                _selectedAttrValue = value;
                OnPropertyChanged();
            }
        }

        private string _currentLayer = "All";
        public string CurrentLayer
        {
            get { return _currentLayer; }
            set
            {
                _currentLayer = value;
                OnPropertyChanged();
            }
        }

        private List<string> _layers;
        public List<string> Layers
        {
            get { return _layers; }
            set
            {
                _layers = value;
                OnPropertyChanged();
            }
        }

        private string _currentEntityType = "Text";
        public string CurrentEntityType
        {
            get => _currentEntityType;
            set
            {
                _currentEntityType = value;
                OnPropertyChanged();
                OnPropertyChanged("IsBlockAttrRowVisible");
            }
        }

        private List<string> _entityTypes = new List<string> { "Text", "Polyline", "Line", "Curve", "Block", "ID", "Cable", "Closure", "Cabinet" };
        public List<string> EntityTypes
        {
            get => _entityTypes;
            set
            {
                _entityTypes = value;
                OnPropertyChanged();
            }
        }

        #region <overrides>
        private List<string> _scaleFactors = new List<string>() { "1", "2", "3", "5", "7", "10" };
        public override List<string> ScaleFactors
        {
            get => _scaleFactors;
            set
            {
                _scaleFactors = value;
                OnPropertyChanged();
            }
        }

        #endregion

#if (PARTNER)
        private List<string> _blockAttrsNames = new List<string> { "*", "ENTITY_CODE", "CEID", "NC/CU" };
#else
        private List<string> _blockAttrsNames = new List<string> { "*" };
#endif

        public List<string> BlockAttrNames
        {
            get => _blockAttrsNames;
            set
            {
                _blockAttrsNames = value;
                OnPropertyChanged();
            }
        }

        private ObservableRangeCollection<ObjectIdItem> _existListItems;
        public ObservableRangeCollection<ObjectIdItem> ExistListItems
        {
            get => _existListItems;
            set
            {
                _existListItems = value;
                OnPropertyChanged();
            }
        }

        private ObjectIdItem _selectedKey;

        public ObjectIdItem SelectedKey
        {
            get => _selectedKey;
            set
            {
                _selectedKey = value;
                OnPropertyChanged();
                OnPropertyChanged("SelectedValue");
            }
        }

        private ObjectIdItem _selectedItem;

        public ObjectIdItem SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (value != null && !value.Equals(_selectedItem) && Mouse.OverrideCursor == null)
                    OnSelectedItem(value);
            }
        }

        public void OnSelectedItem(ObjectIdItem value)
        {
            if (!string.IsNullOrEmpty(CommandArgs.CurrentCommandName))
                return;

            Entity ent;
            if (_selectedItem != null)
            {
                ent = _selectedItem.ObjectId.XCast<Entity>();
                if (ent != null) ent.Unhighlight();
            }

            _selectedItem = value;

            if (value != null)
            {
                CommandLine.Zoom(value.ObjectId, CurrentZoomDisplayFactor);
                ent = value.ObjectId.XCast<Entity>();
                if (ent != null) ent.Highlight();
                CommandContext.CurrentEntity = ent;
            }

            Thread.Sleep(200);
        }

        public int CurrentZoomDisplayFactor
        {
            get => _appSettings.ZoomDisplayFactor;
            set
            {
                UiDispatcher.BeginInvoke(new Action(() =>
                {
                    _appSettings.ZoomDisplayFactor = value;
                    Plugin.Container.RegisterInstance(_appSettings, new ContainerControlledLifetimeManager());
                }));

                OnPropertyChanged();
            }
        }

        private bool _canPopulated = true;
        public bool CanPopulated
        {
            get { return _canPopulated; }
            set
            {
                _canPopulated = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region <Commands>

        public ICommand RunCommand { get; set; }
        public ICommand StopCommand { get; set; }
        public ICommand SelectSetCommand { get; set; }
        public ICommand ExportCommand { get; set; }
        public ICommand UndoCommand { get; set; }
        public ICommand ClearCommand { get; set; }
        public ICommand CloseCommand { get; set; }
        public ICommand RefreshCommand { get; set; }

        public bool IsBlockAttrRowVisible => CurrentEntityType == "Block";

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
                Thread.Sleep(200);
                var layerService = Plugin.GetService<ILayerService>();
                var list = layerService.GetAll().Select(x => x.LayerName).ToList();
                list.Insert(0, "All");
                Layers = list;
                CurrentLayer = "All";
                RunContentText = "Search";
                Mouse.OverrideCursor = null;
            }));
        }

        public void ExecuteSelectSetCommand(object commandParameter)
        {
            Ed.SetImpliedSelection(ExistListItems.Select(x => x.ObjectId).ToArray());
            //CommandMain.SetFocus(Application.DocumentManager.MdiActiveDocument.Window.Handle);
            Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView();
        }

        private void ExecuteExportCommand(object commandParameter)
        {
            acadApp.DocumentManager.ExecuteInCommandContextAsync(items =>
            {
                acadApp.DocumentManager.CurrentDocument.UserData.XAdd(CommandNames.SaveGeoData, items);

                Editor.CommandResult cmdResult = Ed.CommandAsync("." + CommandNames.SaveGeoData);
                cmdResult.OnCompleted(() =>
                {
                    acadApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage($"Command {CommandNames.SaveGeoData} invoked successefully");
                });
                return Task.FromResult(cmdResult);
            }, ExistListItems.Select(x => x.ObjectId));
        }

        private void ExecuteStopCommand(object commandParameter)
        {
            IsCanceled = true;
            using (var commandArgs = CommandArgs.Get(CommandArgs.CurrentCommandName))
            {
                if (commandArgs != null)
                    commandArgs.Cancel();
            }
        }

        public void ExecuteProgressResetCommand(object commandParameter)
        {
            acadApp.DocumentManager.ExecuteInApplicationContext((data) =>
            {
                var param = Convert.ToInt32(commandParameter);
                CommandLine.Cancel();

                UiDispatcher.Invoke(() =>
                {
                    _selectedItem = null;
                    SelectedItem = null;
                    ProgressBarValue = 1;
                    ProgressBarValue = 0;

                    if (param == 0)
                        ((ObservableRangeCollection<ObjectIdItem>)data).Clear();
                });

            }, ExistListItems);
        }

        [CommandAspect(CommandNames.SearchTextRunCommand)]
        [CommandMethod(CommandNames.UserGroup, CommandNames.SearchTextRunCommand, CommandFlags.Transparent)]
        public async void ExecuteRunCommand(string commandParameter)
        {
            if (RunContentText == "Working") return;

            RunContentText = "Working";
            Mouse.OverrideCursor = null;
            IsCanceled = false;
            CancelTokenSource = new CancellationTokenSource();
            CancelToken = CancelTokenSource.Token;

            if (SelectedText != commandParameter)
                SelectedText = commandParameter;

            if (!BlockAttrNames.Contains(SelectedAttrName))
                BlockAttrNames.Add(SelectedAttrName);

            ExecuteProgressResetCommand(0);

            if (CurrentEntityType == "ID")
                await SearchEntitiesAsync<Entity>(commandParameter);
            if (CurrentEntityType == "Text")
                await SearchEntitiesAsync<DBText>(commandParameter);
            else if (CurrentEntityType == "Polyline")
                await SearchEntitiesAsync<Polyline>(commandParameter);
            else if (CurrentEntityType == "Line")
                await SearchEntitiesAsync<Line>(commandParameter);
            else if (CurrentEntityType == "Curve")
                await SearchEntitiesAsync<Curve>(commandParameter);
            else if (CurrentEntityType == "Block")
                SearchEntitiesAsync<BlockReference>(commandParameter);
            else if (CurrentEntityType == "Point")
                await SearchEntitiesAsync<BlockReference>(commandParameter);
            else if (CurrentEntityType == "Cable")
                await SearchElementEntitiesAsync<AcadCable>(commandParameter);
            else if (CurrentEntityType == "Cabinet")
                await SearchElementEntitiesAsync<AcadCabinet>(commandParameter);
            else if (CurrentEntityType == "Closure")
                await SearchElementEntitiesAsync<AcadClosure>(commandParameter);
        }
        private Task SearchEntitiesAsync<T>(string filter) where T : Entity
        {
            if (string.IsNullOrEmpty(filter)) return null;

            IEnumerable<T> items = null;
            List<T> entities = null;
            var layer = CurrentLayer == "All" ? "*" : CurrentLayer;
            Type selectedType = typeof(T);

            try
            {
                if (selectedType == typeof(Entity))
                {
                    items = Db.XReadObjects<T>(x => true, x => filter == "*" ? true : x.Handle.ToString() == filter);
                }
                else if (selectedType == typeof(BlockReference))
                {
                    items = Db.XReadBlocks<BlockReference>(layer, filter, SelectedAttrName, SelectedAttrValue).Cast<T>();
                }
                else if (selectedType == typeof(DBText))
                {
                    items = Db.XReadObjects<T>(layer, filter);
                }
                else if (selectedType == typeof(Curve))
                {
                    items = Db.XReadObjects<T>(layer, true);
                }
                else if (selectedType == typeof(Line) || selectedType == typeof(Polyline) || selectedType == typeof(Polyline2d) || selectedType == typeof(Polyline3d))
                {
                    items = Db.XReadObjects<T>(layer, false);
                }

                if (CanPopulated && items != null)
                {
                    entities = items.ToList();
                    TotalCount = entities.Count();
                }
                else { TotalCount = 1; }
            }
            catch (System.Exception ex)
            {
                items = null;
                Plugin.Logger.ErrorEx(ex);
                Notifications.SendNotifyMessageAsync(NotifyStatus.Ready);
            }

            ExecuteProgressResetCommand(TotalCount);

            if (items == null)
            {
                RunContentText = "Search";
                return null;
            }

            TotalCount = entities.Count();

            int chunkSize = Convert.ToInt32(TotalCount / 10);
            var currentContext = System.Threading.SynchronizationContext.Current;
            var commandArgs = Doc.UserData[CommandNames.SearchTextRunCommand] as CommandAspectAttribute;
            CancellationToken token = (CancellationToken)commandArgs.CancelToken;

            int i = 0;
            BackgroundDispatcher.BeginInvoke(new Action(() =>
            {
                //int chunkNumber = Convert.ToInt32(Math.Floor(TotalCount / 100.0));
                IEnumerable<IEnumerable<T>> chunks = entities.Chunk(chunkSize);

                foreach (var chunk in chunks)
                {
                    List<ObjectIdItem> chunkItems = new List<ObjectIdItem>();
                    foreach (var item in chunk)
                    {
                        if (token.IsCancellationRequested)
                            break;

                        ObjectIdItem objectIdItem = null;
                        currentContext.Send(async _ =>
                        {
                            objectIdItem = await item.XGetDisplayItem<Entity, ObjectIdItem>(token);
                        }, null);

                        if (objectIdItem == null) continue;

                        chunkItems.Add(objectIdItem);
                        i++;
                    }

                    ExistListItems.AddRange(chunkItems, () =>
                    {
                        ProgressBarValue = i;
                        OnPropertyChanged(nameof(FromCount));
                    });

                    //Mouse.OverrideCursor = null;
                    //Task.Delay(1000, token).ConfigureAwait(false);
                    if (token.IsCancellationRequested)
                    {
                        break;
                    };

                    chunkItems.Clear();
                }

                currentContext.Send(_ =>
                {
                    //Mouse.OverrideCursor = Cursors.Wait;
                    if (ExistListItems.Any())
                        SelectedItem = ExistListItems.FirstOrDefault();

                    ProgressBarValue = 1;
                    ProgressBarValue = 0;

                    Notifications.SendNotifyMessageAsync(NotifyStatus.Ready);
                    Mouse.OverrideCursor = null;
                    RunContentText = "Search";
                }, null);
            }));

            return null;
        }

        private async Task AddExistItemsTaskAsync<T>(CancellationToken token, IEnumerable<T> items, Type selectedType) where T : Entity
        {
            int i = 0;
            List<ObjectIdItem> ObjectIdItems = new List<ObjectIdItem>();

            var doc = acadApp.DocumentManager.MdiActiveDocument;
            using (doc.LockDocument())
            {
                await items.LoopAsync(item =>
                {
                    if (token.IsCancellationRequested)
                        return Task.FromCanceled(token);

                    var t = new Task(async () =>
                    {
                        ObjectIdItem objectIdItem;
                        //using (var tr = doc.Database.TransactionManager.TransactionScope())
                        //{
                        objectIdItem = await item.XGetDisplayItem<Entity, ObjectIdItem>(CancelToken);
                        //}

                        if (objectIdItem != null)
                        {
                            ++i;

                            if (selectedType == typeof(Entity))
                                objectIdItem.DisplayName = "ID=" + item.Handle.ToString() + " " + objectIdItem.DisplayName;

                            ObjectIdItems.Add(objectIdItem);

                            UiDispatcher.Invoke(() =>
                            {
                                if (i % 100.0 == 0 || i == TotalCount)
                                {
                                    ProgressBarValue = i * 1.0 / TotalCount * 100.0;
                                    ExistListItems.AddRange(ObjectIdItems);
                                    ObjectIdItems.Clear();
                                }
                            });
                        }
                    });
                    t.Start();

                    return t;
                });
            }
        }

        private static object _lock = new object();

        private async Task SearchElementEntitiesAsync<T>(string filter) where T : IPaletteElement
        {
            int itemCount = 0;
            List<ObjectIdItem> items = null;
            try
            {
                items = Db.XReadElements<T>(filter, CurrentLayer == "All" ? "" : CurrentLayer).ToList();
                itemCount = CanPopulated ? items.Count : 1;
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                Log.Logger.Error(ex.InnerException?.Message ?? ex.Message);
                items = null;
                Notifications.SendNotifyMessageAsync(NotifyStatus.Ready);
                Mouse.OverrideCursor = null;
            }

            ExecuteProgressResetCommand(itemCount);

            if (items == null)
            {
                RunContentText = "Search";
                return;
            }

            await UiDispatcher.BeginInvoke(new Action<CancellationToken>((token) =>
            {
                int i = 0;
                foreach (var item in items)
                {
                    if (!IsCanceled && ++i <= itemCount)
                    {
                        UiDispatcher.Invoke(() =>
                        {
                            ProgressBarValue += 1;
                            ExistListItems.Add(
                                new ObjectIdItem(item.ObjectId,
                                    item.DisplayName.Replace("Acad", "") + ": " + item.DisplayName));
                        });
                    }

                    break;
                }
            }), CancelToken);

            UiDispatcher.Invoke(() =>
            {
                if (ExistListItems.Any())
                {
                    SelectedItem = ExistListItems.FirstOrDefault();
                    if (SelectedItem != null)
                    {
                        CommandLine.Zoom(SelectedItem.ObjectId, CurrentZoomDisplayFactor);
                        var ent = SelectedItem.ObjectId.XCast<Polyline>();
                        ent.Highlight();
                    }
                }
                ProgressBarValue = 1;
                ProgressBarValue = 0;
                Notifications.SendNotifyMessageAsync(NotifyStatus.Ready);
                Mouse.OverrideCursor = null;
                RunContentText = "Search";
            });
        }

        public bool CanExecuteRunCommand(object parameter)
        {
            return !StopButtonEnabled;
        }

        #endregion "Commands"

        #region <Methods>

        public bool OnFindAction(ITaskArgs args)
        {
            return true;
        }

        private void OnParserCompleted(object result)
        {
            //var docName = Application.DocumentManager.GetDocument(ToolsManager.Db).Name.ToLower(); //.Substring(2)
            //var layout = ProjectExplorerViewModel.LayoutItems.
            //    FirstOrDefault(X => X.LayoutId == ProjectExplorerViewModel.CurrentLayout.LayoutId && docName.Contains(X.CADFileName.ToLower()));
            //if (layout != null) // && !layout.FSA)
            //{
            //    //var idx = ProjectExplorerViewModel.LayoutItems.IndexOf(layout);
            //    //var item = ((Layout)layout).CloneTo<Layout>();
            //    //item.FSA = true;
            //    //ProjectExplorerViewModel.LayoutItems[idx] = item;
            //    //ProjectExplorerViewModel.CurrentLayout = ProjectExplorerViewModel.LayoutItems[idx];
            //}
        }

        public override void OnDocumentActivated(object sender, DocumentCollectionEventArgs e)
        {
            UiDispatcher.InvokeAsync(() =>
            {
                UnregisterEvents();
                ExecuteProgressResetCommand(0);
                RegisterEvents();
            });
        }

        public Task<ICommandArgs> ExecuteCommand(ICommandArgs command)
        {
            //throw new NotImplementedException(); 
            return null;
        }

        public void Deactive()
        {
            _appSettings.Save();
        }
        #endregion

        //public class CommandSearchText : CommandBase
        //{
        //    public void SearchTextRunCommand()
        //    {
        //        using (CommandContext context = new CommandContext(CommandNames.SearchTextRunCommand))
        //        {
        //        }
        //    }
        //}
    }

    public class GeometryDto
    {
        public string Type { get; set; }
        public double[,] coordinates { get; set; }

    }

}

//await entities.XCreateDisplayItemsAsync((objectIdItems, progress, token) =>
//{
//    if (token.IsCancellationRequested)
//        return;
//    ExistListItems.AddRange(objectIdItems, async () =>
//    {
//        ProgressBarValue = progress;
//        OnPropertyChanged(nameof(FromCount));
//        await Task.Delay(1000, token);
//    });
//}, CancelToken, Convert.ToInt32(TotalCount / 10));


//foreach (var item in entities) 
//{
//    if (token.IsCancellationRequested)
//        break;

//    ObjectIdItem objectIdItem = null;
//    currentContext.Send(async _ =>
//    {
//        _ = await item.XGetDisplayItem<Entity, ObjectIdItem>(token);
//    }, objectIdItem);

//    if (objectIdItem == null) continue;
//    ++i;

//    chunkItems.Add(objectIdItem);

//    if (i % (decimal)chunkSize == 0 || i == totalCount)
//    {
//        int chunkNumber = Convert.ToInt32(Math.Floor(i * 1.0 / totalCount * 100.0));

//        ExistListItems.AddRange(chunkItems, async () =>
//        {
//            ProgressBarValue = i;
//            OnPropertyChanged(nameof(FromCount));
//            Mouse.OverrideCursor = null;
//            await Task.Delay(1000, token);
//        });

//        chunkItems.Clear();
//    }
//}