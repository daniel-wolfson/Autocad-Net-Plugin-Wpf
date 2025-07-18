using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace ID.Infrastructure.Helpers
{
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
            _enumValues = Enum.GetValues(typeof(T)).Cast<T>().Where((s) =>
            {
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
}