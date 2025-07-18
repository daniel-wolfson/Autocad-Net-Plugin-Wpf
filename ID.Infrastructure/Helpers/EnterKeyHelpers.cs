using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace ID.Infrastructure.Helpers
{
    public static class EnterKeyHelpers
    {
        public static ICommand GetEnterKeyCommand(DependencyObject target)
        {
            return (ICommand)target.GetValue(EnterKeyCommandProperty);
        }

        public static void SetEnterKeyCommand(DependencyObject target, ICommand value)
        {
            target.SetValue(EnterKeyCommandProperty, value);
        }

        public static ICommand GetTextInputCommand(DependencyObject target)
        {
            return (ICommand)target.GetValue(TextInputCommandProperty);
        }
        public static void SetTextInputCommand(DependencyObject target, ICommand value)
        {
            target.SetValue(EnterKeyCommandProperty, value);
        }

        public static readonly DependencyProperty EnterKeyCommandProperty =
            DependencyProperty.RegisterAttached(
                "EnterKeyCommand",
                typeof(ICommand),
                typeof(EnterKeyHelpers),
                new PropertyMetadata(null, OnEnterKeyCommandChanged));

        public static readonly DependencyProperty EnterKeyCommandParamProperty =
            DependencyProperty.RegisterAttached(
                "EnterKeyCommandParam",
                typeof(object),
                typeof(EnterKeyHelpers),
                new PropertyMetadata(null));

        public static readonly DependencyProperty TextInputCommandProperty =
            DependencyProperty.RegisterAttached(
                "TextInputCommand",
                typeof(ICommand),
                typeof(EnterKeyHelpers),
                new PropertyMetadata(null, OnTexInputCommandChanged));

        public static object GetEnterKeyCommandParam(DependencyObject target)
        {
            return (object)target.GetValue(EnterKeyCommandParamProperty);
        }

        public static void SetEnterKeyCommandParam(DependencyObject target, object value)
        {
            target.SetValue(EnterKeyCommandParamProperty, value);
        }

        static void OnEnterKeyCommandChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            ICommand command = (ICommand)e.NewValue;
            FrameworkElement fe = (FrameworkElement)target;
            Control control = (Control)target;
            control.KeyDown += (s, args) =>
            {
                if (args.Key == Key.Enter)
                {
                    // make sure the textbox binding updates its source first
                    BindingExpression b = control.GetBindingExpression(TextBox.TextProperty);
                    if (b != null)
                    {
                        b.UpdateSource();
                    }
                    object commandParameter = GetEnterKeyCommandParam(target);
                    command.Execute(commandParameter);
                }
            };
        }

        static void OnTexInputCommandChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            ICommand command = (ICommand)e.NewValue;
            FrameworkElement fe = (FrameworkElement)target;
            Control control = (Control)target;
            control.TextInput += (s, args) =>
            {
                if (string.IsNullOrEmpty(args.Text))
                {
                    // make sure the textbox binding updates its source first
                    BindingExpression b = control.GetBindingExpression(TextBox.TextProperty);
                    if (b != null)
                    {
                        b.UpdateSource();
                    }
                    object commandParameter = GetEnterKeyCommandParam(target);
                    command.Execute(commandParameter);
                }
            };
        }
    }
}