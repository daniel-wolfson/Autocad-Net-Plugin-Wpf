using System;
using System.Collections.Generic;
using System.ComponentModel;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Intellidesk.AcadNet.Services
{
    public interface IRule
    {
        /// <summary> Rule name </summary>
        string Name { get; set; } //int ID { get; set; }

        /// <summary> Rule color index </summary>
        int ColorIndex { get; set; }

        /// <summary> Rule comment </summary>
        string Comment { get; set; }

        bool isTypeFilterParent { get; set; }

        Type[] TypeFilterOn { get; set; }
        string[] AttributePatternOn { get; set; }
        
        string LayerDestination { get; set; }
        short LayerTypeId { get; set; }
        string LineType { get; set; }
        ObjectId LineTypeId { get; set; }

        [DefaultValue(1)]
        int LineTypeScale { get; set; }

        string[] LayerPatternOn { get; set; }
        string[] LayerPatternOff { get; set; }
        
        string ObjectPattern { get; set; }
        
        /// <summary> Parameter list 1 </summary>
        string ParameterList { get; set; }

        //[DefaultValue(null)]
        //Point3d Position { get; set; }

        /// <summary> Include reading of nested object </summary>
        [DefaultValue(false)]
        bool IncludeNested { get; set; }

        //[DefaultValue(false)]
        //bool IncludeTransform { get; set; }

        [DefaultValue("")]
        string DxfName { get; set; }

        [DefaultValue(null)]
        Point3d MainPosition { get; set; }

        List<Tuple<string, int?, string>> ConfigDictionary { get; set; }

        long StartBlockIndex { get; set; }

        long Id { get; set; }

        string[] LayoutCatalogSite { get; set; }

        string[] LayoutCatalogOptions { get; set; }

        string[] TooNameAttributes { get; set; }
    }
}