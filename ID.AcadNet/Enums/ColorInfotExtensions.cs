using Autodesk.AutoCAD.Colors;
using ID.Infrastructure.Enums;
using ID.Infrastructure.Extensions;

namespace Intellidesk.AcadNet.Enums
{
    public static class ColorInfotExtensions
    {
        public static Color GetColorInfo(this eCableType p)
        {
            var attr = p.GetDataInfo();
            return Color.FromColorIndex(ColorMethod.ByAci, (short)attr.ColorIndex);
        }
    }
}