using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Intellidesk.AcadNet.Helpers
{
    /// <summary> BoolToValueConverter for xaml code </summary>
    public class BoolToValueConverter : IValueConverter
    {
        /// <summary> String false value </summary>
        public String FalseValue { get; set; }

        /// <summary> String true value </summary>
        public String TrueValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return FalseValue;
            return (bool) value ? TrueValue : FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null && value.Equals(TrueValue);
        }
    }

    /// <summary> StateToValueConverter for xaml code </summary>
    public class StateToValueConverter : IValueConverter
    {
        public Dictionary<short?, string> Values = new Dictionary<short?, string>()
        {
            {-1, " Deleleted"},
            {1, " Actived"}
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
                if (Values.ContainsKey((short?) value))
                    return Values[(short) value];
            return " None";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var ret = Values.FirstOrDefault(x => x.Value == (string) value).Key;
            return ret;
        }
    }

    /// <summary> StateToImageConverter for xaml code </summary>
    public class StateToImageConverter : IValueConverter
    {
        public Dictionary<short?, string> Values = new Dictionary<short?, string>()
        {
            {-1, "{DynamicResource LsdsDelete}"},
            {1, "{DynamicResource LsdsNew}"}
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
                if (Values.ContainsKey((short?) value))
                    return Values[(short) value];
            return "{DynamicResource LsdsNone}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var ret = Values.FirstOrDefault(x => x.Value == (string) value).Key;
            return ret;
        }
    }

    /// <summary> ColorToValueConverter for xaml code </summary>
    public class ColorToValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var retColor = Brushes.Black;
            switch ((short?) value)
            {
                case null:
                    retColor = Brushes.Black;
                    break;
                case 0:
                    retColor = Brushes.Black;
                    break;
                case -1:
                    retColor = Brushes.Red;
                    break;
                case 1:
                    retColor = Brushes.Green;
                    break;
            }
            return retColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    /// <summary> Test </summary>
    public class IsReadImageConverter : IValueConverter
    {
        public Image ReadImage { get; set; }
        public Image UnreadImage { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is bool))
            {
                return null;
            }
            bool b = (bool) value;
            if (b)
            {
                return ReadImage;
            }
            else
            {
                return UnreadImage;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    //[ValueConversion(typeof(bool), typeof(GridLength))]
    public class BoolToGridRowHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((bool) value == true) ? new GridLength(1, GridUnitType.Star) : new GridLength(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Don't need any convert back
            return null;
        }
    }

    public class EntityTypeValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                //Type t = (Type)value;
                string s = (string)value;
                if (s.Contains("DB"))
                    return s.Substring(3);
                return s;
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

    }
}
