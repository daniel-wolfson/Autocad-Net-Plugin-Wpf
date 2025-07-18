using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace Intellidesk.AcadNet.Data.Common.Editors
{
    //public delegate string DocumenOpenDataDialogEventHandler(string name, string extension, string title);

    /// <summary> Interaction logic for CadFileNameUserControlEditor.xaml </summary>
    public partial class FileNameEditor : UserControl, ITypeEditor
    {
        //public static event DocumenOpenDataDialogEventHandler DocumenOpenDataDialogEvent;
        //private static string OnDocumenOpenDataDialogEvent(string name, string extension, string title)
        //{
        //    DocumenOpenDataDialogEventHandler handler = DocumenOpenDataDialogEvent;
        //    if (handler != null)
        //        return handler(name, extension, title);
        //    return string.Empty;
        //}
        
        Xceed.Wpf.Toolkit.PropertyGrid.PropertyItem _myPropertyItem;
        //[Editor(typeof(CadNameUserControlEditor), typeof(CadNameUserControlEditor))]
        //public string LastName { get; set; }
        public FileNameEditor()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(string), typeof(FileNameEditor),
                  new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            var defaultExt = "dwg";
            var propName = _myPropertyItem.PropertyDescriptor.Name;

            switch (propName.ToLower())
            {
                case "cadfilename":
                    defaultExt = "dwg";
                    break;
                case "tabfilename":
                    defaultExt = "tab";
                    break;
            }

            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = defaultExt,
                Filter = String.Format("{0} documents (.{0})|*.{0}", defaultExt),
                Title = String.Format("Choice {0} file", propName)
            };
            var result = dlg.ShowDialog();
            if (result == true)
            {
                var file = dlg.FileName;
                //file = OnDocumenOpenDataDialogEvent(ProjectManager.Name, "dwg", "Choice dwg file");
                //file = ToolsManager.DocumenOpenDataDialog(ProjectManager.Name, "dwg", "Choice dwg file");
                if (!String.IsNullOrEmpty(file))
                    Value = file.Substring(file.IndexOf(":") + 1);
            }
        }

        public FrameworkElement ResolveEditor(Xceed.Wpf.Toolkit.PropertyGrid.PropertyItem propertyItem)
        {
            _myPropertyItem = propertyItem;
            var binding = new Binding("Value")
                {
                    Source = propertyItem,
                    Mode = propertyItem.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay
                };
            BindingOperations.SetBinding(this, ValueProperty, binding);
            return this;
        }
    }
}
