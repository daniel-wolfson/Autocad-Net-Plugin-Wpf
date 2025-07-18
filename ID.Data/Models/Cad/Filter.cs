using System;
using System.ComponentModel.DataAnnotations;
using Intellidesk.Data.Models.Entities;
using Intellidesk.Data.Models.EntityMetaData;

namespace Intellidesk.Data.Models.Cad
{
    [MetadataType(typeof(FilterMetaData))]
    public class Filter : BaseEntity
    {
        public int FilterId { get; set; }
        public Nullable<bool> Active { get; set; }
        public string AccessType { get; set; }
        public string FilterName { get; set; }
        public bool FSA { get; set; }
        public string LayoutName { get; set; }
        public string LayoutType { get; set; }
        public string LayoutContents { get; set; }
        public string LayoutVersion { get; set; }
        public string Comment { get; set; }
        public string SiteName { get; set; }
        public string BuildingLevels { get; set; }
        public string CADFileName { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> DateCreated { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<byte> DateModified { get; set; }
        public Nullable<short> LayoutState { get; set; }
        public decimal LayoutID { get; set; }
        public decimal Layout_LayoutID { get; set; }
        public virtual ILayout Layout { get; set; }
    }
}
