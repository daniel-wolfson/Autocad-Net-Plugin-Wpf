using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Intellidesk.AcadNet.Components
{
    public class CustomFontIconCheckBox : CheckBox
    {
        public static readonly DependencyProperty CheckIconProperty =
            DependencyProperty.Register("CheckIcon", typeof(Style), typeof(CustomFontIconCheckBox), new UIPropertyMetadata(null));

        public static readonly DependencyProperty UnCheckIconProperty =
                    DependencyProperty.Register("UnCheckIcon", typeof(Style), typeof(CustomFontIconCheckBox), new UIPropertyMetadata(null));

        public static readonly DependencyProperty CheckIconBrushProperty =
                   DependencyProperty.Register("CheckIconBrush", typeof(Brush), typeof(CustomFontIconCheckBox), new UIPropertyMetadata(null));

        public static readonly DependencyProperty UnCheckIconBrushProperty =
                    DependencyProperty.Register("UnCheckIconBrush", typeof(Brush), typeof(CustomFontIconCheckBox), new UIPropertyMetadata(null));

        public Style CheckIcon
        {
            get { return (Style)GetValue(CheckIconProperty); }
            set { SetValue(CheckIconProperty, value); }
        }

        public Style UnCheckIcon
        {
            get { return (Style)GetValue(UnCheckIconProperty); }
            set { SetValue(UnCheckIconProperty, value); }
        }

        public Brush CheckIconBrush
        {
            get { return (Brush)GetValue(CheckIconBrushProperty); }
            set { SetValue(CheckIconBrushProperty, value); }
        }

        public Brush UnCheckIconBrush
        {
            get { return (Brush)GetValue(UnCheckIconBrushProperty); }
            set { SetValue(UnCheckIconBrushProperty, value); }
        }
    }
}