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
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Intellidesk.AcadNet.ViewModels
{
    public class CablePanelContext : BaseViewModel, ICablePanelContext
    {
        private string _header = "Entity";
        private string _runButtonText;
        private string _currentLayer = "All";

        private List<string> _layers;
        private ObservableRangeCollection<ObjectIdItem> _elementItems = new ObservableRangeCollection<ObjectIdItem>();
        private ObjectIdItem _selectedKey;
        private ObjectIdItem _selectedItem;
        private double _progressBarMax;
        private double _progressBarValue;
        private double _progressStateValue;
        private bool _canPopulated = true;

        #region <Properties>
        public bool LayersReadOnly { get; } = true;
        public bool ColorsReadOnly { get; } = true;
        public Dictionary<short, string> ElementTypes { get; set; }
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
        //public AcadCable CurrentCable
        //{
        //    get { return _currentCable; }
        //    set
        //    {
        //        _currentCable = value;
        //        OnPropertyChanged();
        //    }
        //}

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
            get { return this._selectedKey; }
            set
            {
                this._selectedKey = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged("SelectedValue");
            }
        }
        public ObjectIdItem SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (value != null)
                {
                    Entity ent;
                    if (_selectedItem != null)
                    {
                        ent = _selectedItem.ObjectId.XGetEntity();
                        ent?.Unhighlight();
                    }

                    _selectedItem = value;
                    if (this.IsLoaded)
                        CommandLine.Zoom(_selectedItem.ObjectId, CurrentZoomDisplayFactor);
                    ent = _selectedItem.ObjectId.XGetEntity();
                    ent.Highlight();
                    CommandContext.CurrentEntity = ent;
                    Thread.Sleep(200);
                }
                else
                {
                    _selectedItem = null;
                }
            }
        }
        public int CurrentZoomDisplayFactor
        {
            get { return PluginSettings.ZoomDisplayFactor; }
            set
            {
                UiDispatcher.BeginInvoke(new Action(() =>
                {
                    PluginSettings.ZoomDisplayFactor = value;
                    Plugin.RegisterInstance<IPluginSettings>(PluginSettings);

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

        #region <Contexts>
        public ColorViewModel ColorDataContext { get; }
        public LayerViewModel BodyLayerDataContext { get; }
        public CablePanelElementContext ElementDataContext { get; }
        public ISearchViewModel SearchViewModel { get; }
        #endregion

        #region <ctor>
        public CablePanelContext()
        {
            Header = "Cable: ";
            RunButtonText = "Draw";

            IsSelectionEnabled = true;
            IsPointMonitorEnabled = true;
            IsEntityModifyEnabled = true;

            ElementDataContext = new CablePanelElementContext(eCableType.Cable12x1x12) { Parent = this };
            ColorDataContext = new ColorViewModel(typeof(eCableType), (short)ElementDataContext.CurrentElement.ColorIndex) { Parent = this, IsReadOnly = true };
            BodyLayerDataContext = new LayerViewModel(typeof(Cable).Name,
                ElementDataContext.CurrentElement.LayerName)
            { Parent = this };

            ElementDataContext.CurrentElement = new AcadCable(ElementDataContext.CableType.TypeCode);
            ColorDataContext.CurrentColor = ColorDataContext.Colors.FirstOrDefault(x => x.ColorIndex == ElementDataContext.CableType.ColorIndex);
            BodyLayerDataContext.CurrentLayer = BodyLayerDataContext.Layers.FirstOrDefault(x => x.LayerName == ElementDataContext.CableType.LayerName);

            SearchViewModel = new SearchElementViewModel { Parent = this };
            ProgressBarMinimum = 0;
            ProgressBarMaximum = 100;

            var layerService = Plugin.GetService<LayerService>();
            var list = layerService.GetAll().Select(x => x.LayerName).ToList();
            list.Insert(0, "All");
            Layers = list;

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
            commandLine.SendToExecute(CommandNames.CablePanelRemove);
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
                ElementDataContext.CurrentElement = new AcadCable(ElementDataContext.CableType.TypeCode);
                RunButtonText = !string.IsNullOrEmpty(ElementDataContext.CurrentElement?.Handle) ? "Update" : "Draw";
                ElementItems.Clear();
            });
        }

        public void ExecuteAddTitleCommand(object sender)
        {
            var commandArgs = new CommandArgs(this, CommandNames.AddTitleCable, ElementDataContext);
            commandArgs.SendToExecute();
        }

        public void ExecuteEditCommand(object commandParameter)
        {
            UiDispatcher.InvokeAsync(() =>
            {
                AcadCable cable = commandParameter as AcadCable;
                if (cable == null) return;

                RunButtonText = !string.IsNullOrEmpty(cable.Handle) ? "Update" : "Draw";
                ElementItems.Clear();

                if (!string.IsNullOrEmpty(cable.Handle))
                {
                    List<ObjectIdItem> displayObjectList = new List<ObjectIdItem>();
                    List<string> cableHandles = new List<string> { cable.Handle };

                    if (cable.Items.Length > 0)
                        cableHandles.AddRange(cable.Items);

                    Database db = Ed.Document.Database;
                    foreach (var handle in cableHandles)
                    {
                        DBObject dbObject = db.XGetObject(handle);
                        if (dbObject == null) continue;

                        Type type = dbObject.XGetXDataObjectType();
                        if (type == typeof(AcadCable))
                        {
                            AcadCable acadCable = dbObject.XGetXDataObject<AcadCable>();
                            if (acadCable != null)
                                displayObjectList.Add(DBObjectExtensions.XGetDisplayItem(dbObject, acadCable));
                        }
                        else if (type == typeof(AcadTitle))
                        {
                            AcadTitle acadTitle = dbObject.XGetXDataObject<AcadTitle>();
                            if (acadTitle != null)
                                displayObjectList.Add(acadTitle.GetListItem(dbObject));
                        }
                    }
                    ElementItems.AddRange(displayObjectList);
                }

                ElementDataContext.CurrentElement = cable;

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
                    //Tuple<object, object> tupleCommandParameter = commandParameter as Tuple<object, object>;
                    var cableViewModel = commandParameter as CablePanelElementContext;
                    //var layerService = AcadNetManager.Container.Resolve<LayerService>();
                    //var list = layerService.GetAll().Select(x => x.Name).ToList();
                    //list.Insert(0, "All");
                    //Layers = list;
                    //CurrentElement = "All";

                    if (cableViewModel != null)
                    {
                        BodyLayerDataContext.Load("Cable");
                        ElementDataContext.Reload();

                        ElementItems.Clear();
                        ElementItems.AddRange(Db.XGetObjectDisplayItems(cableViewModel.CurrentElement.Handle));
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
            IsCanceled = false;

            //ExecuteResetCommand(null);
            //ProgressBarValue += 5;
            //ProgressStateValue += 5;
            //ExecuteResetCommand(10);

            if (RunButtonText == "Draw")
            {
                var commandArgs = new CommandArgs(this, CommandNames.AddCable, ElementDataContext);
                commandArgs.SendToExecute(cmdArgs =>
                {
                    if (!cmdArgs.CancelToken.IsCancellationRequested)
                    {
                        RunButtonText = "Update";
                        OnPropertyChanged("ElementDataContext");
                    }

                });
            }
            else if (RunButtonText == "Update")
            {
                var commandArgs = new CommandArgs(this, CommandNames.UpdateCable, ElementDataContext);
                commandArgs.SendToExecute();
            }
        }

        public bool CanExecuteCommand(object parameter)
        {
            return true; // !string.IsNullOrEmpty(SelectedText);
        }

        #endregion <Commands>

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
                Document doc = Application.DocumentManager.MdiActiveDocument;
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

        private void DataContextInit()
        {
            //ElementDataContext.CableType = ElementDataContext.CableTypes.FirstOrDefault(x => x.TypeCode == ElementDataContext.CurrentCable.TypeCode);
            //ColorDataContext.CurrentColor = ColorDataContext.Colors.FirstOrDefault(x => x.ColorIndex == ElementDataContext.CurrentCable.ColorIndex);
            //LayerDataContext.CurrentLayer = LayerDataContext.Layers.FirstOrDefault(x => x.LayerName == ElementDataContext.CurrentCable.LayerName);
        }

        public override void RegisterEvents()
        {
            base.RegisterEvents();
            ElementDataContext.PropertyChanged += OnDataContextPropertyChanged;
            ColorDataContext.PropertyChanged += OnDataContextPropertyChanged;
        }

        public override void UnregisterEvents()
        {
            base.UnregisterEvents();
            ElementDataContext.PropertyChanged -= OnDataContextPropertyChanged;
            ColorDataContext.PropertyChanged -= OnDataContextPropertyChanged;
        }

        #endregion <Methods>

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

        public void Deactive()
        {
            PluginSettings.Save();
            UnregisterEvents();
        }

        public Task<ICommandArgs> ExecuteCommand(ICommandArgs command)
        {
            //throw new NotImplementedException(); 
            return null;
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
                    //AcadCable acadCable = pline.XGetXrecordObject<AcadCable>();
                    //acadCable.DateModified = DateTime.Now;
                    //pline.XSetXrecord(acadCable);

                    if (ElementItems == null || ElementItems.Count == 0) return;
                    var pline = (Polyline)sender;
                    ObjectIdItem item = ElementItems.FirstOrDefault(x => x.ObjectId == pline.ObjectId);
                    if (item != null)
                    {
                        item.DisplayName = $"Cable (vertex: {pline.NumberOfVertices}; length: {pline.Length:F4}";
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
            if (sender is CablePanelElementContext)
            {
                if (e.PropertyName == "CurrentCable")
                {
                    var name = ElementDataContext.CurrentElement.Title;
                    ElementDataContext.CableType = ElementDataContext.CableTypes.Get(ElementDataContext.CurrentElement.TypeCode);
                    ElementDataContext.CurrentElement.Title = name;
                }
                if (e.PropertyName == "CableType")
                {
                    ElementDataContext.CurrentElement.Update(ElementDataContext.CableType.TypeCode, ObjectState.Modified);
                    ColorDataContext.CurrentColor = ColorDataContext.Colors.FirstOrDefault(x => x.ColorIndex == ElementDataContext.CurrentElement.ColorIndex);
                    BodyLayerDataContext.CurrentLayer = BodyLayerDataContext.Layers.FirstOrDefault(x => x.LayerName == ElementDataContext.CurrentElement.LayerName);
                }
            }
            else if (sender is ColorViewModel && !ColorDataContext.IsReadOnly)
            {
                //if (e.PropertyName == "CurrentColor")
                //    ElementDataContext.CurrentCable.ColorIndex = ColorDataContext.CurrentColor.ColorIndex;
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
