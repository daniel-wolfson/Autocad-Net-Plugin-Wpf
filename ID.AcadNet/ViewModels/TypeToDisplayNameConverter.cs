using System;
using System.Globalization;
using System.Windows.Data;

namespace Intellidesk.AcadNet.ViewModels
{
    public class TypeToDisplayNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Type type = value as Type;
            if (type == null) return "";

            string t = ((Type) value).Name;
            return t.Replace("Acad", "");
            //List<Type> values = (List<Type>) value;
            //return values.Select(x => new { Name = x.Name.Replace("Acad", ""), TargetType = x });
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}