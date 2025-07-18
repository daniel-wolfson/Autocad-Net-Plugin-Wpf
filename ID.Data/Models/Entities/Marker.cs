using ID.Infrastructure.Enums;
using ID.Infrastructure.Extensions;
using System;

namespace Intellidesk.Data.Models.Entities
{
    [Serializable]
    public partial class Marker : PaletteElement
    {
        public int ElementId { get; set; }

        public Marker()
        {
            ElementName = default(eOpenCloseType).GetDisplayName();
            TypeCode = (short)default(eOpenCloseType);
            ColorIndex = 7;
            TitleColorIndex = 7;
        }

        public Marker(eOpenCloseType elementType, string name, string handle)
        {
            Title = name;
            Handle = handle;
            ElementName = elementType.GetDisplayName();
            TypeCode = (short)elementType;
            ColorIndex = elementType.GetDataInfo().ColorIndex;
            ColorIndex = elementType.GetDataInfo().TitleColorIndex;
        }
    }
}