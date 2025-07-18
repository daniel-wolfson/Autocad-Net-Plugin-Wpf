using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using ID.Infrastructure.Enums;
using ID.Infrastructure.Extensions;
using Intellidesk.AcadNet.Common.Enums;
using Intellidesk.AcadNet.Common.Internal;
using Intellidesk.Data.Models.Entities;
using Intellidesk.Data.Repositories.Infrastructure;
using Newtonsoft.Json;
using System;

namespace Intellidesk.AcadNet.Common.Models
{
    [PanelMetaData(PaletteNames.Cabinet, typeof(eCabinetType), "BUILDING PARTNER")]
    public class AcadCabinet : Cabinet, IPaletteElement
    {
        public Point3d BasePoint { get; set; }

        public override bool Equals(object obj)
        {
            var acadCabinet = obj as AcadCabinet;
            if (acadCabinet != null)
            {
                // Return true if the fields match:
                return acadCabinet.Title == Title;
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

        public AcadCabinet() : base(eCabinetType.AGC, eCabinetType.AGC.GetDisplayName(), "")
        {
            PaletteType = (short)PaletteNames.Cabinet;
        }

        public AcadCabinet(eCabinetType elementType) : base(elementType, elementType.GetDisplayName(), "")
        {
        }

        public AcadCabinet(eCabinetType elementType, string name) : base(elementType, name, "")
        {
        }

        public AcadCabinet(eCabinetType elementType, string name, string handle) : base(elementType, name, handle)
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

        public override IPaletteElement Update(Enum eType, ObjectState objectState = ObjectState.Unchanged)
        {
            if (this.ObjectState != ObjectState.Edit)
                Title = eType.GetDisplayName();

            TypeCode = (eCabinetType)eType; // (short)Convert.ChangeType(eType, typeof(short));
            ElementName = eType.GetDisplayName();
            ColorIndex = eType.GetDataInfo().ColorIndex;
            TitleColorIndex = eType.GetDataInfo().TitleColorIndex;
            LayerName = eType.GetDataInfo().LayerName;
            ObjectState = objectState;
            Width = TypeCode == eCabinetType.HFD ? 6.15 : 12.04;
            Height = TypeCode == eCabinetType.HFD ? 5.65 : 6.15;
            return this;
        }

        public override PaletteElement Clone()
        {
            return (AcadCabinet)this.MemberwiseClone();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}