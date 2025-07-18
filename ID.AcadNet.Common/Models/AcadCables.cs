using ID.Infrastructure.Enums;
using System.Collections.ObjectModel;
using System.Linq;

namespace Intellidesk.AcadNet.Common.Models
{
    public class AcadCables : ObservableCollection<AcadCable>
    {
        public AcadCables()
        {
            Add(new AcadCable(eCableType.Cable12x1x12));
            Add(new AcadCable(eCableType.Cable24x1x12));
            Add(new AcadCable(eCableType.Cable36x6x6));
            Add(new AcadCable(eCableType.Cable48x8x6));
            Add(new AcadCable(eCableType.Cable96x16x6));
            Add(new AcadCable(eCableType.Cable96x8x12));
            Add(new AcadCable(eCableType.Cable96x8x12mini));
            Add(new AcadCable(eCableType.Cable144x12x12));
            Add(new AcadCable(eCableType.Cable144x12x12Turquoise));
            Add(new AcadCable(eCableType.Cable288x12x14));
            Add(new AcadCable(eCableType.Cable288x12x14Minidust));
            Add(new AcadCable(eCableType.NewCable, "Add Cable..."));
        }

        public bool Contains(string name)
        {
            for (int i = 0; i < Count; i++)
            {
                if (this[i].Title.Equals(name))
                    return true;
            }
            return false;
        }

        public AcadCable Get(eCableType eType)
        {
            return this.Any(x => x.TypeCode == eType)
                ? this.FirstOrDefault(x => x.TypeCode == eType)
                : this.FirstOrDefault(x => x.TypeCode == default(eCableType));
        }
    }
}