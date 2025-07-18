using System.Collections.Generic;

using Intellidesk.AcadNet.Data.Repositories;

namespace Intellidesk.AcadNet.Data.Models.Entities
{
    public class LayoutType
    {
        public int LayoutTypeId { get; set; }

        public string Name { get; set; }

        public virtual ICollection<ILayout> Layouts { get; set; }
    }
}
