using System;
using System.Windows;
using System.Windows.Controls;

namespace Intellidesk.AcadNet.Core
{
    /// <summary>
    /// Exposes attached behaviors that can be
    /// applied to TreeViewItem objects.
    /// </summary>
    public static class TreeViewItemBehavior
    {
        #region IsBroughtIntoViewWhenSelected
        public static bool GetIsBroughtIntoViewWhenSelected(TreeViewItem treeViewItem)
        {
            return (bool)treeViewItem.GetValue(IsBroughtIntoViewWhenSelectedProperty);
        }

        public static void SetIsBroughtIntoViewWhenSelected(TreeViewItem treeViewItem, bool value)
        {
            treeViewItem.SetValue(IsBroughtIntoViewWhenSelectedProperty, value);
        }

        public static readonly DependencyProperty IsBroughtIntoViewWhenSelectedProperty =
         DependencyProperty.RegisterAttached(
         "IsBroughtIntoViewWhenSelected",
         typeof(bool),
         typeof(TreeViewItemBehavior),
         new UIPropertyMetadata(false, OnIsBroughtIntoViewWhenSelectedChanged));

        static void OnIsBroughtIntoViewWhenSelectedChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            TreeViewItem item = depObj as TreeViewItem;
            if (item == null)
                return;

            if (e.NewValue is bool == false)
                return;

            if ((bool)e.NewValue)
                item.Selected += OnTreeViewItemSelected;
            else
                item.Selected -= OnTreeViewItemSelected;
        }

        static void OnTreeViewItemSelected(object sender, RoutedEventArgs e)
        {
            // Only react to the Selected event raised by the TreeViewItem
            // whose IsSelected property was modified. Ignore all ancestors
            // who are merely reporting that a descendant's Selected fired.
            if (!Object.ReferenceEquals(sender, e.OriginalSource))
                return;

            TreeViewItem item = e.OriginalSource as TreeViewItem;
            if (item != null)
                item.BringIntoView();
        }

        #endregion // IsBroughtIntoViewWhenSelected
    }

    public class TreeViewSelectedItemBehaviour
    {
        // Declare our attached property, it needs to be a DependencyProperty so
        // we can bind to it from oout ViewMode.
        public static readonly DependencyProperty TreeViewSelectedItemProperty =
            DependencyProperty.RegisterAttached(
            "TreeViewSelectedItem",
            typeof(object),
            typeof(TreeViewSelectedItemBehaviour),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                new PropertyChangedCallback(TreeViewSelectedItemChanged)));

        // We need a Get method for our new property
        public static object GetTreeViewSelectedItem(DependencyObject dependencyObject)
        {
            return (object)dependencyObject.GetValue(TreeViewSelectedItemProperty);
        }

        // As well as a Set method for our new property
        public static void SetTreeViewSelectedItem(
          DependencyObject dependencyObject, object value)
        {
            dependencyObject.SetValue(TreeViewSelectedItemProperty, value);
        }

        // This is the handler for when our new property's value changes
        // When our property is set to a non null value we need to add an event handler
        // for the TreeView's SelectedItemChanged event
        private static void TreeViewSelectedItemChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs e)
        {
            TreeView tv = dependencyObject as TreeView;

            if (e.NewValue == null && e.OldValue != null)
            {
                tv.SelectedItemChanged -=
                    new RoutedPropertyChangedEventHandler<object>(tv_SelectedItemChanged);
            }
            else if (e.NewValue != null && e.OldValue == null)
            {
                tv.SelectedItemChanged +=
                    new RoutedPropertyChangedEventHandler<object>(tv_SelectedItemChanged);
            }
        }

        // When TreeView.SelectedItemChanged fires, set our new property to the value
        static void tv_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SetTreeViewSelectedItem((DependencyObject)sender, e.NewValue);
        }
    }
}
