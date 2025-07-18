using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Intellidesk.AcadNet.Components
{
    public class CustomDataGrid : DataGrid
    {
        public ICommand DoubleClickCommand
        {
            get { return (ICommand)GetValue(DoubleClickCommandProperty); }
            set { SetValue(DoubleClickCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DoubleClickCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DoubleClickCommandProperty =
            DependencyProperty.Register("DoubleClickCommand", typeof(ICommand), typeof(CustomDataGrid), new UIPropertyMetadata());

        public CustomDataGrid()
            : base()
        {
            this.PreviewMouseDoubleClick += new MouseButtonEventHandler(CustomDataGrid_PreviewMouseDoubleClick);
        }

        void CustomDataGrid_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DoubleClickCommand != null)
            {
                DoubleClickCommand.Execute(((CustomDataGrid)sender).SelectedItem);
            }
        }
    }
}