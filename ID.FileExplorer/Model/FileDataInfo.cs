using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using ID.Infrastucture.Enums;
using Intellidesk.AcadNet.Common.Enums;
using Intellidesk.AcadNet.Common.Extensions;
using Intellidesk.AcadNet.Common.Interfaces;
using Intellidesk.AcadNet.Data.Common.Editors;
using Intellidesk.Data.Models.Cad;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using BlockRef = Intellidesk.Data.Models.Cad.BlockRef;

namespace FileExplorer.Model
{
    public class FileDataInfo : FileDataInfoBase, IFileDataInfo
    {
        #region "Properties"

        [Browsable(false)]
        public decimal LayoutID { get; set; }

        private CoordSystem _coordSystem = CoordSystem.ITM;
        [Category("Coordinates"), DisplayName("Coordinate system")]
        public CoordSystem CoordSystem
        {
            get { return _coordSystem; }
            set { Set(ref _coordSystem, value); }
        }

        private string _name = "new layout";
        [Category("Data"), Description("User file name")]
        public string Name
        {
            get { return _name; }
            set { Set(ref _name, value); }
        }

        private string _customType = "none";
        [Category("Data"), DisplayName("User file type")]
        [Description("User file type")]
        public string CustomType
        {
            get { return _customType; }
            set { Set(ref _customType, value); }
        }

        private string _type = "undefined";
        [Category("Data"), Description("Windows file type"), ReadOnly(true)]
        public override string Type
        {
            get { return _type; }
            set { Set(ref _type, value); }
        }

        private string _comment = "none";
        [Category("Data"), Description("User comment")]
        public string Comment
        {
            get { return _comment; }
            set { Set(ref _comment, value); }
        }

        private string _version = "new";
        [Category("Data"), Description("User version number")]
        public string Version
        {
            get { return _version; }
            set { Set(ref _version, value); }
        }

        private string _siteName = "none";
        [Category("Data"), Description("Placement of area")]
        public string SiteName
        {
            get { return _siteName; }
            set { Set(ref _siteName, value); }
        }

        private string _cadFileName = "noname.dwg";
        [Category("Data"), DisplayName("Full path dwg")]
        [Description("Path by default there is into work directory of project")]
        [Editor(typeof(FileNameEditor), typeof(FileNameEditor))]
        public string CADFileName
        {
            get { return _cadFileName; }
            set { Set(ref _cadFileName, value); }
        }

        private string _tabFileName = "noname.tab";
        [Category("Data"), DisplayName("Full path tab")]
        [Description("Path by default there is into work directory of project")]
        [Editor(typeof(FileNameEditor), typeof(FileNameEditor))]
        public string TABFileName
        {
            get { return _tabFileName; }
            set { Set(ref _tabFileName, value); }
        }

        private FileStatus _fileStatus = FileStatus.Undefined;
        [Category("Data"), DisplayName("Status")]
        public FileStatus FileStatus
        {
            get { return _fileStatus; }
            set { _fileStatus = value; }
        }

        private bool _visible;
        [Category("Data")]
        public bool Visible
        {
            get { return _visible; }
            set { Set(ref _visible, value); }
        }

        private string _accessType = "none";
        [Category("Options admin"), Description("User access type")]
        //[ItemsSource(typeof(AccessTypeItemsSource))]
        public string AccessType
        {
            get { return _accessType; }
            set { Set(ref _accessType, value); }
        }

        private string _contents = "none";
        [Category("Options admin"), Description("File content list that it contains")]
        //[Editor(typeof(MultiSelectComboBoxEditor), typeof(MultiSelectComboBoxEditor))]
        public string Contents
        {
            get { return _contents; }
            set { Set(ref _comment, value); }
        }

        private string _buildingLevels = "none";
        [Category("Options admin"), Description("Level")]
        //[Editor(typeof(MultiSelectComboBoxEditor), typeof(MultiSelectComboBoxEditor))]
        public string BuildingLevels
        {
            get { return _buildingLevels; }
            set { Set(ref _buildingLevels, value); }
        }
        private string _createdBy = Environment.UserName;
        [Category("File info"), DisplayName("Created by"), ReadOnly(true)]
        public string CreatedBy
        {
            get { return _createdBy; }
            set { Set(ref _createdBy, value); }
        }

        private DateTime? _dateCreated = DateTime.Now;
        [Category("File info"), DisplayName("Date created"), ReadOnly(true)]
        public DateTime? DateCreated
        {
            get { return _dateCreated; }
            set { Set(ref _dateCreated, value); }
        }

        private string _modifiedBy = Environment.UserName;
        [Category("File info"), DisplayName("Modified By"), ReadOnly(true)]
        public string ModifiedBy
        {
            get { return _modifiedBy; }
            set { Set(ref _modifiedBy, value); }
        }

        private DateTime? _dateModified = DateTime.Now;
        [Category("File info"), DisplayName("Date modified"), ReadOnly(true)]
        public DateTime? DateModified
        {
            get { return _dateModified; }
            set { Set(ref _dateModified, value); }
        }
        private Extents3d _extents;
        [Category("Extents"), Browsable(false)]
        public Extents3d Extents
        {
            get { return _extents; }
            set { Set(ref _extents, value); }
        }

        #endregion

        #region "Processes"

        [Category("Processes"), DisplayName("Process name 1"), Description("Process name 1")]
        public string ProcessName1 { get; set; }

        [Category("Processes"), DisplayName("Process param 1"), Description("Process parameter 1")]
        public string ProcessParam1 { get; set; }

        [Category("Processes"), DisplayName("Process name 2"), Description("Process name 2")]
        public string ProcessName2 { get; set; }

        [Category("Processes"), DisplayName("Process param 2"), Description("Process parameter 2")]
        public string ProcessParam2 { get; set; }

        [Category("Processes"), DisplayName("Process name 3"), Description("Process name 3")]
        public string ProcessName3 { get; set; }

        [Category("Processes"), DisplayName("Process param 3"), Description("Process parameter 3")]
        public string ProcessParam3 { get; set; }

        [Category("Processes"), DisplayName("Process name 4"), Description("Process name 4")]
        public string ProcessName4 { get; set; }

        [Category("Processes"), DisplayName("Process param 4"), Description("Process parameter 4")]
        public string ProcessParam4 { get; set; }

        #endregion

        [Browsable(false)]
        public string ConfigSetName { get; set; }

        #region "Navigation properties"

        [Browsable(false)]
        public virtual ICollection<BlockDefinition> Blocks { get; set; }

        [Browsable(false)]
        public virtual ICollection<Filter> Filters { get; set; }

        [Browsable(false)]
        public virtual ICollection<BlockRef> Items { get; set; }

        [Browsable(false)]
        public virtual ICollection<Rule> Rules { get; set; }

        //[Browsable(false)]
        //public virtual ICollection<State> States { get; set; }

        [Browsable(false)]
        public virtual ICollection<UserSetting> UserSettings { get; set; }

        #endregion

        #region "IDataErrorInfo Members"

        public string this[string propertyName]
        {
            get
            {
                return ValidationHandler.InvalidPropertyExist(propertyName)
                    ? ValidationHandler[propertyName]
                    : null;
            }
        }

        [Category("Errors"), Description("Errors")]
        public string Error
        {
            get
            {
                return string.Join("\n", ValidationHandler.GetInvalidProperties());
            }
        }

        public override string PropertyValid([CallerMemberName] string propertyName = null)
        {
            var driveName = "";
            string result = "";
            switch (propertyName)
            {
                case "Comment":
                    //result = TryLengthValidate(_comment ?? "", 500);
                    break;
                case "CADFileName":
                    //var saveFileDialog1 = new SaveFileDialog("File will be saved and closed", CADFileName, "dwg", Plugin.Name, SaveFileDialog.SaveFileDialogFlags.DefaultIsFolder);
                    //if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                    //{

                    //}

                    //result = ValidationHandler.ValidateRule("CADFileName", "file not found",
                    //        () => ValidationHandler.FindFileRuleAsync(CADFileName, "dwg", out driveName) == "");

                    ////result = TryFindFileAsync(CADFileName, "dwg", out driveName);
                    ////var userSetting = UserSetting.Load();
                    ////if (driveName != "") userSetting.Drive = driveName;
                    ////userSetting.Save();


                    break;
                case "TABFileName":
                    Mouse.OverrideCursor = Cursors.Wait;

                    result = ValidationHandler.ValidateRule("TABFileName", "file not found",
                        () => ValidationHandler.FindFileRuleAsync(TABFileName, "tab", out driveName) == "");

                    //result = TryFindFileAsync(TABFileName, "tab", out driveName);

                    Mouse.OverrideCursor = null;
                    break;
            }
            return result;
        }

        public List<TypedValue> GetTypedValues()
        {
            List<TypedValue> list = new List<TypedValue>
            {
                new TypedValue((int) DxfCode.ExtendedDataRegAppName, PluginSettings.Name),
                new TypedValue((int) DxfCode.ExtendedDataAsciiString, "CADFileName"),
                new TypedValue((int) DxfCode.ExtendedDataAsciiString, CADFileName),
                new TypedValue((int) DxfCode.ExtendedDataAsciiString, "TABFileName"),
                new TypedValue((int) DxfCode.ExtendedDataAsciiString, TABFileName),
                new TypedValue((int) DxfCode.ExtendedDataAsciiString, "CoordSystem"),
                new TypedValue((int) DxfCode.ExtendedDataAsciiString, CoordSystem),
                new TypedValue((int) DxfCode.ExtendedDataAsciiString, "FileStatus"),
                new TypedValue((int) DxfCode.ExtendedDataAsciiString, FileStatus.Undefined.ToString()),
                new TypedValue((int) DxfCode.ExtendedDataAsciiString, "ExtentsMinPoint"),
                new TypedValue((int) DxfCode.ExtendedDataAsciiString, Extents.MinPoint.ToString()),
                new TypedValue((int) DxfCode.ExtendedDataAsciiString, "ExtentsMaxPoint"),
                new TypedValue((int) DxfCode.ExtendedDataAsciiString, Extents.MaxPoint.ToString()),
                new TypedValue((int) DxfCode.ExtendedDataAsciiString, "Comment"),
                new TypedValue((int) DxfCode.ExtendedDataAsciiString, Comment)
            };
            return list;
        }

        #endregion

        #region ctor

        public FileDataInfo(string filePath)
        {
            Database db = acadApp.DocumentManager.MdiActiveDocument.Database;
            db.XGetProjectInfo(filePath);
        }
        public FileDataInfo(Document doc)
        {
            Database db = acadApp.DocumentManager.MdiActiveDocument.Database;
            //db.GetProjectInfo(doc);
        }
        #endregion


    }
}