using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace Intellidesk.Data.Common.Editors
{
    /// <summary>
    /// Interaction logic for MultiSelectComboBox.xaml
    /// </summary>
    public partial class MultiSelectComboBoxEditor : UserControl, ITypeEditor
    {
        private ObservableCollection<Node> _nodeList;

        public MultiSelectComboBoxEditor()
        {
            InitializeComponent();
            _nodeList = new ObservableCollection<Node>();
        }

        #region Dependency Properties

        public static readonly DependencyProperty ItemsSourceProperty =
             DependencyProperty.Register("ItemsSource", typeof(ObservableCollection<string>), typeof(MultiSelectComboBoxEditor), new FrameworkPropertyMetadata(null,
        new PropertyChangedCallback(MultiSelectComboBoxEditor.OnItemsSourceChanged)));

        public static readonly DependencyProperty SelectedItemsProperty =
         DependencyProperty.Register("SelectedItems", typeof(ObservableCollection<string>), typeof(MultiSelectComboBoxEditor), new FrameworkPropertyMetadata(null,
     new PropertyChangedCallback(MultiSelectComboBoxEditor.OnSelectedItemsChanged)));

        public static readonly DependencyProperty TextProperty =
           DependencyProperty.Register("Text", typeof(string), typeof(MultiSelectComboBoxEditor), new UIPropertyMetadata(string.Empty));

        public static readonly DependencyProperty DefaultTextProperty =
            DependencyProperty.Register("DefaultText", typeof(string), typeof(MultiSelectComboBoxEditor), new UIPropertyMetadata(string.Empty));

        public static readonly DependencyProperty ValueProperty =
                           DependencyProperty.Register("Value", typeof(string), typeof(MultiSelectComboBoxEditor),
                                 new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public ObservableCollection<string> ItemsSource
        {
            get { return (ObservableCollection<string>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public ObservableCollection<string> SelectedItems
        {
            get { return (ObservableCollection<string>)GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public string DefaultText
        {
            get { return (string)GetValue(DefaultTextProperty); }
            set { SetValue(DefaultTextProperty, value); }
        }

        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        #endregion

        #region Events
        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (MultiSelectComboBoxEditor)d;
            control.DisplayInControl();
        }

        private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (MultiSelectComboBoxEditor)d;
            control.SelectNodes();
            control.SetText();
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            var clickedBox = (CheckBox)sender;

            int _selectedCount = 0;
            foreach (Node s in _nodeList)
            {
                if (s.IsSelected) _selectedCount++;
            }

            SetSelectedItems();
            SetText();

        }
        #endregion

        #region Methods
        private void SelectNodes()
        {
            foreach (var keyValue in SelectedItems)
            {
                Node node = _nodeList.FirstOrDefault(i => i.Title == keyValue);
                if (node != null)
                    node.IsSelected = true;
            }
        }

        private void SetSelectedItems()
        {
            if (SelectedItems == null)
                SelectedItems = new ObservableCollection<string>();
            SelectedItems.Clear();

            foreach (Node node in _nodeList)
            {
                if (node.IsSelected)
                {
                    if (this.ItemsSource.Count > 0)

                        SelectedItems.Add(node.Title);
                }
            }
        }

        private void DisplayInControl()
        {
            _nodeList.Clear();
            foreach (var item in this.ItemsSource)
            {
                var node = new Node(item);
                node.IsSelected = Value.Replace(" ","").Split(',').Contains(item);
                _nodeList.Add(node);
            }
            MultiSelectCombo.ItemsSource = _nodeList;
        }

        private void SetText()
        {
            if (this.SelectedItems != null && _nodeList.Count > 0)
            {
                this.Value = String.Join(", ", _nodeList.Where(s => s.IsSelected).Select(x => x.Title));
            }

            // set DefaultText if nothing else selected
            if (string.IsNullOrEmpty(this.Value)) 
            { 
                this.Value = this.DefaultText; 
            }
        }

        #endregion

        public FrameworkElement ResolveEditor(PropertyItem propertyItem)
        {
            var binding = new Binding("Value")
               {
                   Source = propertyItem,
                   Mode = propertyItem.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay
               };
            BindingOperations.SetBinding(this, ValueProperty, binding);

            var items = new ObservableCollection<string>();
            switch (propertyItem.Name)
            {
                case "LayoutContents":
                    items = new LayoutContentsItemsSource();
                    break;
                case "BuildingLevels":
                    items = new BuildingLevelsItemsSource();
                    break;
            }

            //this.DefaultText = "";
            SelectedItems = items;
            ItemsSource = items;

            //var binding1 = new Binding("LayoutContentsItems")
            //{
            //    Source = propertyItem,
            //    Mode = propertyItem.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay
            //};
            //BindingOperations.SetBinding(this, ValueProperty, binding1);
            return this;
        }
    }

    public class Node : INotifyPropertyChanged
    {

        private string _title;
        private bool _isSelected;
        #region ctor
        public Node(string title)
        {
            Title = title;
        }
        #endregion

        #region Properties
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
                NotifyPropertyChanged("Title");
            }
        }
        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                _isSelected = value;
                NotifyPropertyChanged("IsSelected");
            }
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }
}
