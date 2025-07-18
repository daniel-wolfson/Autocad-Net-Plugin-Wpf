using System.Collections.Generic;
using Intellidesk.Data.Models.Mapping;

namespace Intellidesk.Data.Models.DataContext
{
    public class ContextConfiguration
    {
        //[ImportMany(typeof(IEntityConfiguration))]
        public IEnumerable<IEntityConfiguration> Configurations { get; set; }
    }
}
