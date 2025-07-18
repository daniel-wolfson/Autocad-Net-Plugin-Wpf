using ID.Infrastructure;
using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace Intellidesk.AcadNet.Helpers
{
    public class FileNameValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value != null && !Files.IsValidFilename(value.ToString()))
                return new ValidationResult(false, "text contains errors symbols");
            return ValidationResult.ValidResult;
        }
    }

    public class InverseAndBooleansToBooleanConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.LongLength > 0)
            {
                foreach (var value in values)
                {
                    if (value is bool && (bool)value)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
