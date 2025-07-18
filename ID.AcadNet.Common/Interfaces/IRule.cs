using System;
using System.Collections.Generic;
using Intellidesk.Data.Models.EntityMetaData;
using Intellidesk.Data.Repositories.Infrastructure;

namespace Intellidesk.AcadNet.Common.Interfaces
{
    public interface IRule
    {
        List<Tuple<string, int?, string>> ConfigDictionary { get; set; }
        string DxfName { get; set; }
        long Id { get; set; }
        short LayerTypeId { get; set; }
        int LayoutId { get; set; }
        long LineTypeId { get; set; }
        int LineTypeScale { get; set; }
        Point3D MainPosition { get; set; }
        string ObjectPattern { get; set; }
        ObjectState ObjectState { get; set; }
        string ParameterList { get; set; }
        long StartBlockIndex { get; set; }
        string[] AttributePatternOn { get; set; }
    }
}