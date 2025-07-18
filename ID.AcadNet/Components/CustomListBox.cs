using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Intellidesk.AcadNet.Components
{
    public class CustomListBox : ListBox
    {
        public ICommand ClickCommand
        {
            get { return (ICommand)GetValue(ClickCommandProperty); }
            set { SetValue(ClickCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ClickCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ClickCommandProperty =
            DependencyProperty.Register("ClickCommand", typeof(ICommand), typeof(CustomListBox), new UIPropertyMetadata());

        public CustomListBox()
            : base()
        {
            //PreviewMouseDown += new MouseButtonEventHandler(CustomListBox_PreviewMouseDown);
            SelectionChanged += CustomListBox_PreviewMouseDown;
        }

        void CustomListBox_PreviewMouseDown(object sender, SelectionChangedEventArgs e)
        {
            if (ClickCommand != null)
            {
                ClickCommand.Execute(((CustomListBox)sender).SelectedItem);
            }
        }
    }
}