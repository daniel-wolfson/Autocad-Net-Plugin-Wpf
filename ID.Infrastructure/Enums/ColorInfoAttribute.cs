using System;
using Color = System.Drawing.Color;

namespace ID.Infrastructure.Enums
{
    public class DataInfoAttribute : Attribute
    {
        public Type ResourceType { get; set; }
        public string GroupName { get; set; }
        public string Prompt { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public int Order { get; set; }

        public Color Color { get; private set; } = Color.Empty;
        public int ColorIndex { get; private set; } = -3; //dialog by default
        public int TitleColorIndex { get; private set; } = -3; //dialog by default
        public string LayerName { get; private set; }

        /// <summary> color as autocad color index </summary>
        internal DataInfoAttribute(string name, string layerName, short colorIndex = 7, short titleColorIndex = -1)
        {
            Name = name;
            ColorIndex = colorIndex;
            LayerName = layerName;
            TitleColorIndex = titleColorIndex < 0 ? colorIndex : titleColorIndex;
        }

        internal DataInfoAttribute(string name, string layerName, string colorHtml = "#FFFFFFFF")
        {
            Name = name;
            Color = System.Drawing.ColorTranslator.FromHtml(colorHtml);
            LayerName = layerName;
        }
    }
}