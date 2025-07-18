using Intellidesk.Data.Models.Entities;
using System.Collections.Generic;

namespace Intellidesk.Data.Models.Cad
{
    public partial class BlockDefinition : PaletteElement
    {
        public BlockDefinition()
        {
            this.BlockAttributes = new List<BlockAttributeDefinition>();
        }

        public int? BlockId { get; set; }
        public decimal BlockIndex { get; set; }
        public string BlockName { get; set; }
        public string BlockXrefName { get; set; }
        public string BlockHandle { get; set; }
        public ICollection<BlockAttributeDefinition> BlockAttributes { get; set; }

        //[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        //public override void GetObjectData(SerializationInfo info, StreamingContext context)
        //{
        //    info.AddValue("BlockName", BlockName);
        //    info.AddValue("BlockHandle", BlockHandle);
        //}
    }
}
