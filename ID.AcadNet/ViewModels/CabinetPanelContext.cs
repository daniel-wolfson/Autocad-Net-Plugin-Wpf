using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using ID.Infrastructure;
using ID.Infrastructure.Commands;
using ID.Infrastructure.Enums;
using ID.Infrastructure.Interfaces;
using ID.Infrastructure.Models;
using Intellidesk.AcadNet.Common.Core;
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
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Exception = System.Exception;
using ObjectIdItem = Intellidesk.AcadNet.Common.Models.ObjectIdItem;

namespace Intellidesk.AcadNet.ViewModels
{
    public delegate void AcadEntitySelectedEventHandler(object sender, SelectionAddedEventArgs e);

    public class CabinetPanelContext : BaseViewModel, ICabinetPanelContext
    {
        private readonly IDrawService _draw = Plugin.GetService<IDrawService>();
        private readonly IPluginSettings _appSettings;
        private string _runButtonText;
        private string _header = "Entity";
        private string _currentLayer = "All";
        private Cabinet _currentCabinet;
        private List<string> _layers;

        private ObservableRangeCollection<ObjectIdItem> _elementItems =
            new ObservableRangeCollection<ObjectIdItem>();

        private ObjectIdItem _selectedKey = null;
        private ObjectIdItem _selectedItem;
        private double _progressBarMax;
        private double _progressBarValue;
        private double _progressStateValue;
        private bool _canPopulated = true;

        #region <Properties>
        public bool ColorsReadOnly { get; } = true;
        public bool LayersReadOnly { get; } = true;
        public string RunButtonText
        {
            get { return _runButtonText; }
            set
            {
                _runButtonText = value;
                OnPropertyChanged();
            }
        }
        public Action<object> Load { get; set; }
        public string Header
        {
            get { return _header; }
            set
            {
                _header = value;
                OnPropertyChanged();
            }
        }
        public string CurrentLayer
        {
            get { return _currentLayer; }
            set
            {
                _currentLayer = value;
                OnPropertyChanged();
            }
        }
        public Cabinet CurrentCabinet
        {
            get { return _currentCabinet; }
            set
            {
                _currentCabinet = value;
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

        public ObjectIdItem SelectedKey
        {
            get { return _selectedKey; }
            set
            {
                _selectedKey = value;
                OnPropertyChanged();
                OnPropertyChanged("SelectedValue");
            }
        }

        public ObjectIdItem SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                Entity ent;
                if (_selectedItem != null && !_selectedItem.ObjectId.IsErased)
                {
                    ent = _selectedItem.ObjectId.XGetEntity();
                    ent?.Unhighlight();
                }

                if (value != null)
                {
                    _selectedItem = value;
                    if (this.IsLoaded)
                        CommandLine.Zoom(_selectedItem.ObjectId, CurrentZoomDisplayFactor);
                    ent = _selectedItem.ObjectId.XGetEntity();
                    ent.Highlight();
                    CommandContext.CurrentEntity = ent;
                    Thread.Sleep(200);
                }
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
                    Plugin.RegisterInstance(_appSettings);

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
        public double ProgressBarMinimum { get; set; }
        /// <summary> Gets or sets the progress bar's maximum value.</summary>
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
        public double ProgressBarValue
        {
            get { return _progressBarValue; }
            set
            {
                _progressBarValue = value;
                OnPropertyChanged();
            }
        }
        public double ProgressStateValue
        {
            get { return _progressStateValue; }
            set
            {
                _progressStateValue = value;
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
        public new UserSetting CurrentUserSetting
        {
            get { return _currentUserSetting; }
            set
            {
                _currentUserSetting = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region #Contexts#
        public ColorViewModel ColorDataContext { get; }
        public LayerViewModel BodyLayerDataContext { get; }
        public CabinetPanelElementContext BodyElementDataContext { get; }
        public ISearchViewModel SearchViewModel { get; }
        #endregion

        #region <ctor>
        public CabinetPanelContext()
        {
            Header = "Cabinet: ";
            RunButtonText = "Draw";

            IsSelectionEnabled = true;
            IsPointMonitorEnabled = true;
            IsEntityModifyEnabled = true;

            BodyElementDataContext = new CabinetPanelElementContext(eCabinetType.AGC) { Parent = this };
            ColorDataContext = new ColorViewModel(typeof(eCabinetType), (short)BodyElementDataContext.CurrentElement.ColorIndex) { Parent = this, IsReadOnly = true };
            BodyLayerDataContext = new LayerViewModel(typeof(Cabinet).Name,
                BodyElementDataContext.CurrentElement.LayerName)
            { Parent = this };

            BodyElementDataContext.CurrentElement = new AcadCabinet(BodyElementDataContext.CabinetType.TypeCode);
            ColorDataContext.CurrentColor = ColorDataContext.Colors.FirstOrDefault(x => x.ColorIndex == BodyElementDataContext.CabinetType.ColorIndex);
            BodyLayerDataContext.CurrentLayer = BodyLayerDataContext.Layers.FirstOrDefault(x => x.LayerName == BodyElementDataContext.CabinetType.LayerName);

            SearchViewModel = new SearchElementViewModel { Parent = this };

            ProgressBarMinimum = 0;
            ProgressBarMaximum = 100;

            var layerService = Plugin.GetService<LayerService>();
            var list = layerService.GetAll().Select(x => x.LayerName).ToList();
            list.Insert(0, "All");
            Layers = list;

            _appSettings = Plugin.Settings;

            ElementItems.CollectionChanged += (sender, args) => { ElementItems = ElementItems; };
        }
        #endregion

        #region <Commands>

        private ICommand _addCommand, _addTitleCommand, _closeCommand, _editCommand, _getLocationCommand,
                         _selectSetCommand, _refreshCommand, _runCommand, _undoCommand;

        public ICommand RunCommand => _runCommand ?? (_runCommand = new DelegateCommand<object>(ExecuteRunCommand, CanExecuteCommand));
        public ICommand EditCommand => _editCommand ?? (_editCommand = new DelegateCommand<object>(ExecuteEditCommand, CanExecuteCommand));
        public ICommand StopCommand { get; set; }
        public ICommand SelectSetCommand => _selectSetCommand ?? (_selectSetCommand = new DelegateCommand<object>(ExecuteSelectSetCommand));
        public ICommand AddCommand => _addCommand ?? (_addCommand = new DelegateCommand<object>(ExecuteAddCommand));
        public ICommand AddTitleCommand => _addTitleCommand ?? (_addTitleCommand = new DelegateCommand<object>(ExecuteAddTitleCommand));
        public ICommand UndoCommand => _undoCommand ?? (_undoCommand = new DelegateCommand<object>(ExecuteResetCommand));
        public ICommand GetLocationCommand => _getLocationCommand ?? (_getLocationCommand = new DelegateCommand<object>(ExecuteGetLocationCommand));
        public ICommand RefreshCommand => _refreshCommand ?? (_refreshCommand = new DelegateCommand<object>(ExecuteRefreshCommand));
        public ICommand CloseCommand => _closeCommand ?? (_closeCommand = new DelegateCommand<object>(ExecuteClose));

        public void ExecuteClose(object commandParameter)
        {
            var commandLine = Plugin.GetService<ICommandLine>();
            commandLine.SendToExecute(CommandNames.CabinetPanelRmove);
        }

        public void ExecuteSelectSetCommand(object commandParameter)
        {
            Ed.SetImpliedSelection(ElementItems.Select(x => x.ObjectId).ToArray());
            //CommandMain.SetFocus(Application.DocumentManager.MdiActiveDocument.Window.Handle);
            Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView();
        }

        public void ExecuteAddCommand(object sender)
        {
            UiDispatcher.Update(sender, par =>
            {
                BodyElementDataContext.CurrentElement = new AcadCabinet(BodyElementDataContext.CabinetType.TypeCode);
                RunButtonText = !string.IsNullOrEmpty(BodyElementDataContext.CurrentElement?.Handle) ? "Update" : "Draw";
                ElementItems.Clear();
            });
        }

        public void ExecuteAddTitleCommand(object sender)
        {
            var commandArgs = new CommandArgs(this, CommandNames.AddTitleCabinet, BodyElementDataContext);
            commandArgs.SendToExecute();
        }

        public void ExecuteEditCommand(object commandParameter)
        {
            UiDispatcher.InvokeAsync(() =>
            {
                AcadCabinet cabinet = commandParameter as AcadCabinet;
                if (cabinet == null) return;

                RunButtonText = !string.IsNullOrEmpty(cabinet.Handle) ? "Update" : "Draw";
                ElementItems.Clear();

                if (!string.IsNullOrEmpty(cabinet.Handle))
                {
                    List<ObjectIdItem> displayObjectList = new List<ObjectIdItem>();
                    List<string> cabinetHandles = new List<string> { cabinet.Handle };

                    if (cabinet.Items.Length > 0) cabinetHandles.AddRange(cabinet.Items);

                    Database db = Ed.Document.Database;
                    foreach (var handle in cabinetHandles)
                    {
                        DBObject dbObject = db.XGetObject(handle);
                        if (dbObject == null) continue;

                        Type type = dbObject.XGetXDataObjectType();
                        if (type == typeof(AcadCabinet))
                        {
                            AcadCabinet acadCabinet = dbObject.XGetXDataObject<AcadCabinet>();
                            if (acadCabinet != null) displayObjectList.Add(DBObjectExtensions.XGetDisplayItem(dbObject, acadCabinet));
                        }
                        else if (type == typeof(AcadTitle))
                        {
                            AcadTitle acadTitle = dbObject.XGetXDataObject<AcadTitle>();
                            if (acadTitle != null) displayObjectList.Add(acadTitle.GetListItem(dbObject));
                        }
                    }
                    ElementItems.AddRange(displayObjectList);
                }

                BodyElementDataContext.CurrentElement = cabinet;

                //LoadEvents();
            });
        }

        public void ExecuteGetLocationCommand(object commandParameter)
        {
            UiDispatcher.Invoke(() =>
            {
            });
        }

        public void ExecuteResetCommand(object commandParameter)
        {
            UiDispatcher.Invoke(() =>
            {
                ProgressStateValue = 0;
                ProgressBarValue = 0;
                ProgressBarMaximum = commandParameter != null ? Convert.ToInt32(commandParameter) : 0;
                ElementItems.Clear();
                SelectedItem = null;
                CommandLine.Cancel();
            });
        }

        public void ExecuteRefreshCommand(object commandParameter)
        {
            try
            {
                UiDispatcher.Update(commandParameter, par =>
                {
                    var cabinetViewModel = commandParameter as CabinetPanelElementContext;
                    if (cabinetViewModel != null)
                    {
                        BodyLayerDataContext.Load("Cabinet");
                        //ElementDataContext.Reload();

                        ElementItems.Clear();
                        ElementItems.AddRange(Db.XGetObjectDisplayItems(cabinetViewModel.CurrentElement.Handle));
                    }
                });
            }
            catch (Exception)
            {
                //ignore
            }

        }

        public void ExecuteRunCommand(object commandParameter)
        {
            if (RunButtonText == "Draw")
            {
                CommandArgs commandArgs = new CommandArgs(this, CommandNames.AddCabinet, BodyElementDataContext);
                commandArgs.SendToExecute(cmdArgs =>
                {
                    if (!cmdArgs.CancelToken.IsCancellationRequested)
                        RunButtonText = "Update";
                });
            }
            else if (RunButtonText == "Update")
            {
                CommandArgs commandArgs = new CommandArgs(this, CommandNames.UpdateCabinet, BodyElementDataContext);
                commandArgs.SendToExecute();
            }
        }

        public bool CanExecuteCommand(object parameter)
        {
            return true; // !string.IsNullOrEmpty(SelectedText);
        }

        public Task<ICommandArgs> ExecuteCommand(ICommandArgs command)
        {
            //throw new NotImplementedException(); 
            return null;
        }
        #endregion "Commands"

        #region <Methods>
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

        //public override void LoadEvents()
        //{
        //    base.LoadEvents();

        //    RemoveEvents();
        //    ElementDataContext.PropertyChanged += OnDataContextPropertyChanged;
        //    ElementDataContext.CurrentCabinet.PropertyChanged += OnDataContextPropertyChanged;
        //    ColorDataContext.PropertyChanged += OnDataContextPropertyChanged;
        //}

        public override void RegisterEvents()
        {
            base.RegisterEvents();
            BodyElementDataContext.PropertyChanged += OnDataContextPropertyChanged;
            ColorDataContext.PropertyChanged += OnDataContextPropertyChanged;
        }

        public override void UnregisterEvents()
        {
            base.UnregisterEvents();
            BodyElementDataContext.PropertyChanged -= OnDataContextPropertyChanged;
            ColorDataContext.PropertyChanged -= OnDataContextPropertyChanged;
        }

        //public override void RemoveEvents()
        //{
        //    base.RemoveEvents();
        //    ElementDataContext.PropertyChanged -= OnDataContextPropertyChanged;
        //    ElementDataContext.CurrentCabinet.PropertyChanged -= OnDataContextPropertyChanged;
        //    ColorDataContext.PropertyChanged -= OnDataContextPropertyChanged;
        //}

        public void Deactive()
        {
            _appSettings.Save();
            UnregisterEvents();
        }

        #endregion "Methods"

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

            //SetObjectId(objectId);
        }

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

        public override void OnAcadEntityErased(object sender, EventArgs e)
        {
            ObjectIdItem item = ElementItems.FirstOrDefault(x => x.ObjectId == ((DBObject)sender).ObjectId);
            if (item != null)
            {
                ElementItems.Remove(item);
                if (!ElementItems.Any())
                    ExecuteAddCommand(null);
            }
        }

        public override void OnAcadEntityModified(object sender, EventArgs args)
        {
            if (((DBObject)sender).IsErased || ((DBObject)sender).IsModifiedGraphics) return;

            UiDispatcher.InvokeAsync(() =>
            {
                if (sender is Polyline)
                {
                    //AcadCabinet acadCabinet = pline.XGetXrecordObject<AcadCabinet>();
                    //acadCabinet.DateModified = DateTime.Now;
                    //pline.XSetXrecord(acadCabinet);

                    if (ElementItems == null || ElementItems.Count == 0) return;
                    var pline = (Polyline)sender;
                    ObjectIdItem item = ElementItems.FirstOrDefault(x => x.ObjectId == pline.ObjectId);
                    if (item != null)
                    {
                        item.DisplayName = $"Cabinet (vertex: {pline.NumberOfVertices}; length: {pline.Length:F4}";
                        ElementItems.SetItem(ElementItems.IndexOf(item), item);
                    }
                }
                else if (sender is DBText)
                {
                }
            });
        }

        public override void OnDataContextPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is CabinetPanelElementContext)
            {
                if (e.PropertyName == "CurrentCabinet")
                {
                    var name = BodyElementDataContext.CurrentElement.Title;
                    BodyElementDataContext.CabinetType = BodyElementDataContext.CabinetTypes.Get(BodyElementDataContext.CurrentElement.TypeCode);
                    BodyElementDataContext.CurrentElement.Title = name;
                }
                if (e.PropertyName == "CabinetType")
                {
                    BodyElementDataContext.CurrentElement.Update(BodyElementDataContext.CabinetType.TypeCode, ObjectState.Modified);
                    ColorDataContext.CurrentColor = ColorDataContext.Colors.FirstOrDefault(x => x.ColorIndex == BodyElementDataContext.CurrentElement.ColorIndex);
                    BodyLayerDataContext.CurrentLayer = BodyLayerDataContext.Layers.FirstOrDefault(x => x.LayerName == BodyElementDataContext.CurrentElement.LayerName);
                }
            }
            else if (sender is ColorViewModel && !ColorDataContext.IsReadOnly)
            {
                //if (e.PropertyName == "CurrentColor")
                //ElementDataContext.CurrentCabinet.ColorIndex = ColorDataContext.CurrentColor.ColorIndex;
            }
            else if (sender is IPaletteElement)
            {
                ((IPaletteElement)sender).ObjectState = ObjectState.Modified;
            }
        }

        public override event PropertyChangedEventHandler PropertyChanged;
        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            //base.OnPropertyChanged(propertyName);
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
