using ID.Infrastructure.Enums;
using ID.Infrastructure.Extensions;

namespace Intellidesk.Data.Models.Entities
{
    public class ClosureConnect : PaletteElement
    {
        public int ClosureId { get; set; }

        public ClosureConnect()
        {
            ElementName = default(eOpenCloseType).GetDisplayName();
            ElementType = this.GetType().FullName;
            TypeCode = (short)default(eOpenCloseType);
            TypeCodeFullName = typeof(eOpenCloseType).FullName;
            ColorIndex = TypeCode.GetDataInfo().ColorIndex;
            TitleColorIndex = TypeCode.GetDataInfo().TitleColorIndex;
            LayerName = TypeCode.GetDataInfo().LayerName;
            TextAlign = TextHorizontalMode.TextMid;
        }

        public ClosureConnect(eOpenCloseType elementType, string name, string handle)
        {
            Title = name;
            Handle = handle;
            ElementName = elementType.GetDisplayName();
            ElementType = this.GetType().FullName;
            TypeCode = elementType;
            TypeCodeFullName = elementType.GetType().FullName;
            ColorIndex = elementType.GetDataInfo().ColorIndex;
            TitleColorIndex = elementType.GetDataInfo().TitleColorIndex;
            LayerName = elementType.GetDataInfo().LayerName;
            TextAlign = TextHorizontalMode.TextMid;
        }

        public new eOpenCloseType TypeCode
        {
            get => (eOpenCloseType)base.TypeCode;
            set => base.TypeCode = (short)value;
        }
    }
}