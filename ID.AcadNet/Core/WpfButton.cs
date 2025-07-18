using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using Autodesk.Windows;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Intellidesk.AcadNet.Core
{
    public class WpfButton : RibbonButton
    {
        public WpfButton()
        {
            EventManager.RegisterClassHandler(typeof(ButtonBase), UIElement.GotFocusEvent,
                new RoutedEventHandler(OnGotFocus));
        }

        private void OnGotFocus(object sender, RoutedEventArgs e)
        {
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("IsEnable");
        }

        public readonly DependencyProperty IsBubbleSourceProperty = DependencyProperty.RegisterAttached("IsBubbleSource",
            typeof(Boolean), typeof(WpfButton), new PropertyMetadata(false));

        public void SetIsBubbleSource(UIElement element, Boolean value)
        {
            element.SetValue(IsBubbleSourceProperty, value);
        }

        public Boolean GetIsBubbleSource(UIElement element)
        {
            return (Boolean)element.GetValue(IsBubbleSourceProperty);
        }



    }
}