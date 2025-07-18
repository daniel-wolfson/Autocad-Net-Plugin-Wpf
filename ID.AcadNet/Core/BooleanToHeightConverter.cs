using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Intellidesk.AcadNet.Core
{
    public class BooleanToHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (Equals(value, null))
                return new GridLength(0);

            var status = System.Convert.ToBoolean(value);

            return status
                ? new GridLength(32, GridUnitType.Auto)
                : new GridLength(0);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Only one way bindings are supported with this converter");
        }
    }
}
