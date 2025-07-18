using System;
using System.Globalization;
using System.Windows.Data;

namespace ID.Infrastructure.Converters
{
    public class CommandArgsConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            Tuple<object, object> tuple = new Tuple<object, object>(values[0], values[1]);
            return tuple;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null; //throw new NotImplementedException();
        }
    }
}