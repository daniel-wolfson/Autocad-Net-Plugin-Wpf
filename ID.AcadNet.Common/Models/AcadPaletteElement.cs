using Autodesk.AutoCAD.DatabaseServices;
using Intellidesk.Data.Models.Entities;
using Newtonsoft.Json;

namespace Intellidesk.AcadNet.Common.Models
{
    public class AcadPaletteElement1 : PaletteElement
    {
        #region <props>
        public new double Height { get; set; } = 6;
        public new short Weight { get; set; }
        public new short ColorIndex { get; set; }
        public new short TitleColorIndex { get; set; }
        public new double Rotation { get; set; }
        public new long LayerId { get; set; }
        public new int LayoutId { get; set; }
        public new int TabIndex { get; set; }
        #endregion <props>

        #region <ctor>

        public int ElementId { get; set; }

        public AcadPaletteElement1()
        {
            //TEntityType entityType = (TEntityType)(object)(base.TypeCode ?? 0);
            //TypeName = default(TEntityType).GetDisplayName();
            //base.TypeCode = default;
            //ColorIndex = base.ColorIndex.HasValue ? (short)base.ColorIndex : entityType.GetDataInfo().ColorIndex;
            //TitleColorIndex = entityType.GetDataInfo().TitleColorIndex;
            //LayerName = entityType.GetDataInfo().LayerName;
            //TextAlign = TextHorizontalMode.TextMid;
        }

        #endregion <ctor>

        #region <methods>

        public virtual TypedValue[] GetTypedValues()
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

        #endregion
    }
}