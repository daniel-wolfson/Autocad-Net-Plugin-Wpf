using Intellidesk.Data.Models.Entities;

namespace Intellidesk.Data.Models.Cad
{
    public partial class BlockRef : PaletteElement
    {
        public int BlockRefId { get; set; }
        public int BlockId { get; set; }

        //[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        //public override void GetObjectData(SerializationInfo info, StreamingContext context)
        //{
        //    info.AddValue("BlockId", BlockId);
        //}
    }
}
