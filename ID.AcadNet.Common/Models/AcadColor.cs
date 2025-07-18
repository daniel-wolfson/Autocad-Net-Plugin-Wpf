using Autodesk.AutoCAD.Colors;
using Intellidesk.Data.Models.Entities;
using System;
using System.Globalization;
using System.Windows.Media;
using Color = Autodesk.AutoCAD.Colors.Color;

namespace Intellidesk.AcadNet.Common.Models
{
    public class AcadColor
    {
        public string Name
        {
            get;
            set;
        }

        public int ColorIndex { get; set; }

        public string IsVisible
        {
            get
            {
                if (Name.Contains("Select"))
                    return "Collapsed";
                return "Visible";
            }
        }

        public AcadColor(int colorIndex)
        {
            if (colorIndex > 0) Name = $"Color({colorIndex})";
            else if (colorIndex == -1) Name = $"ByDefault";
            else if (colorIndex == -2) Name = $"ByLayerx";
            else if (colorIndex == -3) Name = $"ByDialog";
            ColorIndex = colorIndex;
        }

        public AcadColor(Color color)
        {
            ColorIndex = color.ColorIndex;
            Name = color.ToString();
        }

        public AcadColor(System.Drawing.Color color)
        {
            ColorIndex = Color.FromColor(color).ColorIndex;
            Name = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(color.Name);
        }

        public AcadColor(PaletteElement element)
        {
            ElementType = element.GetType();
            var acadColor = Color.FromColorIndex(ColorMethod.ByAci, element.ColorIndex.HasValue ? (short)element.ColorIndex : (short)0);
            ColorIndex = acadColor.ColorIndex;
            Name = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(acadColor.ColorName);
        }

        public AcadColor(string name, System.Drawing.Color color) : this(color)
        {
            Name = name;
        }

        public SolidColorBrush ColorBrush
        {
            get
            {
                Color color = Color.FromColorIndex(ColorMethod.ByAci, (short)ColorIndex);
                SolidColorBrush brush = new SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(color.ColorValue.R, color.ColorValue.G, color.ColorValue.B));
                return brush;
            }
        }

        public Type ElementType { get; set; }

        public override bool Equals(object obj)
        {
            AcadColor color = obj as AcadColor;
            return color?.ColorIndex == ColorIndex;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}