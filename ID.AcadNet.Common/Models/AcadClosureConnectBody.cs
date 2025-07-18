using Autodesk.AutoCAD.DatabaseServices;
using ID.Infrastructure.Enums;
using Intellidesk.AcadNet.Common.Enums;
using Intellidesk.AcadNet.Common.Internal;
using Intellidesk.Data.Models.Entities;
using Newtonsoft.Json;

namespace Intellidesk.AcadNet.Common.Models
{
    [PanelMetaData(PaletteNames.ClosureConnect, typeof(eBodyType), "BUILDING PARTNER")]
    public class AcadClosureConnectBody : PaletteElement, IPaletteElement
    {
        public AcadClosureConnectBody(eBodyType elementType) : base(typeof(eBodyType), 0)
        {
            //TypeName = default(eBodyType).GetDisplayName();
            //TypeCode = (short)default(eBodyType);
            //ColorIndex = elementType.GetDataInfo().ColorIndex;
            //TitleColorIndex = elementType.GetDataInfo().TitleColorIndex;
            //LayerName = elementType.GetDataInfo().LayerName;
            //TextAlign = ID.Infrastructure.Enums.TextHorizontalMode.TextMid;
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
            var acadCable = obj as AcadClosureConnect;
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