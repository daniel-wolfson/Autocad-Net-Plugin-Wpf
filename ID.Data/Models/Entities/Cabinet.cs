using ID.Infrastructure.Enums;
using ID.Infrastructure.Extensions;
using Intellidesk.Data.Models.EntityMetaData;
using System.ComponentModel.DataAnnotations;

namespace Intellidesk.Data.Models.Entities
{
    [MetadataType(typeof(CabinetMetaData))]
    public partial class Cabinet : PaletteElement
    {
        //public int CabinetId { get; set; }

        public Cabinet()
        {
            ElementName = default(eCabinetType).GetDisplayName();
            ElementType = this.GetType().FullName;
            TypeCode = (short)default(eCabinetType);
            TypeCodeFullName = typeof(eCabinetType).FullName;
            ColorIndex = TypeCode.GetDataInfo().ColorIndex;
            TitleColorIndex = TypeCode.GetDataInfo().TitleColorIndex;
            LayerName = TypeCode.GetDataInfo().LayerName;
            TextAlign = TextHorizontalMode.TextMid;
        }

        public Cabinet(eCabinetType elementType, string name, string handle)
        {
            Title = name;
            Handle = handle;
            ElementName = elementType.GetDisplayName();
            ElementType = this.GetType().FullName;
            TypeCode = elementType;
            TypeCodeFullName = elementType.GetType().FullName;
            LayerName = TypeCode.GetDataInfo().LayerName;
            ColorIndex = elementType.GetDataInfo().ColorIndex;
            TitleColorIndex = elementType.GetDataInfo().TitleColorIndex;

            if (elementType == eCabinetType.HFD)
            {
                Width = 6.15;
                Height = 5.65;
            }
            else
            {
                Width = 12.04;
                Height = 6.15;
            }
        }

        public new eCabinetType TypeCode
        {
            get { return (eCabinetType)base.TypeCode; }
            set { base.TypeCode = (short)value; }
        }

        //public override void UpdateType(Enum typeCode)
        //{
        //    ObjectState = ObjectState != ObjectState.Added ? ObjectState.Modified : ObjectState;
        //    Name = typeCode.ToString();
        //    TypeCode = (eCabinetType)typeCode;
        //    TypeName = typeCode.GetDisplayName();
        //    ColorIndex = typeCode.GetMetaDataInfo().ColorIndex;
        //    TitleColorIndex = typeCode.GetMetaDataInfo().TitleColorIndex;
        //    LayerName = typeCode.GetMetaDataInfo().LayerName;
        //}

        //[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        //public override void GetObjectData(SerializationInfo info, StreamingContext context)
        //{
        //    info.AddValue("Handle", Handle);
        //    info.AddValue("Name", Name);
        //}
    }
}
