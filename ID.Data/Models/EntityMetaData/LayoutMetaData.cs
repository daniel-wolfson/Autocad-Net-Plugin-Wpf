using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Intellidesk.AcadNet.Data.Common.Editors;
using Intellidesk.Data.Common.Editors;
using Intellidesk.Data.Models.Cad;
using Intellidesk.Data.Models.Entities;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using BlockRef = Intellidesk.Data.Models.Cad.BlockRef;

namespace Intellidesk.Data.Models.EntityMetaData
{
    public class LayoutMetaData
    {
        #region "Properties"

        [Browsable(false)]
        public decimal LayoutID { get; set; }

        [Category("Generic"), DisplayName("Layout name")]
        [Required(ErrorMessage = "Layout name not must be empty", AllowEmptyStrings = false)]
        [StringLength(500, ErrorMessage = "Layout name must be 500 characters or less")]
        public string LayoutName { get; set; }

        [Category("Generic"), DisplayName("Layout type")]
        [ItemsSource(typeof(LayoutTypesItemsSource))]
        public string LayoutType { get; set; }

        [Category("Generic"), DisplayName("Access type")]
        public string AccessType { get; set; }

        [Category("Generic"), DisplayName("Comment")]
        [StringLength(500, ErrorMessage = "Layout name must be 500 characters or less")]
        public string Comment { get; set; }

        [Category("Generic"), DisplayName("Layout contents")]
        public string LayoutContents { get; set; }

        [Category("Generic"), DisplayName("Layout version")]
        public string LayoutVersion { get; set; }

        [Category("Location"), DisplayName("Site")]
        public string SiteName { get; set; }

        [Category("Location"), DisplayName("Building and Levels")]
        public string BuildingLevels { get; set; }

        [Category("Files"), DisplayName("File name Dwg")]
        [Description("Path by default there is into work directory of project")]
        public string CADFileName { get; set; }

        [Category("Files"), DisplayName("File name Tab")]
        [Description("Path by default there is into work directory of project")]
        public string TABFileName { get; set; }

        [Category("System Data"), Browsable(true), DisplayName("Created by"), ReadOnly(true)]
        public int CreatedBy { get; set; }

        [Category("System Data"), Browsable(true), DisplayName("Date created"), ReadOnly(true)]
        public DateTime DateCreated { get; set; }

        [Category("System Data"), Browsable(true), DisplayName("Modified by"), ReadOnly(true)]
        public int ModifiedBy { get; set; }

        [Category("System Data"), Browsable(true), DisplayName("Date modified"), ReadOnly(true)]
        public DateTime DateModified { get; set; }

        [Category("System Data"), Browsable(true), DisplayName("Layout state"), ReadOnly(true)]
        public short? LayoutState { get; set; }

        [Category("System Data"), Browsable(true), DisplayName("Laout visible"), ReadOnly(true)]
        public bool Visible { get; set; }

        [Category("Algorithems"), Browsable(true), DisplayName("Laout visible"), ReadOnly(true)]
        public bool FSA { get; set; }

        #endregion

        //#region "overriding properties"

        //[NotMapped, XmlIgnore, Browsable(false)]
        //public List<string> ChangedProperties { get; set; }
        
        //[NotMapped, XmlIgnore, Browsable(false)]
        //public List<string> InvalidProperties { get; set; }

        //[field: NonSerialized] 
        //[Browsable(false)]
        //public event EventHandler<EntityChangedArgs> EntityChangedEvent;

        //#endregion

        #region "Processes"

        [Category("Processes"), Browsable(true), DisplayName("Process name 1"), ReadOnly(false)]
        public string ProcessName1 { get; set; }
        [Category("Processes"), Browsable(true), DisplayName("Process name 2"), ReadOnly(false)]
        public string ProcessName2 { get; set; }
        [Category("Processes"), Browsable(true), DisplayName("Process name 3"), ReadOnly(false)]
        public string ProcessName3 { get; set; }
        [Category("Processes"), Browsable(true), DisplayName("Process name 4"), ReadOnly(false)]
        public string ProcessName4 { get; set; }

        #endregion

        #region "Parameters"

        [Category("Parameters"), Browsable(true), DisplayName("Config set name"), ReadOnly(false)]
        public string ConfigSetName { get; set; }
        [Category("Parameters"), Browsable(true), DisplayName("Parameter 1"), ReadOnly(false)]
        public string Param1 { get; set; }
        [Category("Parameters"), Browsable(true), DisplayName("Parameter 2"), ReadOnly(false)]
        public string Param2 { get; set; }
        [Category("Parameters"), Browsable(true), DisplayName("Parameter 3"), ReadOnly(false)]
        public string Param3 { get; set; }
        [Category("Parameters"), Browsable(true), DisplayName("Parameter 4"), ReadOnly(false)]
        public string Param4 { get; set; }

        //[field: NonSerialized]
        //public override event PropertyChangedEventHandler PropertyChanged;
        //public override void OnPropertyChanged([CallerMemberName] string propertyName = "")
        //{
        //    //if (!object.ReferenceEquals(PropertyChanged, null))
        //    //if (PropertyChanged != null)
        //    //{
        //    if (!ChangedProperties.Contains(propertyName))
        //        ChangedProperties.Add(propertyName);

        //    //PropertyChanged(this, new PropertyChangedEventArgs(propertyName));

        //    if (EntityChangedEvent != null)
        //        EntityChangedEvent(this, new EntityChangedArgs(this, propertyName, (InvalidProperties.Count == 0)));

        //    base.OnPropertyChanged(propertyName);
        //    //}
        //}

        //protected void OnPropertyChanged<T>(Expression<Func<T>> action)
        //{
        //    var propertyName = GetPropertyName(action);
        //    OnPropertyChanged(propertyName);
        //}

        //private static string GetPropertyName<T>(Expression<Func<T>> action)
        //{
        //    var expression = (MemberExpression)action.Body;
        //    var propertyName = expression.Member.Name;
        //    return propertyName;
        //}

        #endregion

        #region "Navigation properties"

        [Browsable(false)]
        public virtual ICollection<Closure> Closures { get; set; }

        [Browsable(false)]
        public virtual ICollection<Cabinet> Cabinets { get; set; }


        [Browsable(false)]
        public virtual ICollection<BlockDefinition> Blocks { get; set; }

        [Browsable(false)]
        public virtual ICollection<BlockRef> BlockItems { get; set; }


        [Browsable(false)]
        public virtual ICollection<UserSetting> UserSettings { get; set; }

        #endregion
    }
}