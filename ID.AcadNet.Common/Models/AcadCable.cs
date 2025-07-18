using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using ID.Infrastructure.Enums;
using ID.Infrastructure.Extensions;
using Intellidesk.AcadNet.Common.Enums;
using Intellidesk.AcadNet.Common.Internal;
using Intellidesk.Data.Models.Entities;
using Newtonsoft.Json;

namespace Intellidesk.AcadNet.Common.Models
{
    [PanelMetaData(PaletteNames.Cable, typeof(eCableType), "BUILDING PARTNER")]
    public class AcadCable : Cable, IPaletteElement
    {
        public Point3d BasePoint { get; set; }

        public PaletteNames PaletteType { get; set; }

        public string IsVisible
        {
            get
            {
                if (Title.Contains("Select"))
                    return "Collapsed";
                return "Visible";
            }
        }

        public AcadCable() : base(eCableType.Cable144x12x12, eCableType.Cable144x12x12.GetDisplayName(), "")
        {
        }

        public AcadCable(eCableType elementType) : base(elementType, elementType.GetDisplayName(), "")
        {
        }

        public AcadCable(eCableType elementType, string name) : base(elementType, name, "")
        {
        }

        public AcadCable(eCableType elementType, string name, string handle) : base(elementType, name, handle)
        {
        }

        public override bool Equals(object obj)
        {
            var acadCable = obj as AcadCable;
            if (acadCable != null)
            {
                // Return true if the fields match:
                return acadCable.Title == Title;
            }
            return false;
        }

        public TypedValue[] GetTypedValues()
        {
            return new[]
            {
                new TypedValue((int) DxfCode.Start, this.GetType().Name),
                new TypedValue((int) DxfCode.Text, this.Title + this.ElementName),
                new TypedValue((int) DxfCode.Handle, this.Handle),
                new TypedValue((int) DxfCode.LayerName, this.LayerId.ToString()),
                new TypedValue((int) DxfCode.Color, this.ColorIndex),
                new TypedValue((int) DxfCode.ExtendedDataAsciiString, JsonConvert.SerializeObject(this))
            };
        }

        //public override event PropertyChangedEventHandler PropertyChanged;
        //protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        //{
        //    PropertyChangedEventHandler handler = PropertyChanged;
        //    handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //}

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        //public TextHorizontalMode TextAlign { get; set; }
    }
}