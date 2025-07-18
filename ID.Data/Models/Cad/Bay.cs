using Intellidesk.Data.Models.Entities;
using Intellidesk.Data.Models.EntityMetaData;

namespace Intellidesk.Data.Models.Cad
{
    public partial class Bay : BaseEntity
    {
        public decimal LayoutID { get; set; }
        public string BayName { get; set; }
        public string BaySide { get; set; }
        public string BayPart { get; set; }
        public double Xmin { get; set; }
        public double Ymin { get; set; }
        public double Xmax { get; set; }
        public double Ymax { get; set; }
        public int BayId { get; set; }
        public decimal Layout_LayoutID { get; set; }
        public virtual Layout Layout { get; set; }
    }
}
