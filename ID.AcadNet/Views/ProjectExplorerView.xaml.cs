using Autodesk.AutoCAD.Windows;
using FileExplorer.Model;
using ID.Infrastructure;
using ID.Infrastructure.Extensions;
using ID.Infrastructure.Interfaces;
using Intellidesk.AcadNet.Common.Core;
using Intellidesk.AcadNet.Common.Enums;
using Intellidesk.AcadNet.Common.Interfaces;
using Intellidesk.AcadNet.Common.Models;
using Intellidesk.AcadNet.Components;
using Intellidesk.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using Files = ID.Infrastructure.Files;
using ListViewItem = System.Windows.Controls.ListViewItem;
using MenuItem = System.Windows.Controls.MenuItem;
using PaletteViewStatus = Intellidesk.AcadNet.Common.Enums.PaletteViewStatus;
using Size = System.Drawing.Size;

namespace Intellidesk.AcadNet.Views
{
    /// <summary> Layout View </summary>
    public partial class ProjectExplorerView : ITabProjectExplorerView
    {
        public PaletteNames PanelTabName => PaletteNames.ProjectExplorer;
        private static CustomTreeView _treeExplorer;

        private IProjectExplorerPanelContext ProjectExplorerDataContext { get; }

        private ScrollViewer _scrollViewer;

        /// <summary> ctor </summary>
        public ProjectExplorerView()
        {
            try
            {
                Name = "ProjectExplorer";
                Title = "Project Explorer";
                _treeExplorer = TreeExplorer;

                Dispatcher.UnhandledException += OnDispatcherUnhandledException;

                InitializeComponent();

                DataContext = ProjectExplorerDataContext = Plugin.GetService<IProjectExplorerPanelContext>();
            }
            catch (Exception)
            {
                // ignored
            }

            //var binding = new System.Windows.Data.Binding("(Validation.HasError)");
            //binding.Source = SearchText;
            //binding.UpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged;
            //binding.Mode = System.Windows.Data.BindingMode.OneWay;
            //binding.NotifyOnValidationError = true;
            //binding.ValidatesOnExceptions = true;
            ////binding.Converter = new TempValueConverter();
            //SearchButton.SetBinding(IsEnabledProperty, binding);

            SetLanguageDictionary();

            //TreeExplorer = new TreeView();
            //TreeExplorer.ItemTemplate = this.Resources["FolderHierarchicalDataTemplate"] as HierarchicalDataTemplate;
            //TreeExplorer.SelectedItemChanged += TreeExplorer_SelectedItemChanged;
            //TreeExplorer.PreviewKeyDown += TreeExplorer_PreviewKeyDown;
            //TreeExplorer.Items.Cast<TreeViewItem>().ToList().ForEach(x => x.Expanded += TreeExplorer_Expanded);
            //TreeHolder.Children.Add(TreeExplorer);

            //_fileExplorerViewModel = new FileExplorerViewModel(this.Dispatcher);
            //_fileExplorerViewModel.LoadLocalExplorer();
            //TreeExplorer.DataContext = FileExplorerViewModel;
            //var selected = TreeExplorer.SelectedItem as TreeViewItem;

            //if (selected != null)
            //{
            //    selected.Items.Add(new TreeViewItem { Header = "NewItemInput1Text" });
            //}
            //else
            //{
            //    TreeExplorer.Items.Add(new TreeViewItem { Header = "NewItemInput2Text" });
            //}

            Loaded += (obj, e) =>
            {
                NotifyButton.Visibility = Visibility.Collapsed;
                //FileExplorerViewModel.LoadLocalExplorer();
            };

            //Create the CablePanelDataContext and expose it using the View's ViewNodel
            if (ProjectExplorerDataContext != null)
            {
                //GrdMain.RowDefinitions[2].Height = new GridLength(
                //    _appSettings.ProjectExplorerFoldersSectionHeight > 600
                //        ? 600 : _appSettings.ProjectExplorerFoldersSectionHeight, GridUnitType.Pixel);

                ////this.DataGrid.ItemTemplate = this.ContentTemplateSelector.SelectTemplate(value, this); 
            }
        }

        #region <Events>

        public void OnActivate(ICommandArgs argument = null)
        {
            IPluginSettings pluginSettings = Plugin.GetService<IPluginSettings>();
            if (argument != null)
            {
                argument.Sender = TreeExplorer;
                ProjectExplorerDataContext.RefreshCommand.Execute(argument);
            }
            else if (!TreeExplorer.HasItems || (TreeExplorer.Items.Count > 0 && ((IFolder)TreeExplorer.Items[0]).Items.Count == 0))
            {
                if (ProjectExplorerDataContext != null)
                    ProjectExplorerDataContext.RefreshCommand.Execute(new CommandArgs(TreeExplorer, "Activate", pluginSettings.CurrentFolder));
            }
        }
        public void OnDeactivate()
        {
        }

        private async void TreeExplorer_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = e.OriginalSource as TreeViewItem;
            if (item == null) return;

            FolderBase folder = item.DataContext as FolderBase;
            if (folder == null) return;

            var tree = (UIElement)sender;
            if ((tree.IsKeyboardFocusWithin || tree.IsMouseCaptureWithin) && !folder.IsFolderLoaded)
                await folder.LoadChildrenAsync();
        }
        private void TreeExplorer_OnSelected(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = e.OriginalSource as TreeViewItem;
            if (item != null)
            {
                //TreeView lv = ((TreeView)sender);
                item.Focus();
                item.BringIntoView();

                if (!((UIElement)sender).IsKeyboardFocusWithin)
                {
                    System.Windows.Point relativePosition = item.TranslatePoint(new System.Windows.Point(0, 0), _scrollViewer);
                    _scrollViewer.ScrollToVerticalOffset(relativePosition.Y - _scrollViewer.ViewportHeight / 2);
                }

                //IFolder folder = item.DataContext as IFolder;
                //if (folder != null)
                //    RefreshButton.ToolTip = "Refresh folder: " + folder.FullPath;
            }
        }
        private void TreeExplorer_Loaded(object sender, RoutedEventArgs e)
        {
            //Border border = VisualTreeHelper.GetChild(sender as DependencyObject, 0) as Border;
            //_scrollViewer = VisualTreeHelper.GetChild(border, 0) as ScrollViewer;
            //var scrollViewer = treeView1.Template.FindName("_tv_scrollviewer_", treeView1) as ScrollViewer;
            //scrollViewer.ScrollToHome();
            _scrollViewer = (ScrollViewer)Common.Extensions.DependencyObjectExtensions.FindVisualChildElement((DependencyObject)sender, typeof(ScrollViewer));
        }
        private void TreeView_OnDragCompleted(object sender, DragCompletedEventArgs e)
        {
            IPluginSettings pluginSettings = Plugin.GetService<IPluginSettings>();
            if (((GridSplitter)sender).Name.Contains("Tree"))
                pluginSettings.ProjectExplorerFoldersSectionHeight = Convert.ToInt32(GrdMain.RowDefinitions[0].ActualHeight);
            //else
            //    _appSettings.ProjectExplorerFilesSectionHeight = Convert.ToInt32(GrdMain.RowDefinitions[5].ActualHeight);

            //_appSettings.ProjectExplorerPropertySectionHeight = Convert.ToInt32(GrdMain.RowDefinitions[7].ActualHeight);
            //var h = GrdMain.RowDefinitions[0].ActualHeight + GrdMain.RowDefinitions[1].ActualHeight + GrdMain.RowDefinitions[2].ActualHeight + GrdMain.RowDefinitions[3].ActualHeight + 10;
            //TreeViewGridSplitter.BringIntoView();
            //if (h > GrdMain.ActualHeight)
            //    GrdMain.RowDefinitions[0].Height = new GridLength(GrdMain.RowDefinitions[0].ActualHeight - (h - GrdMain.ActualHeight)) ;
        }
        private void TreeExplorer_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            //double index = 200 - Math.Truncate(_scrollViewer.ViewportHeight / 2);
            //_scrollViewer.ScrollToVerticalOffset(index);
        }
        private void SearchText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Validation.GetHasError(SearchText))
            {
                SearchButton.IsEnabled = ProjectExplorerDataContext.SearchViewModel.IsSearchEnabled = false;
                NotifyButton.Visibility = Visibility.Visible;
                NotifyButton.ToolTip = Validation.GetErrors(SearchText)[0].ErrorContent;
            }
            else
            {
                SearchButton.IsEnabled = ProjectExplorerDataContext.SearchViewModel.IsSearchEnabled = true;
                NotifyButton.Visibility = Visibility.Collapsed;
                NotifyButton.ToolTip = "";
            }
        }
        void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            string errorMessage = string.Format("An unhandled exception occurred: {0}", e.Exception.Message);
            MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }
        private void PropertyCheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region <Methods>
        public void Refresh(bool flagManualChange = false)
        {
            //throw new NotImplementedException();
        }

        public void Apply()
        {
            //throw new NotImplementedException();
        }

        private void FileItemListViewPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ListViewItem item = (e.OriginalSource as UIElement).GetParentFromVisualTree(typeof(ListViewItem)) as ListViewItem;
            if (item != null && item.ContextMenu == null)
            {
                IFile selectedFileItem = item.Content as IFile;
                if (selectedFileItem != null && (selectedFileItem.FullPath.Contains(".dwg") || selectedFileItem.FullPath.Contains(".lnk")))
                {
                    // e.Handled = true;
                    var rm = Intellidesk.Resources.Properties.Resources.ResourceManager;
                    var menu = new System.Windows.Controls.ContextMenu();

                    var currentFolderPath = ProjectExplorerDataContext.FileExplorerViewModel.CurrentFolder.FullPath;
                    var workItemsFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Intellidesk", "WorkItems");

                    var fullPath = selectedFileItem.FullPath.Contains(".lnk")
                        ? Files.GetShortcutTargetFile(selectedFileItem.FullPath)
                        : selectedFileItem.FullPath;

                    if (currentFolderPath.ToLower() != workItemsFolderPath.ToLower())
                        menu.Items.Add(new MenuItem
                        {
                            Header = $"add '{fullPath}' to work items",
                            Icon = rm.GetImage("property"),
                            Command = ProjectExplorerDataContext.AddToLayotsCommand,
                            CommandParameter = selectedFileItem
                            //Icon.FromHandle(Properties.Resources.open.GetHicon())
                        });
                    else
                        menu.Items.Add(new MenuItem
                        {
                            Header = $"goto folder of '{fullPath}'",
                            Icon = rm.GetImage("property"),
                            Command = ProjectExplorerDataContext.GoToFolderCommand,
                            CommandParameter = selectedFileItem
                            //Icon.FromHandle(Properties.Resources.open.GetHicon())
                        });

                    menu.Items.Add(new MenuItem
                    {
                        Header = $"remove '{selectedFileItem.FullPath}' from work items",
                        Icon = rm.GetImage("property"),
                        Command = ProjectExplorerDataContext.RemoveLayoutCommand,
                        CommandParameter = selectedFileItem
                        //Icon.FromHandle(Properties.Resources.open.GetHicon())
                    });
                    item.ContextMenu = menu;
                }
                item.Focus();
            }
        }
        private void TreeExplorerPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = (e.OriginalSource as UIElement).GetParentFromVisualTree(typeof(TreeViewItem)) as TreeViewItem;
            if (item != null && item.ContextMenu == null)
            {
                IFolder selectedItem = item.Header as IFolder;
                if (selectedItem != null && ProjectExplorerDataContext.FileExplorerViewModel.BaseFolder.Parent != null)
                {
                    // e.Handled = true;
                    var rm = Intellidesk.Resources.Properties.Resources.ResourceManager;
                    System.Windows.Controls.ContextMenu menu = new System.Windows.Controls.ContextMenu();
                    menu.Items.Add(new MenuItem
                    {
                        Header = $"add '{selectedItem.FullPath}' to base work folders",
                        Icon = rm.GetImage("property"),
                        Command = ProjectExplorerDataContext.SetFolderBaseCommand,
                        CommandParameter = selectedItem
                        //Icon.FromHandle(Properties.Resources.open.GetHicon())
                    });
                    item.ContextMenu = menu;
                }
                item.Focus();
            }
        }
        //private MenuItem AddMenuItem(ContextMenu cm, string text, EventHandler handler, object context)
        //{
        //    /MenuItem item = new MenuItem(text, new Icon(typeof(Properties.Resources),"open"));
        //    //item.Click += (sender, args) => { };
        //    //cm.Items.Add(item);
        //    //return item;
        //}

        //public void TreeExplorerRefresh(List<string> expandList = null)
        //{
        //    TreeExplorer.Focus();
        //    TreeExplorer.Expand(null, expandList.LastOrDefault(), expandList);
        //}

        //public async void SetSelectedFolder(IFolder folder)
        //{
        //    await FileExplorerViewModel.LoadFolderChildren(folder);
        //    FileExplorerViewModel.SetCurrentFolder(folder);
        //}

        public void ExpandFolder(IFolder folder, bool isLoadLocalExplorer = false)
        {
            ProjectExplorerDataContext.RefreshCommand.Execute(TreeExplorer);
        }
        private void ExpandAllNodes(TreeViewItem treeItem, List<string> expandList = null)
        {
            if (expandList != null && expandList.Contains(treeItem.Name))
                treeItem.IsExpanded = true;
            foreach (var childItem in treeItem.Items.OfType<TreeViewItem>())
            {
                ExpandAllNodes(childItem);
            }
        }
        private void SetLanguageDictionary()
        {
            ResourceDictionary dict = new ResourceDictionary
            {
                Source = new Uri("/ID.AcadNet;component/Assets\\StringResources.xaml", UriKind.Relative)
            };
            //switch (Thread.CurrentThread.CurrentCulture.ToString())
            //{
            //    case "en-US":
            //        dict.Source = new Uri("/ID.AcadNet;component/Assets\\StringResources.xaml", UriKind.Relative);
            //        break;
            //    case "he-IL":
            //        dict.Source = new Uri("/ID.AcadNet;component/Assets\\StringResources.he-IL.xaml", UriKind.Relative);
            //        break;
            //    default:
            //        dict.Source = new Uri("/ID.AcadNet;component/Assets\\StringResources.xaml", UriKind.Relative);
            //        break;
            //}
            this.Resources.MergedDictionaries.Add(dict);
        }
        #endregion

        #region <Implementation IPanelTabView>
        public Size MinimumSize { get; set; }
        public Size MaximumSize { get; set; }
        public PaletteViewStatus Status { get; set; }
        public Autodesk.AutoCAD.Windows.StateEventIndex TabState { get; set; }
        public Palette ParentPalette { get; set; }
        public object ParentPaletteSet { get; set; }
        public bool IsLive { get; set; }
        public bool IsActive { get; set; }
        public string Title { get; set; }
        public int UniId { get; set; }
        bool IPanelTabView.Visible { get; set; }
        public bool Complete { get; set; }
        public string Comment { get; set; }
        public Size Size { get; set; }
        public ICommandArgs ActivateArgument { get; set; }
        #endregion

        private void Row_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {

            List<string> menuOptions = new List<string>();
            ListViewItem sourceRow = sender as ListViewItem;

            if (sourceRow != null)
            {
                sourceRow.ContextMenu = new System.Windows.Controls.ContextMenu();

                //XmlElement targetItem = sourceRow.DataContext as XmlElement;
                //if (targetItem != null)
                //{
                //    sourceRow.ContextMenu.ItemsSource = new List<MenuItem>(targetItem);
                //}

                if (sourceRow.ContextMenu.Items.Count > 0)
                {
                    sourceRow.ContextMenu.PlacementTarget = this;
                    sourceRow.ContextMenu.IsOpen = true;
                }
                else
                {
                    var menu = new System.Windows.Controls.ContextMenu();
                    menu.Items.Add(new MenuItem
                    {
                        Header = "23234",//$"add '{selectedItem.FullPath}' to work projects",
                        //Icon = rm.GetImage("property"),
                        //Command = ProjectExplorerDataContext.AddToLayotsCommand,
                        //CommandParameter = selectedItem
                        //Icon.FromHandle(Properties.Resources.open.GetHicon())
                    });
                    sourceRow.ContextMenu = menu;
                }
            }

        }
    }
}
