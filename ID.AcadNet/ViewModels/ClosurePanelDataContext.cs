using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Interactivity.InteractionRequest;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

using Intellidesk.AcadNet.Common.Core;
using Intellidesk.AcadNet.Common.Enums;
using Intellidesk.AcadNet.Common.Extensions;
using Intellidesk.AcadNet.Common.Interfaces;
using Intellidesk.AcadNet.Interfaces;
using Intellidesk.AcadNet.Services;
using Intellidesk.AcadNet.Services.Extentions;
using Intellidesk.Common.Enums;
using Intellidesk.Data.Models.Cad;
using Intellidesk.Data.Models.Entities;
using Intellidesk.Data.Repositories.Infrastructure;
using Intellidesk.Infrastructure;
using Intellidesk.Infrastructure.Core;
using Intellidesk.Infrastructure.Tasks;
using IntelliDesk.AcadNet.Model;

using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Closure = Intellidesk.Data.Models.Entities.Closure;
using ObjectIdDisplayItem = IntelliDesk.AcadNet.Model.ObjectIdDisplayItem;
using Exception = System.Exception;

namespace Intellidesk.AcadNet.ViewModels
{
    public class ClosurePanelDataContext : BaseViewModel, IClosurePanelContext
    {
        private SearchTextNotification _notification;
        private string _runButtonText;
        private bool _isEditMode;
        private string _header = "Entity";
        private string _selectedText;
        private List<string> _scaleFactors = new List<string>() { "1", "2", "3", "5", "7", "10" };
        private string _currentLayer = "All";
        private Closure _currentClosure;
        private List<string> _layers;
        private ObservableRangeCollection<ObjectIdDisplayItem> _elementItems = new ObservableRangeCollection<ObjectIdDisplayItem>();
        private ObjectIdDisplayItem _selectedKey = null;
        private ObjectIdDisplayItem _selectedItem;
        private double _progressBarMax;
        private double _progressBarValue;
        private double _progressStateValue;
        private bool _canPopulated = true;

        #region <Property>

        public ColorViewModel<Closure> ColorDataContext { get; }
        public bool ColorsReadOnly { get; } = true;
        public LayerViewModel<Closure> LayerDataContext { get; }
        public bool LayersReadOnly { get; } = true;
        public ClosureViewModel ClosureDataContext { get; }
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
                    this._notification = (SearchTextNotification) value;
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
        public string SelectedText
        {
            get { return _selectedText; }
            set
            {
                _selectedText = value;
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
        public Closure CurrentClosure
        {
            get { return _currentClosure; }
            set
            {
                _currentClosure = value;
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
                    CommandLine.Zoom(ent.ObjectId, CurrentZoomDisplayFactor);
                }
            }
        }
        public int CurrentZoomDisplayFactor
        {
            get { return Plugin.PluginSettings.ZoomDisplayFactor; }
            set
            {
                UiDispatcher.BeginInvoke(new Action(() =>
                {
                    Plugin.PluginSettings.ZoomDisplayFactor = value;
                    Plugin.RegisterInstance(Plugin.PluginSettings);

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
        public ClosurePanelDataContext()
        {
            Header = "Closure: ";
            RunButtonText = "Draw";

            ClosureDataContext = new ClosureViewModel(eClosureType.CL) { Parent = this };
            ColorDataContext = new ColorViewModel<Closure>(ClosureDataContext.CurrentClosure.ColorIndex) { Parent = this, IsReadOnly = true };
            LayerDataContext = new LayerViewModel<Closure>(ClosureDataContext.CurrentClosure.LayerName) { Parent = this };

            ProgressBarMinimum = 0;
            ProgressBarMaximum = 100;

            var layerService = Plugin.GetService<LayerService>();
            var list = layerService.GetAll().Select(x => x.Name).ToList();
            list.Insert(0, "All");
            Layers = list;
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
                var elementType = ClosureDataContext.ClosureType;
                ClosureDataContext.CurrentClosure = new AcadClosure(elementType.TypeCode);
                ColorDataContext.CurrentColor = ColorDataContext.Colors.FirstOrDefault(x => x.ColorIndex == elementType.ColorIndex);
                LayerDataContext.CurrentLayer = LayerDataContext.Layers.FirstOrDefault(x => x.LayerName == elementType.LayerName);
                RunButtonText = !string.IsNullOrEmpty(ClosureDataContext.CurrentClosure?.Handle) ? "Update" : "Draw";
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

                AcadClosure closure = element as AcadClosure;
                if (closure == null) return;

                RunButtonText = !string.IsNullOrEmpty(closure.Handle) ? "Update" : "Draw";

                if (!string.IsNullOrEmpty(closure.Handle))
                {
                    List<ObjectIdDisplayItem> displayObjectList = new List<ObjectIdDisplayItem>();
                    List<string> closureHandles = new List<string> { closure.Handle };

                    if (closure.Items.Length > 0)
                        closureHandles.AddRange(closure.Items);

                    Database db = Ed.Document.Database;
                    foreach (var handle in closureHandles)
                    {
                        DBObject dbObject = db.XGetObject(handle);
                        Type type = dbObject.XGetXDataObjectType();

                        if (type == typeof(AcadClosure))
                        {
                            AcadClosure acadClosure = dbObject.XGetXDataObject<AcadClosure>();
                            if (acadClosure != null) displayObjectList.Add(dbObject.XGetDisplayItem(acadClosure));
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

                ClosureDataContext.ClosureType = ClosureDataContext.ClosureTypes.FirstOrDefault(x => x.TypeCode == closure.TypeCode);
                ColorDataContext.CurrentColor = ColorDataContext.Colors.FirstOrDefault(x => x.ColorIndex == closure.ColorIndex);
                ClosureDataContext.CurrentClosure = closure;

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

        public void ExecuteRunCommand(object commandParameter)
        {
            if (RunButtonText == "Draw")
            {
                CommandArgs commandArgs = new CommandArgs(this, "PARTNERADDCLOSURE", ClosureDataContext);
                commandArgs.Run(cmdArgs =>
                {
                    if (!cmdArgs.CancellationToken.IsCancellationRequested)
                        RunButtonText = "Update";
                });
            }
            else if (RunButtonText == "Update")
            {
                var commandArgs = new CommandArgs(this, "IDUPDATECLOSURE", ClosureDataContext);
                commandArgs.Run();
            }
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
                UiDispatcher.Update(commandParameter, (Action<object>)(par =>
                {
                    var closureViewModel = commandParameter as ClosureViewModel;
                    if (closureViewModel != null)
                    {
                        LayerDataContext.Load();
                        ClosureDataContext.Reload();

                        ElementItems.Clear();
                        ElementItems.AddRange(Db.XGetObjectDisplayItems(closureViewModel.CurrentClosure.Handle));
                    }
                }));
            }
            catch (Exception ex)
            {
                //ignore
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

            ClosureDataContext.PropertyChanged += OnDataContextPropertyChanged;
            ColorDataContext.PropertyChanged += OnDataContextPropertyChanged;
            //ClosureDataContext.CurrentClosure.PropertyChanged -= OnDataContextPropertyChanged;
            //ClosureDataContext.CurrentClosure.PropertyChanged += OnDataContextPropertyChanged;
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
                acadApp.DocumentManager.MdiActiveDocument.Editor.TurnForcedPickOn();
            }
            ClosureDataContext.PropertyChanged -= OnDataContextPropertyChanged;
            ColorDataContext.PropertyChanged -= OnDataContextPropertyChanged;
            Draw.ObjectErased -= OnAcadEntityErased;
            Draw.ObjectModified -= OnAcadEntityModified;
            //ClosureDataContext.CurrentClosure.PropertyChanged -= OnDataContextPropertyChanged;
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

        public void Deactive()
        {
            Plugin.PluginSettings.Save();
            RemoveEvents();
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
            //var objIds = Draw.Closure(closureDataContext.CurrentClosure).ToList();
            //ClosurePanelDataContext sender = commandArgs.Sender as ClosurePanelDataContext;

            if (sender is Polyline)
            {
                var pline = (Polyline)sender;
                string xRecordData = pline.XGetXrecordValue(DxfCodeExt.DataObject).ToString();
                AcadClosure commnadParameter = JsonConvert.DeserializeObject<AcadClosure>(xRecordData);
                pline.XSetXrecord(Db.NamedObjectsDictionaryId, commnadParameter);

                ObjectIdDisplayItem item = ElementItems.FirstOrDefault(x => x.ObjectId == pline.ObjectId);
                if (item != null)
                {
                    item.DisplayName =
                        $"Closure (vertex count: {pline.NumberOfVertices}; length: {Math.Round(pline.GetDistanceAtParameter(pline.EndParam) - pline.GetDistanceAtParameter(pline.StartParam), 4)}";

                    var temp = ElementItems.ToList();
                    ElementItems.Clear();
                    ElementItems.AddRange(temp);
                }
            }
            else if (sender is DBText)
            {
            }
        }

        private void OnDataContextPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is ClosureViewModel)
            {
                if (e.PropertyName == "ClosureType")
                {
                    ColorDataContext.CurrentColor = ColorDataContext.Colors.FirstOrDefault(x => x.ColorIndex == ClosureDataContext.ClosureType.ColorIndex);
                    LayerDataContext.CurrentLayer = LayerDataContext.Layers.FirstOrDefault(x => x.LayerName == ClosureDataContext.ClosureType.LayerName);
                }
            }
            else if (sender is ColorViewModel<Closure> && !ColorDataContext.IsReadOnly)
            {
                if (e.PropertyName == "CurrentColor")
                    ClosureDataContext.CurrentClosure.ColorIndex = ColorDataContext.CurrentColor.ColorIndex;
            }
            else if (sender is IElementDefinition)
            {
                ((IElementDefinition)sender).ObjectState = ObjectState.Modified;
            }
        }

        void OnImpliedSelectionChanged(object sender, EventArgs e)
        {
        }

        //public override event PropertyChangedEventHandler PropertyChanged;
        //protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        //{
        //    //base.OnPropertyChanged(propertyName);
        //    PropertyChangedEventHandler handler = PropertyChanged;
        //    handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //}

        #endregion
    }
}
