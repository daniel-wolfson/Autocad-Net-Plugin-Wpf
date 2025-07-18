using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using ID.Infrastructure;
using ID.Infrastructure.Commands;
using ID.Infrastructure.Enums;
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
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Input;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Closure = Intellidesk.Data.Models.Entities.Closure;
using Exception = System.Exception;
using ObjectIdItem = Intellidesk.AcadNet.Common.Models.ObjectIdItem;

namespace Intellidesk.AcadNet.ViewModels
{
    public class ClosurePanelContext : BaseViewModel, IClosurePanelContext
    {
        #region <fields>
        private readonly IDrawService _draw = Plugin.GetService<IDrawService>();
        private string _runButtonText;
        private bool _isEditMode;
        private string _header = "Entity";
        private string _selectedText;
        private string _currentLayer = "All";
        private Closure _currentClosure;
        private List<string> _layers;
        private ObservableRangeCollection<ObjectIdItem> _elementItems = new ObservableRangeCollection<ObjectIdItem>();
        private ObjectIdItem _selectedKey = null;
        private ObjectIdItem _selectedItem;
        private double _progressBarMax;
        private double _progressBarValue;
        private double _progressStateValue;
        private bool _canPopulated = true;
        #endregion <fields>

        #region <Property>
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
        public ClosurePanelElementContext BodyElementDataContext { get; }
        public ISearchViewModel SearchViewModel { get; }
        #endregion

        #region <ctor>
        public ClosurePanelContext()
        {
            Header = "Closure: ";
            RunButtonText = "Draw";

            IsSelectionEnabled = true;
            IsPointMonitorEnabled = true;
            IsEntityModifyEnabled = true;

            BodyElementDataContext = new ClosurePanelElementContext(eClosureType.Cl) { Parent = this };
            ColorDataContext = new ColorViewModel(typeof(eClosureType), (short)BodyElementDataContext.CurrentElement.ColorIndex) { Parent = this, IsReadOnly = true };
            BodyLayerDataContext = new LayerViewModel(typeof(Closure).Name,
                BodyElementDataContext.CurrentElement.LayerName)
            { Parent = this };

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
            commandLine.SendToExecute(CommandNames.ClosurePanelRemove);
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
                BodyElementDataContext.CurrentElement = new AcadClosure(BodyElementDataContext.ClosureType.TypeCode);
                RunButtonText = !string.IsNullOrEmpty(BodyElementDataContext.CurrentElement?.Handle) ? "Update" : "Draw";
                ElementItems.Clear();
            });
        }

        public void ExecuteAddTitleCommand(object sender)
        {
            var commandArgs = new CommandArgs(this, CommandNames.AddTitleClosure, BodyElementDataContext);
            commandArgs.SendToExecute();
        }

        public void ExecuteEditCommand(object commandParameter)
        {
            UiDispatcher.InvokeAsync(() =>
            {
                AcadClosure closure = commandParameter as AcadClosure;
                if (closure == null) return;

                RunButtonText = !string.IsNullOrEmpty(closure.Handle) ? "Update" : "Draw";
                ElementItems.Clear();

                if (!string.IsNullOrEmpty(closure.Handle))
                {
                    List<ObjectIdItem> displayObjectList = new List<ObjectIdItem>();
                    List<string> closureHandles = new List<string> { closure.Handle };

                    if (closure.Items.Length > 0)
                        closureHandles.AddRange(closure.Items);

                    Database db = Ed.Document.Database;
                    foreach (var handle in closureHandles)
                    {
                        DBObject dbObject = db.XGetObject(handle);
                        if (dbObject == null) continue;

                        Type type = dbObject.XGetXDataObjectType();
                        if (type == typeof(AcadClosure))
                        {
                            AcadClosure acadClosure = dbObject.XGetXDataObject<AcadClosure>();
                            if (acadClosure != null) displayObjectList.Add(DBObjectExtensions.XGetDisplayItem(dbObject, acadClosure));
                        }
                        else if (type == typeof(AcadTitle))
                        {
                            AcadTitle acadTitle = dbObject.XGetXDataObject<AcadTitle>();
                            if (acadTitle != null) displayObjectList.Add(acadTitle.GetListItem(dbObject));
                        }
                    }
                    ElementItems.AddRange(displayObjectList);
                }

                BodyElementDataContext.CurrentElement = closure;

                //LoadEvents();
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
                CommandArgs commandArgs = new CommandArgs(this, CommandNames.AddClosure, BodyElementDataContext);
                commandArgs.SendToExecute(cmdArgs =>
                {
                    if (!cmdArgs.CancelToken.IsCancellationRequested)
                        RunButtonText = "Update";
                });
            }
            else if (RunButtonText == "Update")
            {
                var commandArgs = new CommandArgs(this, CommandNames.UpdateClosure, BodyElementDataContext);
                commandArgs.SendToExecute();
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

        public void ExecuteRefreshCommand(object commandParameter)
        {
            try
            {
                UiDispatcher.Update(commandParameter, par =>
                {
                    var closureViewModel = commandParameter as ClosurePanelElementContext;
                    if (closureViewModel != null)
                    {
                        BodyLayerDataContext.Load("Closure");
                        BodyElementDataContext.Reload();

                        ElementItems.Clear();
                        ElementItems.AddRange(Db.XGetObjectDisplayItems(closureViewModel.CurrentElement.Handle));
                    }
                });
            }
            catch (Exception)
            {
                //ignore
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
                    ExecuteAddCommand(null);
            }
        }

        public override void OnAcadEntityModified(object sender, EventArgs args)
        {
            //var objIds = Draw.Closure(closureDataContext.CurrentClosure).ToList();
            //ClosurePanelDataContext sender = commandArgs.Sender as ClosurePanelDataContext;

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
                            //AcadClosure commnadParameter = JsonConvert.DeserializeObject<AcadClosure>(xRecordData);
                            AcadClosure commnadParameter = new JavaScriptSerializer().Deserialize<AcadClosure>(xRecordData.ToString());

                            pline.XSetXrecord(Db.NamedObjectsDictionaryId, commnadParameter);

                            ObjectIdItem item = ElementItems.FirstOrDefault(x => x.ObjectId == pline.ObjectId);
                            if (item != null)
                            {
                                item.DisplayName =
                                    $"Closure (vertex: {pline.NumberOfVertices}; length: {Math.Round(pline.GetDistanceAtParameter(pline.EndParam) - pline.GetDistanceAtParameter(pline.StartParam), 4)}";

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
            if (sender is ClosurePanelElementContext)
            {
                if (e.PropertyName == "CurrentClosure")
                {
                    var name = BodyElementDataContext.CurrentElement.Title;
                    BodyElementDataContext.ClosureType = BodyElementDataContext.ClosureTypes.Get(BodyElementDataContext.CurrentElement.TypeCode);
                    BodyElementDataContext.CurrentElement.Title = name;
                }
                if (e.PropertyName == "ClosureType")
                {
                    BodyElementDataContext.CurrentElement.Update(BodyElementDataContext.ClosureType.TypeCode, ObjectState.Modified);
                    ColorDataContext.CurrentColor = ColorDataContext.Colors.FirstOrDefault(x => x.ColorIndex == BodyElementDataContext.CurrentElement.ColorIndex);
                    BodyLayerDataContext.CurrentLayer = BodyLayerDataContext.Layers.FirstOrDefault(x => x.LayerName == BodyElementDataContext.CurrentElement.LayerName);
                }
            }
            else if (sender is ColorViewModel && !ColorDataContext.IsReadOnly)
            {
                if (e.PropertyName == "CurrentColor")
                    BodyElementDataContext.CurrentElement.ColorIndex = ColorDataContext.CurrentColor.ColorIndex;
            }
            else if (sender is IPaletteElement)
            {
                ((IPaletteElement)sender).ObjectState = ObjectState.Modified;
            }
        }

        public override event PropertyChangedEventHandler PropertyChanged;
        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
