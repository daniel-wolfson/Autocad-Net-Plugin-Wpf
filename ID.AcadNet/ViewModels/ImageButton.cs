using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Intellidesk.AcadNet.ViewModels
{
    /// <summary> Image Button with states: Disabled, Normal, Active </summary>
    public class ImageButton : Button
    {
        public ImageSource DisabledImage
        {
            get { return (ImageSource)GetValue(DisabledImageProperty); }
            set { SetValue(DisabledImageProperty, value); }
        }
        public static readonly DependencyProperty DisabledImageProperty =
            DependencyProperty.Register("DisabledImage", typeof(ImageSource), typeof(ImageButton), new UIPropertyMetadata(null));


        public ImageSource NormalImage
        {
            get { return (ImageSource)GetValue(NormalImageProperty); }
            set { SetValue(NormalImageProperty, value); }
        }
        public static readonly DependencyProperty NormalImageProperty =
            DependencyProperty.Register("NormalImage", typeof(ImageSource), typeof(ImageButton), new UIPropertyMetadata(null));

        public ImageSource ActiveImage
        {
            get { return (ImageSource)GetValue(ActiveImageProperty); }
            set { SetValue(ActiveImageProperty, value); }
        }
        public static readonly DependencyProperty ActiveImageProperty =
            DependencyProperty.Register("ActiveImage", typeof(ImageSource), typeof(ImageButton), new UIPropertyMetadata(null));
    }
}