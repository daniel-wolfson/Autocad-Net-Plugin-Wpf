using ID.Infrastructure.Interfaces;
using Intellidesk.AcadNet.Data.Common.Editors;
using Intellidesk.Data.General;
using Intellidesk.Data.Models.Entities;
using Intellidesk.Data.Models.EntityMetaData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Closure = Intellidesk.Data.Models.Entities.Closure;
using MultiSelectComboBoxEditor = Intellidesk.Data.Common.Editors.MultiSelectComboBoxEditor;

namespace Intellidesk.Data.Models.Cad
{
    [MetadataType(typeof(LayoutMetaData))]
    public class ILayout : BaseEntity, IDataErrorInfo
    {
        public ILayout()
        {
            this.Closures = new List<Closure>();
            this.Blocks = new List<BlockDefinition>();
            this.BlockItems = new List<BlockRef>();
            this.UserSettings = new List<UserSetting>();

            ValidationHandler = new ValidationHandler();

            WorkItems = new List<IWorkItem>();
        }

        public ILayout(string layoutName) : this()
        {
            LayoutName = layoutName;
        }

        public ILayout(string layoutName, IWorkItem item) : this(layoutName)
        {
            if (item != null)
            {
                WorkItems = new List<IWorkItem>
                {
                    new WorkItem(layoutName, "CommandArgs", item.Work)
                };
            }
        }

        public ILayout(string layoutName, List<IWorkItem> items) : this(layoutName, (WorkItem)null)
        {
            if (items != null && items.Any())
            {
                WorkItems = items;
            }
        }

        #region "Properties"

        [Browsable(false)]
        public decimal LayoutID { get; set; }

        [JsonProperty("WorkItems")]
        public List<IWorkItem> WorkItems { get; set; }

        private string _layoutName = "new layout";
        public string LayoutName
        {
            get { return _layoutName; }
            set { Set(ref _layoutName, value); }
        }

        private string _layoutType = "none";
        public string LayoutType
        {
            get { return _layoutType; }
            set { Set(ref _layoutType, value); }
        }

        private string _accessType = "none";
        //[ItemsSource(typeof(AccessTypeItemsSource))]
        public string AccessType
        {
            get { return _accessType; }
            set { Set(ref _accessType, value); }
        }

        private string _comment = "none";
        public string Comment
        {
            get { return _comment; }
            set { Set(ref _comment, value); }
        }

        private string _layoutContents = "none";
        [Editor(typeof(MultiSelectComboBoxEditor), typeof(MultiSelectComboBoxEditor))]
        public string LayoutContents
        {
            get { return _layoutContents; }
            set { Set(ref _comment, value); }
        }

        private string _layoutVersion = "new";
        public string LayoutVersion
        {
            get { return _layoutVersion; }
            set { Set(ref _layoutVersion, value); }
        }

        private string _siteName = "none";
        public string SiteName
        {
            get { return _siteName; }
            set { Set(ref _siteName, value); }
        }

        private string _buildingLevels = "none";
        [Editor(typeof(MultiSelectComboBoxEditor), typeof(MultiSelectComboBoxEditor))]
        public string BuildingLevels
        {
            get { return _buildingLevels; }
            set { Set(ref _buildingLevels, value); }
        }


        private string _cadFileName = "noname.dwg";
        [Category("Files"), DisplayName("File name Dwg")]
        [Description("Path by default there is into work directory of project")]
        [Editor(typeof(FileNameEditor), typeof(FileNameEditor))]
        public string CADFileName
        {
            get { return _cadFileName; }
            set { Set(ref _cadFileName, value); }
        }


        private string _tabFileName = "noname.tab";
        [Category("Files"), DisplayName("File name Tab")]
        [Description("Path by default there is into work directory of project")]
        [Editor(typeof(FileNameEditor), typeof(FileNameEditor))]
        public string TABFileName
        {
            get { return _tabFileName; }
            set { Set(ref _tabFileName, value); }
        }

        private int _createdBy;
        public int CreatedBy
        {
            get { return _createdBy; }
            set { Set(ref _createdBy, value); }
        }

        private DateTime? _dateCreated = DateTime.Now;
        public Nullable<DateTime> DateCreated
        {
            get { return _dateCreated; }
            set { Set(ref _dateCreated, value); }
        }

        private int _modifiedBy; //Environment.UserName;
        public int ModifiedBy
        {
            get { return _modifiedBy; }
            set { Set(ref _modifiedBy, value); }
        }

        private DateTime? _dateModified = DateTime.Now;
        public DateTime? DateModified
        {
            get { return _dateModified; }
            set { Set(ref _dateModified, value); }
        }

        public short? LayoutState { get; set; }

        private bool _visible;
        public bool Visible
        {
            get { return _visible; }
            set { Set(ref _visible, value); }
        }

        public bool FSA { get; set; }

        #endregion

        #region "Processes"

        public string ProcessName1 { get; set; }
        public string ProcessName2 { get; set; }
        public string ProcessName3 { get; set; }
        public string ProcessName4 { get; set; }

        #endregion

        #region "Parameters"

        public string ConfigSetName { get; set; }
        public string Param1 { get; set; }
        public string Param2 { get; set; }
        public string Param3 { get; set; }
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
        public virtual ICollection<BlockDefinition> Blocks { get; set; }


        [Browsable(false)]
        public virtual ICollection<BlockRef> BlockItems { get; set; }

        [Browsable(false)]
        public virtual ICollection<Cabinet> Cabinets { get; set; }

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
                //var info = this.GetType().GetProperty(propertyName);
                //if (info != null)
                //{
                //    var value= info.GetValue(this, null) as string;
                //    if (string.IsNullOrEmpty(value))
                //        return string.Format("{0} has to be set", info.Name);
                //        return string.Format("{0}'s length has to be at least 5 characters !", info.Name);
                //}
                //return null;
            }
        }

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
                    Mouse.OverrideCursor = Cursors.Wait;

                    result = ValidationHandler.ValidateRule("CADFileName", "file not found",
                            () => ValidationHandler.FindFileRuleAsync(CADFileName, "dwg", out driveName) == "");

                    //result = TryFindFileAsync(CADFileName, "dwg", out driveName);

                    //var userSetting = UserSetting.Load();
                    //if (driveName != "") userSetting.Drive = driveName;
                    //userSetting.Save();

                    Mouse.OverrideCursor = null;

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

        #endregion
    }
}
