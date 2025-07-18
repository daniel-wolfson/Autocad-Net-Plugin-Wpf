using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using ID.Infrastructure;
using ID.Infrastructure.Commands;
using ID.Infrastructure.Enums;
using ID.Infrastructure.Extensions;
using ID.Infrastructure.Interfaces;
using ID.Infrastructure.Models;
using Intellidesk.AcadNet.Common.Core;
using Intellidesk.AcadNet.Common.Enums;
using Intellidesk.AcadNet.Common.Extensions;
using Intellidesk.AcadNet.Common.Interfaces;
using Intellidesk.AcadNet.Common.Models;
using Intellidesk.AcadNet.Interfaces;
using Intellidesk.AcadNet.Model;
using Intellidesk.AcadNet.Services;
using Intellidesk.Data.Models.Cad;
using Intellidesk.Data.Models.Entities;
using Intellidesk.Data.Repositories.Infrastructure;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Input;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Exception = System.Exception;
using ObjectIdItem = Intellidesk.AcadNet.Common.Models.ObjectIdItem;

namespace Intellidesk.AcadNet.ViewModels
{
    public class ClosureConnectPanelContext : BaseViewModel, IClosureConnectPanelContext
    {
        #region <fields>
        private ILayerService _layerService;
        private readonly IDrawService _draw = Plugin.GetService<IDrawService>();
        private string _runButtonText;
        private bool _isEditMode;
        private string _header = "Entity";
        private string _selectedText;
        private string _currentBodyLayer = "All";
        private string _currentMarkerLayer = "All";

        private List<string> _layers;
        private ObservableRangeCollection<ObjectIdItem> _elementItems = new ObservableRangeCollection<ObjectIdItem>();
        private ObjectIdItem _selectedKey = null;
        private ObjectIdItem _selectedItem;
        private bool _canPopulated = true;
        #endregion <fields>

        #region <Property>
        public Action<object> Load { get; set; }
        public bool ColorsReadOnly { get; } = true;
        public bool LayersReadOnly { get; } = true;
        public bool IsEditMode
        {
            get { return _isEditMode; }
            set
            {
                _isEditMode = value;
                OnPropertyChanged();
            }
        }
        public string RunButtonText
        {
            get { return _runButtonText; }
            set
            {
                _runButtonText = value;
                OnPropertyChanged();
            }
        }

        public Action FinishInteraction { get; set; }
        //public INotification Notification
        //{
        //    get { return this._notification; }
        //    set
        //    {
        //        if (value is SearchTextNotification)
        //        {
        //            // To keep the code simple, this is the only property where we are raising the PropertyChanged event,
        //            // as it's required to update the bindings when this property is populated.
        //            // Usually you would want to raise this event for other properties too.
        //            this._notification = (SearchTextNotification)value;
        //            this.OnPropertyChanged();
        //        }
        //    }
        //}
        public string Header
        {
            get { return _header; }
            set
            {
                _header = value;
                OnPropertyChanged();
            }
        }

        public string CurrenBodyLayer
        {
            get { return _currentBodyLayer; }
            set
            {
                _currentBodyLayer = value;
                OnPropertyChanged();
            }
        }

        public string CurrenMarkerLayer
        {
            get { return _currentMarkerLayer; }
            set
            {
                _currentMarkerLayer = value;
                OnPropertyChanged();
            }
        }

        public new UserSetting CurrentUserSetting
        {
            get { return _currentUserSetting; }
            set
            {
                _currentUserSetting = value;
                OnPropertyChanged();
            }
        }

        public List<string> Layers
        {
            get { return _layers; }
            set
            {
                _layers = value;
                OnPropertyChanged();
            }
        }

        public string SelectedText
        {
            get { return _selectedText; }
            set
            {
                _selectedText = value;
                OnPropertyChanged();
            }
        }
        public ObjectIdItem SelectedKey
        {
            get { return this._selectedKey; }
            set
            {
                this._selectedKey = value;
                this.OnPropertyChanged();
            }
        }
        public ObjectIdItem SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                Entity ent;
                if (_selectedItem != null)
                {
                    if (!_selectedItem.ObjectId.IsGroup())
                        ent = _selectedItem.ObjectId.XGetEntity();
                    else
                    {
                        var id = Db.XGetGroupEntities(_selectedItem.ObjectId).FirstOrDefault();
                        ent = id.XGetEntity();
                    }
                    ent.Unhighlight();
                }

                if (value != null)
                {
                    _selectedItem = value;
                    if (!_selectedItem.ObjectId.IsGroup())
                        ent = _selectedItem.ObjectId.XGetEntity();
                    else
                    {
                        var id = Db.XGetGroupEntities(_selectedItem.ObjectId).FirstOrDefault();
                        ent = id.XGetEntity();
                    }

                    CommandContext.CurrentEntity = ent;
                    ent.Highlight();
                    if (this.IsLoaded)
                        CommandLine.Zoom(ent.ObjectId, CurrentZoomDisplayFactor);
                }
            }
        }

        private ClosureConnect _currentBodyElement;
        public ClosureConnect CurrentBodyElement
        {
            get { return _currentBodyElement; }
            set
            {
                _currentBodyElement = value;
                OnPropertyChanged();
            }
        }

        private PaletteElement _currentMarkerElement;
        public PaletteElement CurrentMarkerElement
        {
            get { return _currentMarkerElement; }
            set
            {
                _currentMarkerElement = value;
                OnPropertyChanged();
            }
        }

        private int _currentElementFontSize = 8;

        public int CurrentElementFontSize
        {
            get { return _currentElementFontSize; }
            set
            {
                _currentElementFontSize = value;
                OnPropertyChanged();
            }
        }

        public DelegateCommand<MouseWheelEventArgs> UodateFontSizeCommand
        {
            get
            {
                return new DelegateCommand<MouseWheelEventArgs>(args =>
                {
                    if (args.Delta > 0)
                    {
                        CurrentElementFontSize++;
                    }
                    else
                    {
                        CurrentElementFontSize--;
                    }
                });
            }
        }

        public int CurrentZoomDisplayFactor
        {
            get { return Plugin.Settings.ZoomDisplayFactor; }
            set
            {
                UiDispatcher.BeginInvoke(new Action(() =>
                {
                    Plugin.Settings.ZoomDisplayFactor = value;
                    Plugin.RegisterInstance(Plugin.Settings);

                    ////_appSettings.Save();

                    //Common.Properties.Settings.Default.ZoomDisplayFactor = _currentZoomDisplayFactor;
                    //Common.Properties.Settings.Default.Upgrade();
                    //Common.Properties.Settings.Default.Save();

                    //string path = typeof (Common.Properties.Settings).Assembly.Location;
                    //Configuration config = ConfigurationManager.OpenExeConfiguration(path.Replace("ID.AcadNet.dll", "ID.AcadNet.Common.dll"));
                    //AppSettingsSection appSettings = config.AppSettings;
                    //if (appSettings.IsReadOnly() == false)
                    //{
                    //    appSettings.Settings["ZoomDisplayFactor"].Value = _currentZoomDisplayFactor.ToString();
                    //    config.Save();
                    //}
                }));

                OnPropertyChanged();
            }
        }

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

        #region <Contexts>

        public LayerViewModel MarkerLayerDataContext { get; }

        private PaletteElementContext _bodyDataContext { get; set; }
        public PaletteElementContext BodyElementDataContext
        {
            get => _bodyDataContext;
            set
            {
                _bodyDataContext = value;
                OnPropertyChanged();
            }
        }

        private PaletteElementContext _marketDataContext { get; set; }
        public PaletteElementContext MarkerElementDataContext
        {
            get => _marketDataContext;
            set
            {
                _marketDataContext = value;
                OnPropertyChanged();
            }
        }

        public ISearchViewModel SearchViewModel { get; }
        #endregion

        #region <ctor>
        public ClosureConnectPanelContext(ILayerService layerService)
        {
            _layerService = layerService;

            Header = "Connector: ";
            RunButtonText = "Draw";

            IsSelectionEnabled = true;
            IsPointMonitorEnabled = true;
            IsEntityModifyEnabled = true;

            BodyElementDataContext = new PaletteElementContext() { Parent = this };
            BodyElementDataContext.Load(context =>
            {
                context.ElementItems = new ObservableRangeCollection<PaletteElement>
                {
                    new PaletteElement(eBodyType.Rectangle)
                };

                context.CurrentElement = context.ElementItems.FirstOrDefault();

                context.LayerDataContext = new LayerViewModel(
                    typeof(AcadClosureConnect).FullName, context.CurrentElement.LayerName)
                { Parent = this };

                context.ColorDataContext = new ColorViewModel(typeof(eOpenCloseType),
                    (short)context.CurrentElement.ColorIndex)
                { Parent = this, IsReadOnly = true };
            });

            MarkerElementDataContext = new PaletteElementContext() { Parent = this };
            MarkerElementDataContext.Load(context =>
            {
                context.ElementItems = new ObservableRangeCollection<PaletteElement>
                {
                    new PaletteElement(eOpenCloseType.Open),
                    new PaletteElement(eOpenCloseType.Close)
                };

                context.CurrentElement = context.ElementItems.FirstOrDefault();

                context.LayerDataContext = new LayerViewModel(
                    typeof(AcadClosureConnect).FullName, context.CurrentElement.LayerName)
                { Parent = this };

                context.ColorDataContext = new ColorViewModel(typeof(eOpenCloseType),
                    (short)BodyElementDataContext.ColorDataContext.CurrentColor.ColorIndex)
                { Parent = this, IsReadOnly = true };
            });

            SearchViewModel = new SearchElementViewModel { Parent = this };
            ProgressBarMinimum = 0;
            ProgressBarMaximum = 100;

            ElementItems.CollectionChanged += (sender, args) => { ElementItems = ElementItems; };

            Load = (x) =>
            {
                Thread.Sleep(500);
                UiDispatcher.BeginInvoke(new Action(() =>
                {
                    LoadData();
                }));
            };
        }
        #endregion

        #region <Commands>

        private ICommand _addCommand, _addTitleCommand, _addMarkerCommand, _closeCommand, _editCommand,
            _getLocationCommand, _selectSetCommand, _refreshCommand, _runCommand, _undoCommand;

        public ICommand RunCommand => _runCommand ?? (_runCommand = new DelegateCommand<object>(ExecuteRunCommand, CanExecuteCommand));
        public ICommand EditCommand => _editCommand ?? (_editCommand = new DelegateCommand<object>(ExecuteEditCommand, CanExecuteCommand));
        public ICommand StopCommand { get; set; }
        public ICommand SelectSetCommand => _selectSetCommand ?? (_selectSetCommand = new DelegateCommand<object>(ExecuteSelectSetCommand));
        public ICommand AddCommand => _addCommand ?? (_addCommand = new DelegateCommand<object>(ExecuteAddNewCommand));
        public ICommand AddTitleCommand => _addTitleCommand ?? (_addTitleCommand = new DelegateCommand<object>(ExecuteAddTitleCommand));
        public ICommand AddMarkerCommand => _addMarkerCommand ?? (_addMarkerCommand = new DelegateCommand<object>(ExecuteAddMarkerCommand));
        public ICommand UndoCommand => _undoCommand ?? (_undoCommand = new DelegateCommand<object>(ExecuteResetCommand));
        public ICommand GetLocationCommand => _getLocationCommand ?? (_getLocationCommand = new DelegateCommand<object>(ExecuteGetLocationCommand));
        public ICommand RefreshCommand => _refreshCommand ?? (_refreshCommand = new DelegateCommand<object>(ExecuteRefreshCommand));
        public ICommand CloseCommand => _closeCommand ?? (_closeCommand = new DelegateCommand<object>(ExecuteClose));

        public void ExecuteClose(object commandParameter)
        {
            var commandLine = Plugin.GetService<ICommandLine>();
            commandLine.SendToExecute(CommandNames.ClosureConnectPanelRemove);
        }

        public void ExecuteSelectSetCommand(object commandParameter)
        {
            Ed.SetImpliedSelection(ElementItems.Select(x => x.ObjectId).ToArray());
            //CommandMain.SetFocus(Application.DocumentManager.MdiActiveDocument.Window.Handle);
            Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView();
        }

        public void ExecuteAddNewCommand(object commandParameter)
        {
            ExecuteResetCommand(commandParameter);
        }
        public void ExecuteAddTitleCommand(object commandParameter)
        {
            SendCommandToExecute(CommandNames.AddTitleClosureConnect, ElementItems, commandParameter);
        }

        public void ExecuteAddMarkerCommand(object commandParameter)
        {
            SendCommandToExecute(CommandNames.AddMarkerClosureConnect, this, commandParameter);
        }

        public void ExecuteEditCommand(object commandParameter)
        {
            if (!(commandParameter is IPaletteElement element)) return;

            UiDispatcher.InvokeAsync(() =>
            {
                RunButtonText = !string.IsNullOrEmpty(element.Handle) ? "Update" : "Draw";

                base.ElementItems.Clear();
                BodyElementDataContext.CurrentElement = element as PaletteElement;

                if (!string.IsNullOrEmpty(element.Handle))
                {
                    var displayObjectList = Db.XGetObjectDisplayItems(element.Handle);
                    if (displayObjectList.Any())
                        base.ElementItems.AddRange(displayObjectList);
                }
            });
        }

        public void ExecuteGetLocationCommand(object commandParameter)
        {
            UiDispatcher.Invoke(() =>
            {
            });
        }

        public void ExecuteRunCommand(object commandParameter)
        {
            if (RunButtonText == "Draw")
            {
                //CommandArgs commandArgs = new
                //    CommandArgs(this, CommandNames.AddClosureConnect, commandParameter);
                //commandArgs.SendToExecute(cmd => { RunButtonText = "Update"; });

                SendCommandToExecute(CommandNames.AddClosureConnect);
            }
            else if (RunButtonText == "Update")
            {
                var commandArgs = new CommandArgs(this, CommandNames.UpdateClosureConnect, BodyElementDataContext);
                commandArgs.SendToExecute();
            }
        }

        public void ExecuteResetCommand(object commandParameter)
        {
            var progressBarMaximum = 0;
            string currentTitle = null;

            if (commandParameter != null && commandParameter.GetType() == typeof(int))
            {
                progressBarMaximum = Convert.ToInt32(commandParameter);
            }
            else if (commandParameter != null && commandParameter.GetType() == typeof(PaletteElement))
            {
                currentTitle = BodyElementDataContext.CurrentElement.Title;
                BodyElementDataContext.CurrentElement.Items = new string[] { };
            }

            UiDispatcher.Invoke(() =>
            {
                ProgressStateValue = 0;
                ProgressBarValue = 0;
                ProgressBarMaximum = progressBarMaximum;
                ElementItems.Clear();
                SelectedItem = null;
                CommandLine.Cancel();

                MarkerElementDataContext.CurrentElement.Handle = "";
                BodyElementDataContext.CurrentElement.Handle = "";
                BodyElementDataContext.CurrentElement.Title = currentTitle ?? "";
                RunButtonText = "Draw";
            });
        }

        public void ExecuteRefreshCommand(object commandParameter)
        {
            try
            {
                UiDispatcher.Update(commandParameter, (Action<object>)(par =>
                {
                    var connectorViewModel = commandParameter as ClosureConnectPanelElementContext;
                    if (connectorViewModel != null)
                    {
                        BodyElementDataContext.LayerDataContext.Load("ClosureConnect");
                        BodyElementDataContext.Reload();

                        base.ElementItems.Clear();
                        base.ElementItems.AddRange(Db.XGetObjectDisplayItems((string)connectorViewModel.CurrentElement.Handle));
                    }
                }));
            }
            catch (Exception)
            {
                //ignore
            }

        }

        public bool CanExecuteCommand(object commandParameter)
        {
            return true; // !string.IsNullOrEmpty(SelectedText);
        }

        public Task<ICommandArgs> ExecuteCommand(ICommandArgs commandParameter)
        {
            //throw new NotImplementedException(); 
            return null;
        }

        #endregion "Commands"

        #region <Methods>
        public override void RegisterEvents()
        {
            base.RegisterEvents();
            BodyElementDataContext.PropertyChanged += OnDataContextPropertyChanged;
            BodyElementDataContext.ColorDataContext.PropertyChanged += OnDataContextPropertyChanged;
        }

        public override void UnregisterEvents()
        {
            base.UnregisterEvents();
            BodyElementDataContext.PropertyChanged -= OnDataContextPropertyChanged;
            BodyElementDataContext.ColorDataContext.PropertyChanged -= OnDataContextPropertyChanged;
        }

        public void LoadData()
        {
            var listItems = _layerService.GetAll();
            _layerService.Create(listItems);
            Edit.WriteMessage($"{Plugin.Settings.Prompt}{nameof(ClosureConnect)} data loaded");
        }

        private void SetObjectText(string text)
        {
            //typeTextBox.Text = text;
        }

        private void SetObjectId(DBObject obj)
        {
            if (obj == null)
            {
                SetObjectText("");
            }
            else
            {
                Document doc = acadApp.DocumentManager.MdiActiveDocument;
                DocumentLock loc = doc.LockDocument();
                using (loc)
                {
                    Transaction tr = doc.TransactionManager.StartTransaction();
                    using (tr)
                    {
                        //DBObject obj = Tr.GetObject(id, OpenMode.ForRead);
                        //ResultBuffer ext = obj.XGetXrecord();
                        //if (ext != null)
                        //{
                        //    Header = ext.ToString();
                        //    SetObjectText(obj.GetType().ToString());
                        //    Tr.Commit();
                        //}
                    }
                }
            }
        }

        #endregion

        #region <Events>
        private void OnMonitorPoint(object sender, PointMonitorEventArgs e)
        {
            if ((e.Context.History & PointHistoryBits.FromKeyboard) != PointHistoryBits.FromKeyboard)
                return;

            FullSubentityPath[] paths = e.Context.GetPickedEntities();
            if (paths.Length <= 0)
            {
                SetObjectId(null);
                return;
            }

            ObjectId[] objs = paths[0].GetObjectIds();
            if (objs.Length <= 0)
            {
                SetObjectId(null);
                return;
            }

            // Set the "selected" object to be the last in the list
            ObjectId objectId = objs[objs.Length - 1];

            //SetObjectId(objectId); //TODO ???
        }

        public bool OnFindAction(ITaskArgs args)
        {
            return true;
        }

        public override void OnDocumentActivated(object sender, DocumentCollectionEventArgs e)
        {
            UiDispatcher.InvokeAsync(() =>
            {
                try
                {
                    UnregisterEvents();
                    ExecuteResetCommand(0);
                    ExecuteRefreshCommand(null);
                    RegisterEvents();
                }
                catch (Exception)
                {
                    // ignore
                }
            });

        }

        //private void OnParserCompleted(object result)
        //{
        //    //var docName = Application.DocumentManager.GetDocument(ToolsManager.Db).Name.ToLower(); //.Substring(2)
        //    //var layout = ProjectExplorerViewModel.LayoutItems.
        //    //    FirstOrDefault(X => X.LayoutId == ProjectExplorerViewModel.CurrentLayout.LayoutId && docName.Contains(X.CADFileName.ToLower()));
        //    //if (layout != null) // && !layout.FSA)
        //    //{
        //    //    //var idx = ProjectExplorerViewModel.LayoutItems.IndexOf(layout);
        //    //    //var item = ((Layout)layout).CloneTo<Layout>();
        //    //    //item.FSA = true;
        //    //    //ProjectExplorerViewModel.LayoutItems[idx] = item;
        //    //    //ProjectExplorerViewModel.CurrentLayout = ProjectExplorerViewModel.LayoutItems[idx];
        //    //}
        //}
        //public void Deactive()
        //{
        //    Plugin.PluginSettings.Save();
        //    RemoveEvents();
        //}

        public override void OnAcadEntityErased(object sender, EventArgs e)
        {
            ObjectIdItem item = ElementItems.FirstOrDefault(x => x.ObjectId == ((DBObject)sender).ObjectId);
            if (item != null)
            {
                ElementItems.Remove(item);
                if (!ElementItems.Any())
                    ExecuteAddNewCommand(null);
            }
        }

        public override void OnAcadEntityModified(object sender, EventArgs args)
        {
            //var objIds = Draw.Connector(connectorDataContext.CurrentConnector).ToList();
            //ConnectorPanelDataContext sender = commandArgs.Sender as ConnectorPanelDataContext;

            if (sender is Polyline)
            {
                InfraManager.DelayAction(200, () =>
                {
                    using (acadApp.DocumentManager.MdiActiveDocument.LockDocument())
                    using (((Polyline)sender).Database.TransactionManager.StartTransaction())
                    {
                        var pline = (Polyline)sender;
                        var xRecordData = pline.XGetXrecordValue(DxfCodeExt.DataObject);
                        if (xRecordData != null)
                        {
                            //AcadConnector commnadParameter = JsonConvert.DeserializeObject<AcadConnector>(xRecordData);
                            AcadClosureConnect commnadParameter = new JavaScriptSerializer().Deserialize<AcadClosureConnect>(xRecordData.ToString());

                            pline.XSetXrecord(Db.NamedObjectsDictionaryId, commnadParameter);

                            ObjectIdItem item = ElementItems.FirstOrDefault(x => x.ObjectId == pline.ObjectId);
                            if (item != null)
                            {
                                item.DisplayName =
                                    $"Connector (vertex: {pline.NumberOfVertices}; length: {Math.Round(pline.GetDistanceAtParameter(pline.EndParam) - pline.GetDistanceAtParameter(pline.StartParam), 4)}";

                                var temp = ElementItems.ToList();
                                ElementItems.Clear();
                                ElementItems.AddRange(temp);
                            }
                        }
                    }
                });
            }
            else if (sender is DBText)
            {
            }
        }

        public override void OnDataContextPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is ClosureConnectPanelElementContext && BodyElementDataContext.CurrentElement != null)
            {
                if (e.PropertyName == nameof(CurrentBodyElement))
                {
                    var name = BodyElementDataContext.CurrentElement.Title;
                    BodyElementDataContext.CurrentElement =
                        BodyElementDataContext.ElementItems.FirstOrDefault(x => x.TypeCode == BodyElementDataContext.CurrentElement.TypeCode);
                    BodyElementDataContext.CurrentElement.Title = name;
                }
                if (e.PropertyName == "ConnectorType")
                {
                    BodyElementDataContext.CurrentElement.Update(typeof(eOpenCloseType), BodyElementDataContext.CurrentElement.TypeCode, ObjectState.Modified);
                    BodyElementDataContext.ColorDataContext.CurrentColor = BodyElementDataContext.ColorDataContext.Colors.FirstOrDefault(x => x.ColorIndex == BodyElementDataContext.CurrentElement.ColorIndex);
                    BodyElementDataContext.LayerDataContext.CurrentLayer = BodyElementDataContext.LayerDataContext.Layers.FirstOrDefault(x => x.LayerName == BodyElementDataContext.CurrentElement.LayerName);
                }
            }
            else if (sender is ColorViewModel && !BodyElementDataContext.ColorDataContext.IsReadOnly)
            {
                if (e.PropertyName == "CurrentColor")
                    BodyElementDataContext.CurrentElement.ColorIndex = BodyElementDataContext.ColorDataContext.CurrentColor.ColorIndex;
            }
            else if (sender is IPaletteElement)
            {
                ((IPaletteElement)sender).ObjectState = ObjectState.Modified;
            }
        }

        //public override event PropertyChangedEventHandler PropertyChanged;
        //protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        //{
        //    PropertyChangedEventHandler handler = PropertyChanged;
        //    handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //}

        #endregion
    }
}
