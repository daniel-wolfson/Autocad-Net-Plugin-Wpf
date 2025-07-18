using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Intellidesk.AcadNet.Data.Common.Editors;
using Intellidesk.Data.Common.Editors;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using MultiSelectComboBoxEditor = Intellidesk.Data.Common.Editors.MultiSelectComboBoxEditor;

namespace Intellidesk.Data.Models.Entities
{
    public abstract class BaseLayout : BaseEntity
    {
        [Browsable(false)]
        public decimal LayoutID { get; set; }

        private string _layoutName;
        [Category("Generic"), DisplayName("Layout name")]
        [Required(ErrorMessage = "Layout name not must be empty", AllowEmptyStrings = false)]
        [StringLength(500, ErrorMessage = "Layout name must be 500 characters or less")]
        public string LayoutName
        {
            get { return _layoutName; }
            set
            {
                _layoutName = value ?? "new layout";
                OnPropertyChanged();
            }
        }

        private string _layoutType;
        [Category("Generic"), DisplayName("Layout type")]
        [ItemsSource(typeof(LayoutTypesItemsSource))]
        public string LayoutType
        {
            get { return _layoutType; }
            set
            {
                _layoutType = value ?? "none";
                OnPropertyChanged();
            }
        }

        //private string _accessType;
        //[Category("Generic"), DisplayName("Access type")]
        //[ItemsSource(typeof(AccessTypeItemsSource))]
        //public string AccessType
        //{
        //    get { return _accessType; }
        //    set
        //    {
        //        _accessType = value ?? "none";
        //        OnPropertyChanged();
        //    }
        //}

        private string _comment;
        [Category("Generic"), DisplayName("Comment")]
        [StringLength(500, ErrorMessage = "Layout name must be 500 characters or less")]
        public string Comment
        {
            get { return _comment; }
            set
            {
                _comment = value;
                OnPropertyChanged();
            }
        }

        private string _layoutContents;
        [Category("Generic"), DisplayName("Layout contents")]
        [Editor(typeof(MultiSelectComboBoxEditor), typeof(MultiSelectComboBoxEditor))]
        public string LayoutContents
        {
            get { return _layoutContents; }
            set
            {
                _layoutContents = value ?? "none";
                OnPropertyChanged();
            }
        }

        private string _layoutVersion;
        [Category("Generic"), DisplayName("Layout version")]
        public string LayoutVersion
        {
            get { return _layoutVersion; }
            set
            {
                _layoutVersion = value ?? "new";
                OnPropertyChanged();
            }
        }

        private string _siteName;
        [Category("Location"), DisplayName("Site")]
        public string SiteName
        {
            get { return _siteName; }
            set
            {
                _siteName = value ?? "none";
                OnPropertyChanged();
            }
        }

        private string _buildingLevels;
        [Category("Location"), DisplayName("Building and Levels")]
        [Editor(typeof(MultiSelectComboBoxEditor), typeof(MultiSelectComboBoxEditor))]
        public string BuildingLevels
        {
            get { return _buildingLevels; }
            set
            {
                _buildingLevels = value ?? "none";
                OnPropertyChanged();
            }
        }

        private string _cadFileName;
        [Category("System Data"), DisplayName("File name Dwg")]
        [Description("Path by default there is into work directory of project")]
        [Editor(typeof(FileNameEditor), typeof(FileNameEditor))]
        public string CADFileName
        {
            get { return _cadFileName; }
            set
            {
                _cadFileName = value ?? "none";
                OnPropertyChanged();
            }
        }

        private string _tabFileName;
        [Category("System Data"), DisplayName("File name Tab")]
        [Description("Path by default there is into work directory of project")]
        [Editor(typeof(FileNameEditor), typeof(FileNameEditor))]
        public string TABFileName
        {
            get { return _tabFileName; }
            set
            {
                _tabFileName = value ?? "none";
                OnPropertyChanged();
            }
        }

        private int _createdBy;
        [Category("System Data"), Browsable(true), DisplayName("Created by"), ReadOnly(true)]
        public int CreatedBy
        {
            get { return _createdBy; }
            set
            {
                _createdBy = value; // ?? Environment.UserName;
                OnPropertyChanged();
            }
        }

        private DateTime _dateCreated;
        [Category("System Data"), Browsable(true), DisplayName("Date created"), ReadOnly(true)]
        public DateTime DateCreated
        {
            get { return _dateCreated; }
            set
            {
                _dateCreated = value; // ?? DateTime.Now;
                OnPropertyChanged();
            }
        }

        private int _modifiedBy;
        [Category("System Data"), Browsable(true), DisplayName("Modified by"), ReadOnly(true)]
        public int ModifiedBy
        {
            get { return _modifiedBy; }
            set
            {
                _modifiedBy = value; // ?? Environment.UserName;
                OnPropertyChanged();
            }
        }

        private DateTime _dateModified;
        [Category("System Data"), Browsable(true), DisplayName("Date modified"), ReadOnly(true)]
        public DateTime DateModified
        {
            get { return _dateModified; }
            set
            {
                _dateModified = value; // ?? DateTime.Now;
                OnPropertyChanged();
            }
        }

        [Category("System Data"), Browsable(true), DisplayName("Layout state"), ReadOnly(true)]
        public short? LayoutState { get; set; }

        private bool _visible = true;
        [Category("System Data"), Browsable(true), DisplayName("Laout visible"), ReadOnly(true)]
        public bool Visible
        {
            get { return _visible; }
            set { _visible = value; }
        }
    }
}