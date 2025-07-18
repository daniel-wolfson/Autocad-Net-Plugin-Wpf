using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ID.Infrastructure.Converters
{
    [ValueConversion(typeof(object), typeof(string))]
    public class StringFormatConverter : IValueConverter, IMultiValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(new object[] { value }, targetType, parameter, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            System.Diagnostics.Trace.TraceError("StringFormatConverter: does not support TwoWay or OneWayToSource bindings.");
            return DependencyProperty.UnsetValue;
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                string format = (parameter == null) ? null : parameter.ToString();
                if (String.IsNullOrEmpty(format))
                {
                    System.Text.StringBuilder builder = new System.Text.StringBuilder();
                    for (int index = 0; index < values.Length; ++index)
                    {
                        builder.Append("{" + index + "}");
                    }
                    format = builder.ToString();
                }
                return String.Format(/*culture,*/ format, values);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("StringFormatConverter({0}): {1}", parameter, ex.Message);
                return DependencyProperty.UnsetValue;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            System.Diagnostics.Trace.TraceError("StringFormatConverter: does not support TwoWay or OneWayToSource bindings.");
            return null;
        }
    }
}

//ToolTip="{Binding Path=Hostname,
//    Converter={StaticResource StringFormatConverter},
//    ConverterParameter=Connected to {0}}"

//  <TextBlock>
//  <TextBlock.Text>
//    <Binding Converter = "{StaticResource StringFormatConverter}"
//             ConverterParameter="{}{0}: {1}">
//      <Binding Path = "When" />
//      < Binding Path="Message" />
//    </Binding>
//  </TextBlock.Text>
//  </TextBlock>