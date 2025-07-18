using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Intellidesk.AcadNet.Common.Extensions
{
    public static class TreeViewExtensions
    {
        public static TreeViewItem[] GetTreeViewItems(this TreeView treeView)
        {
            List<TreeViewItem> returnItems = new List<TreeViewItem>();
            for (int x = 0; x < treeView.Items.Count; x++)
            {
                returnItems.AddRange(GetTreeViewItems((TreeViewItem)treeView.Items[x]));
            }
            return returnItems.ToArray();
        }

        private static TreeViewItem[] GetTreeViewItems(TreeViewItem currentTreeViewItem)
        {
            List<TreeViewItem> returnItems = new List<TreeViewItem>();
            returnItems.Add(currentTreeViewItem);
            for (int x = 0; x < currentTreeViewItem.Items.Count; x++)
            {
                returnItems.AddRange(GetTreeViewItems((TreeViewItem)currentTreeViewItem.Items[x]));
            }
            return returnItems.ToArray();
        }

        public static List<TreeViewItem> FindTreeViewItems(this Visual @this)
        {
            if (@this == null)
                return null;

            var result = new List<TreeViewItem>();

            var frameworkElement = @this as FrameworkElement;
            if (frameworkElement != null)
            {
                frameworkElement.ApplyTemplate();
            }

            Visual child = null;
            for (int i = 0, count = VisualTreeHelper.GetChildrenCount(@this); i < count; i++)
            {
                child = VisualTreeHelper.GetChild(@this, i) as Visual;

                var treeViewItem = child as TreeViewItem;
                if (treeViewItem != null)
                {
                    result.Add(treeViewItem);
                    if (!treeViewItem.IsExpanded)
                    {
                        treeViewItem.IsExpanded = true;
                        treeViewItem.UpdateLayout();
                    }
                }
                foreach (var childTreeViewItem in FindTreeViewItems(child))
                {
                    result.Add(childTreeViewItem);
                }
            }
            return result;
        }

        public static TreeViewItem Find(this TreeView treeView, string searchText)
        {
            foreach (object obje in treeView.Items)
            {
                TreeViewItem item = (TreeViewItem)treeView.ItemContainerGenerator.ContainerFromItem(obje);
                if (item != null)
                {
                    if (item.Header.ToString().ToLower() == searchText.ToLower())
                    {
                        item.Background = Brushes.Red;
                        item.IsExpanded = true;
                    }
                    return item;
                }
                //if (item.Items.Count > 0) this.Search(item);
            }
            return null;
        }

        
    }
}