using System.Windows;
using System.Windows.Input;

namespace Intellidesk.AcadNet.Views
{
    public static class CustomCommandCollection
    {
        //public static readonly DependencyProperty DataGridDoubleClickProperty =
        //    DependencyProperty.Register("DataGridDoubleClickCommand", typeof(ICommand), typeof(CommandCollection),
        //                                new PropertyMetadata(AttachOrRemoveDataGridDoubleClickEvent));

        //public static ICommand GetDataGridDoubleClickCommand(DependencyObject obj)
        //{
        //    return (ICommand)obj.GetValue(DataGridDoubleClickProperty);
        //}

        //public static void SetDataGridDoubleClickCommand(DependencyObject obj, ICommand value)
        //{
        //    //obj.SetValue(DataGridDoubleClickProperty, value);
        //}

        public static void AttachOrRemoveDataGridDoubleClickEvent(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            //var dataGrid = obj as DataGrid;
            //if (dataGrid != null)
            //{
            //    var cmd = (ICommand)Args.NewValue;

            //    if (Args.OldValue == null && Args.NewValue != null)
            //    {
            //        dataGrid.MouseDoubleClick += ExecuteDataGridDoubleClick;
            //    }

            //    else if (Args.OldValue != null && Args.NewValue == null)
            //    {
            //        dataGrid.MouseDoubleClick -= ExecuteDataGridDoubleClick;
            //    }
            //}
        }

        private static void ExecuteDataGridDoubleClick(object sender, MouseButtonEventArgs args)
        {
            //var obj = Sender as DependencyObject;
            //var cmd = (ICommand)obj.GetValue(DataGridDoubleClickProperty);

            //if (cmd != null)
            //{
            //    if (cmd.CanExecute(obj))
            //    {
            //        cmd.Execute(obj);
            //    }
            //}
        }
    }
}