using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace ID.Infrastructure.Extensions
{
    public static class DependencyObjectExtensions
    {
        public static T TryFindParent<T>(this DependencyObject current) where T : class
        {
            DependencyObject parent = VisualTreeHelper.GetParent(current);

            if (parent == null)
                return null;

            if (parent is T)
                return parent as T;
            else
                return TryFindParent<T>(parent);
        }

        public static IEnumerable<T> GetVisualChildren<T>(this DependencyObject parent) where T : DependencyObject
        {
            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child is T)
                    yield return (T)child;

                foreach (var descendant in GetVisualChildren<T>(child))
                    yield return descendant;
            }
        }

        public static UIElement GetParentFromVisualTree(this DependencyObject startObject, Type type) //DependencyObject
        {
            var parent = startObject;
            while (parent != null && !type.IsInstanceOfType(parent))
            {
                //if (type.IsInstanceOfType(parent)) break;
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as UIElement;
        }
    }
}
