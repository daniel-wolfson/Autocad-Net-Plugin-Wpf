using Autodesk.AutoCAD.Colors;
using Intellidesk.AcadNet.Services.Extentions;

namespace Intellidesk.AcadNet.Services
{
    public class Colors
    {
        public static Color Black = Color.FromRgb(0, 0, 0); //White
        public static Color White = Color.FromRgb(255, 255, 255); //White
        //public static Color Yellow = Color.FromRgb(255, 255, 0); //Yellow
        public static Color LightGray = Color.FromRgb(211, 211, 211); //LightGray
        public static Color Current = Color.FromRgb(255, 255, 255);

        public static Color GetColorFromIndex(short value = Colors.ByLayer)
        {
            var colorRet = Color.FromColorIndex(ColorMethod.ByAci, value);
            if (value == Colors.ByLayer)
            {
                colorRet = Colors.Current;
            }
            else if (value >= 0)
            {
                colorRet = Color.FromColorIndex(ColorMethod.ByAci, value);
            }
            else if (value == Colors.ByDialog)
            {
                colorRet = CoreExtensions.DlgColor;
            }
            return colorRet;
        }

        public const short Blue = 5;
        public const short Cyan = 4;
        public const short Green = 3;
        public const short Magenta = 6;
        public const short Yellow = 2;
        public const short Red = 1;
        public const short ByLayer = -1;
        public const short ByBlock = 0;
        public const short ByDialog = -3;
    }
}