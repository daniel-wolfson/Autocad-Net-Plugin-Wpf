using Autodesk.Windows;
using Intellidesk.AcadNet.Services.Core;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using IPanelTabView = Intellidesk.AcadNet.Common.Interfaces.IPanelTabView;

namespace Intellidesk.AcadNet.Core
{ //[assembly: CommandClass(typeof(UIBuildService))]

    #region "Delegates & Events"

    public delegate void UIControlClickEventHandler(object sender, UIControlEventArgs e);

    public delegate void PaletteEventHandler(IPanelTabView sender);

    //public delegate void PaletteSetStateEventHandler(PaletteSet Sender, PaletteSetStateEventArgs Args);

    //public class PaletteSetStateEventArgs : EventArgs
    //{
    //    public PaletteSetStateEventArgs(StateEventIndex newState)
    //    {
    //        NewState = newState;
    //    }
    //    public StateEventIndex NewState { get; set; }
    //}

    //public delegate void UIControlClickEventHandler(object Sender, UIControlEventArgs e);

    #endregion

    #region "Enum"

    //public interface ITabControl
    //{
    //    byte TabIndex { get; set; }
    //    string Name { get; set; }
    //    //Sub MakeDownTabId()
    //    //event MenuRotateCnt_EventEventHandler MenuRotateCnt_Event;
    //    //delegate void MenuRotateCnt_EventEventHandler(GetSelectOptions tOption);
    //}

    #endregion

    #region "UI Controls"

    #endregion

    #region "Extensions"

    public static class BindingExtensions
    {
        public static readonly DependencyProperty MyIsEnabledProperty =
            DependencyProperty.RegisterAttached("MyIsEnabled", typeof(RibbonButton), typeof(BindingExtensions));

        public static bool GetMyIsEnabled(this UIElement element)
        {
            return (bool)element.GetValue(MyIsEnabledProperty);
        }

        public static void SetMyIsEnabled(this UIElement element, bool title)
        {
            element.SetValue(MyIsEnabledProperty, title);
        }

        public static readonly DependencyProperty FormatProperty =
            DependencyProperty.RegisterAttached("Format", typeof(string), typeof(BindingExtensions));

        public static string GetFormat(this Binding binding)
        {
            if (binding.Converter is StringFormatConverter)
                return ((StringFormatConverter)binding.Converter).Format;
            return string.Empty;
        }

        public static void SetFormat(this Binding binding, string stringFormat)
        {
            if (binding.Converter == null)
                binding.Converter = new StringFormatConverter(stringFormat);
        }
    }

    public class StringFormatConverter : IValueConverter
    {
        public string Format { get; set; }

        public StringFormatConverter(string format)
        {
            Format = format;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Format(culture, GetEffectiveStringFormat(Format), value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        private string GetEffectiveStringFormat(string stringFormat)
        {
            if (stringFormat.IndexOf('{') < 0)
            {
                stringFormat = "{0:" + stringFormat + "}";
            }
            return stringFormat;
        }
    }

    #endregion
}