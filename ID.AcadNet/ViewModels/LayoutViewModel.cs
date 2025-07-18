using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Media;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.Windows;
using Intellidesk.AcadNet.Common;
using Intellidesk.AcadNet.Common.Core;
using Intellidesk.AcadNet.Core;
using Intellidesk.AcadNet.Common.Enums;
using Intellidesk.AcadNet.Common.Interfaces;
using Intellidesk.AcadNet.Services;
using Intellidesk.Common.Commands;
using Intellidesk.Data.Models;
using Intellidesk.Data.Models.Cad;
using Intellidesk.Data.Repositories;
using Intellidesk.Data.Repositories.EF6.UnitOfWork;
using Intellidesk.Infrastructure;
using Intellidesk.Infrastructure.Core;
using Intellidesk.Infrastructure.Tasks;

using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Rule = Intellidesk.Data.Models.Cad.Rule;
using User = Intellidesk.Data.Models.Cad.User;
using UserSetting = Intellidesk.Data.Models.Cad.UserSetting;
using ILayout = Intellidesk.Data.Models.Cad.ILayout;
using LayoutExtensions = Intellidesk.AcadNet.Core.LayoutExtensions;

namespace Intellidesk.AcadNet.ViewModels
{
    /// <summary> Main view model </summary>
    public class LayoutViewModel : BaseViewModel, IDataErrorInfo, IMapViewModel
    {
        #region "Private variables"

        private readonly ICommandLine _commandLine;
        private readonly IUnitOfWorkAsync _uow = null;

        // Config and Layout
        private Config _currentConfig;
        private ILayout _currentLayout;
        private Rule _currentRule;
        
        private User _currentUser;
        private Filter _currentlayoutfilter;
        private RibbonButton _currentRibbonTab;
        private string[] _layoutsContentsItems;

        // LayoutExplorer
        //private bool _isLayoutExplorerClosed = true;

        private bool _isLsdsCompatible;
        private bool _isLayoutReadOnly;

        // ObservableCollections
        private ObservableCollection<ILayout> _layoutItems = new ObservableCollection<ILayout>();
        private readonly ObservableCollection<UserSetting> _userSettingItems = new ObservableCollection<UserSetting>();
        private readonly ObservableCollection<RibbonButton> _ribbonLayoutItems = new ObservableCollection<RibbonButton>();

        // LayoutExplorer and Ribbon commands

        private ICommand _openLayoutCommand;
        private ICommand _saveLayoutCommand;

        private ICommand _rbnActiveLayoutCommand;
        private ICommand _rbnExport;
        private ICommand _rbnOpenLayoutExplorer;
        private ICommand _rbnOpenConfigCommand;
        private ICommand _rbnOpenLayoutCommand;
        private ICommand _rbnOpenConfigManageCommand;
        private ICommand _rbnUpload;
        private ICommand _rbnRun;
        private ICommand _rbnOpenMapInfo;

        #endregion

        /// <summary> Main view model, Specifies which constructor should be used when creating a part.</summary>
        public LayoutViewModel()
        {
            _commandLine = Plugin.GetService<ICommandLine>();
            _uow = Plugin.GetService<IUnitOfWorkAsync>();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine(" <script src='http://code.jquery.com/jquery-latest.js'></script>");
            sb.AppendLine("<script>");
            sb.AppendLine("$(document).ready(function () {");
            sb.AppendLine("$('div').css('background-color', 'Red'); });");
            sb.AppendLine("</script>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<div>DUMMY</div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");
        }

        #region "Properties"

        private bool _isTabClosed = true;
        public bool IsTabClosed
        {
            get { return _isTabClosed; }
            set
            {
                if (value != _isTabClosed)
                {
                    _isTabClosed = value;
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

        /// <summary> Layouts </summary>
        //[ImportMany(typeof(Layout), AllowRecomposition = true)]
        public ObservableCollection<ILayout> LayoutItems
        {
            get { return _layoutItems; }
            set { _layoutItems = value; }
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

        /// <summary> Layouts ribbon buttons </summary>
        public ObservableCollection<RibbonButton> RibbonLayoutItems
        {
            get { return _ribbonLayoutItems; }
        }

        /// <summary> Current layout ribbon button </summary>
        public RibbonButton CurrentRibbonTab
        {
            get { return _currentRibbonTab; }
            set
            {
                _currentRibbonTab = value;
                OnPropertyChanged();
            }
        }

        /// <summary> User settings </summary>
        //[ImportMany(AllowRecomposition = true)]
        public ObservableCollection<UserSetting> UserSettingItems
        {
            get { return _userSettingItems; }
        }

        public User CurrentUser
        {
            get { return _currentUser; }
            set
            {
                _currentUser = value;
                OnPropertyChanged();
            }
        }

        public MapViewModel DataContext { get; set; }

        /// <summary> Toggle Layout DataTemplate Selector to LayoutView </summary>
        public short ToggleLayoutDataTemplateSelector { get; set; }

        #endregion

        #region "Palette Commands"

        public ICommand OpenLayoutCommand
        {
            get
            {
                return _openLayoutCommand ??
                       (_openLayoutCommand = new CustomDelegateCommand<ILayout>(ExecuteOpenLayout, CanExecuteOpenLayout));
            }
        }
        private bool CanExecuteOpenLayout(ILayout layout)
        {
            return LayoutItems.Count > 0 && LayoutItems.Contains(layout) && layout.IsValid()
                && (layout.LayoutState == null || layout.LayoutState >= 0);
        }
        private void ExecuteCloseTab(object sender, DocumentDestroyedEventArgs e)
        {
            //ExecutePropertyGridRefresh(X => X.CADFileName.ToLower() == e.FileName.ToLower() && X.IsLoaded);
            var item = LayoutItems.FirstOrDefault(x => x.CADFileName.ToLower() == e.FileName.ToLower());
            if (item != null && !item.IsMapLoaded())
            {
                var idx = LayoutItems.IndexOf(item);
                LayoutItems[idx] = item;
                CurrentLayout = item;
            }
            if ((Application.DocumentManager.Count == 0))
                CurrentLayout = null;
        }
        private void ExecuteOpenLayout(ILayout layout)
        {
            Mouse.OverrideCursor = Cursors.Wait;

            var item = LayoutItems.FirstOrDefault(x => x.LayoutID == layout.LayoutID);
            if (item != null)
            {
                var fullPath = item.FindMapFullPath();
                if (fullPath != "" && item.IsMapLoaded())
                {
                    Application.DocumentManager.MdiActiveDocument =
                        Application.DocumentManager.Cast<Document>().FirstOrDefault(x => x.Name.ToLower() == fullPath);
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
        private void ExecuteCreatLayoutCompleted(object taskArgs)
        {
            var args = (ITaskArgs)taskArgs;
            //if (((Data.Models.Intel.LayoutManager_Result)Args.TaskResult).RETURN_CODE == 0)
            //{
            //    LoadLayouts();
            //    var item = _layoutItems.OrderBy(o => o.LayoutId).Last();
            //    var idx = LayoutItems.IndexOf(item);
            //    LayoutItems[idx] = item;
            //    CurrentLayout = LayoutItems[idx];
            //    //MakeRibbonLayouts();
            //}
        }
        private void ExecuteUnDeleteLayoutCompleted(object taskArgs)
        {
            var args = (ITaskArgs)taskArgs;
            if (((LayoutManagerResult)args.TaskResult).RETURN_CODE == 0)
            {
                var tab = (ILayout)args.CommandParameters[0];
                var idx = LayoutItems.IndexOf(tab);
                var item = new ILayout { LayoutState = 1 };
                LayoutItems[idx] = item;
                CurrentLayout = LayoutItems[idx];
                //MakeRibbonLayouts();
            }
        }
        private void ExecuteDeleteLayoutCompleted(object taskArgs)
        {
            var args = (ITaskArgs)taskArgs;
            if (((LayoutManagerResult)args.TaskResult).RETURN_CODE == 0)
            {
                var tab = (ILayout)args.CommandParameters[0];
                var idx = LayoutItems.IndexOf(tab);
                var item = new ILayout { LayoutState = -1 };
                LayoutItems[idx] = item;
                CurrentLayout = LayoutItems[idx];
                //MakeRibbonLayouts();
            }
        }

        private void ExecuteDeleteLayout(ILayout layout)
        {
            SystemSounds.Exclamation.Play();
            //!!!!!!!
            //CommandClassService.CurrentTaskArgs = new TaskArguments
            //{
            //    Title = "Delete the layout:",
            //    Command = "DELETE",
            //    CommandArguments = new ArrayList { layout },
            //    IsTimerOn = false,
            //    ProgressLimit = 100,
            //    ActionCompleted = ExecuteDeleteLayoutCompleted
            //};
            _commandLine.SendToExecute("IDCOMANDSUCCESS");
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

            //!!!!!!!!
            //CommandClassService.CurrentTaskArgs = new TaskArguments
            //{
            //    Title = "New the layout:",
            //    Command = "CREATE",
            //    CommandArguments = new ArrayList { newLayout },
            //    IsTimerOn = false,
            //    ProgressLimit = 100,
            //    ActionCompleted = ExecuteCreatLayoutCompleted
            //};
            _commandLine.SendToExecute("IDCOMANDSUCCESS");
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
            //                LayoutItems = new ObservableCollection<Layout>(LayoutItems.WhereFilter(propertyOrFieldName.Name, value));
            //                OnPropertyChanged(() => LayoutItems);
            //                CurrentLayout = LayoutItems.Count > 0 ? LayoutItems[0] : null;
            //            }
            //        }
            //    }
            //    MakeRibbonLayouts();
            //}
            //else
            //{
            //    LoadLayouts();
            //    CurrentLayout = LayoutItems.Count > 0 ? LayoutItems[0] : null;
            //}
            //CurrentLayoutFilter.Save();
        }
        private void ExecuteFsaApplyLayout(ILayout layout)
        {
            _commandLine.SendToExecute(CommandNames.Apply);
        }
        private void ExecuteFsaUploadLayout(ILayout layout)
        {
            RbnExecuteUpload(null);
        }
        private void ExecuteOpenLayout1(object sender, DocumentCollectionEventArgs e)
        {
            var item = LayoutItems.FirstOrDefault(x => x.CADFileName.ToLower() == e.Document.Name.ToLower());
            if (item != null && !LayoutExtensions.IsMapLoaded(item))
            {
                var idx = LayoutItems.IndexOf(item);
                LayoutItems[idx] = item;
                CurrentLayout = item;
            }
        }
        private void ExecuteSaveLayout(ILayout layout)
        {
            if (layout.IsModified())
            {
                layout.ModifiedBy = CurrentUser.UserId;
                //var loLayout = (LO_Layout)layout.CloneToLO_Layout();
                ////_tabRepository.SetUpdate(loLayout, layout.ChangedProperties.ToArray());
                //layout.ClearChangedProperties();
                ////MakeRibbonLayouts();
            }
            OnPropertyChanged("CurrentLayout");
        }
        private void ExecuteUndeleteLayout(ILayout layout)
        {
            SystemSounds.Exclamation.Play();
            //!!!!!!!!
            //CommandClassService.CurrentTaskArgs = new TaskArguments
            //{
            //    Title = "UnDelete the layout:",
            //    Command = "UNDELETE",
            //    CommandArguments = new ArrayList { layout },
            //    IsTimerOn = false,
            //    ProgressLimit = 100,
            //    ActionCompleted = ExecuteUnDeleteLayoutCompleted
            //};
            _commandLine.SendToExecute("IDCOMANDSUCCESS");
        }

        #endregion

        #region "Ribbon Commands"

        public ICommand RbnOpenMapInfo
        {
            get
            {
                return _rbnOpenMapInfo ??
                      (_rbnOpenMapInfo = new CustomDelegateCommand<RibbonButton>(RbnExecuteMapInfo, RbnCanExecuteMapInfo));
            }
        }
        //MapInfo
        private void RbnExecuteMapInfo(RibbonButton rb)
        {
            ILayout layout = CurrentLayout;
            var item = LayoutItems.FirstOrDefault(x => x.LayoutID == layout.LayoutID);
            if (item != null)
            {
                var fullPath = LayoutExtensions.FindMapFullPath(item);
                if (fullPath != "" && LayoutExtensions.IsMapLoaded(item))
                {
                    Application.DocumentManager.MdiActiveDocument =
                        Application.DocumentManager.Cast<Document>().FirstOrDefault(x => x.Name.ToLower() == fullPath);
                }
                else
                    if (fullPath == "")
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
                    _commandLine.SendToExecute("LOADMAPINFO", fullPath);
                }
            }

        }
        private bool RbnCanExecuteMapInfo(RibbonButton rb)
        {
            return CurrentLayout != null && LayoutExtensions.IsMapLoaded(CurrentLayout)
                && _layoutItems.Any(x => LayoutExtensions.FindMapFullPath(x) != ""); //IsMapInfoClosed;.Contains(ToolsManager.Doc.Name.ToLower()
        }
        public ICommand RbnOpenLayoutExplorer
        {
            get
            {
                return _rbnOpenLayoutExplorer ??
                      (_rbnOpenLayoutExplorer = new CustomDelegateCommand<RibbonButton>(RbnExecuteLayoutExplorer, RbnCanExecuteLayoutExplorer));
            }
        }
        public ICommand RbnUpload
        {
            get
            {
                return _rbnUpload ??
                      (_rbnUpload = new CustomDelegateCommand<RibbonButton>(RbnExecuteUpload, RbnCanExecuteUpload));
            }
        }
        public ICommand RbnExport
        {
            get
            {
                return _rbnExport ??
                      (_rbnExport = new CustomDelegateCommand<RibbonButton>(RbnExecuteExport, RbnCanExecuteExport));
            }
        }
        public ICommand RbnApply
        {
            get
            {
                return _rbnRun ??
                      (_rbnRun = new CustomDelegateCommand<RibbonButton>(RbnExecuteApply, RbnCanExecuteRunFsa));
            }
        }
        public ICommand RbnOpenLayoutCommand
        {
            get
            {
                return _rbnOpenLayoutCommand ??
                      (_rbnOpenLayoutCommand = new CustomDelegateCommand<RibbonButton>(RbnExecuteOpenLayout, RbnCanExecuteOpenLayout));
            }
        }
        public ICommand RbnActiveLayoutCommand
        {
            get
            {
                return _rbnActiveLayoutCommand ??
                      (_rbnActiveLayoutCommand = new CustomDelegateCommand<RibbonButton>(RbnExecuteActiveLayout, RbnCanExecuteActiveLayout));
            }
        }
        public ICommand RbnManageConfigCommand
        {
            get
            {
                return _rbnOpenConfigManageCommand ??
                      (_rbnOpenConfigManageCommand = new CustomDelegateCommand<RibbonButton>(RbnExecuteManageConfig, RbnCanExecuteConfigManage));
            }
        }

        private bool RbnCanExecuteOpenLayout(RibbonButton rb)
        {
            return CanExecuteOpenLayout((ILayout)rb.CommandParameter);
        }
        private bool RbnCanExecuteActiveLayout(RibbonButton rb)
        {
            return rb.CommandParameter.ToString() != "";
        }
        private bool RbnCanExecuteConfigManage(RibbonButton rb)
        {
            return true;
        }
        private bool RbnCanExecuteExport(RibbonButton rb)
        {
            return false;
        }
        private bool RbnCanExecuteLayoutExplorer(RibbonButton rb)
        {
            return IsTabClosed;
        }
        private bool RbnCanExecuteUpload(RibbonButton rb)
        {
            var result = CurrentLayout != null && LayoutExtensions.IsMapLoaded(CurrentLayout)
                //&& GisContext.LoFrames.Count > 0
                //&& GisContext.LoFrames.Any(X => X.LayoutId == CurrentTab.LayoutId) ////&& CurrentLayout.FSA 
                && _layoutItems.Any(x => LayoutExtensions.FindMapFullPath(x) == Doc.Name.ToLower());
            return result;
        }
        private bool RbnCanExecuteRunFsa(RibbonButton rb)
        {
            return true;
            //CurrentLayout.CADFileName.ToLower() == AcadToolsManager.Doc.Name.ToLower();CanExecuteFsaApplyLayout(CurrentLayout); //
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
        private void RbnExecuteLayoutExplorer(RibbonButton rb)
        {
            _commandLine.SendToExecute(CommandNames.Explorer);
        }
        private void RbnExecuteManageConfig(RibbonButton rb)
        {
            //((RibbonCombo)rb.CommandParameter).
        }
        private void RbnExecuteUpload(RibbonButton rb)
        {
            _commandLine.SendToExecute(CommandNames.UpLoad);
        }
        private void RbnExecuteExport(RibbonButton rb)
        {
            _commandLine.SendToExecute(CommandNames.Export);
        }
        private void RbnExecuteApply(RibbonButton rb)
        {
            _commandLine.SendToExecute(CommandNames.Apply);
        }

        #endregion

        #region "Methods"

        private void LoadLayouts()
        {
            _layoutItems.Clear();
            _layoutItems = _uow.RepositoryAsync<ILayout>().Queryable().ToItems().WhenItemChanged(OnlayoutPropertyChanged);
            //MakeRibbonLayouts();
        }

        private void OnlayoutPropertyChanged(object sender, EntityChangedArgs e)
        {
            OnPropertyChanged(); //this, () => CurrentLayout
        }

        /// <summary> Occuring at layoutComboBoxDropDown on event Opened </summary>
        public void OnlayoutComboBoxCurrentChanged(object sender, RibbonPropertyChangedEventArgs e)
        {
            if ((e.NewValue != null) && ((RibbonButton)e.NewValue).CommandParameter.ToString() != "")

                // Sender.Tag == NULL specifies that the event has occurred by selecting user
                if (((RibbonCombo)sender).Tag != null)
                {
                    var tab = LayoutItems.FirstOrDefault(x => x.LayoutID == (decimal)((RibbonButton)e.NewValue).CommandParameter);
                    ExecuteOpenLayout(tab);
                }
        }

        /// <summary> Occuring at layoutComboBoxDropDown on event Opened </summary>
        public void OnlayoutComboBoxDropDownOpened(object sender, EventArgs e)
        {
            ((RibbonCombo)sender).Tag = "OnDropDownOpened";
        }

        /// <summary> Occuring at layoutComboBoxDropDown on event Closed </summary>
        public void OnlayoutComboBoxDropDownClosed(object sender, EventArgs e)
        {
            ((RibbonCombo)sender).Tag = null;
        }

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

            Mouse.OverrideCursor = null;
        }

        private void OnDocumentToBeDeactivated(object sender, DocumentCollectionEventArgs documentCollectionEventArgs)
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

        /// <summary> Test </summary>
        private void UIControlRefresh()
        {
            var ribbomItem = ComponentManager.Ribbon.FindItem("RbnLayoutsId", true);
            if (ribbomItem != null) ((RibbonCombo)ribbomItem).Current = CurrentRibbonTab;
        }

        //public void SaveChanges<T>(T entity, params string[] properties) where T : class
        //{
        //    _layoutsRepository.SetUpdate(entity, (properties.Length > 0)
        //        ? properties
        //        : CurrentLayout.ChangedProperties.ToArray());
        //}

        public void SaveChanges<T>(T entity, string propertyName, object value) where T : class
        {
            //_tabRepository.SetUpdate(entity, propertyName, value);
        }

        #endregion

        //#region "Validations"

        ///// <summary> returnig is dwg conpatible base on lsds rules </summary>
        //public bool IsDwgCompatible(Type[] typeFilterOn, string[] attributePatternOn) //Rule.LsdsTypeFilterOn, Rule.LsdsAttributePatternOn
        //{
        //    bool result;

        //    //Get implied entities //LsdsCommands.CurrentObjectIds
        //    var currentObjectIds = new List<ObjectId>(); // SelectManager.GetImplied();

        //    //Get entities from drawing space model
        //    currentObjectIds = currentObjectIds.XGetObjects(typeFilterOn, attributePatternOn);
        //    result = currentObjectIds.Count != 0;

        //    return result;
        //}

        //private bool IsValid(DependencyObject obj)
        //{
        //    // The dependency object is valid if it has no errors, 
        //    //and all of its children (that are dependency objects) are error-free.
        //    return !Validation.GetHasError(obj) &&
        //        LogicalTreeHelper.GetChildren(obj)
        //        .OfType<DependencyObject>()
        //        .All(child => IsValid(child));
        //}

        //#endregion "Validations"

        #region "IDataErrorInfo Members"

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

        public ICommand CreateCommand => throw new NotImplementedException();

        public ICommand CloseCommand => throw new NotImplementedException();

        public ICommand OpenCommand => throw new NotImplementedException();

        public ICommand RefreshCommand => throw new NotImplementedException();

        public ICommand SaveCommand => throw new NotImplementedException();

        #endregion "IDataErrorInfo Members"

        //#region "OnPropertyChanged"

        //public event PropertyChangedEventHandler PropertyChanged;
        //public void OnPropertyChanged(string propertyName)
        //{
        //    var handler = PropertyChanged;
        //    if (handler != null)
        //        handler(this, new PropertyChangedEventArgs(propertyName));
        //}

        //protected void OnPropertyChanged<T>(Expression<Func<T>> action)
        //{
        //    var propertyName = GetPropertyName(action);
        //    OnPropertyChanged(propertyName);
        //}

        //private static string GetPropertyName<T>(Expression<Func<T>> action)
        //{
        //    var expression = (MemberExpression)action.Body;
        //    var propertyName = expression.Member.Name;
        //    return propertyName;
        //}

        //#endregion
    }
}
