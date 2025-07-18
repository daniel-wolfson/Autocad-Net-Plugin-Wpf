using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Intellidesk.AcadNet.Data.Common.Editors;
using Intellidesk.Data.Common.Editors;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace Intellidesk.AcadNet.Data.Common.Editors
{
    /// <summary> Interaction logic for LayoutContentsEditor.xaml </summary>
    public partial class LayoutContentsEditor : UserControl, ITypeEditor
    {
        Xceed.Wpf.Toolkit.PropertyGrid.PropertyItem _myPropertyItem;
        //private ObservableCollection<Node> _nodeList;

        public LayoutContentsEditor()
        {
            //var content = EditorHelper.GetLayoutDictionary<string>("LAYOUT_CATALOG_OPTIONS", "LAYOUT_CONTENT");
            //var layoutcontentsitems = new ObservableCollection<string>();
            //content.ToList().ForEach(layoutcontentsitems.Add);
            InitializeComponent();
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(string), typeof(LayoutContentsEditor),
                  new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty LayoutContentsItemsProperty =
            DependencyProperty.Register("LayoutContentsItems", typeof(ObservableCollection<string>), typeof(LayoutContentsEditor),
                  new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(Dictionary<string,
            object>), typeof(LayoutContentsEditor), new FrameworkPropertyMetadata(null,
            new PropertyChangedCallback(LayoutContentsEditor.OnItemsSourceChanged)));

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (LayoutContentsEditor)d;
            //control.DisplayInControl();
        }

        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public ObservableCollection<string> LayoutContentsItems
        {
            get { return (ObservableCollection<string>)GetValue(LayoutContentsItemsProperty); }
            set { SetValue(LayoutContentsItemsProperty, value); }
        }

        public Dictionary<string, object> ItemsSource
        {
            get { return (Dictionary<string, object>)GetValue(LayoutContentsItemsProperty); }
            set { SetValue(LayoutContentsItemsProperty, value); }
        }
        
        public FrameworkElement ResolveEditor(Xceed.Wpf.Toolkit.PropertyGrid.PropertyItem propertyItem)
        {
            _myPropertyItem = propertyItem;

            switch (propertyItem.Name)
            {
                case "LayoutContents":
                    DataContext = new LayoutContentsItemsSource();
                    LayoutContentsItems = (ObservableCollection<string>)DataContext;
                    break;
                case "BuildingLevels":
                    DataContext = new BuildingLevelsItemsSource();
                    LayoutContentsItems = (ObservableCollection<string>)DataContext;
                    break;
            }

            var binding = new Binding("Value")
                {
                    Source = propertyItem,
                    Mode = propertyItem.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay
                };
            BindingOperations.SetBinding(this, ValueProperty, binding);
            var binding1 = new Binding("LayoutContentsItems")
            {
                Source = propertyItem,
                Mode = propertyItem.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay
            };
            BindingOperations.SetBinding(this, ValueProperty, binding1);
            return this;
        }

        private void ComboBox1_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedValue = ((ComboBox)sender).SelectedValue.ToString();
            var fieldData = String.IsNullOrEmpty(FieldData.Text) 
                ? new List<string>() : FieldData.Text.Replace(" ","").Split(',').ToList();

            if (!fieldData.Contains(selectedValue))
            {
                fieldData.Add(selectedValue);
                FieldData.Text = String.Join(", ", fieldData);
            }
            else
            {
                fieldData.Remove(selectedValue);
                FieldData.Text = String.Join(", ", fieldData);
            }
        }

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            ComboBox1.Focus();
            System.Windows.Forms.SendKeys.SendWait("%{DOWN}");
        }
    }
}
