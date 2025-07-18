using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Intellidesk.Data.Models.Entities;
using Intellidesk.Data.Models.EntityMetaData;
using Intellidesk.Data.Repositories.Infrastructure;

namespace Intellidesk.Data.Models.Cad
{
    //[MetadataType(typeof(RuleMetaData))]
    public partial class  Rule : BaseEntity, IObjectState
    {
        public static string[] CommonAttributePatternOn { get; set; }
        public static Type[] CommonTypeFilterOn { get; set; }

        //IRule properties
        public short LayerTypeId { get; set; }
        public long LineTypeId { get; set; }
        public int LayoutId { get; set; }
        public int LineTypeScale { get; set; }
        public string ObjectPattern { get; set; }
        public string ParameterList { get; set; }
        public string DxfName { get; set; }
        public Point3D MainPosition { get; set; }
        public List<Tuple<string, int?, string>> ConfigDictionary { get; set; }
        public long StartBlockIndex { get; set; }
        public long Id { get; set; }

        //[NotMapped]
        //public string[] FilterBlockAttributesOn
        //{
        //    get { return FilterBlockAttributesOn_Data.Split(';'); }
        //    set { FilterBlockAttributesOn_Data = String.Join(";", value); }
        //}

        //[NotMapped]
        //public string[] LayoutCatalogSite
        //{
        //    get { return LayoutCatalogSite_Data.Split(';'); }
        //    set { LayoutCatalogSite_Data = String.Join(";", value); }
        //}

        //[NotMapped]
        //public string[] LayoutCatalogOptions
        //{
        //    get { return LayoutCatalogOptions_Data.Split(';'); }
        //    set { LayoutCatalogOptions_Data = String.Join(";", value); }
        //}

        //[NotMapped]
        //public string[] LayerPatternOn
        //{
        //    get { return LayerPatternOn_Data.Split(';'); }
        //    set { LayerPatternOn_Data = String.Join(";", value); }
        //}

        //[NotMapped]
        //public string[] LayerPatternOff
        //{
        //    get { return LayerPatternOff_Data.Split(';'); }
        //    set { LayerPatternOff_Data = String.Join(";", value); }
        //}

        //[NotMapped]
        //public string[] AttributePatternOn
        //{
        //    get { return AttributePatternOn_Data.Split(';'); }
        //    set { AttributePatternOn_Data = String.Join(";", value); }
        //}

        //[NotMapped]
        //public string[] TypeFilterOn
        //{
        //    get { return TypeFilterOn_Data.Split(';'); }
        //    set { TypeFilterOn_Data = String.Join(";", value); }
        //    //get { return Array.ConvertAll(TypeFilterOn_Data.Split(';'), Type.GetType);  }
        //    //set { TypeFilterOn_Data = String.Join(";", value.Select(p => p.Name).ToArray()); }
        //}

        //[NotMapped]
        //public string[] TooNameAttributes
        //{
        //    get { return TooNameAttributes_Data.Split(';'); }
        //    set { TypeFilterOn_Data = String.Join(";", value); }
        //}


        //public Rule Default()
        //{
        //    var rule = new Rule()
        //    {
        //        LayoutCatalogOptions = new[] { "" },
        //        AttributePatternOn = new[] { "ENTITY_CODE", "ENTITY_CODES", "TOOL_ID" },
        //        ColorIndex = 191,
        //        ConfigDictionary = new List<Tuple<string, int?, string>>(),
        //        FilterBlockAttributesOn = new string[] { },
        //        isTypeFilterParent = true,
        //        IncludeNested = true,
        //        LayerDestination = ".LSDS_DEFAULT_FYSICAL",
        //        LayerPatternOn = new[] { "0" },
        //        TypeFilterOn = new[] { "BlockReference" },//typeof(BlockReference)
        //        LineType = "DASHED"
        //    };
        //    return rule;
        //}

        //public ObjectState ObjectState { get; set; }
    }
}