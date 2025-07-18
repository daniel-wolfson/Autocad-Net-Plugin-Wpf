using System.Windows;
using System.Windows.Input;

namespace Intellidesk.AcadNet.Helpers
{
    public static class UiConfigurator
    {
        public static readonly DependencyProperty CustomValueProperty = DependencyProperty.RegisterAttached(
            "CustomValue", typeof(bool), typeof(UiConfigurator), new PropertyMetadata(false));

        public static void SetCustomValue(DependencyObject element, bool value)
        {
            element.SetValue(CustomValueProperty, value);
        }

        public static ICommand GetCustomValue(DependencyObject element)
        {
            return (ICommand)element.GetValue(CustomValueProperty);
        }
    }
}