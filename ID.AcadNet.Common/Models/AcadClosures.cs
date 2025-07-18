using ID.Infrastructure.Enums;
using System.Collections.ObjectModel;
using System.Linq;

namespace Intellidesk.AcadNet.Common.Models
{
    public class AcadClosures : ObservableCollection<AcadClosure>
    {
        public AcadClosures()
        {
            Add(new AcadClosure(eClosureType.Cl));
            Add(new AcadClosure(eClosureType.NewClosure, "Add Closure..."));
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

        public AcadClosure Get(eClosureType eType)
        {
            return this.Any(x => x.TypeCode == eType)
                ? this.FirstOrDefault(x => x.TypeCode == eType)
                : this.FirstOrDefault(x => x.TypeCode == default(eClosureType));
        }
    }
}