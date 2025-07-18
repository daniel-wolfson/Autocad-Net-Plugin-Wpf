using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Intellidesk.AcadNet.Core
{
	public class IconButton
	{
		#region "Fields"

		public static readonly DependencyProperty IconProperty;
		public static readonly DependencyProperty IconSizeProperty;
        //public static readonly DependencyProperty FontSizeProperty;
		public static readonly DependencyProperty OrientationProperty;

		#endregion
		
        #region "Ctor"

		static IconButton()
		{
			//register attached dependency property
			var metadata = new FrameworkPropertyMetadata(null);
			IconProperty = DependencyProperty.RegisterAttached("Icon", typeof(FrameworkElement), typeof(IconButton), metadata);

			metadata = new FrameworkPropertyMetadata(default(Orientation));
			OrientationProperty = DependencyProperty.RegisterAttached("Orientation", typeof(Orientation), typeof(IconButton), metadata);

			metadata = new FrameworkPropertyMetadata(16d);
			IconSizeProperty = DependencyProperty.RegisterAttached("IconSize", typeof(double), typeof(IconButton), metadata);

            //metadata = new FrameworkPropertyMetadata(16);
            //FontSizeProperty = DependencyProperty.RegisterAttached("FontSize", typeof(fonts), typeof(IconButton), metadata);
        }

		#endregion
		
        #region "Logic"

		public static FrameworkElement GetIcon(DependencyObject obj)
		{
			return (FrameworkElement)obj.GetValue(IconProperty);
		}

		public static double GetIconSize(DependencyObject obj)
		{
			return (double)obj.GetValue(IconSizeProperty);
		}

		public static Orientation GetOrientation(DependencyObject obj)
		{
			return (Orientation)obj.GetValue(OrientationProperty);
		}

        //public static int GetFontSize(DependencyObject obj)
        //{
        //    return (int)obj.GetValue(FontSizeProperty);
        //}

		public static void SetIcon(DependencyObject obj, object value)
		{
			obj.SetValue(IconProperty, value);
		}

		public static void SetIconSize(DependencyObject obj, double value)
		{
			obj.SetValue(IconSizeProperty, value);
		}

		public static void SetOrientation(DependencyObject obj, Orientation value)
		{
			obj.SetValue(OrientationProperty, value);
		}

        //public static void SetFontSize(DependencyObject obj, int value)
        //{
        //    obj.SetValue(FontSizeProperty, value);
        //}

		#endregion
	}

    public class Img
    {
        public static readonly DependencyProperty IconProperty;
        public static readonly DependencyProperty IconSizeProperty;
        public static readonly DependencyProperty OrientationProperty;
        public static readonly DependencyProperty NormalImageProperty;

        static Img()
        {
            var metadata = new FrameworkPropertyMetadata(null);
            IconProperty = DependencyProperty.RegisterAttached("Icon", typeof(FrameworkElement), typeof(Img), metadata);

            metadata = new FrameworkPropertyMetadata(null);
            NormalImageProperty = DependencyProperty.RegisterAttached("NormalImage", typeof(ImageSource), typeof(Img), metadata);

            metadata = new FrameworkPropertyMetadata(default(Orientation));
            OrientationProperty = DependencyProperty.RegisterAttached("Orientation", typeof(Orientation), typeof(Img), metadata);

            metadata = new FrameworkPropertyMetadata(16d);
            IconSizeProperty = DependencyProperty.RegisterAttached("IconSize", typeof(double), typeof(Img), metadata);
        }

        public FrameworkElement GetIcon(DependencyObject obj)
        {
            return (FrameworkElement)obj.GetValue(IconProperty);
        }

        public static ImageSource GetNormalImage(DependencyObject obj)
        {
            return (ImageSource)obj.GetValue(NormalImageProperty);
        }

        public static void SetNormalImage(DependencyObject obj, double value)
        {
            obj.SetValue(NormalImageProperty, value);
        }

        public static void SetIcon(DependencyObject obj, FrameworkElement value)
        {
            obj.SetValue(IconProperty, value);
        }

        public static void SetIconSize(DependencyObject obj, double value)
        {
            obj.SetValue(IconSizeProperty, value);
        }

        public static void SetOrientation(DependencyObject obj, Orientation value)
        {
            obj.SetValue(OrientationProperty, value);
        }
    }
}

