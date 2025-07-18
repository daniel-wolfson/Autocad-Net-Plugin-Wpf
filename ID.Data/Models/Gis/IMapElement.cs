using System.Collections.Generic;
using System.Data.Entity.Spatial;
using Intellidesk.Data.Models.Cad;

namespace Intellidesk.Data.Models.Gis
{
    public interface IMapElement
    {
        ICollection<AttributeDefinition> Attributes { get; set; }
        string Handle { get; set; }
        int LayerId { get; set; }
        ILayout Layout { get; set; }
        int LayoutId { get; set; }
        DbGeography Location { get; set; }
        string Name { get; set; }
        int Pk { get; set; }
        int TabIndex { get; set; }
        short TypeId { get; set; }
        string XrefName { get; set; }
    }
}