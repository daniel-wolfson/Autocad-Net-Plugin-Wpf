using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Resources;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Intellidesk.Resources
{
    public static class ResourceManagerExt
    {
       public static System.Windows.Controls.Image GetImageUri(this ResourceManager rm, string uriString)
       {

            return new System.Windows.Controls.Image
            {
                Source = new BitmapImage(new Uri(uriString)) //"pack://application:,,,/ID.AcadNet;component/Resources/MetroBlack/property.png"
            };
        }

        public static Bitmap GetBitmap(this ResourceManager rm, string imageName)
        {
            try
            {
                return rm.GetObject(imageName) as Bitmap;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static BitmapImage GetBitmapImage(this ResourceManager rm, string imageName, int row , int col)
        {
            try
            {
                var bitmap = rm.GetObject(imageName) as Bitmap;
                var map = new Dictionary<int, int>() { { 0, 0 }, { 1, 16 }, { 2, 32 } };
                if (bitmap != null)
                {
                    var croppedImage = bitmap.Clone(new Rectangle(map[col - 1] * (col - 1), map[row - 1] * (row - 1), map[row], map[row]), bitmap.PixelFormat);
                    return ConvertToBitmapSource(croppedImage);
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static BitmapImage GetBitmapImage(this ResourceManager rm, string imageName)
        {
            try
            {
                var bitmap = rm.GetObject(imageName) as Bitmap;
                return ConvertToBitmapSource(bitmap);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static System.Windows.Controls.Image GetImage(this ResourceManager rm, string imageName)
        {
            try
            {
                var bitmap = rm.GetObject(imageName) as Bitmap;
                if (bitmap != null)
                {
                    BitmapImage bitmapImage = ConvertToBitmapSource(bitmap);
                    return new System.Windows.Controls.Image { Source = bitmapImage };
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static BitmapImage ConvertToBitmapSource(Bitmap image)
        {
            if (image == null) return null;

            var stream = new MemoryStream();
            image.SetResolution(96, 96);
            image.Save(stream, ImageFormat.Png);
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.StreamSource = stream;
            bmp.EndInit();
            return bmp;
        }

        public static string getString(this ResourceManager rm, string stringName)
        {
            try
            {
                //ResourceManager rm = new ResourceManager("Intellidesk.AcadNet.Resources." + stringName, GetType().Assembly);
                return rm.GetString(stringName);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string GetStyleProperty(string assemblyName = "ID.Acadnet", string urlString = "Assets/Fonts") {
            var myResourceDictionary = new ResourceDictionary();
            myResourceDictionary.Source = new Uri($"/{assemblyName};component/{urlString}.xaml", UriKind.RelativeOrAbsolute);
            Style s = (Style)myResourceDictionary["fa-spinner"];
            Setter setter = s.Setters[0] as Setter;
            return setter.Value.ToString();
        }
    }

    //ResourceDictionary resourceDictionary = new ResourceDictionary();
    //resourceDictionary.Source = new Uri("/ID.AcadNet.Resources;component/Assets\\Styles.xaml", UriKind.Relative);
    // resourceDictionary["LinkStyle"] as Style;
}