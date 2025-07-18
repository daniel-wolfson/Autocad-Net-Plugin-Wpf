using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Intellidesk.AcadNet.Common;
using Intellidesk.Common.Enums;
using Intellidesk.AcadNet.Common.Core;
using Intellidesk.AcadNet.Common.Extensions;
using Intellidesk.AcadNet.Common.Interfaces;
using Intellidesk.AcadNet.Interfaces;
using Intellidesk.AcadNet.Services;
using Intellidesk.AcadNet.Services.Extentions;
using Intellidesk.Data.Models.Cad;
using Intellidesk.Data.Models.Entities;
using Intellidesk.Data.Repositories.Infrastructure;
using Intellidesk.Infrastructure;
using Intellidesk.Infrastructure.Core;
using Intellidesk.Infrastructure.Tasks;
using IntelliDesk.AcadNet.Model;
using Prism.Commands;
using Prism.Interactivity.InteractionRequest;

using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using DelegateCommand = Prism.Commands.DelegateCommand;
using ObjectIdDisplayItem = IntelliDesk.AcadNet.Model.ObjectIdDisplayItem;
using Exception = System.Exception;

namespace Intellidesk.AcadNet.ViewModels
{
    public class CabinetPanelDataContext : BaseViewModel, ICabinetPanelContext
    {
        private SearchTextNotification _notification;
        private readonly IPluginSettings _appSettings;
        public static event AcadEntitySelectedEventHandler EntitySelected;
        public static int indexPickSelectionAdded = 0;
        private string _runButtonText;
        private string _header = "Entity";
        private string _currentLayer = "All";
        private Cabinet _currentCabinet;
        private List<string> _scaleFactors = new List<string>() { "1", "2", "3", "5", "7", "10" };
        private List<string> _layers;
        private ObservableRangeCollection<ObjectIdDisplayItem> _elementItems = new ObservableRangeCollection<ObjectIdDisplayItem>();
        private ObjectIdDisplayItem _selectedKey = null;
        private ObjectIdDisplayItem _selectedItem;
        private double _progressBarMax;
        private double _progressBarValue;
        private double _progressStateValue;
        private bool _canPopulated = true;

        #region <Properties> 
        public bool ColorsReadOnly { get; } = true;
        public ColorViewModel<Cabinet> ColorDataContext { get; }
        public LayerViewModel<Cabinet> LayerDataContext { get; }
        public bool LayersReadOnly { get; } = true;
        public CabinetViewModel CabinetDataContext { get; }
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
        public Action FinishInteraction { get; set; }
        public INotification Notification
        {
            get { return this._notification; }
            set
            {
                if (value is SearchTextNotification)
                {
                    // To keep the code simple, this is the only property where we are raising the PropertyChanged event,
                    // as it's required to update the bindings when this property is populated.
                    // Usually you would want to raise this event for other properties too.
                    this._notification = value as SearchTextNotification;
                    this.OnPropertyChanged();
                }
            }
        }
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
        public List<string> ScaleFactors
        {
            get { return _scaleFactors; }
            set
            {
                _scaleFactors = value;
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
        public ObservableRangeCollection<ObjectIdDisplayItem> ElementItems
        {
            get { return _elementItems; }
            set
            {
                _elementItems = value;
                OnPropertyChanged();
            }
        }
        public ObjectIdDisplayItem SelectedKey
        {
            get { return this._selectedKey; }
            set
            {
                this._selectedKey = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged("SelectedValue");
            }
        }
        public ObjectIdDisplayItem SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                Entity ent;
                if (_selectedItem != null)
                {
                    ent = _selectedItem.ObjectId.XGetEntity();
                    ent?.Unhighlight();
                }

                if (value != null)
                {
                    _selectedItem = value;
                    CommandLine.Zoom(_selectedItem.ObjectId, CurrentZoomDisplayFactor);
                    ent = _selectedItem.ObjectId.XGetEntity();
                    ent.Highlight();
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
                    Plugin.RegisterInstance<IPluginSettings>(_appSettings);

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
        public UserSetting CurrentUserSetting
        {
            get { return _currentUserSetting; }
            set
            {
                _currentUserSetting = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region <ctor>
        public CabinetPanelDataContext()
        {
            Header = "Cabinet: ";
            RunButtonText = "Draw";

            CabinetDataContext = new CabinetViewModel(eCabinetType.AGC) { Parent = this };
            ColorDataContext = new ColorViewModel<Cabinet>(CabinetDataContext.CurrentCabinet.ColorIndex) { Parent = this, IsReadOnly = true };
            LayerDataContext = new LayerViewModel<Cabinet>(CabinetDataContext.CurrentCabinet.LayerName) { Parent = this };

            ProgressBarMinimum = 0;
            ProgressBarMaximum = 100;

            var layerService = Plugin.GetService<LayerService>();
            var list = layerService.GetAll().Select(x => x.Name).ToList();
            list.Insert(0, "All");
            Layers = list;

            _appSettings = Plugin.PluginSettings;
        }
        #endregion

        #region <Commands>

        private ICommand _addCommand, _closeCommand, _editCommand, _getLocationCommand,
                         _selectSetCommand, _refreshCommand, _runCommand, _undoCommand;

        public ICommand RunCommand => _runCommand ?? (_runCommand = new DelegateCommand<object>(ExecuteRunCommand, CanExecuteCommand));
        public ICommand EditCommand => _editCommand ?? (_editCommand = new DelegateCommand<object>(ExecuteEditCommand, CanExecuteCommand));
        public ICommand StopCommand { get; set; }
        public ICommand SelectSetCommand => _selectSetCommand ?? (_selectSetCommand = new DelegateCommand<object>(ExecuteSelectSetCommand));
        public ICommand AddCommand => _addCommand ?? (_addCommand = new DelegateCommand<object>(ExecuteAddCommand));
        public ICommand UndoCommand => _undoCommand ?? (_undoCommand = new DelegateCommand<object>(ExecuteResetCommand));
        public ICommand GetLocationCommand => _getLocationCommand ?? (_getLocationCommand = new DelegateCommand<object>(ExecuteGetLocationCommand));
        public ICommand RefreshCommand => _refreshCommand ?? (_refreshCommand = new DelegateCommand<object>(ExecuteRefresh));
        public ICommand CloseCommand => _closeCommand ?? (_closeCommand = new DelegateCommand<object>(ExecuteRefresh));

        public void ExecuteClose(object commandParameter)
        {
            UiDispatcher.BeginInvoke(new Action(() =>
            {
                Mouse.OverrideCursor = Cursors.Wait;
                Thread.Sleep(500);
                Mouse.OverrideCursor = null;
            }));
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
                eCabinetType eCabinetType = CabinetDataContext.CabinetType.TypeCode;
                DataInfoAttribute attr = eCabinetType.GetMetaDataInfo();

                CabinetDataContext.CurrentCabinet = new AcadCabinet(eCabinetType);
                ColorDataContext.CurrentColor = ColorDataContext.Colors
                    .FirstOrDefault(x => x.ColorIndex == attr.ColorIndex) ?? new AcadColor(attr.ColorIndex);
                LayerDataContext.CurrentLayer = LayerDataContext.Layers.FirstOrDefault(x => x.LayerName == attr.LayerName);
                RunButtonText = !string.IsNullOrEmpty(CabinetDataContext.CurrentCabinet?.Handle) ? "Update" : "Draw";
                ElementItems.Clear();
            });
        }

        public void ExecuteEditCommand(object commandParameter)
        {
            UiDispatcher.UpdateAsync(commandParameter, element =>
            {
                //ExecuteResetCommand(null);
                //Mouse.OverrideCursor = Cursors.Wait;
                //ExecuteResetCommand(10);
                //ProgressBarValue += 5;
                //ProgressStateValue += 5;

                AcadCabinet cabinet = element as AcadCabinet;
                if (cabinet == null) return;

                RunButtonText = !string.IsNullOrEmpty(cabinet.Handle) ? "Update" : "Draw";

                if (!string.IsNullOrEmpty(cabinet.Handle))
                {
                    List<ObjectIdDisplayItem> displayObjectList = new List<ObjectIdDisplayItem>();
                    List<string> cabinetHandles = new List<string> { cabinet.Handle };

                    if (cabinet.Items.Length > 0) cabinetHandles.AddRange(cabinet.Items);

                    Database db = Ed.Document.Database;
                    foreach (var handle in cabinetHandles)
                    {
                        DBObject dbObject = db.XGetObject(handle);
                        Type type = dbObject.XGetXDataObjectType();

                        if (type == typeof(AcadCabinet))
                        {
                            AcadCabinet acadCabinet = dbObject.XGetXDataObject<AcadCabinet>();
                            if (acadCabinet != null) displayObjectList.Add(dbObject.XGetDisplayItem(acadCabinet));
                        }
                        else if (type == typeof(AcadTitle))
                        {
                            AcadTitle acadTitle = dbObject.XGetXDataObject<AcadTitle>();
                            if (acadTitle != null) displayObjectList.Add(acadTitle.GetListItem(dbObject));
                        }
                    }
                    ElementItems.Clear();
                    ElementItems.AddRange(displayObjectList);
                }

                CabinetDataContext.CabinetType = CabinetDataContext.CabinetTypes.FirstOrDefault(x => x.TypeCode == cabinet.TypeCode);
                ColorDataContext.CurrentColor = ColorDataContext.Colors.FirstOrDefault(x => x.ColorIndex == cabinet.ColorIndex);
                CabinetDataContext.CurrentCabinet = cabinet;
                //ProgressBarValue += 5;
                //ProgressStateValue += 5;
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

        public void ExecuteRefresh(object commandParameter)
        {
            try
            {
                UiDispatcher.Update(commandParameter, par =>
                {
                    var cabinetViewModel = commandParameter as CabinetViewModel;
                    if (cabinetViewModel != null)
                    {
                        LayerDataContext.Load();
                        //CabinetDataContext.Reload();

                        ElementItems.Clear();
                        ElementItems.AddRange(Db.XGetObjectDisplayItems(cabinetViewModel.CurrentCabinet.Handle));
                    }
                });
            }
            catch (Exception ex)
            {
                //ignore
            }

        }

        public void ExecuteRunCommand(object commandParameter)
        {
            if (RunButtonText == "Draw")
            {
                CommandArgs commandArgs = new CommandArgs(this, "PARTNERADDCABINET", CabinetDataContext);
                commandArgs.Run(cmdArgs =>
                {
                    if (!cmdArgs.CancellationToken.IsCancellationRequested)
                        RunButtonText = "Update";
                });
            }
            else if (RunButtonText == "Update")
            {
                CommandArgs commandArgs = new CommandArgs(this, "IDUPDATECABINET", CabinetDataContext);
                commandArgs.Run();
            }
        }

        public bool CanExecuteCommand(object parameter)
        {
            return true; // !string.IsNullOrEmpty(SelectedText);
        }

        #endregion "Commands"

        #region <Methods>

        public void LoadEvents()
        {
            RemoveEvents();
            if (acadApp.DocumentManager.MdiActiveDocument != null)
            {
                acadApp.DocumentManager.DocumentActivated += OnDocumentActivated;
                acadApp.DocumentManager.DocumentToBeDestroyed += OnDocumentToBeDestroyed;
                acadApp.DocumentManager.MdiActiveDocument.ImpliedSelectionChanged += OnImpliedSelectionChanged;
                acadApp.DocumentManager.MdiActiveDocument.Editor.TurnForcedPickOn();
            }
            CabinetDataContext.PropertyChanged += OnDataContextPropertyChanged;
            CabinetDataContext.CurrentCabinet.PropertyChanged += OnDataContextPropertyChanged;
            ColorDataContext.PropertyChanged += OnDataContextPropertyChanged;
            Draw.ObjectErased += OnAcadEntityErased;
            Draw.ObjectModified += OnAcadEntityModified;
        }

        public void RemoveEvents()
        {
            if (acadApp.DocumentManager.MdiActiveDocument != null)
            {
                acadApp.DocumentManager.DocumentActivated -= OnDocumentActivated;
                acadApp.DocumentManager.DocumentToBeDestroyed -= OnDocumentToBeDestroyed;
                acadApp.DocumentManager.MdiActiveDocument.ImpliedSelectionChanged -= OnImpliedSelectionChanged;
                acadApp.DocumentManager.MdiActiveDocument.Editor.TurnForcedPickOff();
            }
            CabinetDataContext.PropertyChanged -= OnDataContextPropertyChanged;
            CabinetDataContext.CurrentCabinet.PropertyChanged -= OnDataContextPropertyChanged;
            ColorDataContext.PropertyChanged -= OnDataContextPropertyChanged;
            Draw.ObjectErased -= OnAcadEntityErased;
            Draw.ObjectModified -= OnAcadEntityModified;
        }

        public void SetObjectText(string text)
        {
            //typeTextBox.Text = text;
        }

        public void SetObjectId(DBObject obj)
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

        public void Deactive()
        {
            _appSettings.Save();
            RemoveEvents();
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
            //var layout = ProjectExplorerPanel.LayoutItems.
            //    FirstOrDefault(X => X.LayoutId == ProjectExplorerPanel.CurrentLayout.LayoutId && docName.Contains(X.CADFileName.ToLower()));
            //if (layout != null) // && !layout.FSA)
            //{
            //    //var idx = ProjectExplorerPanel.LayoutItems.IndexOf(layout);
            //    //var item = ((Layout)layout).CloneTo<Layout>();
            //    //item.FSA = true;
            //    //ProjectExplorerPanel.LayoutItems[idx] = item;
            //    //ProjectExplorerPanel.CurrentLayout = ProjectExplorerPanel.LayoutItems[idx];
            //}
        }

        public void OnDocumentActivated(object sender, DocumentCollectionEventArgs e)
        {
            try
            {
                ExecuteResetCommand(0);
                ExecuteRefresh(null);
                LoadEvents();
            }
            catch (Exception)
            {
                // ignore
            }

        }

        public void OnDocumentToBeDestroyed(object sender, DocumentCollectionEventArgs e)
        {
            RemoveEvents();
        }

        Task<CommandArgs> IPanelDataContext.ExecuteCommand(CommandArgs command)
        {
            //throw new NotImplementedException(); 
            return null;
        }

        public void OnAcadEntityErased(object sender, EventArgs e)
        {
            ObjectIdDisplayItem item = ElementItems.FirstOrDefault(x => x.ObjectId == ((DBObject)sender).ObjectId);
            ElementItems.Remove(item);

            //ExecuteAddCommand(null);
        }

        public void OnAcadEntityModified(object sender, EventArgs args)
        {
            if (((DBObject)sender).IsErased || ((DBObject)sender).IsModifiedGraphics) return;

            UiDispatcher.UpdateAsync(sender, element =>
            {
                if (element is Polyline)
                {
                    //AcadCabinet acadCabinet = pline.XGetXrecordObject<AcadCabinet>();
                    //acadCabinet.DateModified = DateTime.Now;
                    //pline.XSetXrecord(acadCabinet);

                    if (ElementItems == null || ElementItems.Count == 0) return;
                    var pline = (Polyline)element;
                    ObjectIdDisplayItem item = ElementItems.FirstOrDefault(x => x.ObjectId == pline.ObjectId);
                    if (item != null)
                    {
                        item.DisplayName = $"Cabinet (vertex count: {pline.NumberOfVertices}; length: {pline.Length:F4}";
                        ElementItems.SetItem(ElementItems.IndexOf(item), item);
                    }
                }
                else if (element is DBText)
                {
                }
            });
        }

        void OnSelectionAdded(object sender, SelectionAddedEventArgs e)
        {
            ObjectId[] addedIds = e.AddedObjects.GetObjectIds();
            for (int i = 0; i < addedIds.Length; i++)
            {
                ObjectId oid = addedIds[i];

                //if (IsInList(oid.ObjectClass.DxfName))
                //{
                //    e.Remove(i);
                //}
            }
        }

        private void OnDataContextPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is CabinetViewModel)
            {
                if (e.PropertyName == "CabinetType")
                {
                    ColorDataContext.CurrentColor = ColorDataContext.Colors.FirstOrDefault(x => x.ColorIndex == CabinetDataContext.CabinetType.ColorIndex);
                    LayerDataContext.CurrentLayer = LayerDataContext.Layers.FirstOrDefault(x => x.LayerName == CabinetDataContext.CabinetType.LayerName);
                }
            }
            else if (sender is ColorViewModel<Cabinet> && !ColorDataContext.IsReadOnly)
            {
                if (e.PropertyName == "CurrentColor")
                    CabinetDataContext.CurrentCabinet.ColorIndex = ColorDataContext.CurrentColor.ColorIndex;
            }
            else if (sender is IElementDefinition)
            {
                ((IElementDefinition)sender).ObjectState = ObjectState.Modified;
            }
        }

        void OnImpliedSelectionChanged(object sender, EventArgs e)
        {
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
