using System.Windows;
using FileExplorer.Model;
using Intellidesk.AcadNet.Common.Interfaces;

namespace FileExplorer.Helper
{
    public static class ColumnWidthHelper
    {
        /// <summary>
        /// Min width
        /// </summary>
        public static readonly DependencyProperty MinWidthProperty =
            DependencyProperty.RegisterAttached("MinWidth", typeof(double), typeof(ColumnWidthHelper));

        public static void SetMinWidth(DependencyObject element, double value)
        {
            if (element == null)
                return;
            element.SetValue(MinWidthProperty, value);
        }

        public static double GetMinWidth(DependencyObject element)
        {
            if (element == null) return 0;
            return (double)element.GetValue(MinWidthProperty);
        }

        /// <summary>
        /// Max width
        /// </summary>
        public static readonly DependencyProperty MaxWidthProperty =
            DependencyProperty.RegisterAttached("MaxWidth", typeof(double), typeof(ColumnWidthHelper));

        public static void SetMaxWidth(DependencyObject element, double value)
        {
            if (element == null)
                return;
            element.SetValue(MaxWidthProperty, value);
        }

        public static double GetMaxWidth(DependencyObject element)
        {
            if (element == null) return 0;
            return (double)element.GetValue(MaxWidthProperty);
        }


        /// <summary>
        /// Sort property name
        /// </summary>
        public static readonly DependencyProperty SortPropertyNameProperty =
            DependencyProperty.RegisterAttached("SortPropertyName", typeof(string), typeof(ColumnWidthHelper));

        public static void SetSortPropertyName(DependencyObject element, string value)
        {
            if (element == null)
                return;
            element.SetValue(SortPropertyNameProperty, value);
        }

        public static string GetSortPropertyName(DependencyObject element)
        {
            if (element == null) return string.Empty;
            return (string)element.GetValue(SortPropertyNameProperty);
        }

        /// <summary>
        /// Max width
        /// </summary>
        public static readonly DependencyProperty FolderProperty =
            DependencyProperty.RegisterAttached("Folder", typeof(IFolder), typeof(ColumnWidthHelper));

        public static void SetFolder(DependencyObject element, IFolder value)
        {
            if (element == null)
                return;
            element.SetValue(FolderProperty, value);
        }

        public static IFolder GetFolder(DependencyObject element)
        {
            if (element == null) return null;
            return (IFolder)element.GetValue(FolderProperty);
        }
    }
}