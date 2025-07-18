using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Intellidesk.Data.Models.Cad;

namespace Intellidesk.AcadNet.Interfaces
{
    public interface ILayout
    {
        string this[string propertyName] { get; }

        string AccessType { get; set; }
        ICollection<BlockDefinition> Blocks { get; set; }
        string BuildingLevels { get; set; }
        string CADFileName { get; set; }
        string Comment { get; set; }
        string ConfigSetName { get; set; }
        int CreatedBy { get; set; }
        DateTime? DateCreated { get; set; }
        DateTime? DateModified { get; set; }
        string Error { get; }
        ICollection<Filter> Filters { get; set; }
        bool FSA { get; set; }
        ICollection<BlockRef> Items { get; set; }
        string LayoutContents { get; set; }
        decimal LayoutID { get; set; }
        string LayoutName { get; set; }
        short? LayoutState { get; set; }
        string LayoutType { get; set; }
        string LayoutVersion { get; set; }
        int ModifiedBy { get; set; }
        string Param1 { get; set; }
        string Param2 { get; set; }
        string Param3 { get; set; }
        string Param4 { get; set; }
        string ProcessName1 { get; set; }
        string ProcessName2 { get; set; }
        string ProcessName3 { get; set; }
        string ProcessName4 { get; set; }
        ICollection<Rule> Rules { get; set; }
        string SiteName { get; set; }
        string TABFileName { get; set; }
        ICollection<UserSetting> UserSettings { get; set; }
        bool Visible { get; set; }

        string PropertyValid([CallerMemberName] string propertyName = null);
    }
}