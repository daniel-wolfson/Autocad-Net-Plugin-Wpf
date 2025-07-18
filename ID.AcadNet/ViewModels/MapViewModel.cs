using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.Windows;
using ID.Infrastructure;
using ID.Infrastructure.Commands;
using ID.Infrastructure.Interfaces;
using Intellidesk.AcadNet.Common.Core;
using Intellidesk.AcadNet.Common.Interfaces;
using Intellidesk.AcadNet.WebBrowser;
using Intellidesk.Data.Models;
using Intellidesk.Data.Repositories;
using Intellidesk.Data.Repositories.EF6.UnitOfWork;
using Prism.Commands;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using ILayout = Intellidesk.Data.Models.Cad.ILayout;
using LayoutExtensions = Intellidesk.AcadNet.Core.LayoutExtensions;
using User = Intellidesk.Data.Models.Cad.User;
using UserSetting = Intellidesk.Data.Models.Cad.UserSetting;

namespace Intellidesk.AcadNet.ViewModels
{
    /// <summary> Main view model </summary>
    public class MapViewModel : BaseViewModel, IDataErrorInfo, IMapViewModel
    {
        #region "Private variables"

        private readonly ICommandLine _commandLine;
        private readonly IUnitOfWorkAsync _uow = null;

        // Config and Layout
        private ILayout _currentLayout;

        private User _currentUser;
        private RibbonButton _currentRibbonTab;

        // LayoutExplorer
        //private bool _isLayoutExplorerClosed = true;

        private bool _isLayoutReadOnly;

        #endregion

        /// <summary> Main view model, Specifies which constructor should be used when creating a part.</summary>
        public MapViewModel()
        {
            _commandLine = Plugin.GetService<ICommandLine>();
            _uow = Plugin.GetService<IUnitOfWorkAsync>();

            //StringBuilder sb = new StringBuilder();
            //sb.AppendLine("<html>");
            //sb.AppendLine("<head>");
            //sb.AppendLine(" <script src='http://code.jquery.com/jquery-latest.js'></script>");
            //sb.AppendLine("<script>");
            //sb.AppendLine("$(document).ready(function () {");
            //sb.AppendLine("$('div').css('background-color', 'Red'); });");
            //sb.AppendLine("</script>");
            //sb.AppendLine("</head>");
            //sb.AppendLine("<body>");
            //sb.AppendLine("<div>DUMMY</div>");
            //sb.AppendLine("</body>");
            //sb.AppendLine("</html>");
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
        public ObservableCollection<ILayout> LayoutItems { get; set; } = new ObservableCollection<ILayout>();

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
        public ObservableCollection<RibbonButton> RibbonLayoutItems { get; } = new ObservableCollection<RibbonButton>();

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
        public ObservableCollection<UserSetting> UserSettingItems { get; } = new ObservableCollection<UserSetting>();

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

        private ICommand _createCommand, _closeCommand, _openCommand, _refreshCommand, _loadGovmapCommand, _loadLeafletmapCommand, _saveCommand;

        public ICommand CreateCommand => _createCommand ?? (_createCommand = new DelegateCommand<object>(ExecuteCreate));
        public ICommand CloseCommand => _closeCommand ?? (_closeCommand = new DelegateCommand<object>(ExecuteClose));
        public ICommand OpenCommand => _openCommand ?? (_openCommand = new DelegateCommand<ILayout>(ExecuteOpen, CanExecuteOpenLayout));
        public ICommand RefreshCommand => _refreshCommand ?? (_refreshCommand = new DelegateCommand<Grid>(ExecuteRefresh, CanExecuteByBusy));
        public ICommand SaveCommand => _saveCommand ?? (_saveCommand = new DelegateCommand<Grid>(ExecuteSave, CanExecuteByBusy));

        public ICommand LoadGovMapCommand =>
            _loadGovmapCommand ?? (_loadGovmapCommand = new DelegateCommand<Grid>(ExecuteLoadGovMap, CanExecuteByBusy));
        public ICommand LoadLeafletMapCommand =>
            _loadLeafletmapCommand ?? (_loadLeafletmapCommand = new DelegateCommand<Grid>(ExecuteLoadLeafletMap, CanExecuteByBusy));

        private bool CanExecuteOpenLayout(ILayout layout)
        {
            return LayoutItems.Count > 0 && LayoutItems.Contains(layout) && layout.IsValid()
                && (layout.LayoutState == null || layout.LayoutState >= 0);
        }
        private bool CanExecuteByBusy(object item)
        {
            return !PluginSettings.Busy;
        }

        private void ExecuteCreate(object commandParameter)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            Mouse.OverrideCursor = null;
        }
        private void ExecuteClose(object commandParameter)
        {
            var commandLine = Plugin.GetService<ICommandLine>();
            commandLine.SendToExecute(CommandNames.MapViewPanelRemove);
        }
        private void ExecuteOpen(ILayout layout)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            Mouse.OverrideCursor = null;
        }

        private void ExecuteRefresh(Grid grid)
        {
            if (!WebBrowserInitializer.IsHostExistAlert()) return;

            if (WebBrowserInitializer.WebBrowser != null)
            {
                //WebBrowserInitializer.WebBrowser.Visibility = Visibility.Hidden;
                WebBrowserInitializer.WebBrowser.Dispose();
                WebBrowserInitializer.WebBrowser = null;
            }

            WebBrowserInitializer webBrowserInitializer = new WebBrowserInitializer();
            webBrowserInitializer.Initialize();

            if (WebBrowserInitializer.WebBrowser != null &&
                grid != null && !grid.Children.Contains(WebBrowserInitializer.WebBrowser))
            {
                grid.Children.Add(WebBrowserInitializer.WebBrowser);
            }
        }
        private void ExecuteLoadGovMap(Grid grid)
        {
            WebBrowserInitializer.Url = "http://www.govmap.gov.il/";
            ExecuteRefresh(grid);
        }
        private void ExecuteLoadLeafletMap(Grid grid)
        {
            IPluginSettings pluginSetting = Plugin.GetService<IPluginSettings>();
            WebBrowserInitializer.Url = $"http://{pluginSetting.MapitWebHost}{pluginSetting.MapItPath}";
            ExecuteRefresh(grid);
        }

        private void ExecuteSave(object commandParameter)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            Mouse.OverrideCursor = null;
        }
        #endregion

        #region "Methods"

        private void LoadLayouts()
        {
            LayoutItems.Clear();
            LayoutItems = _uow.RepositoryAsync<ILayout>().Queryable().ToItems().WhenItemChanged(OnlayoutPropertyChanged);
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
                    ExecuteOpen(tab);
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

        public override void OnDocumentActivated(object sender, DocumentCollectionEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            var currentRibbonLayoutName = "none";

            UnregisterEvents();

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

            RegisterEvents();
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
