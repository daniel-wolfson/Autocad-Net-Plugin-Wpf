using System;
using System.Collections.Generic;
using System.ComponentModel;
using Intellidesk.Data.Models;
using Intellidesk.Data.Models.Cad;
using Intellidesk.Data.Repositories.Infrastructure;

namespace Intellidesk.Data.Repositories
{
    public interface ILayout : IObjectState
    {
        string AccessType { get; set; }
        ILayout Clone();
        string BuildingLevels { get; set; }
        string CADFileName { get; set; }

        [Browsable(false)]
        List<string> ChangedProperties { get; set; }
        string Comment { get; set; }
        int CreatedBy { get; set; }
        DateTime DateCreated { get; set; }
        //DateTime DateModified { get; set; }
        [Browsable(false)]
        event EventHandler<EntityChangedArgs> EntityChangedEvent;

        bool FSA { get; set; }
        List<string> InvalidProperties { get; set; }
        string LayoutContents { get; set; }
        decimal LayoutID { get; set; }
        string LayoutName { get; set; }
        short? LayoutState { get; set; }
        string LayoutType { get; set; }
        string LayoutVersion { get; set; }
        int ModifiedBy { get; set; }
        //void OnPropertyChanged(string propertyName);
        //event PropertyChangedEventHandler PropertyChanged;
        string SiteName { get; set; }
        string TABFileName { get; set; }
        //string this[string propertyName] { get; set; }
        bool Visible { get; set; }
    }
}