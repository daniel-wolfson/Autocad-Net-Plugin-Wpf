using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Intellidesk.AcadNet.Common;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using Intellidesk.AcadNet.Common.Core;
using Intellidesk.AcadNet.Common.Interfaces;
using Intellidesk.AcadNet.Core;
using Intellidesk.AcadNet.Interfaces;
using Intellidesk.AcadNet.Services;
using Intellidesk.AcadNet.Services.Core;
using Intellidesk.AcadNet.Services.Extentions;
using Intellidesk.Data.Models.Cad;
using Intellidesk.Infrastructure;
using Intellidesk.Infrastructure.Core;
using Intellidesk.Infrastructure.Enums;
using Intellidesk.Infrastructure.Tasks;
using Prism.Commands;
using Prism.Interactivity.InteractionRequest;
using ObjectIdDisplayItem = IntelliDesk.AcadNet.Model.ObjectIdDisplayItem;

namespace Intellidesk.AcadNet.ViewModels
{
    public class SearchTextPanel : BaseViewModel, ISearchTextPanel
    {
        private SearchTextNotification _notification;
        private IPluginSettings _appSettings;
        public Action<object> Load { get; set; }

        public SearchTextPanel()
        {
            ProgressBarMinimum = 0;
            ProgressBarMaximum = 100;

            var layerService = Plugin.GetService<LayerService>();
            var list = layerService.GetAll().Select(x => x.Name).ToList();
            list.Insert(0, "All");
            Layers = list;

            RunCommand = new DelegateCommand<string>(ExecuteRunCommand, CanExecuteRunCommand);
            StopCommand = new DelegateCommand<object>(ExecuteStopCommand);
            SelectSetCommand = new DelegateCommand<object>(ExecuteSelectSetCommand);
            ClearCommand = new DelegateCommand<object>(ExecuteProgressResetCommand);

            _appSettings = Plugin.GetService<IPluginSettings>();
        }

        #region "Properties"

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
                    this._notification = (SearchTextNotification)value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _selectedText;

        public string SelectedText
        {
            get { return _selectedText; }
            set
            {
                _selectedText = value;
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

        private List<string> _scaleFactors = new List<string>() { "1", "2", "3", "5", "7", "10" };
        public List<string> ScaleFactors
        {
            get { return _scaleFactors; }
            set
            {
                _scaleFactors = value;
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

        private ObservableRangeCollection<ObjectIdDisplayItem> _existListItems = new ObservableRangeCollection<ObjectIdDisplayItem>();

        public ObservableRangeCollection<ObjectIdDisplayItem> ExistListItems
        {
            get { return _existListItems; }
            set
            {
                _existListItems = value;
                OnPropertyChanged();
            }
        }

        private ObjectIdDisplayItem selectedKey = null;

        public ObjectIdDisplayItem SelectedKey
        {
            get { return this.selectedKey; }
            set
            {
                this.selectedKey = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged("SelectedValue");
            }
        }

        private ObjectIdDisplayItem _selectedItem;

        public ObjectIdDisplayItem SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                Entity ent;
                if (_selectedItem != null)
                {
                    ent = _selectedItem.ObjectId.XCast<DBText>();
                    ent.Unhighlight();
                }

                if (value != null)
                {
                    _selectedItem = value;
                    CommandLine.Zoom(_selectedItem.ObjectId, CurrentZoomDisplayFactor);
                    ent = _selectedItem.ObjectId.XCast<DBText>();
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
                    Plugin.Container.RegisterInstance<IPluginSettings>(_appSettings);

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

        #region "Commands"

        private ICommand _refreshLayoutCommand;
        public ICommand RunCommand { get; set; }
        public ICommand StopCommand { get; set; }
        public ICommand SelectSetCommand { get; set; }
        public ICommand ClearCommand { get; set; }
        public ICommand ClickCommand { get; set; }

        public ICommand RefreshLayoutCommand => _refreshLayoutCommand 
            ?? (_refreshLayoutCommand = new DelegateCommand<object>(ExecuteRefreshLayout));

        private void ExecuteRefreshLayout(object commandParameter)
        {
            UiDispatcher.BeginInvoke(new Action(() =>
            {
                Mouse.OverrideCursor = Cursors.Wait;
                Thread.Sleep(500);
                var layerService = Plugin.GetService<LayerService>();
                var list = layerService.GetAll().Select(x => x.Name).ToList();
                list.Insert(0, "All");
                Layers = list;
                CurrentLayer = "All";
                Mouse.OverrideCursor = null;
            }));
        }

        public void ExecuteSelectSetCommand(object commandParameter)
        {
            Ed.SetImpliedSelection(ExistListItems.Select(x => x.ObjectId).ToArray());
            //CommandMain.SetFocus(Application.DocumentManager.MdiActiveDocument.Window.Handle);
            Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView();
        }

        public void ExecuteStopCommand(object commandParameter)
        {
            IsCanceled = true;
        }

        public void ExecuteProgressResetCommand(object commandParameter)
        {
            UiDispatcher.Invoke(() =>
            {
                ProgressStateValue = 0;
                ProgressBarValue = 0;
                ProgressBarMaximum = commandParameter != null ? Convert.ToInt32(commandParameter) : 0;
                ExistListItems.Clear();
                SelectedItem = null;
                CommandLine.Cancel();
            });
        }

        public void ExecuteRunCommand(string commandParameter)
        {
            IsCanceled = false;
            if (SelectedText != commandParameter)
                SelectedText = commandParameter;

            ExecuteProgressResetCommand(null);

            Commands.DelayAction(200, () =>
            {
                Mouse.OverrideCursor = Cursors.Wait;
                Notifications.DisplayNotifyMessageAsync(NotifyStatus.Working);

                var ents = Db.ReadObjectsDynamic<DBText>
                            (CurrentLayer == "All" ? "" : CurrentLayer)
                            .Where(x => x.TextString.Contains(commandParameter ?? ""))
                            .ToList();

                BackgroundDispatcher.BeginInvoke(new Action(() =>
                {
                    ExecuteProgressResetCommand(ents.Count);

                    var objectIdItems = new List<ObjectIdDisplayItem>();
                    var i = CanPopulated ? ents.Count : 1;

                    ents.Take(i).ForEach(ent => UiDispatcher.Invoke(() =>
                    {
                        if (!IsCanceled)
                        {
                            ProgressBarValue += 1;
                            ProgressStateValue += 1;
                            objectIdItems.Add(new ObjectIdDisplayItem(ent.ObjectId, ent.TextString));
                        }
                    }));

                    UiDispatcher.Invoke(() =>
                    {
                        ExistListItems.AddRange(objectIdItems);
                        objectIdItems.Clear();
                        if (ExistListItems.Any())
                        {
                            SelectedItem = ExistListItems.FirstOrDefault();
                            if (SelectedItem != null)
                            {
                                CommandLine.Zoom(SelectedItem.ObjectId, CurrentZoomDisplayFactor);
                                var ent = SelectedItem.ObjectId.XCast<DBText>();
                                ent.Highlight();
                            }
                        }
                        ProgressBarValue = 1; ProgressBarValue = 0;
                        Notifications.DisplayNotifyMessageAsync(NotifyStatus.Ready);
                        Mouse.OverrideCursor = null;
                    });
                }));

                Mouse.OverrideCursor = null;
            });
        }

        public void ExecuteStopCommand(string commandParameter)
        {
            IsCanceled = true;
        }

        public bool CanExecuteRunCommand(object parameter)
        {
            return true; // !string.IsNullOrEmpty(SelectedText);
        }

        #endregion "Commands"

        #region "Methods"

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
            ExecuteProgressResetCommand(0);
            ExecuteRefreshLayout(null);
        }

        public void Deactive()
        {
            _appSettings.Save();
        }

        public void OnDocumentToBeDestroyed(object sender, DocumentCollectionEventArgs e)
        {
            //Application.DocumentManager.DocumentActivated -= CablePanelDataContext.OnDocumentActivated;
            //Application.DocumentManager.DocumentActivated += CablePanelDataContext.OnDocumentActivated;
            //Application.DocumentManager.DocumentToBeDestroyed -= CablePanelDataContext.OnDocumentToBeDestroyed;
        }

        public void ExecuteRefresh(object value)
        {
            //throw new NotImplementedException();
        }

        Task<CommandArgs> IPanelDataContext.ExecuteCommand(CommandArgs command)
        {
            //throw new NotImplementedException(); 
            return null;
        }

        #endregion
    }
}
