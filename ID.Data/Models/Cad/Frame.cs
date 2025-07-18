using System;
using Intellidesk.Data.Models.Entities;
using Intellidesk.Data.Models.EntityMetaData;

namespace Intellidesk.Data.Models.Cad
{
    public partial class Frame : BaseEntity
    {
        public Nullable<decimal> BlockID { get; set; }
        public Nullable<decimal> FrameID { get; set; }
        public decimal LayoutID { get; set; }
        public decimal BlockIndex { get; set; }
        public decimal FrameIndex { get; set; }
        public short FrameTypeID { get; set; }
        public double Xmin { get; set; }
        public double Ymin { get; set; }
        public double Xmax { get; set; }
        public double Ymax { get; set; }
        public virtual Block Block { get; set; }
    }
}
