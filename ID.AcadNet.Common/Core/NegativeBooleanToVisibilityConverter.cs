﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Intellidesk.AcadNet.Common.Core
{
    [Localizability(LocalizationCategory.NeverLocalize)]
    public sealed class NegativeBooleanToVisibilityConverter : IValueConverter
    {
        /// <summary> 
        /// Convert bool or Nullable&lt;bool&gt; to Visibility
        /// </summary> 
        /// <param name="value">bool or Nullable&lt;bool&gt;</param>
        /// <param name="targetType">Visibility</param>
        /// <param name="parameter">null</param>
        /// <param name="culture">null</param> 
        /// <returns>Visible or Collapsed</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool bValue = false;
            if (value is bool)
            {
                bValue = (bool)value;
            }
            else if (value is Nullable<bool>)
            {
                Nullable<bool> tmp = (Nullable<bool>)value;
                bValue = tmp.HasValue ? tmp.Value : false;
            }
            return (bValue) ? Visibility.Collapsed : Visibility.Visible;
        }

        /// <summary>
        /// Convert Visibility to boolean 
        /// </summary>
        /// <param name="value"></param> 
        /// <param name="targetType"></param> 
        /// <param name="parameter"></param>
        /// <param name="culture"></param> 
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility)
            {
                return (Visibility)value == Visibility.Visible;
            }
            else
            {
                return false;
            }
        }
    }
}