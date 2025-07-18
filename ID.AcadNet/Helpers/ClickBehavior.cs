using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Intellidesk.AcadNet.Helpers
{
    public static class ItemClickCommand
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached("Command", typeof(ICommand),
            typeof(ItemClickCommand), new PropertyMetadata(null, OnCommandPropertyChanged));

        public static void SetCommand(DependencyObject d, ICommand value)
        {
            d.SetValue(CommandProperty, value);
        }

        public static ICommand GetCommand(DependencyObject d)
        {
            return (ICommand)d.GetValue(CommandProperty);
        }

        private static void OnCommandPropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var control = d as ListBox;
            if (control != null)
                control.PreviewMouseDown += OnItemClick;
        }

        private static void OnItemClick(object sender, MouseButtonEventArgs e)
        {
            var control = sender as ListBox;
            var command = GetCommand(control);

            if (command != null && command.CanExecute((ListBox)sender != null))
                command.Execute(((ListBox)sender).SelectedItem);
        }
    }
}
