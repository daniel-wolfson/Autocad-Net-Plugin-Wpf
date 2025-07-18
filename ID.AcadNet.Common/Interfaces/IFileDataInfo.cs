using Autodesk.AutoCAD.DatabaseServices;
using ID.Infrastucture.Enums;
using Intellidesk.AcadNet.Common.Enums;
using Intellidesk.Data.Models.Cad;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Intellidesk.AcadNet.Common.Interfaces
{
    public interface IFileDataInfo
    {
        string this[string propertyName] { get; }

        string AccessType { get; set; }
        ICollection<BlockDefinition> Blocks { get; set; }
        string BuildingLevels { get; set; }
        string CADFileName { get; set; }
        string Comment { get; set; }
        string ConfigSetName { get; set; }
        string Contents { get; set; }
        CoordSystem CoordSystem { get; set; }
        string CreatedBy { get; set; }
        string CustomType { get; set; }
        DateTime? DateCreated { get; set; }
        DateTime? DateModified { get; set; }
        string Error { get; }
        Extents3d Extents { get; set; }
        FileStatus FileStatus { get; set; }
        ICollection<Filter> Filters { get; set; }
        ICollection<BlockRef> Items { get; set; }
        decimal LayoutID { get; set; }
        string ModifiedBy { get; set; }
        string Name { get; set; }
        string ProcessName1 { get; set; }
        string ProcessName2 { get; set; }
        string ProcessName3 { get; set; }
        string ProcessName4 { get; set; }
        string ProcessParam1 { get; set; }
        string ProcessParam2 { get; set; }
        string ProcessParam3 { get; set; }
        string ProcessParam4 { get; set; }
        ICollection<Rule> Rules { get; set; }
        string SiteName { get; set; }
        //ICollection<State> States { get; set; }
        string TABFileName { get; set; }
        ICollection<UserSetting> UserSettings { get; set; }
        string Version { get; set; }
        bool Visible { get; set; }

        List<TypedValue> GetTypedValues();
        string PropertyValid([CallerMemberName] string propertyName = null);
        bool IsModified();
        bool IsValid();
    }
}