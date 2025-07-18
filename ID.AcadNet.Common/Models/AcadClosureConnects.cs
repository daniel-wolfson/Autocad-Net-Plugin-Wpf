using ID.Infrastructure.Enums;
using System.Collections.ObjectModel;
using System.Linq;

namespace Intellidesk.AcadNet.Common.Models
{
    public class AcadClosureConnects : ObservableCollection<AcadClosureConnect>
    {
        public AcadClosureConnects()
        {
            Add(new AcadClosureConnect(eOpenCloseType.Open));
            Add(new AcadClosureConnect(eOpenCloseType.Close));
        }

        public AcadClosureConnect Get(eOpenCloseType eType)
        {
            return this.Any(x => x.TypeCode == eType)
                ? this.FirstOrDefault(x => x.TypeCode == eType)
                : this.FirstOrDefault(x => x.TypeCode == default(eOpenCloseType));
        }
    }
}