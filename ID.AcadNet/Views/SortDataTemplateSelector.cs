using System;
using System.Windows;
using System.Windows.Controls;
using Xceed.Wpf.AvalonDock.Controls;

namespace Intellidesk.AcadNet.Views
{
    public class SortDataTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            string numberStr = item as string;

            if (numberStr != null)
            {
                int num;
                UserControl ucControl = container.FindVisualAncestor<UserControl>();

                try
                {
                    num = Convert.ToInt32(numberStr);
                }
                catch
                {
                    return null;
                }

                // Select one of the DataTemplate objects, based on the 
                // value of the selected item in the ComboBox.
                if (num < 5)
                {
                    return ucControl.FindResource("numberTemplate") as DataTemplate;
                }
                else
                {
                    return ucControl.FindResource("largeNumberTemplate") as DataTemplate;

                }
            }

            return null;
        }

    }
}