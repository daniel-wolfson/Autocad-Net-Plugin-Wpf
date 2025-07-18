using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace Intellidesk.AcadNet.Data.Common.Editors
{
    /// <summary> Interaction logic for LastNameUserControl.xaml </summary>
    public partial class CadFileNameEditor : UserControl, ITypeEditor
    {
        //public static event DocumenOpenDataDialogEventHandler DocumenOpenDataDialogEvent;
        //private static string OnDocumenOpenDataDialogEvent(string name, string extension, string title)
        //{
        //    DocumenOpenDataDialogEventHandler handler = DocumenOpenDataDialogEvent;
        //    if (handler != null)
        //        return handler(name, extension, title);
        //    return string.Empty;
        //}

        public CadFileNameEditor()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(string), typeof(CadFileNameEditor),
                  new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            //var file = OnDocumenOpenDataDialogEvent(ProjectManager.Name, "dwg", "Choice dwg file");
            //if (!String.IsNullOrEmpty(file))
            //{
            //    Value = file;
            //}
        }

        public FrameworkElement ResolveEditor(Xceed.Wpf.Toolkit.PropertyGrid.PropertyItem propertyItem)
        {
            var binding = new Binding("Value")
            {
                Source = propertyItem,
                Mode = propertyItem.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay
            };
            BindingOperations.SetBinding(this, ValueProperty, binding);
            return this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

    }
}
