using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Windows;
using ID.Infrastructure;
using ID.Infrastructure.Commands;
using ID.Infrastructure.Enums;
using ID.Infrastructure.Extensions;
using ID.Infrastructure.Interfaces;
using ID.Infrastructure.Models;
using Intellidesk.AcadNet.Common.Enums;
using Intellidesk.AcadNet.Common.Extensions;
using Intellidesk.AcadNet.Common.Interfaces;
using Intellidesk.AcadNet.Common.Internal;
using Intellidesk.AcadNet.Common.Models;
using Intellidesk.AcadNet.Interfaces;
using Intellidesk.AcadNet.Services;
using Intellidesk.Data.Models.Cad;
using Intellidesk.Data.Models.Entities;
using Intellidesk.Data.Repositories.EF6.DataContext;
using Prism.Events;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Intellidesk.AcadNet.Common.Core
{
    public abstract class BaseViewModel : BaseEntity, IBaseViewModel
    {
        #region <fields>
        protected static ObjectId _lastSelectAddedObjectId = ObjectId.Null;
        protected static string _lastHandle = null;
        protected readonly object Busy = new object();
        private double _progressBarMax;
        private double _progressStateValue;
        private double _progressBarValue;
        protected UserSetting _currentUserSetting;

        protected IDataContextAsync AcadNetData = null;
        protected CancellationTokenSource CancelTokenSource = new CancellationTokenSource();
        protected CancellationToken CancelToken;
        #endregion

        #region <props>

        protected Document Doc => acadApp.DocumentManager.MdiActiveDocument;
        protected Editor Ed => Doc.Editor;
        protected Database Db => Doc.Database;

        public IPluginSettings PluginSettings => Plugin.GetService<IPluginSettings>();
        public ICommandLine CommandLine => Plugin.GetService<ICommandLine>();
        public Dispatcher UiDispatcher => Dispatcher.CurrentDispatcher;
        protected IEventAggregator EventAggregator => Plugin.GetService<IEventAggregator>();

        protected Dispatcher BackgroundDispatcher { get; set; }
        public new IBasePanelContext Parent { get; set; }
        public bool IsLoaded { get; set; }
        public bool IsActive { get; set; }
        public bool IsReadonly { get; set; }

        private bool _switchSizeMode;
        public bool SwitchSizeMode
        {
            get => _switchSizeMode;
            set
            {
                PaletteSet parentPaletteSet = ToolsManager.PaletteTabs.RootPaletteSet;
                parentPaletteSet.Visible = false;

                if (value)
                {
                    if (PluginSettings.ToolPanelWidth != 0)
                        PluginSettings.ToolPanelLastWidth = PluginSettings.ToolPanelWidth;

                    PluginSettings.ToolPanelWidth = parentPaletteSet.GetSize().Width;
                    var size = new System.Drawing.Size(0, parentPaletteSet.GetSize().Height);
                    parentPaletteSet.SetSize(size);
                }

                parentPaletteSet.Visible = true;
                _switchSizeMode = false;
            }
        }

        private ILogger _logger;
        public ILogger Logger => _logger ?? (_logger = Plugin.GetService<ILogger>());

        private bool _isCanceled;
        public bool IsCanceled
        {
            get
            {
                return _isCanceled;
            }
            set
            {
                lock (Busy)
                {
                    if (value && this.CancelTokenSource != null)
                        this.CancelTokenSource.Cancel();
                    _isCanceled = value;
                }
            }
        }

        private ObservableRangeCollection<ObjectIdItem> _elementItems = new ObservableRangeCollection<ObjectIdItem>();
        public virtual ObservableRangeCollection<ObjectIdItem> ElementItems
        {
            get => _elementItems;
            set
            {
                _elementItems = value;
                OnPropertyChanged();
                DetailsCount = _elementItems.Count;
            }
        }

        private int _detailsCount;
        public virtual int DetailsCount
        {
            get => _detailsCount;
            set
            {
                _detailsCount = value;
                OnPropertyChanged();
            }
        }

        private List<string> _scaleFactors = new List<string> { "1", "2", "3", "5", "7", "10" };
        public virtual List<string> ScaleFactors
        {
            get => _scaleFactors;
            set
            {
                _scaleFactors = value;
                OnPropertyChanged();
            }
        }
        public UserSetting CurrentUserSetting { get; set; }
        public bool IsSelectionEnabled { get; set; } = false;
        public bool IsPointMonitorEnabled { get; set; } = false;
        public bool IsEntityModifyEnabled { get; set; } = false;
        bool IBaseViewModel.IsReadOnly { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        #endregion

        #region <ctor>
        protected BaseViewModel()
        {
            new Thread(() =>
            {
                BackgroundDispatcher = Dispatcher.CurrentDispatcher;
                Dispatcher.Run();
            }).Start();
        }

        #endregion

        #region <Events>

        public virtual void RegisterEvents()
        {
            UnregisterEvents();

            if (acadApp.DocumentManager.MdiActiveDocument != null)
            {
                acadApp.DocumentManager.DocumentActivated += OnDocumentActivated;
                acadApp.DocumentManager.DocumentToBeDestroyed += OnDocumentToBeDestroyed;
                acadApp.DocumentManager.MdiActiveDocument.CommandCancelled += OnCommandCancelled;

                if (IsSelectionEnabled)
                {
                    acadApp.DocumentManager.MdiActiveDocument.ImpliedSelectionChanged += OnImpliedSelectionChanged;
                    acadApp.DocumentManager.MdiActiveDocument.Editor.SelectionAdded += OnSelectionAdded;
                }
                if (IsPointMonitorEnabled)
                {
                    acadApp.DocumentManager.MdiActiveDocument.Editor.PointMonitor += OnPointMonitor;
                    // Need to enable the AutoCAD input event mechanism to do a pick under the prevailing
                    // pick aperture on all digitizer events, regardless of whether a point is being acquired 
                    // or whether any OSNAP modes are currently active.
                    acadApp.DocumentManager.MdiActiveDocument.Editor.TurnForcedPickOn();
                }
                if (IsEntityModifyEnabled)
                {
                    Edit.ObjectErased += OnAcadEntityErased;
                    Edit.ObjectModified += OnAcadEntityModified;
                }
                EventAggregator.GetEvent<NotifyMessageHandleEvent>().Subscribe(OnSelectionAddedEvent);
            }
        }

        private void DocumentManager_DocumentActivated(object sender, DocumentCollectionEventArgs e)
        {
            throw new NotImplementedException();
        }

        public virtual void UnregisterEvents()
        {
            if (acadApp.DocumentManager.MdiActiveDocument != null)
            {
                acadApp.DocumentManager.DocumentActivated -= OnDocumentActivated;
                acadApp.DocumentManager.DocumentToBeDestroyed -= OnDocumentToBeDestroyed;
                acadApp.DocumentManager.MdiActiveDocument.CommandCancelled -= OnCommandCancelled;

                if (IsSelectionEnabled)
                {
                    acadApp.DocumentManager.MdiActiveDocument.ImpliedSelectionChanged -= OnImpliedSelectionChanged;
                    acadApp.DocumentManager.MdiActiveDocument.Editor.SelectionAdded -= OnSelectionAdded;
                }
                if (IsPointMonitorEnabled)
                {
                    acadApp.DocumentManager.MdiActiveDocument.Editor.PointMonitor -= OnPointMonitor;
                    acadApp.DocumentManager.MdiActiveDocument.Editor.TurnForcedPickOff();
                }
                if (IsEntityModifyEnabled)
                {
                    Edit.ObjectErased -= OnAcadEntityErased;
                    Edit.ObjectModified -= OnAcadEntityModified;
                }
                EventAggregator.GetEvent<NotifyMessageHandleEvent>().Unsubscribe(OnSelectionAddedEvent);
            }
        }

        #endregion

        #region <Callback>

        public virtual void OnDocumentActivated(object sender, DocumentCollectionEventArgs e) => RegisterEvents();

        public virtual void OnDocumentToBeDestroyed(object sender, DocumentCollectionEventArgs e)
        {
            UnregisterEvents();
            if (Doc != null && Doc.UserData != null && Doc.UserData.Count > 0)
                Doc.UserData.Clear();
        }

        private void OnCommandCancelled(object sender, CommandEventArgs e)
        {
            _lastSelectAddedObjectId = ObjectId.Null;
        }

        protected virtual void OnSelectionAdded(object sender, SelectionAddedEventArgs e)
        {
            if (!Utils.Utils.IsModelSpace(false) || e.AddedObjects.Count == 0 || (short)acadApp.GetSystemVariable("TILEMODE") == 0)
                return;

            ObjectId id = e.AddedObjects.GetObjectIds().Last();

            if (_lastSelectAddedObjectId == id) return;
            _lastSelectAddedObjectId = id;

            EventAggregator.GetEvent<NotifyMessageHandleEvent>().Publish(id.Handle.Value);
        }

        private void OnSelectionAddedEvent(long handleLong)
        {
            string handle = handleLong.ToString("X");

            UiDispatcher.Invoke(() =>
            {
                DBObject dbObject = HostApplicationServices.WorkingDatabase.XGetObject(handle);

                string ownerHandle = dbObject.XGetData(TypeCodes.OwnerHandle);
                if (string.IsNullOrEmpty(ownerHandle)) return;

                dbObject = ownerHandle == dbObject.Handle.ToString()
                    ? dbObject : Db.XGetObject(ownerHandle);

                Type type = dbObject.XGetXDataObjectType(TypeCodes.OwnerType);
                if (type == null) return;

                IPaletteElement element = dbObject.XGetXDataObject(type);
                if (element != null)
                {
                    if (_lastHandle != null && _lastHandle != element.Handle)
                    {
                        Doc.Editor.SelectionAdded -= OnSelectionAdded;
                        Doc.ImpliedSelectionChanged -= OnImpliedSelectionChanged;
                        Selects.SelectClear();
                        Ed.SetImpliedSelection(new[] { dbObject.ObjectId });
                        Doc.ImpliedSelectionChanged += OnImpliedSelectionChanged;
                        Doc.Editor.SelectionAdded += OnSelectionAdded;
                        _lastHandle = element.Handle;
                    }

                    _lastHandle = element.Handle;
                    //element.ObjectState = ObjectState.Edit;

                    var commandArgs = new CommandArgs(null, "Edit", element);
                    //Ed.WriteMessage($"{PluginSettings.Prompt}Selection added object type: {type}, name: {element}");

                    PaletteNames paletteTabName = (PaletteNames)Enum.ToObject(typeof(PaletteNames), element.PaletteType);
                    var tabs = ToolsManager.PaletteTabs;
                    IPanelTabView paletteTab = tabs[paletteTabName];
                    if (paletteTab != null)
                    {
                        paletteTab.OnActivate(commandArgs);
                        if (!paletteTab.IsActive)
                            tabs.Activate(paletteTabName);
                    }
                }
            });
        }

        protected virtual void OnPointMonitor(object sender, PointMonitorEventArgs e)
        {
            try
            {
                Editor ed = (Editor)sender;
                Document doc = ed.Document;

                if (!Utils.Utils.IsModelSpace(false)
                    || doc.CommandInProgress.Contains(CommandNames.UserGroup)
                    || (doc.UserData != null && doc.UserData.Keys.Cast<string>().Any(x => x.StartsWith(CommandNames.UserGroup)))
                    ) return;

                List<string> curveInfo = new List<string>();
                short tilemode = (short)acadApp.GetSystemVariable("TILEMODE");
                FullSubentityPath[] paths = e.Context.GetPickedEntities();

                if (paths.Length == 0 || tilemode == 0) return;

                using (var tr = doc.TransactionManager.StartTransaction())
                {
                    foreach (FullSubentityPath path in paths)
                    {
                        try
                        {
                            ObjectId[] ids = path.GetObjectIds();
                            if (ids.Length == 0) continue;

                            ObjectId id = ids[ids.GetUpperBound(0)];
                            DBObject obj = tr.GetObject(id, OpenMode.ForRead);
                            IPaletteElement element = obj.XGetDataObject();

                            if (element != null)
                            {
                                curveInfo.Add($"  Title: {element.Title}");
                                curveInfo.Add($"  Element: {element.ElementName.Replace("Acad", "")}");

                                string typeName = null;
                                if (element.TypeCodeFullName != null)
                                {
                                    Type typeEnum = Type.GetType($"{element.TypeCodeFullName},{typeof(eBodyType).Assembly.FullName}");
                                    typeName = Enum.GetName(typeEnum, element.TypeCode);
                                    curveInfo.Add($"  Type: { typeName ?? element.TypeCodeFullName.Split('.').Last() }");

                                    if (obj.GetType().BaseType == typeof(Curve))
                                    {
                                        if (obj is Polyline)
                                            curveInfo.Add($"  Length: {(obj as Polyline).Length():F2}");
                                        else if (obj is Circle)
                                            curveInfo.Add($"  Radius: {(obj as Circle).Radius:F2}");
                                    }
                                    //else if (obj is DBText)
                                    //{
                                    //    curveInfo.Add($"  Text: {(obj as DBText).TextString}");
                                    //}
                                    else if (obj is BlockReference)
                                    {
                                        BlockReference bref = obj as BlockReference;
                                        BlockTableRecord bdef = (BlockTableRecord)tr.GetObject(bref.BlockTableRecord, OpenMode.ForRead);
                                        if (!bdef.HasAttributeDefinitions) return;

                                        curveInfo.Add($"  Attributes: {bref.XGetAttributesData()}");
                                    }

                                }

                            }
                        }
                        catch (Exception ex)
                        {
                            Plugin.Logger.ErrorEx(ex);
                        }
                        //tr.Commit();
                    }

                    // Add the tooltip of the lengths of the curves detected
                    if (curveInfo.Any())
                    {
                        var curveInfoString = string.Join<string>("<LineBreak/>", curveInfo);
                        e.AppendToolTipText(
                            $"]]><LineBreak/><Bold>  {Plugin.Settings.Name}</Bold><LineBreak/><LineBreak/>{curveInfoString}<![CDATA[");
                    }
                }
            }
            catch
            {
                // Not sure what we might get here, but not real action
                // needed (worth adding an Exception parameter and a
                // breakpoint, in case things need investigating).
            }
        }

        protected virtual void OnImpliedSelectionChanged(object sender, EventArgs e)
        {
            Doc.Editor.SelectionAdded -= OnSelectionAdded;
            PromptSelectionResult psr = Doc.Editor.SelectLast(); //.SelectImplied();
            Doc.Editor.SelectionAdded += OnSelectionAdded;

            if (psr.Value != null)
            {
                var ids = psr.Value.GetObjectIds();
                ObjectId objectId = ids.LastOrDefault();

                //base.SelectionChanged(objectId);

                ObjectIdItem item = ElementItems.FirstOrDefault(x => x.ObjectId == objectId);
                ElementItems.Insert(ElementItems.IndexOf(item), item);
                //ExecuteRefresh(ids.LastOrDefault());
                //doc.Editor.SetImpliedSelection(new ObjectId[] {});
            }
            else
            {
                _lastSelectAddedObjectId = ObjectId.Null;
            }
        }

        public virtual void OnDataContextPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }

        public virtual void OnAcadEntityErased(object sender, EventArgs e)
        {
        }

        public virtual void OnAcadEntityModified(object sender, EventArgs args)
        {
        }

        #endregion

        #region <Validations>

        /// <summary> returnig is dwg conpatible base on lsds rules </summary>
        public bool IsDwgCompatible(Type[] typeFilterOn = null, string[] attributePatternOn = null) //Rule.LsdsTypeFilterOn, Rule.LsdsAttributePatternOn
        {
            //Get implied entities //LsdsCommands.CurrentObjectIds
            var currentObjectIds = new List<ObjectId>(); // SelectManager.GetImplied();

            //Get entities from drawing space model
            //currentObjectIds = currentObjectIds.XGetObjects(typeFilterOn, attributePatternOn);
            var result = currentObjectIds.Count != 0;

            return result;
        }

        public bool IsDwgOpen(Type[] typeFilterOn = null, string[] attributePatternOn = null) //Rule.LsdsTypeFilterOn, Rule.LsdsAttributePatternOn
        {
            return Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument != null;
        }

        private bool IsValid(DependencyObject obj)
        {
            // The dependency object is valid if it has no errors, 
            //and all of its children (that are dependency objects) are error-free.
            return !Validation.GetHasError(obj) &&
                LogicalTreeHelper.GetChildren(obj)
                .OfType<DependencyObject>()
                .All(child => IsValid(child));
        }

        #endregion <Validations>

        #region <Progressbar>

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

        #endregion Progressbar>

        #region <UiDispatcher>

        //public static Dispatcher UiDispatcher { get; set; }

        protected void RunOnUIThread(Action action)
        {
            if (null == action)
                return;

            if (UiDispatcher.CheckAccess())
                action();
            else
                UiDispatcher.Invoke(action, DispatcherPriority.Render);
        }

        protected async void RunOnUIThreadAsync(Action action)
        {
            if (null == action)
                return;

            if (UiDispatcher.CheckAccess())
                action();
            else
                await UiDispatcher.BeginInvoke(DispatcherPriority.Normal, action);
        }

        #endregion <UiDispatcher>

        #region <public methods>

        public void SendCommandToExecute(string commandName, object sender = null, object commandParameter = null, Action<ICommandArgs> commandCallBack = null)
        {
            if (!acadApp.DocumentManager.MdiActiveDocument.UserData.ContainsKey(commandName))
            {
                CommandArgs command = null;
                if (sender != null && commandParameter != null)
                {
                    command = new CommandArgs(sender, commandName, commandParameter, true);
                    command.CommandCallBack = commandCallBack;
                }
                else if (commandCallBack != null && sender == null && commandParameter == null)
                {
                    command = new CommandArgs(null, commandName, null, true);
                    command.CommandCallBack = commandCallBack;
                }

                acadApp.DocumentManager.MdiActiveDocument.UserData.Add(commandName, command);
            }

            acadApp.DocumentManager.MdiActiveDocument.SendStringToExecute(commandName + " ", true, false, false);
            Autodesk.AutoCAD.Internal.Utils.PostCommandPrompt();
        }

        #endregion <public methods>
    }
}
