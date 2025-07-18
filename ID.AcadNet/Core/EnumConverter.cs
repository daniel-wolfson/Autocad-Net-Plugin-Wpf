using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Markup;

namespace Intellidesk.AcadNet.Core
{
    public class EnumConverter1 : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            Enum enumValue = default(Enum);
            if (parameter is Type)
            {
                enumValue = (Enum)Enum.Parse((Type)parameter, value.ToString());
            }
            return enumValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            int returnValue = 0;
            if (parameter is Type)
            {
                returnValue = (int)Enum.Parse((Type)parameter, value.ToString());
            }
            return returnValue;
        }
    }

    public class EnumValueConverter<T> : IValueConverter
    {
        //
        // Generates a cached list of enum values; items with blank DisplayAttribute 
        // values are ignored. This item is placed here for convenience; you can put 
        // this value anywhere.
        //
        private IList<T> _enumValues = null;
        private void MakeEnumValues()
        {
            if (_enumValues != null) return;
            _enumValues = Enum.GetValues(typeof(T)).Cast<T>().Where((s) => {
                var name = GetDisplayAttribute(s);
                var retval = (name == null) || (name != "");
                return retval;
            }).ToList();
        }
        public IList<T> EnumValues { get { MakeEnumValues(); return _enumValues; } }

        private string GetDisplayAttribute(T vv)
        {
            // Note: the GetTypeInfo only works when you have a using System.Reflection;
            var cda = vv.GetType().GetTypeInfo().GetDeclaredField(vv.ToString())
                 .GetCustomAttribute<DisplayAttribute>();
            return cda?.Name;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is T)
            {
                var name = GetDisplayAttribute((T)value);
                if (name != null) return name;
            }
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EnumToItemsSource : MarkupExtension
    {
        private readonly Type _type;

        public EnumToItemsSource(Type type)
        {
            _type = type;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Enum.GetValues(_type)
                .Cast<object>()
                .Select(e => new { Value = (int)e, DisplayName = e.ToString() });
        }
    }
}