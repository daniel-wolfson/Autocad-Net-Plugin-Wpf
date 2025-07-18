using Intellidesk.Data.Models.Entities;

namespace Intellidesk.Data.Models.Cad
{
    public partial class BlockAttributeDefinition : PaletteElement
    {
        public int? BlockId { get; set; }
        public int? BlockAttributeId { get; set; }
        public decimal BlockIndex { get; set; }
        public decimal BlockAttributeIndex { get; set; }
        public string BlockAttributeName { get; set; }
        public string BlockAttributeValue { get; set; }
        public virtual BlockDefinition Block { get; set; }

        //[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        //public override void GetObjectData(SerializationInfo info, StreamingContext context)
        //{
        //    info.AddValue("BlockAttributeName", BlockAttributeName);
        //    info.AddValue("BlockAttributeValue", BlockAttributeValue);
        //}
    }
}
