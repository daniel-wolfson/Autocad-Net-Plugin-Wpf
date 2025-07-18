using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.Windows;
using FileExplorer.Extensions;
using FileExplorer.Model;
using FileExplorer.ViewModel;
using ID.Infrastructure;
using ID.Infrastructure.Commands;
using ID.Infrastructure.Enums;
using ID.Infrastructure.Extensions;
using ID.Infrastructure.General;
using ID.Infrastructure.Interfaces;
using ID.Infrastructure.Models;
using Intellidesk.AcadNet.Common.Core;
using Intellidesk.AcadNet.Common.Enums;
using Intellidesk.AcadNet.Common.Extensions;
using Intellidesk.AcadNet.Common.Interfaces;
using Intellidesk.AcadNet.Common.Internal;
using Intellidesk.AcadNet.Common.Models;
using Intellidesk.AcadNet.Components;
using Intellidesk.AcadNet.Core;
using Intellidesk.Data.General;
using Intellidesk.Data.Models;
using Intellidesk.Data.Models.Cad;
using Intellidesk.Data.Repositories;
using Intellidesk.Data.Repositories.EF6.UnitOfWork;
using Intellidesk.Data.Services;
using Prism.Commands;
using Prism.Events;
using Serilog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Unity;
using Unity.Lifetime;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Files = ID.Infrastructure.Files;
using ILayout = Intellidesk.Data.Models.Cad.ILayout;
using IPanelTabView = Intellidesk.AcadNet.Common.Interfaces.IPanelTabView;
using LayoutExtensions = Intellidesk.AcadNet.Core.LayoutExtensions;
using Rule = Intellidesk.Data.Models.Cad.Rule;
using User = Intellidesk.Data.Models.Cad.User;
using UserSetting = Intellidesk.Data.Models.Cad.UserSetting;

namespace Intellidesk.AcadNet.ViewModels
{
    /// <summary> Main view model </summary>
    public class ProjectExplorerPanelContext : BaseViewModel, IProjectExplorerPanelContext, IDataErrorInfo
    {
        #region Private variables

        //private readonly IUnityContainer UnityContainer;
        private readonly ICommandLine _commandLine;
        private readonly ILayoutService _layoutService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IUnitOfWorkAsync _uow = null;

        // Config and Layout
        private Config _currentConfig;
        private Document _currenDocument;
        private ILayout _currentLayout;
        private Rule _currentRule;
        private User _currentUser;
        private Filter _currentlayoutfilter;
        private RibbonButton _currentRibbonLayout;
        private string[] _layoutsContentsItems;


        // ProjectExplorer
        //private bool _showAll;
        private bool _isLayoutReadOnly;
        private ObservableCollection<ILayout> _layoutItems;
        private ICommand _fsaApplyLayoutCommand, _fsaUploadLayoutCommand;
        private ICommand _findCommand, _layoutFilterCommand;
        private ICommand _newLayoutCommand, _clearDwgBufferCommand;

        private ICommand _deleteLayoutCommand, _purgeLayoutCommand;
        private ICommand _editCommand, _workItemsDisplayCommand, _workItemsOpenAllCommand, _refreshCommand, _closeCommand;
        private ICommand _showAllFoldersCommand;

        private ICommand _setFolderBaseCommand, _addToLayotsCommand, _gotoFolderCommand, _removeToLayotsCommand, _treeCleanMarkCommand;
        private ICommand _openLayoutCommand, _saveLayoutCommand, _undeleteLayoutCommand;

        private ICommand _rbnExport, _rbnOpenExplorer, _rbnOpenSearch;
        private ICommand _rbnActiveLayoutCommand, _rbnOpenConfigCommand, _rbnOpenLayoutCommand,
                         _rbnOpenConfigManageCommand, _raiseSaveConfirmationCommand;
        private ICommand _rbnUpload, _rbnOpenAll, _rbnRun, _rbnRefresh, _rbnLayerQueries, _rbnBayQueries, _rbnClean, _rbnRuler, _rbnPlotW;
        private ICommand _rbnMapView, _rbnPointOnMap, _rbnFindOnMap, _rbnUcsChange, _rbnConvertToMarkers;
        private ICommand _rbnCopyAsBlock, _rbnPasteAsBlock;
        private ICommand _rbnCable, _rbnClosure, _rbnClosureConnect, _rbnCabinet, _rbnHelpCommand;

        #endregion
        public ISearchViewModel SearchViewModel { get; }

        private readonly FileExplorerViewModel _fileExplorerViewModel;
        public IFileExplorerViewModel FileExplorerViewModel => _fileExplorerViewModel;

        #region <ctor>
        public ProjectExplorerPanelContext()
        {
            ShowAllFolders = PluginSettings.ShowAllFolders;
            _commandLine = Plugin.GetService<ICommandLine>();
            _uow = Plugin.GetService<IUnitOfWorkAsync>();
            _eventAggregator = Plugin.GetService<IEventAggregator>();
            _fileExplorerViewModel = new FileExplorerViewModel();
            SearchViewModel = new SearchViewModel() { Parent = this };

            //var userService = Plugin.GetService<IAdminService>();
            //_currentUser = userService.GetUserByName(Environment.UserName);
            //_userSettingItems = _currentUser.Settings.ToItems();
            //CurrentUserSetting = _userSettingItems.FirstOrDefault(x => x.IsActive);

            _projectExplorerFoldersRowDefinitionHeight = PluginSettings.ProjectExplorerFoldersSectionHeight;
            _projectExplorerFilesSectionHeight = PluginSettings.ProjectExplorerFilesSectionHeight;

            if (PluginSettings.ProjectExplorerPropertySectionDisplay)
                _projectExplorerPropertySectionHeight = PluginSettings.ProjectExplorerPropertySectionHeight;

            if (CurrentUserSetting != null)
            {
                ToggleLayoutDataTemplateSelector = CurrentUserSetting.ToggleLayoutDataTemplateSelector;

                //IFilterService
                var layoutFilterService = Plugin.GetService<IFilterService>();
                LayoutFilterItems = layoutFilterService.GetFilters().ToItems();
                CurrentLayoutFilter = LayoutFilterItems.FirstOrDefault();

                //ILayoutService
                _layoutService = Plugin.GetService<ILayoutService>();
                _layoutItems = _layoutService.GetLayouts().ToItems(OnlayoutPropertyChanged);
                CurrentLayout = LayoutItems.FirstOrDefault(x => x.LayoutID == CurrentUserSetting.LayoutId) ??
                                LayoutItems.FirstOrDefault();

                MakeRibbonLayouts(_layoutItems);

                var dataConfigService = Plugin.GetService<IDataConfigService>();
                ConfigItems = dataConfigService.GetItems();
                CurrentConfig = ConfigItems.FirstOrDefault(x => x.ConfigSetName == CurrentUserSetting.ConfigSetName);
            }
        }
        #endregion

        #region <Properties>
        public Action<object> Load { get; set; }
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
        public ObservableCollection<Config> ConfigItems { get; }

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
        public ObservableCollection<ILayout> LayoutItems
        {
            get { return _layoutItems; }
            set
            {
                _layoutItems = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<RibbonButton> _layoutItemButtons = new ObservableCollection<RibbonButton>();
        /// <summary> Layouts ribbon buttons </summary>
        public ObservableCollection<RibbonButton> LayoutItemButtons
        {
            get { return _layoutItemButtons; }
            set
            {
                _layoutItemButtons = value;
                this.RibbonRefresh();
                OnPropertyChanged();
            }
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
        public ILayout CurrentLayout
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
        public ObservableCollection<Rule> RuleItems { get; private set; }

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
        public ObservableCollection<UserSetting> UserSettingItems { get; }

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
        public ObservableCollection<Filter> LayoutFilterItems { get; }

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

        private string _resultMessage;
        public string InteractionResultMessage
        {
            get { return _resultMessage; }
            set { _resultMessage = value; OnPropertyChanged(); }
        }
        private ICommand _raiseNotificationCommand;

        #region <Palette ICommands, Executes, CanExecutes>

        #region <Commands ICommand>

        public ICommand CreateLayoutCommand => _newLayoutCommand ??
                (_newLayoutCommand = new DelegateCommand<ILayout>(ExecuteCreateLayout, CanExecuteCreateLayout));

        public ICommand ClearDataCommand => _clearDwgBufferCommand ??
                (_clearDwgBufferCommand = new DelegateCommand<ILayout>(ExecuteClearData, CanExecuteClearData));

        public ICommand DeleteLayoutCommand => _deleteLayoutCommand ??
                (_deleteLayoutCommand = new DelegateCommand<ILayout>(ExecuteDeleteLayout, CanExecuteDeleteLayout));

        public ICommand EditCommand => _editCommand ??
                (_editCommand = new DelegateCommand<object>(ExecuteEdit, CanExecuteByBusy));

        public ICommand WorkItemsDisplayCommand => _workItemsDisplayCommand ??
                (_workItemsDisplayCommand = new DelegateCommand<object>(ExecuteDisplayWorkItems, CanExecuteByBusy));

        public ICommand WorkItemsOpenAllCommand => _workItemsOpenAllCommand ??
                (_workItemsOpenAllCommand = new DelegateCommand<object>(ExecuteWorkItemsOpenAll, CanExecuteByBusy));

        public ICommand RefreshCommand => _refreshCommand ??
                (_refreshCommand = new DelegateCommand<object>(ExecuteRefreshCommand, CanExecuteByBusy));

        public ICommand SetFolderBaseCommand => _setFolderBaseCommand ??
                (_setFolderBaseCommand = new DelegateCommand<IFolder>(ExecuteSetFolderBase, CanExecuteByBusy));
        public ICommand GoToFolderCommand => _gotoFolderCommand ??
                        (_gotoFolderCommand = new DelegateCommand<IFile>(ExecuteGoToFolder, CanExecuteByBusy));
        public ICommand AddToLayotsCommand => _addToLayotsCommand ??
                (_addToLayotsCommand = new DelegateCommand<IFile>(ExecuteAddToLayouts, CanExecuteByBusy));

        public ICommand RemoveLayoutCommand => _removeToLayotsCommand ??
                (_removeToLayotsCommand = new DelegateCommand<IFile>(ExecuteRemoveLayouts, CanExecuteByBusy));
        public ICommand FindFolderCommand => _findCommand ??
                (_findCommand = new DelegateCommand<object>(ExecuteFind, CanExecuteByBusy));

        public ICommand TreeCleanMarkCommand => _treeCleanMarkCommand ??
                (_treeCleanMarkCommand = new DelegateCommand<object>(ExecuteTreeCleanMark, CanExecuteByBusy));

        public ICommand FilterLayoutCommand => _layoutFilterCommand ??
                (_layoutFilterCommand = new DelegateCommand<Filter>(ExecuteFilterLayout, x => true));

        public ICommand FsaApplyLayoutCommand => _fsaApplyLayoutCommand ??
                (_fsaApplyLayoutCommand = new DelegateCommand<ILayout>(ExecuteFsaApplyLayout, CanExecuteFsaApplyLayout));

        public ICommand FsaUploadLayoutCommand => _fsaUploadLayoutCommand ??
                (_fsaUploadLayoutCommand = new DelegateCommand<ILayout>(ExecuteFsaUploadLayout, CanExecuteFsaUploadLayout));

        public ICommand OpenLayoutCommand => _openLayoutCommand ??
                (_openLayoutCommand = new DelegateCommand<ILayout>(ExecuteOpenLayout, CanExecuteOpenLayout));

        public ICommand PurgeLayoutCommand => _purgeLayoutCommand ??
                (_purgeLayoutCommand = new DelegateCommand<ILayout>(ExecutePurgeLayout, CanExecutePurgeLayout));

        public ICommand SaveCommand => _saveLayoutCommand ??
                (_saveLayoutCommand = new DelegateCommand<object>(ExecuteSave, CanExecuteSaveLayout));

        public ICommand ShowAllFoldersCommand => _showAllFoldersCommand ??
                (_showAllFoldersCommand = new DelegateCommand<object>(ExecuteShowAll, CanExecuteByBusy));

        public ICommand UndeleteLayoutCommand => _undeleteLayoutCommand ??
                (_undeleteLayoutCommand = new DelegateCommand<ILayout>(ExecuteUndeleteLayout, CanExecuteUndeleteLayout));

        #endregion

        #region Commands CanExecute

        private bool CanExecuteByBusy(object item)
        {
            return !PluginSettings.Busy;
        }
        private bool CanExecuteCreateLayout(ILayout layout)
        {
            return LayoutItems != null;
        }
        private bool CanExecuteDeleteLayout(ILayout layout)
        {
            return true; //layout.LayoutState == null || layout.LayoutState >= 0;
        }
        private bool CanExecuteClearData(ILayout layout)
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
        private bool CanExecuteFsaApplyLayout(ILayout layout)
        {
            return RbnCanExecuteRunFsa(null);
        }
        private bool CanExecuteFsaUploadLayout(ILayout layout)
        {
            return RbnCanExecuteUpload(null);
        }
        private bool CanExecuteOpenLayout(ILayout layout)
        {
            var result = layout != null && !layout.IsModified() && layout.IsValid()
                && (layout.LayoutState == null || layout.LayoutState >= 0);
            return result;
        }
        private bool CanExecutePurgeLayout(ILayout layout)
        {
            return layout != null && LayoutItems.Count > 0 && LayoutItems.Contains((ILayout)layout)
                && (layout.LayoutState != null && layout.LayoutState < 0);
        }
        private bool CanExecuteSaveLayout(object item)
        {
            return true;
            //return item != null && item.DataInfo != null && item.DataInfo.IsModified() && item.DataInfo.IsValid();
            //&& LayoutItems.Contains(layout)
        }
        private bool CanExecuteUndeleteLayout(ILayout layout)
        {
            return layout != null && LayoutItems.Count > 0 && LayoutItems.Contains(layout)
                && (layout.LayoutState != null && layout.LayoutState < 0);
        }

        #endregion

        #region <Commands Execute>

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

        private void ExecuteCreateLayout(ILayout layout)
        {
            SystemSounds.Exclamation.Play();
            var newLayout = new ILayout
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
            CommandLine.Execute("COMMANDCOMPLETE", taskArguments);
        }

        private void ExecuteClearData(ILayout layout)
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

        private void ExecuteDeleteLayout(ILayout layout)
        {
            SystemSounds.Exclamation.Play();

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
                        var item = new ILayout { LayoutState = -1 };
                        LayoutItems[idx] = item;
                        CurrentLayout = LayoutItems[idx];
                        MakeRibbonLayouts(LayoutItems);
                    }
                };
            CommandLine.Execute("COMMANDCOMPLETE", taskArguments);
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

        private void ExecuteFsaApplyLayout(ILayout layout)
        {
            _commandLine.SendToExecute(CommandNames.Apply);
        }

        private void ExecuteFsaUploadLayout(ILayout layout)
        {
            RbnExecuteUpload(null);
        }

        private void ExecuteOpenLayout(ILayout layout)
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

        private void ExecuteRefreshLayout(ILayout layout)
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

                var paletteTab = Plugin.GetService<IPanelTabView>("ProjectExplorer");
                var grd = (CustomDataGrid)paletteTab.FindName("DataGrid");
                if (grd != null)
                    grd.Items.Refresh();
                //grd.SelectedItem = PluginBuilder.ProjectExplorerViewModel.CurrentLayout;

                MakeRibbonLayouts(LayoutItems);

                _eventAggregator.GetEvent<NotifyMessageStringEvent>().Publish("Ready");
                Mouse.OverrideCursor = null;
            }));
        }

        private async void ExecuteFind(object commandParameter)
        {
            using (CommandArgs command = new CommandArgs(commandParameter, "Find", null))
            {
                await ExecuteCommand(command);
            }
        }

        public void ExecuteEdit(object value)
        {
            //var command = value as CommandArgs
            //    ?? new CommandArgs(value, "Refresh", FileExplorerViewModel.CurrentFolder.FullPath);
            //ExecuteCommand(command);
            Process.Start(((IFolder)value).FullPath);
        }

        public async void ExecuteDisplayWorkItems(object value)
        {
            await ExecuteCommand(new CommandArgs(value, "WorkItems",
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\IntelliDesk\\WorkItems"));
        }

        public void ExecuteWorkItemsOpenAll(object value)
        {
            _commandLine.SendToExecute(CommandNames.WorkFilesOpenAll);
        }

        public async void ExecuteRefreshCommand(object value)
        {
            CommandArgs command = value as CommandArgs ?? new CommandArgs(value, "Refresh", FileExplorerViewModel.CurrentFolder.FullPath);
            await ExecuteCommand(command);
        }

        private async void ExecuteGoToFolder(IFile value)
        {
            Notifications.DisplayNotifyMessage(NotifyStatus.Working);

            if (value.FullPath.Contains(".lnk"))
                await ExecuteCommand(new CommandArgs(null, "FIND", Path.GetDirectoryName(Path.GetFullPath(Files.GetShortcutTargetFile(value.FullPath)))));
            else
                await ExecuteCommand(new CommandArgs(null, "FIND", Path.GetDirectoryName(Path.GetFullPath(value.FullPath))));

            Notifications.DisplayNotifyMessage(NotifyStatus.Ready);
        }

        private void ExecuteAddToLayouts(IFile value)
        {
            if (!value.FullPath.Contains(".lnk"))
            {
                Notifications.DisplayNotifyMessage(NotifyStatus.Working);

                //_layoutService.AddAndSave(value.FullPath, "OnLoadCleaning", "commandArgs");
                //LayoutItems = _layoutService.GetLayouts().ToItems();
                Files.CreateShortcutTargetFile(value.FullPath);

                if (ComponentManager.Ribbon != null)
                    MakeRibbonLayouts(LayoutItems);

                Notifications.DisplayNotifyMessage(NotifyStatus.Ready);
            }
        }

        private void ExecuteRemoveLayouts(IFile value)
        {
            InfraManager.RunOnUIThread(() => { Mouse.OverrideCursor = Cursors.Wait; Notifications.DisplayNotifyMessage(NotifyStatus.Working); });

            //_layoutService.AddAndSave(value.FullPath, "OnLoadCleaning", "commandArgs");
            //LayoutItems = _layoutService.GetLayouts().ToItems();

            try
            {
                var workItemsFolderPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Intellidesk", "WorkItems");
                var fileName = Files.GetFileName(value.FullPath);
                var path = Path.Combine(workItemsFolderPath, fileName + (!fileName.Contains(".lnk") ? ".lnk" : ""));
                File.Delete(path);

                if (ComponentManager.Ribbon != null)
                    MakeRibbonLayouts(LayoutItems);

                if (FileExplorerViewModel.CurrentFolder.FullPath.ToLower() == workItemsFolderPath.ToLower())
                    ExecuteRefreshCommand(null);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex.InnerException?.Message ?? ex.Message);
            }

            InfraManager.RunOnUIThread(() => { Notifications.DisplayNotifyMessage(NotifyStatus.Ready); Mouse.OverrideCursor = null; });
        }
        private async void ExecuteSetFolderBase(IFolder value)
        {
            Notifications.DisplayNotifyMessage(NotifyStatus.Working);
            ShowAllFolders = false;
            await AddBaseFolder(value);
            Notifications.DisplayNotifyMessage(NotifyStatus.Ready);
        }

        private async void ExecuteTreeCleanMark(object control)
        {
            ShowAllFolders = true;
            FolderBase.IsCleaning = true;

            await Task.Run(() =>
            {
                InfraManager.RunOnUIThread(async () =>
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

        private async void ExecuteShowAll(object control)
        {
            if (ShowAllFolders)
            {
                await ExecuteCommand(new CommandArgs(control, "ShowAll", PluginSettings.IncludeFolders));
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
                    await ExecuteCommand(new CommandArgs(control, "ShowAll", PluginSettings.IncludeFolders));

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

                LocalVirtualFolder localVirtualFolder = new LocalVirtualFolder();
                FileExplorerViewModel.BaseFolder = await localVirtualFolder.SetFoldersAsync(items); ;
                OnPropertyChanged("BaseFolder");

                await ExecuteCommand(new CommandArgs(control, "Save", FileExplorerViewModel.BaseFolder));
            }
        }

        private void ExecuteUndeleteLayout(ILayout layout)
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
                        var item = new ILayout { LayoutState = 1 };
                        LayoutItems[idx] = item;
                        CurrentLayout = LayoutItems[idx];
                        MakeRibbonLayouts(LayoutItems);
                    }
                };
            CommandLine.Execute("COMMANDCOMPLETE", taskArguments);
        }

        private void ExecutePurgeLayout(ILayout layout)
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
            CommandLine.Execute("COMMANDCOMPLETE", taskArguments);
        }

        private void ExecutePropertyGridRefresh(Func<ILayout, bool> exp, ILayout layout, string propertyName)
        {
            //PropertyDescriptor descriptor = TypeDescriptor.GetProperties(this)[propertyName];
            //ReadOnlyAttribute attr = (ReadOnlyAttribute)descriptor.Attributes[typeof(ReadOnlyAttribute)];
            //FieldInfo isReadOnly = attr.GetType().GetField("isReadOnly", BindingFlags.NonPublic | BindingFlags.Instance);
            //isReadOnly.SetValue(attr, false);
            //var pi = typeof (Layout).GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Instance);
            //object obj;
            //pi.GetValue(obj)

            var item = LayoutItems.Select(x => (ILayout)x).FirstOrDefault(exp);
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

        #region <Ribbon Commands: ICommand, Execute, CanExecute>

        public ICommand CloseCommand => _closeCommand ?? (_closeCommand = new DelegateCommand<object>(ExecuteClose));

        public ICommand RbnMapView => _rbnMapView
            ?? (_rbnMapView = new DelegateCommand<RibbonButton>(RbnExecuteMapView, RbnCanExecuteMapView));

        public ICommand RbnPointOnMap => _rbnPointOnMap
            ?? (_rbnPointOnMap = new DelegateCommand<RibbonButton>(RbnExecuteSentToMap, RbnCanExecuteMapAction));

        public ICommand RbnFindOnMap => _rbnFindOnMap
            ?? (_rbnFindOnMap = new DelegateCommand<RibbonButton>(RbnExecuteFindOnMap, RbnCanExecuteMapAction));

        public ICommand RbnUcsChange => _rbnUcsChange
            ?? (_rbnUcsChange = new DelegateCommand<RibbonButton>(RbnExecuteUcsChange, RbnCanExecuteMapAction));

        public ICommand RbnConvertToMarkers => _rbnConvertToMarkers
                    ?? (_rbnConvertToMarkers = new DelegateCommand<RibbonButton>(RbnExecuteConvertToMarkers, RbnCanExecuteMapAction));

        public ICommand RbnCopyAsBlock => _rbnCopyAsBlock
            ?? (_rbnCopyAsBlock = new DelegateCommand<RibbonButton>(RbnExecuteCopyAsBlock, RbnCanExecuteMapAction));

        public ICommand RbnPasteAsBlock => _rbnPasteAsBlock ??
            (_rbnPasteAsBlock = new DelegateCommand<RibbonButton>(RbnExecutePasteAsBlock, RbnCanExecuteMapAction));

        public ICommand RbnCable => _rbnCable
            ?? (_rbnCable = new DelegateCommand<RibbonButton>(RbnExecuteCable, RbnCanExecuteMapAction));

        public ICommand RbnClosure => _rbnClosure
                    ?? (_rbnClosure = new DelegateCommand<RibbonButton>(RbnExecuteClosure, RbnCanExecuteMapAction));

        public ICommand RbnClosureConnect => _rbnClosureConnect
                    ?? (_rbnClosureConnect = new DelegateCommand<RibbonButton>(RbnExecuteClosureConnect, RbnCanExecuteMapAction));

        public ICommand RbnCabinet => _rbnCabinet
                    ?? (_rbnCabinet = new DelegateCommand<RibbonButton>(RbnExecuteCabinet, RbnCanExecuteMapAction));

        public ICommand RbnOpenExplorer => _rbnOpenExplorer
            ?? (_rbnOpenExplorer = new DelegateCommand<RibbonButton>(RbnExecuteExplorer, RbnCanExecuteExplorer));

        public ICommand RbnOpenSearch => _rbnOpenSearch
            ?? (_rbnOpenSearch = new DelegateCommand<RibbonButton>(RbnExecuteSearch, RbnCanExecuteExplorer));

        public ICommand RbnOpenAll => _rbnOpenAll
            ?? (_rbnOpenAll = new DelegateCommand<RibbonButton>(RbnExecuteOpenAllWorkFiles));
        public ICommand RbnUpload => _rbnUpload
                   ?? (_rbnUpload = new DelegateCommand<RibbonButton>(RbnExecuteUpload, RbnCanExecuteUpload));
        public ICommand RbnExport => _rbnExport
            ?? (_rbnExport = new DelegateCommand<RibbonButton>(RbnExecuteExport, RbnCanExecuteExport));

        public ICommand RbnApply => _rbnRun
            ?? (_rbnRun = new DelegateCommand<RibbonButton>(RbnExecuteApply, RbnCanExecuteRunFsa));

        public ICommand RbnOpenConfigCommand => _rbnOpenConfigCommand
            ?? (_rbnOpenConfigCommand = new DelegateCommand<RibbonButton>(RbnExecuteOpenConfig, RbnCanExecuteConfig));

        public ICommand RbnOpenLayoutCommand => _rbnOpenLayoutCommand
            ?? (_rbnOpenLayoutCommand = new DelegateCommand<RibbonButton>(RbnExecuteOpenLayout, RbnCanExecuteOpenLayout));

        public ICommand RbnActiveLayoutCommand => _rbnActiveLayoutCommand
            ?? (_rbnActiveLayoutCommand = new DelegateCommand<RibbonButton>(RbnExecuteActiveLayout, RbnCanExecuteActiveLayout));

        public ICommand RbnManageConfigCommand => _rbnOpenConfigManageCommand
            ?? (_rbnOpenConfigManageCommand = new DelegateCommand<RibbonButton>(RbnExecuteManageConfig, RbnCanExecuteConfigManage));

        public ICommand RbnLayerQueries => _rbnLayerQueries
            ?? (_rbnLayerQueries = new DelegateCommand<RibbonButton>(rb =>
                _commandLine.SendToExecute(CommandNames.LayerQueries, null), rb => IsPluginCompatible));

        public ICommand RbnBayQueries => _rbnBayQueries
             ?? (_rbnBayQueries = new DelegateCommand<RibbonButton>(rb =>
                 _commandLine.SendToExecute(CommandNames.BayQueries, null), rb => IsPluginCompatible));

        public ICommand RbnRefresh => _rbnRefresh
            ?? (_rbnRefresh = new DelegateCommand<RibbonButton>(rb =>
                _commandLine.SendToExecute(CommandNames.Refresh, null), rb => IsPluginCompatible));

        public ICommand RbnPurge => _rbnClean
            ?? (_rbnClean = new DelegateCommand<RibbonButton>(rb =>
                _commandLine.SendToExecute(CommandNames.Purge, null), rb => IsPluginCompatible));

        public ICommand RbnRuler => _rbnRuler
            ?? (_rbnRuler = new DelegateCommand<RibbonButton>(rb =>
                _commandLine.SendToExecute(CommandNames.Ruler, null), rb => IsPluginCompatible));

        public ICommand RbnPlotW => _rbnPlotW
            ?? (_rbnPlotW = new DelegateCommand<RibbonButton>(rb =>
                _commandLine.SendToExecute(CommandNames.PlotWindow, null), rb => IsPluginCompatible));

        public ICommand RbnHelpCommand => _rbnHelpCommand
            ?? (_rbnHelpCommand = new DelegateCommand<RibbonButton>(RbnExecuteHelpCommand, RbnCanExecuteHelpCommand));

        // CanExecutes ===========================================
        private bool RbnCanExecuteMapAction(RibbonButton rb)
        {
            return Plugin.Initilized && Application.DocumentManager.MdiActiveDocument != null;
        }
        private bool RbnCanExecuteMapView(RibbonButton rb)
        {
            return Plugin.Initilized && Application.DocumentManager.MdiActiveDocument != null && !PluginSettings.Busy;
            //return CurrentLayout != null && CurrentLayout.IsLayoutLoaded()
            //    && _layoutItems.Any(X => X.FindLayoutFullPath() != "")
            //    && CanExecuteClearData(CurrentLayout);
            //IsMapInfoClosed;.Contains(ToolsManager.Doc.Name.ToLower()
        }
        private bool RbnCanExecuteOpenLayout(RibbonButton rb)
        {
            return Plugin.Initilized && CanExecuteOpenLayout((ILayout)rb.CommandParameter);
        }
        private bool RbnCanExecuteActiveLayout(RibbonButton rb)
        {
            return Plugin.Initilized && rb.CommandParameter.ToString() != "";
        }
        private bool RbnCanExecuteConfig(RibbonButton rb)
        {
            return Plugin.Initilized && ConfigItems.Count > 0 && ConfigItems.Contains((Config)rb.CommandParameter);
        }
        private bool RbnCanExecuteConfigManage(RibbonButton rb)
        {
            return Plugin.Initilized;
        }
        private bool RbnCanExecuteExport(RibbonButton rb)
        {
            return Plugin.Initilized;
        }
        private bool RbnCanExecuteExplorer(RibbonButton rb)
        {
            return Plugin.Initilized;
        }
        private bool RbnCanExecuteHelpCommand(RibbonButton rb)
        {
            return Plugin.Initilized;
        }
        private bool RbnCanExecuteUpload(RibbonButton rb)
        {
            var result = CurrentLayout != null && IsPluginCompatible && LayoutExtensions.IsLayoutLoaded(CurrentLayout)
                //&& Lsds2Context.LoFrames.Count > 0
                //&& Lsds2Context.LoFrames.Any(X => X.LayoutId == CurrentLayout.LayoutId) ////&& CurrentLayout.FSA 
                && _layoutItems.Any(x => LayoutExtensions.FindLayoutFullPath(x) == Doc.Name.ToLower());
            return Plugin.Initilized && result;
        }
        private bool RbnCanExecuteRunFsa(RibbonButton rb)
        {
            return Plugin.Initilized && IsPluginCompatible;
            //CurrentLayout.CADFileName.ToLower() == AcadToolsManager.Doc.Name.ToLower();CanExecuteFsaApplyLayout(CurrentLayout); //
        }

        // Executes ===========================================
        private void ExecuteClose(object commandParameter)
        {
            var commandLine = Plugin.GetService<ICommandLine>();
            commandLine.SendToExecute(CommandNames.ExplorerPanelRemove);
        }

        private void RbnExecuteMapView(RibbonButton rb)
        {
            _commandLine.SendToExecute(CommandNames.Map);
        }
        private void RbnExecuteSentToMap(RibbonButton rb)
        {
            _commandLine.SendToExecute(CommandNames.PointOnMap);
            //Commands.Create(CommandNames.PointOnMap,
            //    new TaskArguments() { DisplayName = "Send to map",
            //        CommandParameters = new ArrayList() { "100" }});
        }
        private void RbnExecuteFindOnMap(RibbonButton rb)
        {
            _commandLine.SendToExecute(CommandNames.FindTextOnMap);
        }
        private void RbnExecuteUcsChange(RibbonButton rb)
        {
            _commandLine.SendToExecute(CommandNames.UcsChange);
        }
        private void RbnExecuteConvertToMarkers(RibbonButton rb)
        {
            _commandLine.SendToExecute(CommandNames.ConvertToMarkers);
        }
        private void RbnExecuteCopyAsBlock(RibbonButton rb)
        {
            _commandLine.SendToExecute(CommandNames.CopyAsBlock);
        }
        private void RbnExecutePasteAsBlock(RibbonButton rb)
        {
            _commandLine.SendToExecute(CommandNames.PasteAsBlock);
        }
        private void RbnExecuteCable(RibbonButton rb)
        {
            _commandLine.SendToExecute(CommandNames.Cable);
        }
        private void RbnExecuteClosure(RibbonButton rb)
        {
            _commandLine.SendToExecute(CommandNames.Closure);
        }
        private void RbnExecuteClosureConnect(RibbonButton rb)
        {
            _commandLine.SendToExecute(CommandNames.ClosureConnect);
        }
        private void RbnExecuteCabinet(RibbonButton rb)
        {
            _commandLine.SendToExecute(CommandNames.Cabinet);
        }
        private void RbnExecuteOpenLayout(RibbonButton rb)
        {
            if (CanExecuteOpenLayout((ILayout)rb.CommandParameter))
                ExecuteOpenLayout((ILayout)rb.CommandParameter);
        }
        private void RbnExecuteActiveLayout(RibbonButton rb)
        {
            if (CanExecuteOpenLayout((ILayout)rb.CommandParameter))
                ExecuteOpenLayout((ILayout)rb.CommandParameter);
        }
        private void RbnExecuteOpenConfig(RibbonButton rb)
        {
            CurrentConfig = (Config)rb.CommandParameter;
        }
        private void RbnExecuteExplorer(RibbonButton rb)
        {
            _commandLine.SendToExecute(CommandNames.Explorer);
        }
        private void RbnExecuteSearch(RibbonButton rb)
        {
            _commandLine.SendToExecute(CommandNames.Search);
        }
        private void RbnExecuteHelpCommand(RibbonButton rb)
        {
            var td = new TaskDialog
            {
                WindowTitle = "Plugin info:",
                MainInstruction = "Plugin name: IntelliDesk",
                ContentText = "version 4.0 for " + CommandNames.UserGroup + " (01.10.2015)",
                MainIcon = TaskDialogIcon.Information,
                CommonButtons = TaskDialogCommonButtons.Ok
            };
            td.Show();
        }
        private void RbnExecuteManageConfig(RibbonButton rb)
        {
            //((RibbonCombo)rb.CommandParameter).
        }
        private void RbnExecuteOpenAllWorkFiles(RibbonButton rb)
        {
            _commandLine.SendToExecute(CommandNames.WorkFilesOpenAll, null);
        }
        private void RbnExecuteUpload(RibbonButton rb)
        {
            _commandLine.SendToExecute(CommandNames.UpLoad, null);
        }
        private void RbnExecuteExport(RibbonButton rb)
        {
            _commandLine.SendToExecute(CommandNames.Export, null);
        }
        private void RbnExecuteApply(RibbonButton rb)
        {
            _commandLine.SendToExecute(CommandNames.Apply, null);
        }

        #endregion

        #region <Methods>

        public async Task<ICommandArgs> ExecuteCommand(ICommandArgs command)
        {
            var commandName = command.CommandName.ToUpper();
            if (commandName != "WORKITEMS" && !FileExplorerViewModel.BaseFolder.Folders.Any()) return null;

            Mouse.OverrideCursor = Cursors.Wait; Notifications.DisplayNotifyMessage(NotifyStatus.Working);
            bool isCursorDelay = false;

            string path = command.CommandParameter?.ToString() ?? "*";
            string root = Path.GetPathRoot(path);
            string searchPath = !string.IsNullOrEmpty(root) ? path.Replace(root, "") : path;

            switch (commandName)
            {
                case "FIND":
                    if (path != FileExplorerViewModel.CurrentFolder.FullPath)
                    {
                        await FileExplorerViewModel.BaseFolder.LoadFoldersByPathAsync(searchPath.ToList(), folder =>
                        {
                            FileExplorerViewModel.SetCurrentFolder(folder);
                        });
                    }
                    break;
                case "SEARCH":
                    SimpleActionResult simpleResult = Files.FindPath(
                        FileExplorerViewModel.BaseFolder.Folders.Select(x => x.FullPath).ToArray(),
                        command.CommandParameter.ToString(), null,
                        PluginSettings.SearchIncludeSubdir ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

                    if (simpleResult.StatusCode == HttpStatusCode.Found)
                    {
                        SearchViewModel.IsSearchEnabled = true;
                        command.CommandParameter = simpleResult.ActionResult;

                        searchPath = command.CommandParameter.ToString();
                        root = Path.GetPathRoot(searchPath);
                        searchPath = !string.IsNullOrEmpty(root) ? searchPath.Replace(root, "") : searchPath;

                        await FileExplorerViewModel.BaseFolder.LoadFoldersByPathAsync(searchPath.ToList(), folder =>
                        {
                            FileExplorerViewModel.SetCurrentFolder(folder);
                        });
                    }
                    else
                    {
                        SearchViewModel.IsSearchEnabled = false;
                        command.Cancel(new ErrorNotifyArgs(simpleResult.Message.ToString()));
                    }
                    break;

                case "ACTIVATE":
                    if (command.Sender != null && !((CustomTreeView)command.Sender).HasItems)
                    {
                        FileExplorerViewModel.CurrentFolder = FolderBase.BaseFolder.Folders.First();
                        LoadBaseFolder();
                    }
                    isCursorDelay = true;
                    break;

                case "SHOWALL":
                case "LOAD":
                    if (ShowAllFolders)
                    {
                        if (!FileExplorerViewModel.Computer.IsFolderLoaded)
                            await LoadComputerFolders(FileExplorerViewModel.Computer);
                    }
                    else
                    {
                        LoadBaseFolder();

                        var folders = FileExplorerViewModel.BaseFolder.Folders.Select(x => x.FullPath).ToArray();
                        simpleResult = Files.FindPath(folders, path);

                        if (simpleResult.StatusCode == HttpStatusCode.Found && true)
                        {
                            IFolder folder = await FileExplorerViewModel.BaseFolder.LoadFoldersByPathAsync(folders.ToList()); // searchPath.ToList()
                            if (folder != null)
                                FileExplorerViewModel.SetCurrentFolder(folder);
                        }
                    }
                    isCursorDelay = true;
                    break;

                case "WORKITEMS":
                    using (LocalVirtualFolder virtualFolder = new LocalVirtualFolder("Workitems", command.CommandParameter.ToString()))
                    {
                        await LoadComputerFolders(virtualFolder);
                    }

                    isCursorDelay = true;
                    break;

                case "REFRESH":
                    if (!ShowAllFolders && FileExplorerViewModel.CurrentFolder != null)
                    {
                        if (FileExplorerViewModel.CurrentFolder != null)
                        {
                            await FileExplorerViewModel.CurrentFolder.LoadChildrenAsync(false);
                            FileExplorerViewModel.SetCurrentFolder(FileExplorerViewModel.CurrentFolder);
                        }
                    }
                    break;
                case "SAVE":
                    LoadBaseFolder();
                    FileExplorerViewModel.SetCurrentFolder(FileExplorerViewModel.BaseFolder.Folders.First());
                    ShowAllFolders = false;
                    break;
            }

            if (!command.CancelToken.IsCancellationRequested && command.CommandParameter != null)
            {
            }

            if (isCursorDelay)
                InfraManager.DelayAction(1000, (Action)(() =>
                {
                    Notifications.SendNotifyMessageAsync((NotifyArgs)command.NotifyArgs);
                    Mouse.OverrideCursor = null;
                }));
            else
            {
                Notifications.SendNotifyMessageAsync(command.NotifyArgs);
                Mouse.OverrideCursor = null;
            }

            command.Clean();
            return command;
        }

        public async Task<ICommandArgs> ExecuteCommand2(ICommandArgs command)
        {
            bool isCursorDelay = false;
            Mouse.OverrideCursor = Cursors.Wait; Notifications.DisplayNotifyMessage(NotifyStatus.Working);

            var commandName = command.CommandName.ToUpper();
            switch (commandName)
            {
                case "SEARCH":
                    SimpleActionResult simpleResult = Files.FindPath(
                        FileExplorerViewModel.BaseFolder.Folders.Select(x => x.FullPath).ToArray(),
                        command.CommandParameter.ToString(), null);

                    if (simpleResult.StatusCode == HttpStatusCode.Found)
                        command.CommandParameter = simpleResult.ActionResult;
                    else
                        command.Cancel(new ErrorNotifyArgs(simpleResult.Message.ToString()));
                    break;

                case "ACTIVATE":
                    if (command.Sender != null && !((CustomTreeView)command.Sender).HasItems)
                    {
                        FileExplorerViewModel.CurrentFolder = FolderBase.BaseFolder.Folders.First();
                        LoadBaseFolder();
                    }
                    isCursorDelay = true;
                    break;

                case "LOAD":
                case "SHOWALL":
                    if (ShowAllFolders)
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
                case "REFRESH":
                    if (!ShowAllFolders && FileExplorerViewModel.CurrentFolder != null)
                    {
                        if (FileExplorerViewModel.CurrentFolder != null)
                        {
                            await FileExplorerViewModel.CurrentFolder.LoadChildrenAsync(true);
                            FileExplorerViewModel.SetCurrentFolder(FileExplorerViewModel.CurrentFolder);
                        }
                    }
                    break;
            }

            if (!command.CancelToken.IsCancellationRequested && command.CommandParameter != null)
            {
                if (commandName == "SEARCH" || commandName == "FIND" || commandName == "LOAD")
                {
                    string path = command.CommandParameter.ToString();
                    string root = Path.GetPathRoot(path);
                    string searchPath = command.CommandParameter.ToString().Replace(root, "");

                    if (FileExplorerViewModel.CurrentFolder != null)
                        if (commandName == "LOAD")
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
                if (commandName == "SAVE")
                {
                    LoadBaseFolder();
                    FileExplorerViewModel.SetCurrentFolder(FileExplorerViewModel.BaseFolder.Folders.First());
                    ShowAllFolders = false;
                }
            }

            if (isCursorDelay)
                InfraManager.DelayAction(1000, (Action)(() =>
                {
                    Notifications.SendNotifyMessageAsync((NotifyArgs)command.NotifyArgs);
                    Mouse.OverrideCursor = null;
                }));
            else
            {
                Notifications.SendNotifyMessageAsync(command.NotifyArgs);
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

                FileExplorerViewModel.BaseFolder = await new LocalVirtualFolder().SetFoldersAsync(PluginSettings.IncludeFolders.ToArray()); ;
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
            IFolder baseFolder = null;
            if (computer is LocalVirtualFolder)
            {
                baseFolder = ((LocalVirtualFolder)computer).RealFolder;
                FileExplorerViewModel.SetCurrentFolder(baseFolder);
            }
            else
            {
                FileExplorerViewModel.Items.AddRange(new[] { new LocalFolder("Computer", null) { IsLoading = true } });

                await computer.LoadChildrenAsync();

                var workFolder = PluginSettings.IncludeFolders.FirstOrDefault();
                var pathRoot = Path.GetPathRoot(workFolder);
                var baseFolderPath = workFolder?.Replace(pathRoot, "");
                var currentDrive = computer.Folders.FirstOrDefault(x => x.Name == pathRoot);

                if (currentDrive != null)
                {
                    await currentDrive.GetChildrenAsync();

                    if (!string.IsNullOrEmpty(baseFolderPath))
                    {
                        baseFolder = await new LocalVirtualFolder().SetFoldersAsync(PluginSettings.IncludeFolders.ToArray());
                        //baseFolder = await currentDrive.LoadFoldersByPathAsync(baseFolderPath.ToList());
                    }
                    else
                    {
                        baseFolder = currentDrive;
                        Plugin.Container.RegisterInstance("BaseFolder", baseFolder,
                            new ContainerControlledLifetimeManager());
                    }
                    currentDrive.IsExpanded = true;

                    FileExplorerViewModel.BaseFolder = baseFolder;
                    FileExplorerViewModel.Items.Clear();
                    FileExplorerViewModel.Items.AddRange(computer.Folders);
                    FileExplorerViewModel.SetCurrentFolder(baseFolder.Folders.First());
                    FileExplorerViewModel.IsLoading = false;
                }
            }
        }

        private void LoadLayouts()
        {
            _layoutItems.Clear();

            _layoutItems = _layoutService.GetLayouts()
                .ToItems()
                .WhenItemChanged(OnlayoutPropertyChanged);

            //_uow.JsonRepositoryAsync<Layout>() //.GetLayouts()
            //.Queryable()
            //.ToItems()
            //.WhenItemChanged(OnlayoutPropertyChanged);

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

            RuleItems = _uow.RepositoryAsync<Rule>()
                .Queryable()
                .WhenEmpty(() => new Rule())
                .Where(x => x.LayoutId == CurrentUserSetting.LayoutId)
                .ToItems();
        }

        private void MakeRibbonLayouts(ICollection<ILayout> layoutItems)
        {
            if (ComponentManager.Ribbon == null) return;

            RibbonButton rb;
            ObservableCollection<RibbonButton> ribbonButtons = new ObservableCollection<RibbonButton>();

            LayoutItemButtons.Clear();

            var workItemsFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Intellidesk", "WorkItems");
            var fullPaths = Directory.GetFiles(workItemsFolderPath, "*.lnk").ToDictionary(k => k, v => Path.GetFullPath(Files.GetShortcutTargetFile(v)));

            Dictionary<string, string> realPathsResults = new Dictionary<string, string>();
            fullPaths.ForEach(item =>
            {
                if (File.Exists(item.Value))
                {
                    if (!realPathsResults.ContainsKey(item.Value))
                        realPathsResults.Add(item.Key, item.Value);
                }
                else
                {
                    try
                    {
                        File.Delete(item.Key);
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Error(ex.InnerException?.Message ?? ex.Message);
                    }
                }
            });

            if (realPathsResults.Count > 0)
            {
                foreach (var realPath in realPathsResults)
                {
                    var layoutId = realPath.Key.Replace("\\", "__").Replace(".", "_");
                    rb = new RibbonButton
                    {
                        CommandHandler = RbnActiveLayoutCommand,
                        CommandParameter = realPath.Value,
                        Height = 32,
                        Name = "LayoutRibbonItem_" + layoutId,
                        Orientation = Orientation.Horizontal,
                        Size = RibbonItemSize.Standard,
                        ShowText = true,
                        Text = realPath.Value,
                        Tag = "",
                        Id = layoutId
                    };
                    var tt = new RibbonToolTip
                    {
                        Command = "Command: WORKITEMOPEN " + realPath.Value,
                        Content = "LayoutId =" + layoutId + ";\n" +
                                  "CADNameFile =" + realPath.Value + ";\n",
                        IsHelpEnabled = false,
                        Title = realPath.Key
                    };
                    rb.ToolTip = tt;

                    ribbonButtons.Add(rb);
                    //if (layout.LayoutID == CurrentUserSetting.LayoutId)
                    //CurrentRibbonLayout = rb;
                }
            }
            else
            {
                rb = new RibbonButton
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
                ribbonButtons.Add(rb);
            }

            LayoutItemButtons = ribbonButtons;
            CurrentRibbonLayout = LayoutItemButtons.FirstOrDefault();

            RibbonRefresh();
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

        public override void OnDocumentActivated(object sender, DocumentCollectionEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            var currentRibbonLayoutName = "none";

            UnregisterEvents();

            var item = (e == null)
                ? LayoutItems.FirstOrDefault(x => x.FindLayoutFullPath() == Application.DocumentManager.MdiActiveDocument.Name.ToLower())
                : LayoutItems.FirstOrDefault(x => x.FindLayoutFullPath() == e.Document.Name.ToLower());

            if (item != null)
            {
                currentRibbonLayoutName = item.LayoutID.ToString();
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
                IsLayoutReadOnly = !item.IsLayoutReadOnly();
            }

            CurrentRibbonLayout = LayoutItemButtons.FirstOrDefault(x => x.Id == currentRibbonLayoutName);

            IsPluginCompatible = true; //IsDwgCompatible();

            //if (ComponentManager.Ribbon != null && ComponentManager.Ribbon.IsVisible)
            //    ComponentManager.Ribbon.Focus();
            //else
            //    UIHelper.AcadWindowSetFocus(Application.MainWindow.Handle);

            RegisterEvents();
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
        public void RibbonRefresh()
        {
            if (ComponentManager.Ribbon == null) return;

            RibbonCombo layoutComboBox = ComponentManager.Ribbon.FindItem("RbnLayoutsId", true) as RibbonCombo;
            if (layoutComboBox != null)
            {
                if (LayoutItemButtons != null && LayoutItemButtons.Count > 0)
                {
                    layoutComboBox.Items.Clear();
                    layoutComboBox.Items.AddRange(LayoutItemButtons);
                    layoutComboBox.Current = CurrentRibbonLayout;

                    RibbonCombo settingsGallery = ComponentManager.Ribbon.FindItem("SettingsGalleryId", true) as RibbonCombo;
                    settingsGallery.Items.Clear();

                    var layout = LayoutItems.FirstOrDefault(x => x.LayoutName == CurrentRibbonLayout.Text);

                    if (layout != null && layout.WorkItems != null && layout.WorkItems.Any())
                        settingsGallery.Items.AddRange(layout.WorkItems.Select(x => new RibbonButton() { Text = x.Work }));

                    settingsGallery.Current = settingsGallery.Items.FirstOrDefault();
                }
            }
        }

        //public void SaveChanges<T>(T entity, params string[] properties) where T : class
        //{
        //    _layoutsRepository.SetUpdate(entity, (properties.Length > 0)
        //        ? properties
        //        : CurrentLayout.ChangedProperties.ToArray());
        //}

        public void SaveChanges(ILayout entity, string propertyName, object value)
        {
            _uow.Repository<ILayout>().Update((ILayout)entity, propertyName, value);
        }

        #endregion

        #region <Validations>

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

        #region <IDataErrorInfo Members>

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

        //public bool IsActive { get; set; }

        //public event EventHandler IsActiveChanged;
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
