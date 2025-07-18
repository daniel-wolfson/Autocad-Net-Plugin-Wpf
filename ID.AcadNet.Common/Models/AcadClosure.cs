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
    [PanelMetaData(PaletteNames.Closure, typeof(eClosureType), "BUILDING PARTNER")]
    public class AcadClosure : Closure, IPaletteElement
    {
        public Point3d BasePoint { get; set; }

        public AcadClosure() : base(eClosureType.Cl, eClosureType.Cl.GetDisplayName(), "")
        {
        }

        public AcadClosure(eClosureType elementType) : base(elementType, elementType.GetDisplayName(), "")
        {
        }

        public AcadClosure(eClosureType elementType, string name) : base(elementType, name, "")
        {
        }

        public AcadClosure(eClosureType elementType, string name, string handle) : base(elementType, name, handle)
        {
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

        public string IsVisible
        {
            get
            {
                if (Title.Contains("Select"))
                    return "Collapsed";
                return "Visible";
            }
        }


        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}