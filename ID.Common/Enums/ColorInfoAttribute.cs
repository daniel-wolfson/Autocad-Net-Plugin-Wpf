using System;
using Color = System.Drawing.Color;

namespace Intellidesk.Common.Enums
{
    public class DataInfoAttribute : Attribute
    {
        /// <summary> color as autocad color index </summary>
        internal DataInfoAttribute(string layerName, short colorIndex = 7, short titleColorIndex = -1)
        {
            ColorIndex = colorIndex;
            LayerName = layerName;
            TitleColorIndex = titleColorIndex < 0 ? colorIndex : titleColorIndex;
        }

        /// <summary> color as html color hex string </summary>
        internal DataInfoAttribute(string layerName, string colorHtml = "#FFFFFFFF")
        {
            Color = System.Drawing.ColorTranslator.FromHtml(colorHtml);
            LayerName = layerName;
            DisplayName = layerName;
        }

        internal DataInfoAttribute(string displayName, string layerName, string colorHtml = "#FFFFFFFF")
        {
            Color = System.Drawing.ColorTranslator.FromHtml(colorHtml);
            LayerName = layerName;
            DisplayName = displayName;
        }

        /// <summary> Argb (int) color format </summary>
        public string DisplayName { get; private set; }
        public Color Color { get; private set; } = Color.Empty;
        public short ColorIndex { get; private set; } = -3; //dialog by default
        public short TitleColorIndex { get; private set; } = -3; //dialog by default
        public string LayerName { get; private set; }

        //int rgb = EntityColor.LookUpRgb(byt);
        //long b = rgb & 0xffL;
        //long g = (rgb & 0xff00L) >> 8;
        //long r = rgb >> 16;
    }
}