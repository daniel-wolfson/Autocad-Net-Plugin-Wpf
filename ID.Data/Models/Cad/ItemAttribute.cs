using Intellidesk.Data.Models.Entities;

namespace Intellidesk.Data.Models.Cad
{
    public partial class BlockItemAttributeDef : PaletteElement
    {
        public int? ItemId { get; set; }
        public int? BlockItemAttributeDefId { get; set; }
        public string Value { get; set; }
        public virtual BlockRef Instance { get; set; }

        //[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        //public override void GetObjectData(SerializationInfo info, StreamingContext context)
        //{
        //    info.AddValue("ItemId", ItemId);
        //    info.AddValue("Value", Value);
        //}
    }
}
