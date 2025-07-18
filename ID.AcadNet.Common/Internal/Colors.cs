using Autodesk.AutoCAD.Colors;
using ID.Infrastructure;
using Intellidesk.AcadNet.Common.Extensions;
using Intellidesk.AcadNet.Common.Interfaces;

namespace Intellidesk.AcadNet.Common.Internal
{
    public class Colors
    {
        public static Color Black = Color.FromRgb(0, 0, 0); //White
        public static Color White = Color.FromRgb(255, 255, 255); //White
        //public static Color Yellow = Color.FromRgb(255, 255, 0); //Yellow
        public static Color LightGray = Color.FromRgb(211, 211, 211); //LightGray
        public static Color Current = Color.FromRgb(255, 255, 255);

        public static Color GetColorFromIndex(short value = ByLayer)
        {
            Color colorRet = Color.FromColorIndex(ColorMethod.ByAci, value);
            if (value == ByLayer)
            {
                ILayerService layerService = Plugin.GetService<ILayerService>();
                var layer = layerService.Current();
                colorRet = Colors.GetColorFromIndex(layer.ColorIndex);
            }
            else if (value == ByDialog)
            {
                colorRet = ColorExtensions.DlgColor;
            }
            else if (value >= 0)
            {
                colorRet = Color.FromColorIndex(ColorMethod.ByAci, value);
            }
            return colorRet;
        }

        public const short Blue = 5;
        public const short Cyan = 4;
        public const short Green = 3;
        public const short Magenta = 6;
        public const short YellowColorIndex = 2;
        public const short RedColorIndex = 1;
        public const short ByBlock = 0;
        public const short ByLayer = -1;
        public const short ByDialog = -3;
    }
}