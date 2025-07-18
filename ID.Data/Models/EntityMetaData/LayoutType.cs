using System.Collections.Generic;
using Intellidesk.AcadNet.Data.Repositories;
using Intellidesk.Data.Repositories;

namespace Intellidesk.Data.Models.EntityMetaData
{
    public class LayoutType
    {
        public int LayoutTypeId { get; set; }

        public string Name { get; set; }

        public virtual ICollection<ILayout> Layouts { get; set; }
    }
}
