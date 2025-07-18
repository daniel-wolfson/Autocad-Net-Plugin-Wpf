using System;
using Autodesk.AutoCAD.Colors;

namespace Intellidesk.AcadNet.Common.Core
{
    public class ColorInfoAttribute : Attribute
    {
        internal ColorInfoAttribute(Color color)
        {
            this.Color = color;
        }
        public Color Color { get; private set; }
    }

    public static class ColorInfotExtensions
    {
        //public static Color GetColorAttr(this CableType p)
        //{
        //    var attr = p.GetAttribute<ColorInfoAttribute>();
        //    return attr.Color;
        //}
    }
}