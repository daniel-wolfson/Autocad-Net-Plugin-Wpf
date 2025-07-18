using Intellidesk.Data.Models.Entities;
using System.Collections.Generic;

namespace Intellidesk.Data.Models.Cad
{
    public partial class AttributeDefinition : PaletteElement
    {
        public int? MapAttributeId { get; set; }
        public decimal AttributeIndex { get; set; }
        public string AttributeName { get; set; }
        public string AttributeValue { get; set; }
        public new ICollection<AttributeDefinition> Attributes { get; set; }

        //[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        //public override void GetObjectData(SerializationInfo info, StreamingContext context)
        //{
        //    info.AddValue("AttributeName", AttributeName);
        //    info.AddValue("AttributeValue", AttributeValue);
        //}
    }
}