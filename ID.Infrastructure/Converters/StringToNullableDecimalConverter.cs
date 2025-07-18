using System;
using System.Globalization;
using System.Windows.Data;

namespace ID.Infrastructure.Converters
{
    public class StringToNullableDecimalConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            decimal? d = (decimal?)value;
            if (d.HasValue)
                return d.Value.ToString(culture);
            else
                return String.Empty;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            string s = (string)value;
            if (String.IsNullOrEmpty(s))
                return null;
            else
                return (decimal?)decimal.Parse(s, culture);
        }
    }
}