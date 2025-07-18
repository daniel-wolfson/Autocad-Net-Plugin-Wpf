using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Unity;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.Windows;
using FileExplorer.Extensions;
using FileExplorer.Model;
using FileExplorer.ViewModel;
using Intellidesk.AcadNet.Common.Core;
using Intellidesk.AcadNet.Core;
using Intellidesk.AcadNet.Interfaces;
using Intellidesk.Infrastructure.Core;
using Intellidesk.AcadNet.Common.Enums;
using Intellidesk.Infrastructure.Tasks;
using Microsoft.Practices.Prism.Commands;

using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using ITabView = Intellidesk.AcadNet.Common.Interfaces.ITabView;
using Rule = Intellidesk.Data.Models.Cad.Rule;
using User = Intellidesk.Data.Models.Cad.User;
using UserSetting = Intellidesk.Data.Models.Cad.UserSetting;
using Layout = Intellidesk.Data.Models.Cad.Layout;
using LayoutExtensions = Intellidesk.AcadNet.Core.LayoutExtensions;

using System.Net;
using Intellidesk.AcadNet.Common;
using Intellidesk.AcadNet.Common.Interfaces;
using Intellidesk.AcadNet.Components;
using Intellidesk.AcadNet.Services;
using Intellidesk.Data.Models;
using Intellidesk.Data.Models.Cad;
using Intellidesk.Data.Repositories;
using Intellidesk.Data.Repositories.EF6.UnitOfWork;
using Intellidesk.Data.Services;
using Intellidesk.Infrastructure;
using Intellidesk.Infrastructure.Enums;
using Prism.Interactivity.InteractionRequest;

namespace Intellidesk.AcadNet.ViewModels
{
    /// <summary> Main view model </summary>
    public class ProjectExplorerPanel : BaseViewModel, IProjectExplorerPanel, IDataErrorInfo
    {
        #region Private variables

        //private readonly IUnityContainer UnityContainer;
        private readonly ICommandLine _commandLine;
        private readonly ILayoutService _layoutService;
        private readonly InteractionRequestViewModel _interactionRequestViewModel;

        private readonly IEventAggregator _eventAggregator;
        private SubscriptionToken _notifyMessageStringSubscriptionToken;
        private readonly IUnitOfWorkAsync _uow = null;

        // Config and Layout
        private Config _currentConfig;
        private Document _currenDocument;
        private Layout _currentLayout;
        private Rule _currentRule;
        private UserSetting _currentUserSetting;
        private User _currentUser;
        private Filter _currentlayoutfilter;
        private RibbonButton _currentRibbonLayout;
        private string[] _layoutsContentsItems;


        // ProjectExplorer
        //private bool _showAll;
        private bool _isLayoutReadOnly;

        // ObservableCollections
        private ObservableCollection<Config> _configItems;
        private ObservableCollection<Layout> _layoutItems;
        private ObservableCollection<Rule> _ruleItems;
        private ObservableCollection<Filter> _layoutFilterItems;
        private ObservableCollection<UserSetting> _userSettingItems;
        private ObservableCollection<RibbonButton> _ribbonLayoutItems;

        private ICommand _fsaApplyLayoutCommand, _fsaUploadLayoutCommand;
        private ICommand _findCommand, _layoutFilterCommand;
        private ICommand _newLayoutCommand, _clearDwgBufferCommand;

        private ICommand _deleteLayoutCommand, _purgeLayoutCommand;
        private ICommand _editCommand, _refreshCommand;
        private ICommand _showAllFoldersCommand;

        private ICommand _setFolderBaseCommand, _treeCleanMarkCommand;
        private ICommand _openLayoutCommand, _saveLayoutCommand, _undeleteLayoutCommand;

        private ICommand  _rbnExport, _rbnOpenExplorer;
        private ICommand _rbnActiveLayoutCommand, _rbnOpenConfigCommand, _rbnOpenLayoutCommand, 
                         _rbnOpenConfigManageCommand, _raiseSaveConfirmationCommand;
        private ICommand _rbnUpload, _rbnRun, _rbnRefresh, _rbnClean;
        private ICommand _rbnMapView, _rbnPointOnMap, _rbnFindOnMap, _rbnUcsChange;
        private ICommand _rbnCopyAsBlock, _rbnPasteAsBlock;
        private ICommand _rbnCable, _rbnClosure, _rbnCabinet, _rbnHelpCommand;
        
        private Dispatcher UIDispatcher;

        #endregion

        public Action<object> Load { get; set; }

        private FileExplorerViewModel _fileExplorerViewModel;
        public IFileExplorerViewModel FileExplorerViewModel => _fileExplorerViewModel ??
            (_fileExplorerViewModel = new FileExplorerViewModel());

        private SearchViewModel _searchViewModel;
        public ISearchViewModel SearchViewModel => _searchViewModel ?? 
            (_searchViewModel = new SearchViewModel() { Parent = this });

        #region ctor

        public ProjectExplorerPanel()
        {
            ShowAllFolders = PluginSettings.ShowAllFolders;
            _commandLine = Plugin.GetService<ICommandLine>();
            _uow = Plugin.GetService<IUnitOfWorkAsync>();
            _eventAggregator = Plugin.GetService<IEventAggregator>();

            var userService = Plugin.GetService<IUserService>();
            _currentUser = userService.GetUserByName(Environment.UserName);

            _userSettingItems = _currentUser.Settings.ToItems();
            CurrentUserSetting = _userSettingItems.FirstOrDefault(x => x.IsActive);

            _projectExplorerFoldersRowDefinitionHeight = PluginSettings.ProjectExplorerFoldersSectionHeight;
            _projectExplorerFilesSectionHeight = PluginSettings.ProjectExplorerFilesSectionHeight;

            if (PluginSettings.ProjectExplorerPropertySectionDisplay)
                _projectExplorerPropertySectionHeight = PluginSettings.ProjectExplorerPropertySectionHeight;

            if (CurrentUserSetting != null)
            {
                ToggleLayoutDataTemplateSelector = CurrentUserSetting.ToggleLayoutDataTemplateSelector;

                var layoutFilterService = Plugin.GetService<IFilterService>();
                _layoutFilterItems = layoutFilterService.GetFilters().ToItems();
                CurrentLayoutFilter = LayoutFilterItems.FirstOrDefault();

                _layoutService = Plugin.GetService<ILayoutService>();
                _layoutItems = _layoutService.GetLayouts().ToItems(OnlayoutPropertyChanged);
                CurrentLayout = LayoutItems.FirstOrDefault(x => x.LayoutID == CurrentUserSetting.LayoutId) ??
                                LayoutItems.FirstOrDefault();

                MakeRibbonLayouts(_layoutItems);

                var dataConfigService = Plugin.GetService<IDataConfigService>();
                _configItems = dataConfigService.GetItems();
                CurrentConfig = ConfigItems.FirstOrDefault(x => x.ConfigSetName == CurrentUserSetting.ConfigSetName);

                //_interactionRequestViewModel = UnityContainer.Resolve<InteractionRequestViewModel>();
                //_interactionRequestViewModel.ConfirmationRequest = 
            }

            //Application.DocumentManager.DocumentActivated += OnDocumentActivated;
            //Application.DocumentManager.DocumentDestroyed += ExecuteCloseLayout;
            //Application.DocumentManager.DocumentToBeDeactivated += OnDocumentToBeDeactivated;
            //Layout.EntityChangedEvent += OnlayoutPropertyChanged;
        }

        #endregion

        #region Properties

        private bool _isTabButtonActive = true;
        public bool IsTabButtonActive
        {
            get { return _isTabButtonActive; }
            set
            {
                _isTabButtonActive = value;
                OnPropertyChanged();
            }
        }

        private bool _isPluginCompatible = true;

        /// <summary> defining state of current palette </summary>
        public bool IsPluginCompatible
        {
            get { return _isPluginCompatible; }
            set
            {
                if (value != _isPluginCompatible)
                {
                    _isPluginCompatible = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary> defining ReadOnly state of current layout </summary>
        public bool IsLayoutReadOnly
        {
            get { return _isLayoutReadOnly; }
            set
            {
                if (value != _isLayoutReadOnly)
                {
                    _isLayoutReadOnly = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary> Configs observableCollection </summary>
        //[ImportMany(AllowRecomposition = true)]
        public ObservableCollection<Config> ConfigItems
        {
            get { return _configItems; }
        }

        /// <summary> Current config </summary>
        public Config CurrentConfig
        {
            get { return _currentConfig; }
            set
            {
                if (_currentConfig != null)
                {
                    if (!_currentConfig.Equals(value))
                    {
                        _currentConfig = value;
                        OnPropertyChanged();
                        //CurrentUserSetting.ConfigSetName = _currentConfig.ConfigSetName;
                        //CurrentUserSetting.Save();
                        LoadRules();
                    }
                }
                else
                {
                    _currentConfig = value;
                    OnPropertyChanged();//"CurrentConfig"
                    LoadRules();
                }
            }
        }

        /// <summary> Layouts </summary>
        //[ImportMany(typeof(Layout), AllowRecomposition = true)]
        public ObservableCollection<Layout> LayoutItems
        {
            get { return _layoutItems; }
            set { _layoutItems = value; OnPropertyChanged(); }
        }

        public Document CurrentDocument
        {
            get { return _currenDocument; }
            set
            {
                _currenDocument = value;
                OnPropertyChanged();
            }
        }

        /// <summary> Current layout </summary>
        public Layout CurrentLayout
        {
            get { return _currentLayout; }
            set
            {
                if (_currentLayout != null && value != null)
                {
                    _currentLayout = value;
                    OnPropertyChanged();

                    CurrentUserSetting.LayoutId = _currentLayout.LayoutID;
                    //CurrentUserSetting.Save();
                    //-- CurrentRibbonLayout = _ribbonlayouts.FirstOrDefault(X => X.Name.Contains(Convert.ToString(value.LayoutId)));
                }
                else
                {
                    _currentLayout = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary> Layouts ribbon buttons </summary>
        public ObservableCollection<RibbonButton> RibbonLayoutItems
        {
            get { return _ribbonLayoutItems; }
        }

        private int _projectExplorerFoldersRowDefinitionHeight; //Settings.Default.ProjectExplorerFoldersSectionHeight;
        public int ProjectExplorerFoldersSectionHeight
        {
            get { return _projectExplorerFoldersRowDefinitionHeight; }
            set
            {
                _projectExplorerFoldersRowDefinitionHeight = value;
                OnPropertyChanged();
            }
        }

        private int _projectExplorerFilesSectionHeight;
        public int ProjectExplorerFilesSectionHeight
        {
            get { return _projectExplorerFilesSectionHeight; }
            set
            {
                _projectExplorerFilesSectionHeight = value;
                OnPropertyChanged();
            }
        }


        private int _projectExplorerPropertySectionHeight;
        public int ProjectExplorerPropertySectionHeight
        {
            get { return _projectExplorerPropertySectionHeight; }
            set
            {
                _projectExplorerPropertySectionHeight = value;
                OnPropertyChanged();
            }
        }

        public bool ShowAllFolders
        {
            get { return PluginSettings.ShowAllFolders; }
            set
            {
                PluginSettings.ShowAllFolders = value;
                OnPropertyChanged();
            }
        }
        public bool ProjectExplorerPropertySectionDisplay
        {
            get { return PluginSettings.ProjectExplorerPropertySectionDisplay; }
            set
            {
                PluginSettings.ProjectExplorerPropertySectionDisplay = value;
                OnPropertyChanged();
            }
        }
        /// <summary> Current layout ribbon button </summary>
        public RibbonButton CurrentRibbonLayout
        {
            get { return _currentRibbonLayout; }
            set
            {
                _currentRibbonLayout = value;
                OnPropertyChanged();
            }
        }

        /// <summary> Rules </summary>
        //[ImportMany(AllowRecomposition = true)]
        public ObservableCollection<Rule> RuleItems
        {
            get { return _ruleItems; }
        }

        /// <summary> CurrentRule </summary>
        public Rule CurrentRule
        {
            get { return _currentRule; }
            set
            {
                _currentRule = value;
                OnPropertyChanged();
            }
        }

        /// <summary> User settings </summary>
        //[ImportMany(AllowRecomposition = true)]
        public ObservableCollection<UserSetting> UserSettingItems
        {
            get { return _userSettingItems; }
        }

        /// <summary> Current user setting </summary>
        //public UserSetting CurrentUserSetting
        //{
        //    get { return _currentUserSetting; }
        //    set
        //    {
        //        _currentUserSetting = value;
        //        OnPropertyChanged();
        //    }
        //}
        public User CurrentUser
        {
            get { return _currentUser; }
            set
            {
                _currentUser = value;
                OnPropertyChanged();
            }
        }

        /// <summary> User settings </summary>
        //[ImportMany(AllowRecomposition = true)]
        public ObservableCollection<Filter> LayoutFilterItems
        {
            get { return _layoutFilterItems; }
        }

        /// <summary> Current user setting </summary>
        public Filter CurrentLayoutFilter
        {
            get { return _currentlayoutfilter; }
            set
            {
                _currentlayoutfilter = value;
                OnPropertyChanged();
            }
        }

        /// <summary> Toggle Layout DataTemplate Selector to ProjectExplorerView </summary>
        public short ToggleLayoutDataTemplateSelector { get; set; }

        /// <summary> LayoutsContentsItems </summary>
        public string[] LayoutsContentsItems
        {
            get { return _layoutsContentsItems; }
            set
            {
                _layoutsContentsItems = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Interactions

        private InteractionRequest<INotification> _notificationRequest;
        public InteractionRequest<INotification> NotificationRequest
        {
            get
            {
                return _notificationRequest
                    ?? (_notificationRequest = new InteractionRequest<INotification>());
            }
        }

        private InteractionRequest<IConfirmation> _confirmationRequest;
        public InteractionRequest<IConfirmation> ConfirmationRequest
        {
            get
            {
                return _confirmationRequest
                    ?? (_confirmationRequest = new InteractionRequest<IConfirmation>());
            }
        }

        private string _resultMessage;
        public string InteractionResultMessage
        {
            get { return _resultMessage; }
            set { _resultMessage = value; OnPropertyChanged(); }
        }

        private ICommand _raiseNotificationCommand;

        public ICommand RaiseNotificationCommand
        {
            get
            {
                return _raiseNotificationCommand
                    ?? (_raiseNotificationCommand = new Prism.Commands.DelegateCommand<Layout>(RaiseNotification));
            }
        }

        private void RaiseNotification(Layout layout)
        {
            NotificationRequest.Raise(
               new Notification { Content = CurrentLayout.LayoutID + " " + CurrentLayout.LayoutName, Title = "Confirmation" });
        }

        public ICommand RaiseSaveConfirmationCommand
        {
            get
            {
                return _raiseSaveConfirmationCommand
                    ?? (_raiseSaveConfirmationCommand = new Prism.Commands.DelegateCommand<Layout>(RaiseSaveConfirmation));
            }
        }
        private void RaiseSaveConfirmation(Layout layout)
        {
            ConfirmationRequest.Raise(
               new Confirmation { Content = CurrentLayout.LayoutID + " " + CurrentLayout.LayoutName, Title = "Confirmation" },
               c => { InteractionResultMessage = c.Confirmed ? "The user accepted." : "The user cancelled."; });
        }

        #endregion

        #region Palette ICommands, Executes, CanExecutes

        #region Commands ICommand

        public ICommand CreateLayoutCommand
        {
            get
            {
                return _newLayoutCommand ??
                       (_newLayoutCommand = new Prism.Commands.DelegateCommand<Layout>(ExecuteCreateLayout, CanExecuteCreateLayout));
            }
        }
        public ICommand ClearDataCommand
        {
            get
            {
                return _clearDwgBufferCommand ??
                       (_clearDwgBufferCommand = new Prism.Commands.DelegateCommand<Layout>(ExecuteClearData, CanExecuteClearData));
            }
        }

        public ICommand DeleteLayoutCommand
        {
            get
            {
                return _deleteLayoutCommand ??
                       (_deleteLayoutCommand = new Prism.Commands.DelegateCommand<Layout>(ExecuteDeleteLayout, CanExecuteDeleteLayout));
            }
        }
        public ICommand EditCommand
        {
            get
            {
                return _editCommand ??
                    (_editCommand = new Prism.Commands.DelegateCommand<object>(ExecuteEdit, CanExecuteByBusy));
            }
        }
        public ICommand RefreshCommand
        {
            get
            {
                return _refreshCommand ??
                    (_refreshCommand = new Prism.Commands.DelegateCommand<object>(ExecuteRefresh, CanExecuteByBusy));
            }
        }
        public ICommand SetFolderBaseCommand
        {
            get
            {
                return _setFolderBaseCommand ??
                    (_setFolderBaseCommand = new Prism.Commands.DelegateCommand<IFolder>(ExecuteSetFolderBase, CanExecuteByBusy));
            }
        }
        public ICommand FindFolderCommand
        {
            get
            {
                return _findCommand ??
                    (_findCommand = new Prism.Commands.DelegateCommand<object>(ExecuteFind, CanExecuteByBusy));
            }
        }
        public ICommand TreeCleanMarkCommand
        {
            get
            {
                return _treeCleanMarkCommand ??
                    (_treeCleanMarkCommand = new Prism.Commands.DelegateCommand<object>(ExecuteTreeCleanMark, CanExecuteByBusy));
            }
        }
        public ICommand FilterLayoutCommand
        {
            get
            {
                return _layoutFilterCommand ??
                       (_layoutFilterCommand = new Prism.Commands.DelegateCommand<Filter>(ExecuteFilterLayout, x => true));
            }
        }
        public ICommand FsaApplyLayoutCommand
        {
            get
            {
                return _fsaApplyLayoutCommand ??
                       (_fsaApplyLayoutCommand = new Prism.Commands.DelegateCommand<Layout>(ExecuteFsaApplyLayout, CanExecuteFsaApplyLayout));
            }
        }
        public ICommand FsaUploadLayoutCommand
        {
            get
            {
                return _fsaUploadLayoutCommand ??
                       (_fsaUploadLayoutCommand = new Prism.Commands.DelegateCommand<Layout>(ExecuteFsaUploadLayout, CanExecuteFsaUploadLayout));
            }
        }
        public ICommand OpenLayoutCommand
        {
            get
            {
                return _openLayoutCommand ??
                    (_openLayoutCommand = new Prism.Commands.DelegateCommand<Layout>(ExecuteOpenLayout, CanExecuteOpenLayout));
            }
        }
        public ICommand PurgeLayoutCommand
        {
            get
            {
                return _purgeLayoutCommand ??
                       (_purgeLayoutCommand = new Prism.Commands.DelegateCommand<Layout>(ExecutePurgeLayout, CanExecutePurgeLayout));
            }
        }
        public ICommand SaveCommand
        {
            get
            {
                return _saveLayoutCommand ??
                       (_saveLayoutCommand = new Prism.Commands.DelegateCommand<object>(ExecuteSave, CanExecuteSaveLayout));
            }
        }
        public ICommand ShowAllFoldersCommand
        {
            get
            {
                return _showAllFoldersCommand ??
                        (_showAllFoldersCommand = new Prism.Commands.DelegateCommand<object>(ExecuteShowAll, CanExecuteByBusy));
            }
        }
        public ICommand UndeleteLayoutCommand
        {
            get
            {
                return _undeleteLayoutCommand ??
                       (_undeleteLayoutCommand = new Prism.Commands.DelegateCommand<Layout>(ExecuteUndeleteLayout, CanExecuteUndeleteLayout));
            }
        }

        #endregion

        #region Commands CanExecute

        private bool CanExecuteByBusy(object item)
        {
            return !PluginSettings.Busy;
        }
        private bool CanExecuteCreateLayout(Layout layout)
        {
            return LayoutItems != null;
        }
        private bool CanExecuteDeleteLayout(Layout layout)
        {
            return true; //layout.LayoutState == null || layout.LayoutState >= 0;
        }
        private bool CanExecuteClearData(Layout layout)
        {
            //try
            //{
            //    var serviceChannelFactory = new WebChannelFactory<IStateService>
            //        (new WebHttpBinding(), new Uri("http://vmmapinfo.partnergsm.co.il/AcadNetGis/AcadNetStateService/StateService.svc"));
            //    var webchannel = serviceChannelFactory.CreateChannel();
            //    var data = webchannel.GetData();
            //    return data.Count > 0;
            //}
            //catch (Exception)
            //{
            //    return false;
            //}
            return false;
        }
        private bool CanExecuteFsaApplyLayout(Layout layout)
        {
            return RbnCanExecuteRunFsa(null);
        }
        private bool CanExecuteFsaUploadLayout(Layout layout)
        {
            return RbnCanExecuteUpload(null);
        }
        private bool CanExecuteOpenLayout(Layout layout)
        {
            var result = layout != null && !layout.IsModified() && layout.IsValid()
                && (layout.LayoutState == null || layout.LayoutState >= 0);
            return result;
        }
        private bool CanExecutePurgeLayout(Layout layout)
        {
            return layout != null && LayoutItems.Count > 0 && LayoutItems.Contains((Layout)layout)
                && (layout.LayoutState != null && layout.LayoutState < 0);
        }
        private bool CanExecuteSaveLayout(object item)
        {
            return true;
            //return item != null && item.DataInfo != null && item.DataInfo.IsModified() && item.DataInfo.IsValid();
            //&& LayoutItems.Contains(layout)
        }
        private bool CanExecuteUndeleteLayout(Layout layout)
        {
            return layout != null && LayoutItems.Count > 0 && LayoutItems.Contains(layout)
                && (layout.LayoutState != null && layout.LayoutState < 0);
        }

        #endregion

        #region Commands Execute

        public void ExecuteCloseLayout(object sender, DocumentDestroyedEventArgs e)
        {
            //ExecutePropertyGridRefresh(X => X.CADFileName.ToLower() == e.FileName.ToLower() && X.IsLoaded);
            var item = LayoutItems.FirstOrDefault(x => x.CADFileName.ToLower() == e.FileName.ToLower());
            if (item != null && !LayoutExtensions.IsLayoutLoaded(item))
            {
                var idx = LayoutItems.IndexOf(item);
                LayoutItems[idx] = item;
                CurrentLayout = item;
            }
            if (Application.DocumentManager.Count == 0)
                CurrentLayout = null;
        }

        private void ExecuteCreateLayout(Layout layout)
        {
            SystemSounds.Exclamation.Play();
            var newLayout = new Layout
            {
                LayoutID = -1,
                LayoutName = "new project",
                CADFileName = "",
                CreatedBy = CurrentUser.UserId,
                DateCreated = DateTime.Now,
                LayoutVersion = "(new)"
            };

            var taskArguments = Plugin.GetService<ITaskArguments>();
            taskArguments.Title = "New project:";
            taskArguments.Command = "CREATE";
            taskArguments.CommandParameters = new ArrayList { newLayout };
            taskArguments.IsTimerOn = false;
            taskArguments.ProgressLimit = 100;
            taskArguments.ActionCompleted = taskArgs =>
                {
                    var args = (ITaskArgs)taskArgs;
                    if (((LayoutManagerResult)args.TaskResult).RETURN_CODE == 0)
                    {
                        //LoadLayouts();
                        var item = _layoutItems.OrderBy(o => o.LayoutID).Last();
                        var idx = LayoutItems.IndexOf(item);
                        LayoutItems[idx] = item;
                        CurrentLayout = LayoutItems[idx];
                        MakeRibbonLayouts(LayoutItems);
                    }
                };
            Commands.Execute("COMMANDCOMPLETE", taskArguments);
        }

        private void ExecuteClearData(Layout layout)
        {
            SystemSounds.Exclamation.Play();
            //!!!!!!!!
            //CommandClassService.CurrentTaskArgs = new TaskArguments
            //    {
            //        Title = "Clear Data:",
            //        Command = "ClearData",
            //        CommandArguments = new ArrayList { layout },
            //        IsTimerOn = false,
            //        ProgressLimit = 100,
            //        ActionCompleted = null
            //    };

            //CommandManager.SendToExecute("COMMANDCOMPLETE");
            //StateService.Start<IStateService, string>(X => X.ClearData());

            //var url = new Uri("http://vmmapinfo.partnergsm.co.il/AcadNetGis/AcadNetStateService/StateService.svc");
            //using (var serviceChannelFactory = new WebChannelFactory<IStateService>(new WebHttpBinding(), url))
            //{
            //    var webchannel = serviceChannelFactory.CreateChannel();
            //    var result = webchannel.ClearData();
            //}
        }

        private void ExecuteDeleteLayout(Layout layout)
        {
            SystemSounds.Exclamation.Play();
            AcadNetManager.InteractionRequestViewModel.RaiseConfirmation();

            var taskArguments = Plugin.GetService<ITaskArguments>();
            taskArguments.Title = "Delete the project:";
            taskArguments.Command = "DELETE";
            taskArguments.CommandParameters = new ArrayList { layout };
            taskArguments.IsTimerOn = false;
            taskArguments.ProgressLimit = 100;
            taskArguments.ActionCompleted = taskArgs =>
                {
                    var args = (ITaskArgs)taskArgs;
                    if (((LayoutManagerResult)args.TaskResult).RETURN_CODE == 0)
                    {
                        //layout = (Layout)Args.CommandArguments[0]; //????
                        var idx = LayoutItems.IndexOf(layout);
                        var item = new Layout { LayoutState = -1 };
                        LayoutItems[idx] = item;
                        CurrentLayout = LayoutItems[idx];
                        MakeRibbonLayouts(LayoutItems);
                    }
                };
            Commands.Execute("COMMANDCOMPLETE", taskArguments);
        }

        private void ExecuteFilterLayout(Filter filter)
        {
            //if (filter.Active)
            //{
            //    foreach (var layoutFilter in LayoutFilterItems)
            //    {
            //        foreach (var prop in layoutFilter.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
            //                .Where(X => !(new[] { "Active", "FilterName", "Item" }).Contains(X.Name))) //.Where(X => X.GetValue(layoutFilter, null) != null)
            //        {
            //            var value = prop.GetValue(layoutFilter, null);
            //            if (value != null)
            //            {
            //                var propertyOrFieldName = typeof(Layout).GetProperty(prop.Name);
            //                _layoutItems = new ObservableCollection<Layout>(LayoutItems.WhereFilter(propertyOrFieldName.Name, value));
            //                OnPropertyChanged();
            //                CurrentLayout = LayoutItems.Count > 0 ? LayoutItems[0] : null;
            //            }
            //        }
            //    }
            //        MakeRibbonLayouts();
            //    }
            //    else
            //    {
            //        LoadLayouts();
            //        CurrentLayout = LayoutItems.Count > 0 ? LayoutItems[0] : null;
            //    }
            //    CurrentLayoutFilter.Save();
        }

        private void ExecuteFsaApplyLayout(Layout layout)
        {
            _commandLine.SendToExecute("PARTNERFSAAPPLY");
        }

        private void ExecuteFsaUploadLayout(Layout layout)
        {
            RbnExecuteUpload(null);
        }

        private void ExecuteOpenLayout(Layout layout)
        {
            Mouse.OverrideCursor = Cursors.Wait;

            var item = LayoutItems.FirstOrDefault(x => x.LayoutID == layout.LayoutID);
            if (item != null)
            {
                var fullPath = item.FindLayoutFullPath(); //"c:" + layout.CADFileName;
                if (fullPath != "" && item.IsLayoutLoaded())
                {
                    var doc = Application.DocumentManager.Cast<Document>()
                        .FirstOrDefault(x => x.Name.ToLower() == fullPath.ToLower());
                    if (doc != null)
                        Application.DocumentManager.MdiActiveDocument = doc;
                }
                else if (fullPath == "")
                {
                    var td = new TaskDialog
                    {
                        WindowTitle = PluginSettings.Name,
                        MainInstruction = "File not found!",
                        ContentText = "Change name or path to file using the explorer and try again!",
                        MainIcon = TaskDialogIcon.Shield,
                        CommonButtons = TaskDialogCommonButtons.Ok
                    };
                    td.Show();
                }
                else
                {
                    Documents.DocumentAction(fullPath, DocumentOptions.OpenAndActive);
                }
            }

            Mouse.OverrideCursor = null;
        }

        private void ExecuteOpenLayout(object sender, DocumentCollectionEventArgs e)
        {
            var item = LayoutItems.FirstOrDefault(x => x.CADFileName.ToLower() == e.Document.Name.ToLower());
            if (item != null && !LayoutExtensions.IsLayoutLoaded(item))
            {
                var idx = LayoutItems.IndexOf(item);
                LayoutItems[idx] = item;
                CurrentLayout = item;
            }
        }

        private void ExecuteRefreshLayout(Layout layout)
        {
            UiDispatcher.BeginInvoke(new Action(() =>
            {
                Mouse.OverrideCursor = Cursors.Wait;
                _eventAggregator.GetEvent<NotifyMessageStringEvent>().Publish("Refresh...");

                var item = LayoutItems.FirstOrDefault(x => x.LayoutID == layout.LayoutID);
                var lastIndex = item != null ? LayoutItems.IndexOf(item) : -1;
                var items = _layoutService.GetLayouts().ToList();

                items.Clear(OnlayoutPropertyChanged);
                LayoutItems = items.ToItems(OnlayoutPropertyChanged);

                if (_layoutItems.Any())
                    CurrentLayout = LayoutItems[lastIndex > 0 ? lastIndex : 0];

                var paletteTab = Plugin.GetService<ITabView>("ProjectExplorer");
                var grd = (CustomDataGrid)paletteTab.FindName("DataGrid");
                if (grd != null)
                    grd.Items.Refresh();
                //grd.SelectedItem = PluginBuilder.ProjectExplorerPanel.CurrentLayout;

                MakeRibbonLayouts(LayoutItems);

                _eventAggregator.GetEvent<NotifyMessageStringEvent>().Publish("Ready");
                Mouse.OverrideCursor = null;
            }));
        }

        private void ExecuteFind(object commandParameter)
        {
            var command = new CommandArgs(commandParameter, "Find", null);
            ExecuteCommand(command);
        }
        public void ExecuteEdit(object value)
        {
            //var command = value as CommandArgs
            //    ?? new CommandArgs(value, "Refresh", FileExplorerViewModel.CurrentFolder.FullPath);
            //ExecuteCommand(command);
            Process.Start(((IFolder)value).FullPath);
        }
        public void ExecuteRefresh(object value)
        {
            var command = value as CommandArgs
                ?? new CommandArgs(value, "Refresh", FileExplorerViewModel.CurrentFolder.FullPath);
            ExecuteCommand(command);
        }

        private async void ExecuteSetFolderBase(IFolder value)
        {
            Commands.RunOnUIThread(() => { Mouse.OverrideCursor = Cursors.Wait; Notifications.DisplayNotifyMessage(NotifyStatus.Working); });
            ShowAllFolders = false;
            await AddBaseFolder(value);
            Commands.RunOnUIThread(() => { Notifications.DisplayNotifyMessage(NotifyStatus.Ready); Mouse.OverrideCursor = null; });
        }

        private async void ExecuteTreeCleanMark(object control)
        {
            ShowAllFolders = true;
            FolderBase.IsCleaning = true;

            await Task.Run(() =>
            {
                Commands.RunOnUIThread(async () =>
                {
                    FolderBase.IsReloading = true;
                    FolderBase.IsCleaning = true;
                    //FolderBase.ReloadingCondition = (localFolder) => FileExplorerViewModel.CurrentFolder.FullPath == localFolder.FullPath;

                    var currentFolder = await ((FolderBase)FileExplorerViewModel.CurrentFolder).LoadChildrenAsync();

                    await ((CustomTreeView)control).Expand(null, currentFolder.FullPath, currentFolder.ToList());

                    FileExplorerViewModel.SetCurrentFolder(currentFolder);

                    FolderBase.IsReloading = false;
                    FolderBase.IsCleaning = false;
                });
            });

        }

        private void ExecuteShowAll(object control)
        {
            if (ShowAllFolders)
            {
                ExecuteCommand(new CommandArgs(control, "ShowAll", PluginSettings.WorksFolders.WorksObjects));
            }
            else
            {
                //IEnumerable<string> checkedItems = FileExplorerViewModel.GetCheckedPaths();
                //var items = checkedItems as IList<string> ?? checkedItems.ToList();

                //if (items.Any())
                //{
                //    if (!items.Any(X => AppSettings.CurrentFolder.Contains(X)))
                //        AppSettings.CurrentFolder = items.FirstOrDefault();

                //    FileExplorerViewModel.SetCheckedPaths(items.ToArray());
                //}
                //else
                //{
                //    Application.ShowAlertDialog("no checked folders!");
                //    ShowAllFolders = true;
                //}

                if (FileExplorerViewModel.BaseFolder == null)
                {
                    Application.ShowAlertDialog("no base folder. (Right click on folder)!");
                    ShowAllFolders = true;
                }

                if (!ShowAllFolders)
                    ExecuteCommand(new CommandArgs(control, "ShowAll", PluginSettings.WorksFolders.WorksObjects));

                //PluginSettings.IncludeFolders.AddRange(items.ToArray());
                //FileExplorer.Properties.Settings.Default.ProjectFolders.Clear();
                //FileExplorer.Properties.Settings.Default.ProjectFolders.AddRange(checkedItems.ToArray());
                //FileExplorer.Properties.Settings.Default.Save();
            }
            //FileExplorerViewModel.Items.CollectionChanged += (sender, e) => { Mouse.OverrideCursor = null; };
        }

        private async void ExecuteSave(object control)
        {
            if (!ShowAllFolders) return;

            string[] items = FileExplorerViewModel.GetCheckedPaths().ToArray();
            if (items.Any())
            {
                Mouse.OverrideCursor = Cursors.Wait;
                Notifications.DisplayNotifyMessage(NotifyStatus.Working);

                FileExplorerViewModel.BaseFolder = await new BaseVirtualFolder().SetFoldersAsync(items); ;
                OnPropertyChanged("BaseFolder");

                await ExecuteCommand(new CommandArgs(control, "Save", FileExplorerViewModel.BaseFolder));
            }
        }

        private void ExecuteUndeleteLayout(Layout layout)
        {
            SystemSounds.Exclamation.Play();
            var taskArguments = Plugin.GetService<ITaskArguments>();
            taskArguments.Title = "UnDelete the project:";
            taskArguments.Command = "UNDELETE";
            taskArguments.CommandParameters = new ArrayList { layout };
            taskArguments.IsTimerOn = false;
            taskArguments.ProgressLimit = 100;
            taskArguments.ActionCompleted = taskArgs =>
                {
                    var args = (ITaskArgs)taskArgs;
                    if (((LayoutManagerResult)args.TaskResult).RETURN_CODE == 0)
                    {
                        //var layout = (Layout)Args.CommandArguments[0];
                        var idx = LayoutItems.IndexOf(layout);
                        var item = new Layout { LayoutState = 1 };
                        LayoutItems[idx] = item;
                        CurrentLayout = LayoutItems[idx];
                        MakeRibbonLayouts(LayoutItems);
                    }
                };
            Commands.Execute("COMMANDCOMPLETE", taskArguments);
        }

        private void ExecutePurgeLayout(Layout layout)
        {
            SystemSounds.Exclamation.Play();
            var taskArgs = new TaskArguments
            {
                Title = "Purge the project:",
                Command = "PURGE",
                CommandParameters = new ArrayList { layout },
                Content = "WARNING!!! Operation is purges the layout without of recovery!\n",
                IsTimerOn = false,
                ProgressLimit = 100,
                ActionCompleted = args =>
                    {
                        var _args = (ITaskArgs)args;
                        if (((LayoutManagerResult)_args.TaskResult).RETURN_CODE > 0)
                        {
                            //var layout = (Layout)Args.CommandArguments[0];
                            LayoutItems.Remove(layout);

                            var item = LayoutItems.Last();
                            var idx = LayoutItems.IndexOf(item);

                            LayoutItems[idx] = item;
                            CurrentLayout = LayoutItems[idx];
                            MakeRibbonLayouts(LayoutItems);
                        }
                    }
            };
            var taskArguments = Plugin.Container.BuildUp<ITaskArguments>(taskArgs);
            Commands.Execute("COMMANDCOMPLETE", taskArguments);
        }

        private void ExecutePropertyGridRefresh(Func<Layout, bool> exp, Layout layout, string propertyName)
        {
            //PropertyDescriptor descriptor = TypeDescriptor.GetProperties(this)[propertyName];
            //ReadOnlyAttribute attr = (ReadOnlyAttribute)descriptor.Attributes[typeof(ReadOnlyAttribute)];
            //FieldInfo isReadOnly = attr.GetType().GetField("isReadOnly", BindingFlags.NonPublic | BindingFlags.Instance);
            //isReadOnly.SetValue(attr, false);
            //var pi = typeof (Layout).GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Instance);
            //object obj;
            //pi.GetValue(obj)

            var item = LayoutItems.Select(x => (Layout)x).FirstOrDefault(exp);
            if (item != null)
            {
                //item.IsLoaded = false;
                var idx = LayoutItems.IndexOf(item);
                LayoutItems[idx] = item;
                CurrentLayout = item;
            }
        }

        #endregion

        #endregion

        #region Ribbon Commands: ICommand, Execute, CanExecute

        public ICommand RbnMapView => _rbnMapView
            ?? (_rbnMapView = new Prism.Commands.DelegateCommand<RibbonButton>(RbnExecuteMapView, RbnCanExecuteMapView));

        public ICommand RbnPointOnMap => _rbnPointOnMap
            ?? (_rbnPointOnMap = new Prism.Commands.DelegateCommand<RibbonButton>(RbnExecuteSentToMap, RbnCanExecuteMapAction));

        public ICommand RbnFindOnMap => _rbnFindOnMap
            ?? (_rbnFindOnMap = new Prism.Commands.DelegateCommand<RibbonButton>(RbnExecuteFindOnMap, RbnCanExecuteMapAction));

        public ICommand RbnUcsChange => _rbnUcsChange
            ?? (_rbnUcsChange = new Prism.Commands.DelegateCommand<RibbonButton>(RbnExecuteUcsChange, RbnCanExecuteMapAction));

        public ICommand RbnCopyAsBlock => _rbnCopyAsBlock
            ?? (_rbnCopyAsBlock = new Prism.Commands.DelegateCommand<RibbonButton>(RbnExecuteCopyAsBlock, RbnCanExecuteMapAction));

        public ICommand RbnPasteAsBlock => _rbnPasteAsBlock ??
            (_rbnPasteAsBlock = new Prism.Commands.DelegateCommand<RibbonButton>(RbnExecutePasteAsBlock, RbnCanExecuteMapAction));

        public ICommand RbnCable => _rbnCable
            ?? (_rbnCable = new Prism.Commands.DelegateCommand<RibbonButton>(RbnExecuteCable, RbnCanExecuteMapAction));

        public ICommand RbnClosure => _rbnClosure
                    ?? (_rbnClosure = new Prism.Commands.DelegateCommand<RibbonButton>(RbnExecuteClosure, RbnCanExecuteMapAction));

        public ICommand RbnCabinet => _rbnCabinet
                    ?? (_rbnCabinet = new Prism.Commands.DelegateCommand<RibbonButton>(RbnExecuteCabinet, RbnCanExecuteMapAction));

        public ICommand RbnOpenExplorer => _rbnOpenExplorer
            ?? (_rbnOpenExplorer = new Prism.Commands.DelegateCommand<RibbonButton>(RbnExecuteExplorer, RbnCanExecuteExplorer));

        public ICommand RbnUpload => _rbnUpload
            ?? (_rbnUpload = new Prism.Commands.DelegateCommand<RibbonButton>(RbnExecuteUpload, RbnCanExecuteUpload));

        public ICommand RbnExport => _rbnExport
            ?? (_rbnExport = new Prism.Commands.DelegateCommand<RibbonButton>(RbnExecuteExport, RbnCanExecuteExport));

        public ICommand RbnApply => _rbnRun
            ?? (_rbnRun = new Prism.Commands.DelegateCommand<RibbonButton>(RbnExecuteApply, RbnCanExecuteRunFsa));

        public ICommand RbnOpenConfigCommand => _rbnOpenConfigCommand
            ?? (_rbnOpenConfigCommand = new Prism.Commands.DelegateCommand<RibbonButton>(RbnExecuteOpenConfig, RbnCanExecuteConfig));

        public ICommand RbnOpenLayoutCommand => _rbnOpenLayoutCommand
            ?? (_rbnOpenLayoutCommand = new Prism.Commands.DelegateCommand<RibbonButton>(RbnExecuteOpenLayout, RbnCanExecuteOpenLayout));

        public ICommand RbnActiveLayoutCommand => _rbnActiveLayoutCommand
            ?? (_rbnActiveLayoutCommand = new Prism.Commands.DelegateCommand<RibbonButton>(RbnExecuteActiveLayout, RbnCanExecuteActiveLayout));

        public ICommand RbnManageConfigCommand => _rbnOpenConfigManageCommand
            ?? (_rbnOpenConfigManageCommand = new Prism.Commands.DelegateCommand<RibbonButton>(RbnExecuteManageConfig, RbnCanExecuteConfigManage));

        public ICommand RbnRefresh
        {
            get
            {
                return _rbnRefresh ?? (_rbnRefresh = new Prism.Commands.DelegateCommand<RibbonButton>(rb => _commandLine.SendToExecute("PARTNERREFRESH", null), rb => IsPluginCompatible));
            }
        }
        public ICommand RbnPurge
        {
            get { return _rbnClean ?? (_rbnClean = new Prism.Commands.DelegateCommand<RibbonButton>(rb => _commandLine.SendToExecute("PARTNERPURGE", null), rb => IsPluginCompatible)); }
        }
        public ICommand RbnHelpCommand => _rbnHelpCommand
            ?? (_rbnHelpCommand = new Prism.Commands.DelegateCommand<RibbonButton>(RbnExecuteHelpCommand, RbnCanExecuteHelpCommand));

        // CanExecutes ===========================================
        private bool RbnCanExecuteMapAction(RibbonButton rb)
        {
            return Application.DocumentManager.MdiActiveDocument != null;
        }
        private bool RbnCanExecuteMapView(RibbonButton rb)
        {
            return Application.DocumentManager.MdiActiveDocument != null && !PluginSettings.Busy;
            //return CurrentLayout != null && CurrentLayout.IsLayoutLoaded()
            //    && _layoutItems.Any(X => X.FindLayoutFullPath() != "")
            //    && CanExecuteClearData(CurrentLayout);
            //IsMapInfoClosed;.Contains(ToolsManager.Doc.Name.ToLower()
        }
        private bool RbnCanExecuteOpenLayout(RibbonButton rb)
        {
            return CanExecuteOpenLayout((Layout)rb.CommandParameter);
        }
        private bool RbnCanExecuteActiveLayout(RibbonButton rb)
        {
            return rb.CommandParameter.ToString() != "";
        }
        private bool RbnCanExecuteConfig(RibbonButton rb)
        {
            return ConfigItems.Count > 0 && ConfigItems.Contains((Config)rb.CommandParameter);
        }
        private bool RbnCanExecuteConfigManage(RibbonButton rb)
        {
            return true;
        }
        private bool RbnCanExecuteExport(RibbonButton rb)
        {
            return false;
        }
        private bool RbnCanExecuteExplorer(RibbonButton rb)
        {
            return true; //IsTabButtonActive;
        }
        private bool RbnCanExecuteHelpCommand(RibbonButton rb)
        {
            return true;
        }
        private bool RbnCanExecuteUpload(RibbonButton rb)
        {
            var result = CurrentLayout != null && IsPluginCompatible && LayoutExtensions.IsLayoutLoaded(CurrentLayout)
                //&& Lsds2Context.LoFrames.Count > 0
                //&& Lsds2Context.LoFrames.Any(X => X.LayoutId == CurrentLayout.LayoutId) ////&& CurrentLayout.FSA 
                && _layoutItems.Any(x => LayoutExtensions.FindLayoutFullPath(x) == Doc.Name.ToLower());
            return result;
        }
        private bool RbnCanExecuteRunFsa(RibbonButton rb)
        {
            return IsPluginCompatible;
            //CurrentLayout.CADFileName.ToLower() == AcadToolsManager.Doc.Name.ToLower();CanExecuteFsaApplyLayout(CurrentLayout); //
        }

        // Executes ===========================================
        private void RbnExecuteMapView(RibbonButton rb)
        {
            _commandLine.SendToExecute("PARTNERMAPVIEW");
        }
        private void RbnExecuteSentToMap(RibbonButton rb)
        {
            _commandLine.SendToExecute("PARTNERPOINTONMAP");
            //Commands.Create("PARTNERPOINTONMAP",
            //    new TaskArguments() { DisplayName = "Send to map",
            //        CommandParameters = new ArrayList() { "100" }});
        }
        private void RbnExecuteFindOnMap(RibbonButton rb)
        {
            _commandLine.SendToExecute("PARTNERFINDONMAP");
        }
        private void RbnExecuteUcsChange(RibbonButton rb)
        {
            _commandLine.SendToExecute("PARTNERUCSCHANGE");
        }
        private void RbnExecuteCopyAsBlock(RibbonButton rb)
        {
            _commandLine.SendToExecute("PARTNERCOPYASBLOCK");
        }
        private void RbnExecutePasteAsBlock(RibbonButton rb)
        {
            _commandLine.SendToExecute("PARTNERPASTEASBLOCK");
        }
        private void RbnExecuteCable(RibbonButton rb)
        {
            _commandLine.SendToExecute("PARTNERCABLE");
        }
        private void RbnExecuteClosure(RibbonButton rb)
        {
            _commandLine.SendToExecute("PARTNERCLOSURE");
        }
        private void RbnExecuteCabinet(RibbonButton rb)
        {
            _commandLine.SendToExecute("PARTNERCABINET");
        }
        private void RbnExecuteOpenLayout(RibbonButton rb)
        {
            if (CanExecuteOpenLayout((Layout)rb.CommandParameter))
                ExecuteOpenLayout((Layout)rb.CommandParameter);
        }
        private void RbnExecuteActiveLayout(RibbonButton rb)
        {
            if (CanExecuteOpenLayout((Layout)rb.CommandParameter))
                ExecuteOpenLayout((Layout)rb.CommandParameter);
        }
        private void RbnExecuteOpenConfig(RibbonButton rb)
        {
            CurrentConfig = (Config)rb.CommandParameter;
        }
        private void RbnExecuteExplorer(RibbonButton rb)
        {
            _commandLine.SendToExecute("PARTNEREXPLORER");
        }
        private void RbnExecuteHelpCommand(RibbonButton rb)
        {
            var td = new TaskDialog
            {
                WindowTitle = "Plugin info:",
                MainInstruction = "Plugin name: IntelliDesk",
                ContentText = "version 3.0 for Partner (01.10.2015)",
                MainIcon = TaskDialogIcon.Information,
                CommonButtons = TaskDialogCommonButtons.Ok
            };
            td.Show();
        }
        private void RbnExecuteManageConfig(RibbonButton rb)
        {
            //((RibbonCombo)rb.CommandParameter).
        }
        private void RbnExecuteUpload(RibbonButton rb)
        {
            _commandLine.SendToExecute("PARTNERFSAUPLOAD", null);
        }
        private void RbnExecuteExport(RibbonButton rb)
        {
            _commandLine.SendToExecute("PARTNEREXPORT", null);
        }
        private void RbnExecuteApply(RibbonButton rb)
        {
            _commandLine.SendToExecute("PARTNERFSAAPPLY", null);
        }

        #endregion

        #region Methods

        public async Task<CommandArgs> ExecuteCommand(CommandArgs command)
        {
            Mouse.OverrideCursor = Cursors.Wait; Notifications.DisplayNotifyMessage(NotifyStatus.Working);

            bool isCursorDelay = false;
            switch (command.CommandName)
            {
                case "Search":
                    SimpleActionResult simpleResult = Files.FindPath(
                        FileExplorerViewModel.BaseFolder.Folders.Select(x => x.FullPath).ToArray(),
                        command.CommandParameter.ToString(), null);

                    if (simpleResult.StatusCode == HttpStatusCode.Found)
                        command.CommandParameter = simpleResult.ActionResult;
                    else
                        command.Cancel(new ErrorNotifyArgs(simpleResult.Message.ToString()));
                    break;

                case "Activate":
                    if (command.Sender != null && !((CustomTreeView)command.Sender).HasItems)
                    {
                        FileExplorerViewModel.CurrentFolder = FolderBase.BaseFolder.Folders.First();
                        LoadBaseFolder();
                    }
                    isCursorDelay = true;
                    break;

                case "Load":
                case "ShowAll":
                    if (this.ShowAllFolders)
                    {
                        if (!FileExplorerViewModel.Computer.IsFolderLoaded)
                            await LoadComputerFolders(FileExplorerViewModel.Computer);
                    }
                    else
                    {
                        //await AddBaseFolder();
                        LoadBaseFolder();
                    }
                    isCursorDelay = true;
                    break;
                case "Refresh":
                    if (!this.ShowAllFolders && FileExplorerViewModel.CurrentFolder != null)
                    {
                        if (FileExplorerViewModel.CurrentFolder != null)
                        {
                            await FileExplorerViewModel.CurrentFolder.LoadChildrenAsync(true);
                            FileExplorerViewModel.SetCurrentFolder(FileExplorerViewModel.CurrentFolder);
                        }
                    }
                    break;
            }

            if (!command.CancellationToken.IsCancellationRequested && command.CommandParameter != null)
            {
                if (command.CommandName == "Search" || command.CommandName == "Find" || command.CommandName == "Load")
                {
                    string path = command.CommandParameter.ToString();
                    string root = Path.GetPathRoot(path);
                    string searchPath = command.CommandParameter.ToString().Replace(root, "");

                    if (FileExplorerViewModel.CurrentFolder != null)
                        if (command.CommandName == "Load")
                        {
                            FileExplorerViewModel.SetCurrentFolder(FileExplorerViewModel.CurrentFolder);
                        }
                        else if (path != FileExplorerViewModel.CurrentFolder.FullPath)
                        {
                            await FileExplorerViewModel.BaseFolder.LoadFoldersByPathAsync(searchPath.ToList(), folder =>
                            {
                                FileExplorerViewModel.SetCurrentFolder(folder);
                            });
                        }
                }
                if (command.CommandName == "Save")
                {
                    LoadBaseFolder();
                    FileExplorerViewModel.SetCurrentFolder(FileExplorerViewModel.BaseFolder.Folders.First());
                    ShowAllFolders = false;
                }
            }

            if (isCursorDelay)
                Commands.DelayAction(1000, () =>
                {
                    Notifications.DisplayNotifyMessage(command.NotifyArgs);
                    Mouse.OverrideCursor = null;
                });
            else
            {
                Notifications.DisplayNotifyMessage(command.NotifyArgs);
                Mouse.OverrideCursor = null;
            }
            return command;
        }

        private async Task AddBaseFolder(IFolder folder)
        {
            if (folder != null && !PluginSettings.IncludeFolders.Contains(folder.FullPath))
            {
                PluginSettings.IncludeFolders.Add(folder.FullPath);
                PluginSettings.Save();

                FileExplorerViewModel.BaseFolder = await new BaseVirtualFolder().SetFoldersAsync(PluginSettings.IncludeFolders.ToArray()); ;
                LoadBaseFolder();
            }
        }

        private void LoadBaseFolder()
        {
            FileExplorerViewModel.Items.Clear();
            FileExplorerViewModel.Items.AddRange(FileExplorerViewModel.BaseFolder.Folders);
        }

        private async Task LoadComputerFolders(IFolder computer)
        {
            FileExplorerViewModel.Items.AddRange(new[] { new LocalFolder("Computer", null) { IsLoading = true } });

            await computer.LoadChildrenAsync();
            var workFolder = PluginSettings.IncludeFolders.FirstOrDefault() ?? PluginSettings.WorksFolders.WorksObjects;
            var pathRoot = Path.GetPathRoot(workFolder);
            var baseFolderPath = workFolder.Replace(pathRoot ?? "", "");
            var currentDrive = computer.Folders.FirstOrDefault(x => x.Name == pathRoot);

            if (currentDrive != null)
            {
                await currentDrive.GetChildrenAsync();

                IFolder baseFolder;
                if (!string.IsNullOrEmpty(baseFolderPath))
                {
                    baseFolder = await new BaseVirtualFolder().SetFoldersAsync(PluginSettings.IncludeFolders.ToArray());
                    //baseFolder = await currentDrive.LoadFoldersByPathAsync(baseFolderPath.ToList());
                }
                else
                {
                    baseFolder = currentDrive;
                    Plugin.Container.RegisterInstance("BaseFolder", baseFolder, new ContainerControlledLifetimeManager());
                }

                currentDrive.IsExpanded = true;

                FileExplorerViewModel.BaseFolder = baseFolder;
                FileExplorerViewModel.Items.Clear();
                FileExplorerViewModel.Items.AddRange(computer.Folders);
                FileExplorerViewModel.SetCurrentFolder(baseFolder.Folders.First());
                FileExplorerViewModel.IsLoading = false;
            }
        }

        private void LoadLayouts()
        {
            _layoutItems.Clear();

            _layoutItems = _uow.RepositoryAsync<Layout>()
                .Queryable()
                .ToItems()
                .WhenItemChanged(OnlayoutPropertyChanged);

            MakeRibbonLayouts(_layoutItems);
        }

        private void LoadRules()
        {
            // if saved currentUserSetting.Id not found then set Id as "0"
            if (LayoutItems.Count > 0 && CurrentUserSetting.LayoutId < 0)
            {
                var item = LayoutItems.FirstOrDefault();
                if (item != null)
                    CurrentUserSetting.LayoutId = item.LayoutID;
            }

            // if currentUserSetting.ConfigSetName not found then set ConfigSetName as "Default"
            if (CurrentUserSetting.ConfigSetName != "Default" && ConfigItems.All(x => x.ConfigSetName == CurrentUserSetting.ConfigSetName))
                CurrentUserSetting.ConfigSetName = "Default";

            _ruleItems = _uow.RepositoryAsync<Rule>()
                .Queryable()
                .WhenEmpty(() => new Rule())
                .Where(x => x.LayoutId == CurrentUserSetting.LayoutId)
                .ToItems();
        }

        private void MakeRibbonLayouts(ICollection<Layout> layoutItems)
        {
            if (_ribbonLayoutItems == null)
                _ribbonLayoutItems = new ObservableCollection<RibbonButton>();
            else
                _ribbonLayoutItems.Clear();

            var rb = new RibbonButton
            {
                CommandHandler = null,
                CommandParameter = "",
                Height = 32,
                IsToolTipEnabled = false,

                Name = "LayoutRibbonItem_none",
                Orientation = Orientation.Horizontal,
                Size = RibbonItemSize.Standard,
                ShowText = true,
                Text = "none",
                Id = "none"
            };
            _ribbonLayoutItems.Add(rb);
            CurrentRibbonLayout = rb;

            if (layoutItems.Count > 0)
                foreach (var layout in layoutItems.Where(x => !(x.LayoutState < 0)))
                {
                    rb = new RibbonButton
                    {
                        CommandHandler = RbnActiveLayoutCommand,
                        CommandParameter = layout.LayoutID,
                        Height = 32,
                        Name = "LayoutRibbonItem_" + layout.LayoutID,
                        Orientation = Orientation.Horizontal,
                        Size = RibbonItemSize.Standard,
                        ShowText = true,
                        Text = layout.LayoutName,
                        Tag = "",
                        Id = layout.LayoutID.ToString()
                    };
                    var tt = new RibbonToolTip
                    {
                        Command = "Command: PARTNERLAYOUTOPEN " + layout.CADFileName,
                        Content = "LayoutId =" + layout.LayoutID + ";\n" +
                                  "CADNameFile =" + layout.CADFileName + ";\n" +
                                  "FSA Uploaded =" + layout.FSA,
                        IsHelpEnabled = false,
                        Title = layout.LayoutName
                    };
                    rb.ToolTip = tt;
                    _ribbonLayoutItems.Add(rb);

                    if (layout.LayoutID == CurrentUserSetting.LayoutId)
                        CurrentRibbonLayout = rb;
                }
            //UIControlRefresh;
        }

        #region "Events"

        private void OnlayoutPropertyChanged(object sender, EntityChangedArgs e)
        {
            OnPropertyChanged("CurrentLayout");
            MakeRibbonLayouts(_layoutItems);
        }

        /// <summary> Occuring at layoutComboBoxDropDown on event Opened </summary>
        public void OnlayoutComboBoxCurrentChanged(object sender, RibbonPropertyChangedEventArgs e)
        {
            if ((e.NewValue != null) && ((RibbonButton)e.NewValue).CommandParameter.ToString() != "")

                // Sender.Tag == NULL specifies that the event has occurred by selecting user
                if (((RibbonCombo)sender).Tag != null)
                {
                    var layout = LayoutItems.FirstOrDefault(x => x.LayoutID == (decimal)((RibbonButton)e.NewValue).CommandParameter);
                    ExecuteOpenLayout(layout);
                }
        }

        public void OnDocumentActivated(object sender, DocumentCollectionEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            var currentRibbonLayoutName = "none";

            var item = (e == null)
                ? LayoutItems.FirstOrDefault(x => LayoutExtensions.FindLayoutFullPath(x) == Application.DocumentManager.MdiActiveDocument.Name.ToLower())
                : LayoutItems.FirstOrDefault(x => LayoutExtensions.FindLayoutFullPath(x) == e.Document.Name.ToLower());

            if (item != null)
            {
                currentRibbonLayoutName = Convert.ToString(item.LayoutID);
                CurrentLayout = item;

                if (!IsTabButtonActive)
                {
                    var view = ToolsManager.PaletteTabs.Current;
                    if (view.Name == "Explorer")
                    {
                        var grd = view.FindName("DataGrid") as DataGrid;
                        if (grd != null) grd.SelectedItem = item;
                    }
                }
                IsLayoutReadOnly = !LayoutExtensions.IsLayoutReadOnly(item);
            }

            CurrentRibbonLayout = RibbonLayoutItems.FirstOrDefault(x => x.Id == currentRibbonLayoutName);

            IsPluginCompatible = true; //IsDwgCompatible();

            //if (ComponentManager.Ribbon != null && ComponentManager.Ribbon.IsVisible)
            //    ComponentManager.Ribbon.Focus();
            //else
            //    UIHelper.AcadWindowSetFocus(Application.MainWindow.Handle);

            Mouse.OverrideCursor = null;
        }

        public void OnDocumentToBeDeactivated(object sender, DocumentCollectionEventArgs documentCollectionEventArgs)
        {
            //if (Lsds2Context.LoFrames.Count > 0 && LayoutItems.Any(X => X.CADFileName.ToLower() == documentCollectionEventArgs.Document.Name.ToLower()))
            //{
            //    var options = new TaskDialogOptions
            //    {
            //        Title = ProjectManager.Name + " new task: Apply FSA",
            //        MainInstruction = "The FSA has been applyed! On change current layout the frames will not saved",
            //        Content = "Do you want to change layout?",
            //        MainIcon = VistaTaskDialogIcon.Warning,
            //        Handle = Application.MainWindow.Handle,
            //        CustomButtons = new[] { "&Close without frame's save", "&Cancel" },
            //    };
            //    var result = TaskDialogInterop.TaskDialog.Show(options);
            //    if (result.CustomButtonResult == 1)
            //        Application.DocumentManager.MdiActiveDocument = documentCollectionEventArgs.Document;
            //}
        }

        #endregion

        /// <summary> Test </summary>
        private void UIControlRefresh()
        {
            var ribbomItem = ComponentManager.Ribbon.FindItem("LayoutRibbonComboId", true);
            if (ribbomItem != null) ((RibbonCombo)ribbomItem).Current = CurrentRibbonLayout;
        }

        //public void SaveChanges<T>(T entity, params string[] properties) where T : class
        //{
        //    _layoutsRepository.SetUpdate(entity, (properties.Length > 0)
        //        ? properties
        //        : CurrentLayout.ChangedProperties.ToArray());
        //}

        public void SaveChanges(Layout entity, string propertyName, object value)
        {
            _uow.Repository<Layout>().Update((Layout)entity, propertyName, value);
        }

        #endregion

        #region Validations

        private bool IsValid(DependencyObject obj)
        {
            // The dependency object is valid if it has no errors, 
            //and all of its children (that are dependency objects) are error-free.
            return !Validation.GetHasError(obj) &&
                LogicalTreeHelper.GetChildren(obj)
                .OfType<DependencyObject>()
                .All(child => IsValid(child));
        }

        #endregion "Validations"

        #region IDataErrorInfo Members

        public string this[string propertyName]
        {
            get
            {
                //string error = (CurrentLayout as IDataErrorInfo)[propertyName];
                //validProperties[propertyName] = String.IsNullOrEmpty(error);
                //ValidateProperties();
                //if (IsPropertiesValid)
                //    _layoutsRepository.SetLayoutUpdate(CurrentLayout); //this.CurrentLayout.LayoutId, propertyName, true
                //CommandManager.InvalidateRequerySuggested();
                //return error;
                //if (propertyName == "CurrentLayout" && !File.Exists(CurrentLayout.CADFileName))
                //{
                //    return "File not found";
                //}
                return null;
            }
        }

        public string Error
        {
            get { return (CurrentLayout as IDataErrorInfo).Error; }
        }

        #endregion "IDataErrorInfo Members"

        public bool IsActive { get; set; }

        public event EventHandler IsActiveChanged;
    }
}

//internal static class AttributesValidation
//{
//    public static string Validate(IDataErrorInfo source, string columnName)
//    {
//        var type = source.GetType();
//        var property = type.GetProperty(columnName);
//        var validators = (ValidationAttribute[])property.GetCustomAttributes(typeof(ValidationAttribute), true);
//        if (validators.Any())
//        {
//            var value = property.GetValue(source, null);
//            var errors = validators.Where(v => !v.IsValid(value)).Select(v => v.ErrorMessage ?? "").ToArray();
//            return string.Join(Environment.NewLine, errors);
//        }
//        return string.Empty;
//    }
//}

//    if (FullPath == "")
//    {
//        Mouse.OverrideCursor = null;
//        EventAggregator.GetEvent<NotifyMessageStringEvent>().Publish("Ready");

//        var td = new TaskDialog
//        {
//            WindowTitle = rb.GroupName,
//            MainInstruction = "File not found!",
//            ContentText = "Change name or path to file using the explorer and try again!",
//            MainIcon = TaskDialogIcon.Shield,
//            CommonButtons = TaskDialogCommonButtons.Ok
//        };
//        td.Show();
//    }
