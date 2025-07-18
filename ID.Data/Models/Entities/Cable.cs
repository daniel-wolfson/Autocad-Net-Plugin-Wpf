using ID.Infrastructure.Enums;
using ID.Infrastructure.Extensions;
using System;

namespace Intellidesk.Data.Models.Entities
{
    [Serializable]
    public class Cable : PaletteElement
    {
        public int CableId { get; set; }

        public Cable()
        {
            Title = ElementName = default(eCableType).GetDisplayName();
            TypeCode = (short)default(eCableType);
            ElementType = this.GetType().FullName;
            TypeCode = (short)default(eCableType);
            TypeCodeFullName = typeof(eCableType).FullName;
            ColorIndex = TypeCode.GetDataInfo().ColorIndex;
            TitleColorIndex = TypeCode.GetDataInfo().TitleColorIndex;
            LayerName = TypeCode.GetDataInfo().LayerName;
            TextAlign = TextHorizontalMode.TextMid;
        }

        public Cable(eCableType elementType, string name, string handle)
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
        }

        public new eCableType TypeCode
        {
            get { return (eCableType)base.TypeCode; }
            set { base.TypeCode = (short)value; }
        }

        //public override void UpdateType(Enum typeCode)
        //{
        //    ObjectState = ObjectState != ObjectState.Added ? ObjectState.Modified : ObjectState;
        //    Name = typeCode.ToString();
        //    TypeCode = (eCableType)typeCode;
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
        //    info.AddValue("ElementTypeId", TypeId);
        //    info.AddValue("ColorIndex", ColorIndex);

        //    //if (Titles.Any())
        //    //{
        //    //    SerializationInfo info2 = new SerializationInfo(typeof (List<Title>), new FormatterConverter());
        //    //    foreach (var title in Titles)
        //    //    {
        //    //        info2.AddValue("Name", title.Name);
        //    //        info2.AddValue("Handle", title.Handle);
        //    //        info.AddValue("Red", Red);
        //    //        info.AddValue("Green", Green);
        //    //        info.AddValue("Blue", Blue);
        //    //    }
        //    //    info.AddValue("Title", info2);
        //    //}
        //}

        //public override event PropertyChangedEventHandler PropertyChanged;
        //protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        //{
        //    PropertyChangedEventHandler handler = PropertyChanged;
        //    handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //}
    }
}