using ID.Infrastructure.Enums;
using System.Collections.ObjectModel;
using System.Linq;

namespace Intellidesk.AcadNet.Common.Models
{
    public class AcadCabinets : ObservableCollection<AcadCabinet>
    {
        public AcadCabinets()
        {
            Add(new AcadCabinet(eCabinetType.AGC));
            Add(new AcadCabinet(eCabinetType.HFD));
            Add(new AcadCabinet(eCabinetType.NewCabinet, "Add Cabinet..."));
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

        public AcadCabinet Get(eCabinetType eType)
        {
            return this.Any(x => x.TypeCode == eType)
                ? this.FirstOrDefault(x => x.TypeCode == eType)
                : this.FirstOrDefault(x => x.TypeCode == default(eCabinetType));
        }
    }
}