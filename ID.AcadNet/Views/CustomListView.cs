using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Unity;

using FileExplorer;
using FileExplorer.Model;
using FileExplorer.ViewModel;
using Intellidesk.AcadNet.Common.Core;
using Intellidesk.Infrastructure;
using Intellidesk.Infrastructure.Core;
using Intellidesk.AcadNet.Common.Enums;
using Intellidesk.Infrastructure.Enums;
using Intellidesk.Infrastructure.Extensions;
using Intellidesk.Infrastructure.Logging;

namespace Intellidesk.AcadNet.Views
{
    public class CustomListView : ListView
    {
        public event EventHandler<ContentEventArgs<IFile>> ItemClicked;

        public ISortOrder ListViewModel
        {
            get { return DataContext as ISortOrder; }
        }

        public ICommand DoubleClickCommand
        {
            get { return (ICommand)GetValue(DoubleClickCommandProperty); }
            set { SetValue(DoubleClickCommandProperty, value); }
        }

        public ICommand SelectionCommand
        {
            get { return (ICommand)GetValue(SelectionCommandProperty); }
            set { SetValue(SelectionCommandProperty, value); }
        }

        public static readonly DependencyProperty SelectionCommandProperty =
            DependencyProperty.Register("SelectionCommand", typeof(ICommand), typeof(CustomListView), new UIPropertyMetadata());


        // Using a DependencyProperty as the backing store for DoubleClickCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DoubleClickCommandProperty =
            DependencyProperty.Register("DoubleClickCommand", typeof(ICommand), typeof(CustomListView), new UIPropertyMetadata());

        public CustomListView()
        {
            //this.Click += new MouseButtonEventHandler(CustomListView_PreviewMouseSingleClick);
            PreviewMouseDoubleClick += CustomListView_PreviewMouseDoubleClick;
            SelectionChanged += CustomListView_Selection;
        }

        private void CustomListView_Selection(object sender, SelectionChangedEventArgs e)
        {
            var colHeader = (e.OriginalSource as DependencyObject).TryFindParent<GridViewColumnHeader>();
            if (colHeader != null) return;

            if (SelectionCommand != null)
                SelectionCommand.Execute(((CustomListView)sender).SelectedItem);
        }

        void CustomListView_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left) return;

            var colHeader = (e.OriginalSource as DependencyObject).TryFindParent<GridViewColumnHeader>();
            if (colHeader != null) return;

            if (DoubleClickCommand != null)
                DoubleClickCommand.Execute(((CustomListView)sender).SelectedItem);
        }

        #region DP

        /// <summary>
        ///Content view
        /// </summary>
        public static readonly DependencyProperty ContentViewProperty =
            DependencyProperty.Register("FolderDetailsCollectionView", typeof(ICollectionView), typeof(CustomListView));

        public static void SetContentView(DependencyObject element, ICollectionView value)
        {
            if (element == null)
                return;
            element.SetValue(ContentViewProperty, value);
        }

        public static ICollectionView GetContentView(DependencyObject element)
        {
            if (element == null) return null;
            return (ICollectionView)element.GetValue(ContentViewProperty);
        }

        /// <summary>
        ///Is Loading
        /// </summary>
        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.Register("IsLoading", typeof(bool), typeof(CustomListView));

        public static void SetIsLoading(DependencyObject element, bool value)
        {
            if (element == null)
                return;
            element.SetValue(IsLoadingProperty, value);
        }

        public static bool GetIsLoading(DependencyObject element)
        {
            if (element == null)
                return false;
            return (bool)element.GetValue(IsLoadingProperty);
        }

        /// <summary>
        ///Is content empty
        /// </summary>
        public static readonly DependencyProperty IsEmptyProperty =
            DependencyProperty.Register("IsEmpty", typeof(bool), typeof(CustomListView));

        public static void SetIsEmpty(DependencyObject element, bool value)
        {
            if (element == null)
                return;
            element.SetValue(IsEmptyProperty, value);
        }

        public static bool GetIsEmpty(DependencyObject element)
        {
            if (element == null)
                return false;
            return (bool)element.GetValue(IsEmptyProperty);
        }

        /// <summary>
        ///Empty hint
        /// </summary>
        public static readonly DependencyProperty EmptyHintProperty =
            DependencyProperty.Register("EmptyHint", typeof(string), typeof(CustomListView));

        public static void SetEmptyHint(DependencyObject element, string value)
        {
            if (element == null)
                return;
            element.SetValue(EmptyHintProperty, value);
        }

        public static string GetEmptyHint(DependencyObject element)
        {
            if (element == null)
                return string.Empty;
            return (string)element.GetValue(EmptyHintProperty);
        }

        /// <summary>
        ///Search folder width
        /// </summary>
        public static readonly DependencyProperty FolderPathWidthProperty =
            DependencyProperty.Register("FolderPathWidth", typeof(double), typeof(CustomListView));

        public static void SetFolderPathWidth(DependencyObject element, double value)
        {
            if (element == null)
                return;
            element.SetValue(FolderPathWidthProperty, value);
        }

        public static double GetFolderPathWidth(DependencyObject element)
        {
            if (element == null)
                return 0;
            return (double)element.GetValue(FolderPathWidthProperty);
        }

        /// <summary>
        ///Is check all enabled
        /// </summary>
        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register("IsChecked", typeof(bool?), typeof(CustomListView));

        public static void SetIsChecked(DependencyObject element, bool? value)
        {
            if (element == null)
                return;
            element.SetValue(IsCheckedProperty, value);
        }

        public static bool? GetIsChecked(DependencyObject element)
        {
            if (element == null)
                return false;
            return (bool)element.GetValue(IsCheckedProperty);
        }
        public bool IsChecked
        {
            get { return (bool)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }

        /// <summary> 
        /// Is check all enabled 
        /// </summary>
        public static readonly DependencyProperty IsCheckEnabledProperty =
            DependencyProperty.Register("IsCheckEnabled", typeof(bool), typeof(CustomListView),
                new PropertyMetadata(true));

        public static void SetIsCheckEnabled(DependencyObject element, bool value)
        {
            if (element == null)
                return;
            element.SetValue(IsCheckEnabledProperty, value);
        }

        public static bool GetIsCheckEnabled(DependencyObject element)
        {
            if (element == null)
                return false;
            return (bool)element.GetValue(IsCheckEnabledProperty);
        }

        public bool IsCheckEnabled
        {
            get { return (bool)GetValue(IsCheckEnabledProperty); }
            set { SetValue(IsCheckEnabledProperty, value); }
        }

        #endregion

        #region ListViewFileDetails events

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

        /// <summary>
        /// double click folder item to open sub directory
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        void LvContent_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left) return;

            var colHeader = (e.OriginalSource as DependencyObject).TryFindParent<GridViewColumnHeader>();
            if (colHeader != null)
            {
                return;
            }

            IFile item = this.SelectedItem as IFile;
            if (ItemClicked != null && item != null)
            {
                ItemClicked(this, new ContentEventArgs<IFile>(item));
            }

            e.Handled = true;
        }

        //private T TryFindParent<T>(DependencyObject current) where T : class
        //{
        //    DependencyObject parent = VisualTreeHelper.GetParent(current);

        //    if (parent == null)
        //        return null;

        //    if (parent is T)
        //        return parent as T;
        //    else
        //        return TryFindParent<T>(parent);
        //}

        //IEnumerable<T> GetVisualChildren<T>(DependencyObject parent) where T : DependencyObject
        //{
        //    int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
        //    for (int i = 0; i < childrenCount; i++)
        //    {
        //        DependencyObject child = VisualTreeHelper.GetChild(parent, i);
        //        if (child is T)
        //            yield return (T)child;

        //        foreach (var descendant in GetVisualChildren<T>(child))
        //            yield return descendant;
        //    }
        //}

        void UcContent_ItemClicked(object sender, ContentEventArgs<IFile> e)
        {
            IFile item = e.Content;
            if (item == null) return;

            if (item is IFolder)
            {
                IFolder folder = item as IFolder;
                folder.IsExpanded = true;
                folder.IsSelected = true;
                //DataContext.FileExplorerViewModel.SetCurrentFolder(folder);
            }
            else if (item is LocalFile)
            {
                if (File.Exists(item.FullPath))
                {
                    var eventAggregator = PluginBootstrapper.PluginContainer.Resolve<IEventAggregator>();
                    try
                    {
                        Mouse.OverrideCursor = Cursors.Wait;
                        eventAggregator.GetEvent<NotifyMessageStringEvent>().Publish("Loading...");
                        Thread.Sleep(1000);
                        Process.Start(item.FullPath);
                    }
                    catch (Exception ex)
                    {
                        eventAggregator.GetEvent<NotifyMessageEvent>().Publish(new NotifyArgs(NotifyStatus.Error, "Error", ex.ToStringMessage()));
                        Logger.Debug("Exception:", ex);
                    }
                    finally
                    {
                        eventAggregator.GetEvent<NotifyMessageStringEvent>().Publish("Ready");
                        Mouse.OverrideCursor = null;
                    }
                }
            }
        }

        #endregion

        #region Header sort

        public void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Thumb thumb = e.OriginalSource as Thumb;
            if (thumb == null) return;

            GridViewColumnHeader header = thumb.TemplatedParent as GridViewColumnHeader;
            if (header == null) return;

            GridViewColumn column = header.Column;
            if (column == null) return;

            double minWidth = ColumnPropertyHelper.GetMinWidth(column);
            double maxWidth = ColumnPropertyHelper.GetMaxWidth(column);

            if (header.Column.ActualWidth < minWidth)
                header.Column.Width = minWidth;
            if (header.Column.ActualWidth > maxWidth)
                header.Column.Width = maxWidth;
        }

        GridViewColumnHeader lastHeader = null;
        ListSortDirection lastDirection = ListSortDirection.Ascending;

        void Header_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader header = e.OriginalSource as GridViewColumnHeader;
            if (header == null || header.Column == null ||
                string.IsNullOrEmpty(ColumnPropertyHelper.GetSortPropertyName(header.Column)))
            {
                return;
            }

            ListSortDirection direction = ListSortDirection.Ascending;
            if (header != lastHeader)
            {
                direction = ListSortDirection.Descending;
            }
            else
            {
                if (lastDirection == ListSortDirection.Ascending)
                {
                    direction = ListSortDirection.Descending;
                }
                else
                {
                    direction = ListSortDirection.Ascending;
                }
            }

            //SetSortOrder(header, direction);
        }

        private void SetSortOrder(GridViewColumnHeader header, ListSortDirection direction)
        {
            if (lastHeader == null)
            {
                lastHeader = ColNameHeader;
            }
            if (lastHeader == null)
            {
                return;
            }

            string propName = ColumnPropertyHelper.GetSortPropertyName(header.Column);
            if (string.IsNullOrEmpty(propName))
            {
                return;
            }

            ListViewModel.SetSortOrder(propName, direction);

            lastHeader.ContentTemplate = NormalHeaderDataTemplate;
            switch (direction)
            {
                case ListSortDirection.Ascending:
                    header.ContentTemplate = AscHeaderDataTemplate;
                    break;
                case ListSortDirection.Descending:
                    header.ContentTemplate = DescHeaderDataTemplate;
                    break;
                default:
                    header.ContentTemplate = NormalHeaderDataTemplate;
                    break;
            }
            lastHeader = header;
            lastDirection = direction;
        }

        public void SetSortOrderCallback()
        {
            this.ListViewModel.SetSortOrderCallback(SortOrderCallback);
            SortOrderCallback();
        }

        public void SortOrderCallback(bool isRefresh = true)
        {
            if (lastHeader != null && lastHeader != ColNameHeader)
            {
                lastHeader.ContentTemplate = NormalHeaderDataTemplate;
            }
            lastHeader = ColNameHeader;
            if (lastHeader != null)
            {
                lastHeader.ContentTemplate = AscHeaderDataTemplate;
                lastDirection = ListSortDirection.Ascending;
            }
        }

        GridViewColumnHeader colNameHeader = null;
        private GridViewColumnHeader ColNameHeader
        {
            get
            {
                if (colNameHeader == null)
                {
                    var allHeaders = this.GetVisualChildren<GridViewColumnHeader>();
                    foreach (var item in allHeaders)
                    {
                        if (item.Column != null &&
                            ColumnPropertyHelper.GetSortPropertyName(item.Column) == FileExplorerViewModel.SortPropertyName)
                        {
                            colNameHeader = item;
                            break;
                        }
                    }
                }
                return colNameHeader;
            }
        }

        private DataTemplate AscHeaderDataTemplate
        {
            get { return GetHeaderTemplate("AscSortHeaderTemplate") as DataTemplate; }
        }

        private DataTemplate DescHeaderDataTemplate
        {
            get { return GetHeaderTemplate("DescSortHeaderTemplate") as DataTemplate; }
        }

        private DataTemplate NormalHeaderDataTemplate
        {
            get { return GetHeaderTemplate("NormalSortHeaderTemplate") as DataTemplate; }
        }

        private object GetHeaderTemplate(string resName)
        {
            object result = null;
            if (string.IsNullOrEmpty(resName))
            {
                return result;
            }

            if (this.Resources.Contains(resName))
            {
                result = this.Resources[resName];
            }
            return result;
        }

        #endregion
    }
}