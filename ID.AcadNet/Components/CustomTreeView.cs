using FileExplorer;
using FileExplorer.Model;
using FileExplorer.ViewModel;
using ID.Infrastructure.Extensions;
using Intellidesk.AcadNet.Common.Core;
using Intellidesk.AcadNet.Common.Enums;
using Intellidesk.AcadNet.Common.Extensions;
using Intellidesk.AcadNet.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Intellidesk.AcadNet.Components
{
    public class CustomTreeView : TreeView
    {
        private IFolder _lastFolder;
        private readonly List<TreeViewItem> _list = new List<TreeViewItem>();

        public bool IsItemsLoaded
        {
            get { return Items.Count > 0 && ((IFolder)Items[0]).Items.Count > 0; }
        }

        public ICommand SelectedItemChangedCommand
        {
            get { return (ICommand)GetValue(SelectedItemChangedCommandProperty); }
            set { SetValue(SelectedItemChangedCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DoubleClickCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedItemChangedCommandProperty =
            DependencyProperty.Register("SelectedItemChangedCommand", typeof(ICommand), typeof(CustomTreeView), new UIPropertyMetadata());

        void CustomTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            IFolder currentFolder;
            if (SelectedItemChangedCommand == null) return;

            if (this.IsKeyboardFocusWithin)
            {
                currentFolder = e.NewValue as IFolder;
                if (currentFolder != null)
                {
                    ((FileExplorerViewModel)DataContext).SetCurrentFolder(currentFolder);
                }
            }
            else
            {
                var viewModel = (FileExplorerViewModel)DataContext;
                if (viewModel != null)
                {
                    currentFolder = viewModel.CurrentFolder;
                    if (currentFolder != null && currentFolder.IsSelected)
                        ((FileExplorerViewModel)DataContext).SetCurrentFolder(currentFolder);
                }
                TreeViewItem item = sender as TreeViewItem;
                if (item != null)
                {
                    var scrollViewer = (ScrollViewer)((DependencyObject)sender).FindVisualChildElement(typeof(ScrollViewer));
                    System.Windows.Point relativePosition = item.TranslatePoint(new System.Windows.Point(0, 0), scrollViewer);
                    scrollViewer.ScrollToVerticalOffset(relativePosition.Y);
                    //item.BringIntoView();
                    e.Handled = true;
                }
            }
        }

        public CustomTreeView()
        {
            LostFocus += CustomTreeView_LostFocus;
            SelectedItemChanged += CustomTreeView_SelectedItemChanged;
            PreviewKeyDown += TreeExplorer_PreviewKeyDown;

            Loaded += (sender, args) =>
            {
                //TreeExplorer_Init();
            };
        }

        private void CustomTreeView_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        public void SetCurrentFolder(IFolder folder)
        {
            var treeViewItem = this.ItemContainerGenerator.ContainerFromItem(folder) as TreeViewItem;
            if (treeViewItem != null)
                treeViewItem.IsSelected = true;
        }

        private void TreeViewSelectedItemChanged(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = sender as TreeViewItem;
            if (item != null)
            {
                item.BringIntoView();
                e.Handled = true;
            }
        }

        //public async Task<IFolder> Init(CommandArgs commandArgs, List<string> expandList = null,
        //    Action<IFolder> asyncCallBack = null)
        //{
        //    TreeViewItem treeViewItem;
        //    IFolder localFolder = null;

        //    if (!PluginBuilder.PaletteTabs[PaletteTabNames.ProjectExplorer.ToString()].Current)
        //        return new LocalFolder("C:\\", null);

        //        var items = this.Items.Cast<object>().ToList();
        //        foreach (var obj in items)
        //        {
        //            treeViewItem = this.ItemContainerGenerator.ContainerFromItem(obj) as TreeViewItem;
        //            if (treeViewItem != null)
        //            {
        //                localFolder = treeViewItem.DataContext as IFolder;
        //                treeViewItem.IsExpanded = true;

        //                if (expandList != null && expandList.Any() && localFolder != null)
        //                {
        //                    if (localFolder.CanReloading())
        //                        localFolder = await ((FolderBase)localFolder).LoadFolderChildren();

        //                    var folder = localFolder.Folders.FirstOrDefault(X => X.FullPath.ToUpper() == expandList[0].ToUpper() + "\\");

        //                    expandList.RemoveAt(0);
        //                    if (folder != null)
        //                    {
        //                        treeViewItem = treeViewItem.ItemContainerGenerator.ContainerFromItem(folder) as TreeViewItem;
        //                        await Expand(treeViewItem, commandArgs, expandList, asyncCallBack);
        //                    }
        //                }
        //            }
        //    }
        //}

        public async Task<IFolder> Expand(TreeViewItem treeItem, ICommandArgs commandArgs, List<string> expandList = null, Action<IFolder> asyncCallBack = null)
        {
            TreeViewItem treeViewItem;
            IFolder localFolder = null;

            if (!ToolsManager.PaletteTabs[PaletteNames.ProjectExplorer.ToString()].IsActive)
                return new LocalFolder("C:\\", null);

            if (treeItem == null && this.Items.Count > 0)
            {
                var items = this.Items.Cast<object>().ToList();
                foreach (var obj in items)
                {
                    treeViewItem = this.ItemContainerGenerator.ContainerFromItem(obj) as TreeViewItem;
                    if (treeViewItem != null)
                    {
                        localFolder = treeViewItem.DataContext as IFolder;
                        treeViewItem.IsExpanded = true;

                        if (expandList != null && expandList.Any() && localFolder != null)
                        {
                            if (localFolder.CanReloading())
                                localFolder = await ((FolderBase)localFolder).LoadChildrenAsync();

                            var folder = localFolder.Folders.FirstOrDefault(x => x.FullPath.ToUpper() == expandList[0].ToUpper() + "\\");

                            expandList.RemoveAt(0);
                            if (folder != null)
                            {
                                treeViewItem = treeViewItem.ItemContainerGenerator.ContainerFromItem(folder) as TreeViewItem;
                                await Expand(treeViewItem, commandArgs, expandList, asyncCallBack);
                            }
                        }
                    }
                }
            }

            if (treeItem != null && expandList != null)
            {
                localFolder = treeItem.DataContext as IFolder;
                treeItem.IsExpanded = true;

                if (expandList.Count >= 0 && localFolder != null)
                {
                    if (localFolder.CanReloading() || (commandArgs.CommandName == "Refresh"
                        && (localFolder.FullPath.Split('\\').LastOrDefault() == commandArgs.CommandParameter.ToString().Split('\\').LastOrDefault())))
                    {
                        FolderBase.IsReloading = true;
                        localFolder = await ((FolderBase)localFolder).LoadChildrenAsync();
                    }

                    if (expandList.Count == 0)
                    {
                        treeItem.IsSelected = true;
                        treeItem.Focus();
                        if (asyncCallBack != null)
                            asyncCallBack(localFolder);
                    }
                    else
                    {
                        var item = localFolder.Folders.FirstOrDefault(
                                    x => x.FullPath.ToUpper().Contains("\\" + expandList[0].ToUpper()));

                        if (item != null)
                        {
                            expandList.RemoveAt(0);
                            treeViewItem =
                                treeItem.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                            await Expand(treeViewItem, commandArgs, expandList, asyncCallBack);
                        }
                        else
                        {
                            expandList.Clear();
                        }
                    }
                }
            }
            return localFolder;
        }

        public async Task<IFolder> Expand(TreeViewItem treeItem, string targetPath = "", List<string> expandList = null, Action<IFolder> asyncCallBack = null)
        {
            TreeViewItem treeViewItem;
            IFolder localFolder = null;

            if (!ToolsManager.PaletteTabs[PaletteNames.ProjectExplorer.ToString()].IsActive)
                return new LocalFolder("C:\\", null);

            if (treeItem == null && this.Items.Count > 0)
            {
                var items = this.Items.Cast<object>().ToList();
                foreach (var obj in items)
                {
                    treeViewItem = this.ItemContainerGenerator.ContainerFromItem(obj) as TreeViewItem;
                    if (treeViewItem != null)
                    {
                        localFolder = treeViewItem.DataContext as IFolder;
                        treeViewItem.IsExpanded = true;

                        if (expandList != null && expandList.Any() && localFolder != null)
                        {
                            if (localFolder.CanReloading())
                                localFolder = await ((FolderBase)localFolder).LoadChildrenAsync();

                            var folder = localFolder.Folders.FirstOrDefault(
                                    x => x.FullPath.ToUpper() == expandList[0].ToUpper() + "\\");

                            expandList.RemoveAt(0);
                            if (folder != null)
                            {
                                treeViewItem =
                                    treeViewItem.ItemContainerGenerator.ContainerFromItem(folder) as TreeViewItem;
                                await Expand(treeViewItem, targetPath, expandList, asyncCallBack);
                            }
                        }
                    }
                }
            }

            if (treeItem != null && expandList != null)
            {
                localFolder = treeItem.DataContext as IFolder;
                treeItem.IsExpanded = true;

                if (expandList.Count >= 0 && localFolder != null)
                {
                    if (localFolder.CanReloading()) //|| (localFolder.FullPath.Split('\\').LastOrDefault() == targetPath.Split('\\').LastOrDefault()))
                    {
                        FolderBase.IsReloading = true;
                        localFolder = await ((FolderBase)localFolder).LoadChildrenAsync();
                    }

                    if (expandList.Count == 0)
                    {
                        treeItem.IsSelected = true;
                        treeItem.Focus();
                        if (asyncCallBack != null)
                            asyncCallBack(localFolder);
                    }
                    else
                    {
                        var item = localFolder.Folders.FirstOrDefault(
                                    x => x.FullPath.ToUpper().Contains("\\" + expandList[0].ToUpper()));

                        if (item != null)
                        {
                            expandList.RemoveAt(0);
                            treeViewItem =
                                treeItem.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                            await Expand(treeViewItem, targetPath, expandList, asyncCallBack);
                        }
                        else
                        {
                            expandList.Clear();
                        }
                    }
                }

                //if (treeItem.HasItems)
                //{
                //    treeViewItem = treeItem.ItemContainerGenerator.ContainerFromItem(treeItem.Items[0]) as TreeViewItem;
                //    TreeExplorerInit(treeViewItem, expandList);
                //}

                //if (this.Items.Count > 0)
                //    this.Items.Cast<object>().ToList().ForEach(o =>
                //        {
                //            var treeViewItem = this.ItemContainerGenerator.ContainerFromItem(o) as TreeViewItem;
                //            if (treeViewItem != null)
                //            {
                //                //treeViewItem.Expanded += TreeExplorer_Expanded;
                //                treeViewItem.IsExpanded = true;
                //                //_list.Add(treeViewItem);
                //            }
                //        });
            }
            return localFolder;
        }

        //!!!!!!!!del!!!!!!!!!
        void TreeExplorer_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            IFolder folder = e.NewValue as IFolder;
            if (folder.IsNull())
            {
                return;
            }
            ((FileExplorerViewModel)DataContext).SetCurrentFolder(folder);
        }

        void TreeExplorer_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            IFolder folder = SelectedItem as IFolder;
            if (folder.IsNull())
            {
                return;
            }

            IFolder newSelectFolder = null;
            switch (e.Key)
            {
                case Key.Left:
                    newSelectFolder = MoveLeft(folder);
                    break;

                case Key.Right:
                    MoveRight(folder);
                    break;

                case Key.Up:
                    newSelectFolder = MoveUp(folder);
                    break;

                case Key.Down:
                    newSelectFolder = MoveDown(folder);
                    break;
                default:
                    break;
            }

            if (!newSelectFolder.IsNull())
            {
                TreeViewItem treeItem = ItemContainerGenerator.ContainerFromItem(newSelectFolder) as TreeViewItem;
                if (treeItem != null)
                {
                    treeItem.Focus();
                }
            }
            e.Handled = true;
        }

        protected void ItemsEventClear()
        {
            //_list.ForEach(item => item.Expanded -= TreeExplorer_Expanded);
            _list.Clear();
        }

        public void ExpandAll() //TreeView treeView
        {
            ExpandSubContainers(this);
        }

        #region Tree key navigation
        private IFolder MoveLeft(IFolder folder)
        {
            IFolder newSelectFolder = null;
            if (folder.IsNull())
            {
                return newSelectFolder;
            }

            ///The last folder explanded and selection will cause a splash
            ///So only one action can be trigged here
            if (!_lastFolder.IsNull() && _lastFolder == folder)
            {
                _lastFolder.IsExpanded = false;
                _lastFolder = null;
                return newSelectFolder;
            }

            if (folder.Parent == folder ||
                (!folder.Parent.VirtualParent.IsNull() && folder.Parent.VirtualParent == folder.Parent.VirtualParent.Parent))
            {
                if (folder.Parent == folder)
                {
                    newSelectFolder = folder;
                }
                else
                {
                    newSelectFolder = folder.Parent.VirtualParent;
                }
                newSelectFolder.IsSelected = true;
                _lastFolder = newSelectFolder;
                return newSelectFolder;
            }
            newSelectFolder = folder.Parent;
            newSelectFolder.IsSelected = true;
            _lastFolder = newSelectFolder;
            return newSelectFolder;
        }

        private async Task<IFolder> MoveRight(IFolder folder)
        {
            IFolder newSelectFolder = null;
            if (folder.IsNull())
            {
                return newSelectFolder;
            }

            await folder.GetFoldersAsync();

            folder.IsExpanded = true;
            if (folder.Folders.IsNullOrEmpty())
            {
                newSelectFolder = folder;
                newSelectFolder.IsSelected = true;
                return folder;
            }

            IFolder firstChild = folder.Folders.FirstOrDefault();
            newSelectFolder = firstChild;
            if (newSelectFolder != null) newSelectFolder.IsSelected = true;

            return newSelectFolder;
        }

        private IFolder MoveUp(IFolder folder)
        {
            IFolder newSelectFolder = null;
            if (folder.IsNull())
            {
                return newSelectFolder;
            }

            if (folder.Parent == folder)
            {
                var allTreeItems = Items;
                int index = allTreeItems.IndexOf(folder);
                if (index > 0)
                {
                    newSelectFolder = allTreeItems[index - 1] as IFolder;
                    if (newSelectFolder.IsExpanded)
                    {
                        newSelectFolder = newSelectFolder.Folders.LastOrDefault();
                    }
                    newSelectFolder.IsSelected = true;
                }
                return newSelectFolder;
            }

            var upFolders = folder.Parent.Folders;
            int upIndex = upFolders.IndexOf(folder);
            if (upIndex > 0)
            {
                var upFolder = upFolders[upIndex - 1];
                newSelectFolder = upFolder;

                ///Folder with no sub folder, the IsExpanded maybe true
                while (!upFolder.IsNull() && upFolder.IsExpanded)
                {
                    newSelectFolder = upFolder.Folders.LastOrDefault();
                    if (!newSelectFolder.IsNull())
                    {
                        if (!newSelectFolder.IsExpanded || upFolder.Folders.IsNullOrEmpty())
                        {
                            break;
                        }
                        else
                        {
                            upFolder = newSelectFolder;
                        }
                    }
                    else
                    {
                        newSelectFolder = upFolder;
                        break;
                    }
                }
                if (!newSelectFolder.IsNull())
                {
                    newSelectFolder.IsSelected = true;
                }
            }
            else
            {
                if (folder.Parent == folder ||
                    (!folder.Parent.VirtualParent.IsNull() && folder.Parent.VirtualParent == folder.Parent.VirtualParent.Parent))
                {
                    var allTreeItems = Items;
                    int index = -1;
                    if (folder.Parent == folder)
                    {
                        index = allTreeItems.IndexOf(folder);
                        if (index > 0)
                        {
                            newSelectFolder = allTreeItems[index - 1] as IFolder;
                            newSelectFolder.IsSelected = true;
                        }
                    }
                    else
                    {
                        newSelectFolder = folder.Parent.VirtualParent;
                        newSelectFolder.IsSelected = true;
                    }
                }
                else
                {
                    newSelectFolder = folder.Parent;
                    newSelectFolder.IsSelected = true;
                }
            }
            return newSelectFolder;
        }

        private IFolder MoveDown(IFolder folder)
        {
            IFolder newSelectFolder = null;
            if (folder.IsNull())
            {
                return newSelectFolder;
            }

            if (folder.IsExpanded)
            {
                var nextFolder = folder.Folders.FirstOrDefault();
                if (!nextFolder.IsNull())
                {
                    newSelectFolder = nextFolder;
                    newSelectFolder.IsSelected = true;
                    return newSelectFolder;
                }
            }

            var parentFolders = folder.Parent.Folders;
            int downIndex = parentFolders.IndexOf(folder);
            if (downIndex >= 0 && downIndex < parentFolders.Count - 1)
            {
                newSelectFolder = parentFolders[downIndex + 1];
                newSelectFolder.IsSelected = true;
            }
            else
            {
                var parent = folder.Parent;
                while (!parent.IsNull())
                {
                    if (parent == folder)
                    {
                        var allTreeItems = Items;
                        int index = allTreeItems.IndexOf(folder.Parent);
                        if (index < allTreeItems.Count - 1)
                        {
                            newSelectFolder = allTreeItems[index + 1] as IFolder;
                            newSelectFolder.IsSelected = true;
                        }
                        break;
                    }
                    else
                    {
                        parentFolders = parent.Parent.Folders;
                        downIndex = parentFolders.IndexOf(parent);
                        if (downIndex >= 0 && downIndex < parentFolders.Count - 1)
                        {
                            newSelectFolder = parentFolders[downIndex + 1];
                            newSelectFolder.IsSelected = true;
                            break;
                        }
                        else
                        {
                            folder = parent;
                            parent = parent.Parent;
                        }
                    }
                }
            }

            return newSelectFolder;
        }

        #endregion

        void ExpandSubContainers(ItemsControl parentContainer)
        {
            foreach (object item in parentContainer.Items)
            {
                TreeViewItem currentContainer = parentContainer.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                if (currentContainer != null && currentContainer.Items.Count > 0)
                {
                    // Expand the current item. 
                    currentContainer.IsExpanded = true;
                    if (currentContainer.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
                    {
                        // If the sub containers of current item is not ready, we need to wait until 
                        // they are generated. 
                        currentContainer.ItemContainerGenerator.StatusChanged += delegate
                        {
                            ExpandSubContainers(currentContainer);
                        };
                    }
                    else
                    {
                        // If the sub containers of current item is ready, we can directly go to the next 
                        // iteration to expand them. 
                        ExpandSubContainers(currentContainer);
                    }
                }
            }
        }

        //!!!!!!!!del!!!!!!!!!
        void ItemCheckBox_Click(object sender, RoutedEventArgs e)
        {
            IFolder folder = ColumnPropertyHelper.GetFolder(this);
            //If search view the current folder is null
            if (folder != null) return;

            //SearchViewModel sv = this.FileOrderViewModel as SearchViewModel;
            //if (sv == null) return;

            //var list = sv.FolderDetailsCollectionView.SourceCollection as IList<IFile>;
            //if (list == null) return;

            //if (list.All(item => item.IsChecked == true))
            //{
            //    SetIsChecked(this, true);
            //}
            //else if (list.All(item => item.IsChecked == false))
            //{
            //    SetIsChecked(this, false);
            //}
            //else
            //{
            //    SetIsChecked(this, null);
            //}
        }

        //!!!!!!!!del!!!!!!!!!
        void Header_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader clickHeader = e.OriginalSource as GridViewColumnHeader;
            if (clickHeader == null || clickHeader.Column == null ||
                string.IsNullOrEmpty(ColumnPropertyHelper.GetSortPropertyName(clickHeader.Column)))
            {
                return;
            }

            //ListSortDirection direction = ListSortDirection.Ascending;
            //if (clickHeader != lastHeader)
            //{
            //    direction = ListSortDirection.Descending;
            //}
            //else
            //{
            //    if (lastDirection == ListSortDirection.Ascending)
            //    {
            //        direction = ListSortDirection.Descending;
            //    }
            //    else
            //    {
            //        direction = ListSortDirection.Ascending;
            //    }
            //}

            //SetSortOrder(clickHeader, direction);
        }
    }
}