using ID.Infrastructure.Enums;
using ID.Infrastructure.Extensions;
using System;

namespace Intellidesk.Data.Models.Entities
{
    [Serializable]
    public partial class Title : PaletteElement
    {
        public int TitleId { get; set; }

        public Title()
        {
            ElementName = default(eTitleType).GetDisplayName();
            TypeCode = (short)default(eTitleType);
            ColorIndex = 7;
            TitleColorIndex = 7;
        }

        public Title(eTitleType cableType, string name, string handle)
        {
            Title = name;
            Handle = handle;
            ElementName = cableType.GetDisplayName();
            TypeCode = (short)cableType;
            ColorIndex = cableType.GetDataInfo().ColorIndex;
            ColorIndex = cableType.GetDataInfo().TitleColorIndex;
        }
    }
}