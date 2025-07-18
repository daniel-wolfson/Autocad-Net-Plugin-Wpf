using ID.Infrastructure.Enums;
using ID.Infrastructure.Extensions;

namespace Intellidesk.Data.Models.Entities
{
    public class Closure : PaletteElement
    {
        public int ClosureId { get; set; }

        public Closure()
        {
            ElementName = default(eClosureType).GetDisplayName();
            ElementType = this.GetType().FullName;
            TypeCode = (short)default(eClosureType);
            ColorIndex = TypeCode.GetDataInfo().ColorIndex;
            TitleColorIndex = TypeCode.GetDataInfo().TitleColorIndex;
            LayerName = TypeCode.GetDataInfo().LayerName;
            TextAlign = TextHorizontalMode.TextMid;
        }

        public Closure(eClosureType elementType, string name, string handle)
        {
            Title = name;
            Handle = handle;
            ElementName = elementType.GetDisplayName();
            ElementType = this.GetType().FullName;
            TypeCode = elementType;
            ColorIndex = elementType.GetDataInfo().ColorIndex;
            TitleColorIndex = elementType.GetDataInfo().TitleColorIndex;
            LayerName = elementType.GetDataInfo().LayerName;
            TextAlign = TextHorizontalMode.TextMid;
        }

        public new eClosureType TypeCode
        {
            get { return (eClosureType)base.TypeCode; }
            set { base.TypeCode = (short)value; }
        }

        //[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        //public override void GetObjectData(SerializationInfo info, StreamingContext context)
        //{
        //    info.AddValue("Handle", Handle);
        //    info.AddValue("Name", Name);
        //    info.AddValue("ElementTypeId", TypeId);
        //    info.AddValue("ColorIndex", ColorIndex);
        //}
    }
}