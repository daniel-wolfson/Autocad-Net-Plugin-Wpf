using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Intellidesk.Data.Models;
using Intellidesk.Data.Models.Cad;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using ItemCollection = Xceed.Wpf.Toolkit.PropertyGrid.Attributes.ItemCollection;

namespace Intellidesk.Data.Common.Editors
{
    public delegate string GetDataContextHandler();

    public static class EditorHelper
    {
        public static event GetDataContextHandler GetDataContextEvent;
        private static string OnGetDataContextEvent()
        {
            var handler = GetDataContextEvent;
            if (handler != null)
                return handler();
            return null;
        }
        //[Import(typeof(Config))]
        //public static Config CurrentConfig;

        public static ItemCollection GetLayoutDictionary(string paramName, string key)
        {
            dynamic dataContext = OnGetDataContextEvent(); // ((UserControl)UIManager.PaletteViewCurrent).DataContext; !!!!!!!!!!!
            var items = new ItemCollection();

            if (dataContext != null)
            {
                var firstOrDefault = ((List<LayoutDictionary>)dataContext.CurrentConfig.LayoutOptions)
                    .Where(x => x.ParameterName == paramName)
                    .FirstOrDefault(x => x.Key.ToString() == key);
                if (firstOrDefault != null)
                {
                    var list = new List<string>(firstOrDefault.Value.Split(','));
                    list.ForEach(x => items.Add(x));
                }
            }
            return items;
        }

        public static ObservableCollection<T> GetLayoutDictionary<T>(string paramName, string key) where T : class
        {
            dynamic dataContext = OnGetDataContextEvent(); // ((UserControl)UIManager.PaletteViewCurrent).DataContext; // !!!!!!!!!!!

            var t = typeof(T);
            var items = (ObservableCollection<T>)Activator.CreateInstance((typeof(ObservableCollection<>).MakeGenericType(t)));

            if (dataContext != null)
            {
                var firstOrDefault = ((List<LayoutDictionary>) dataContext.CurrentConfig.LayoutOptions)
                    .Where(x => x.ParameterName == paramName)
                    .FirstOrDefault(x => x.Key.ToString() == key);
                if (firstOrDefault != null)
                {
                    var list = new List<string>(firstOrDefault.Value.Split(','));
                    list.ForEach(x => ((IList)items).Add(x));
                }
            }
            return items;
        }
    }

    public class StringRangeValidationRule : ValidationRule
    {
        private int _minimumLength = -1;
        private int _maximumLength = -1;

        public int MinimumLength
        {
            get { return _minimumLength; }
            set { _minimumLength = value; }
        }

        public int MaximumLength
        {
            get { return _maximumLength; }
            set { _maximumLength = value; }
        }

        public string ErrorMessage { get; set; }

        public override ValidationResult Validate(object value,
            CultureInfo cultureInfo)
        {
            var result = new ValidationResult(true, null);
            var inputString = (value ?? string.Empty).ToString();

            if (inputString.Length < this.MinimumLength ||
                   (this.MaximumLength > 0 &&
                    inputString.Length > this.MaximumLength))
            {
                result = new ValidationResult(false, this.ErrorMessage);
            }
            return result;
        }
    }

    public class CadFileFoundValidationRule : ValidationRule
    {
        //public class ErrorLayoutObject : DependencyObject
        //{
        //    public static readonly DependencyProperty ErrorLayoutProperty =
        //        DependencyProperty.Register("ErrorLayout", typeof(int), typeof(ErrorLayoutObject),
        //            new PropertyMetadata(0));
        //    public int ErrorLayout
        //    {
        //        get { return (int)GetValue(ErrorLayoutProperty); }
        //        set { SetValue(ErrorLayoutProperty, value); }
        //    }
        //}
        //public static readonly DependencyProperty ErrorLayoutProperty =
        //        DependencyProperty.Register("ErrorLayout", typeof(int), typeof(CadFileFoundValidationRule),
        //            new PropertyMetadata(0));
        //public static readonly DependencyProperty ErrorLayoutProperty =
        //    DependencyProperty.RegisterAttached("ErrorLayout", typeof(Boolean), typeof(CadFileFoundValidationRule));

        public string ErrorMessage { get; set; }

        public string ErrorLayout { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var result = new ValidationResult(true, null);
            var inputString = (value ?? string.Empty).ToString();

            if (!System.IO.File.Exists(inputString))
            {
                result = new ValidationResult(false, this.ErrorMessage);
            }
            return result;
        }
    }

    //Custom editors that are used as attributes MUST implement the ITypeEditor interface.
    public class NameEditor : Xceed.Wpf.Toolkit.PropertyGrid.Editors.ITypeEditor
    {
        public FrameworkElement ResolveEditor(PropertyItem propertyItem)
        {
            var textBox = new TextBox { Background = new SolidColorBrush(Colors.Red) };

            //create the binding from the bound property item to the editor
            var binding = new Binding("Value")
                {
                    Source = propertyItem,
                    ValidatesOnExceptions = true,
                    ValidatesOnDataErrors = true,
                    Mode = propertyItem.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay
                }; //bind to the Value property of the PropertyItem
            BindingOperations.SetBinding(textBox, TextBox.TextProperty, binding);
            return textBox;
        }
    }

   
    public class BuildingLevelsItemsSource : ObservableCollection<string>
    {
        public BuildingLevelsItemsSource()
        {
            EditorHelper.GetLayoutDictionary<string>("LAYOUT_CATALOG_SITE", "LC - Lachish Israel").ToList().ForEach(x => Add(x.Trim()));
        }
    }

    public class LayoutContentsIItemsSource : IItemsSource
    {
        public ItemCollection GetValues()
        {
            return EditorHelper.GetLayoutDictionary("LAYOUT_CATALOG_OPTIONS", "LAYOUT_CONTENT");
        }
    }
    public class LayoutContentsItemsSource : ObservableCollection<string>
    {
        public LayoutContentsItemsSource()
        {
            EditorHelper.GetLayoutDictionary<string>("LAYOUT_CATALOG_OPTIONS", "LAYOUT_CONTENT").ToList().ForEach(x => Add(x.Trim()));
        }
    }

    public class LayoutTypesItemsSource : IItemsSource
    {
        public ItemCollection GetValues()
        {
            return EditorHelper.GetLayoutDictionary("LAYOUT_CATALOG_OPTIONS", "CONTENT_TYPE");
        }
    }

    public class ProcessesItemsSource : IItemsSource
    {
        //[Import(typeof(Config))]
        //public Config CurrentConfig;

        public ItemCollection GetValues()
        {
            return EditorHelper.GetLayoutDictionary("LAYOUT_CATALOG_OPTIONS", "PROCESSES");
        }
    }

    public class BoolToValueConverter<T> : IValueConverter
    {
        public T FalseValue { get; set; }
        public T TrueValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return FalseValue;
            else
                return (bool)value ? TrueValue : FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null && value.Equals(TrueValue);
        }
    }
}
