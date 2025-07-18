using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Windows.Input;
using System.Xml.Serialization;
using AcadNet.Data.Editors;
using AcadNet.Tools;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using NetTools;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace AcadNet.Data.Models.Gis
{
    public partial class TabContext
    {
        public TabContext(string connectionString) //: base(connectionString)
        {
        }

        //public static List<LO_Item> TabItems = new List<LO_Item>();


        public class Tab : BaseLayout, INotifyPropertyChanged, IDataErrorInfo
        {
            public static Tab MakeLayoutSample()
            {
                var newTab = new Tab()
                {
                    CreatedBy = Environment.UserName,
                    DateCreated = DateTime.Now,
                    DateModified = DateTime.Now,
                    FSA = false,
                    TabID = 42,
                    TabName = "Demo layout",
                    LayoutState = null,
                    ModifiedBy = Environment.UserName,
                    CADFileName = @"\Dwg\KBIE3-OV-ww42.3.dwg",
                    LayoutVersion = "..."
                };
                return newTab;
            }

            public Tab()
            {
            }

            public Tab(string Name, string UserName, string Pub, string LayoutID)
            {
            }

            [Browsable(false)]
            public decimal TabID { get; set; }

            [Category("Generic"), Browsable(true), DisplayName("FSA Uploaded"), ReadOnly(true)]
            public bool FSA { get; set; }

            private string _tabName;

            [Category("Generic"), DisplayName("Layout name")]
            [Required(ErrorMessage = "Layout name not must be empty", AllowEmptyStrings = false)]
            [StringLength(500, ErrorMessage = "Layout name must be 500 characters or less")]
            public string TabName
            {
                get { return _tabName; }
                set
                {
                    _tabName = value ?? "new tab";
                    OnPropertyChanged(); //() => TabName
                }
            }

            
        }

        public class TabDictionary : INotifyPropertyChanged
        {
            private string _parameterName;
            private object _key;
            private string _value;

            [Category("Generic")]
            [DisplayName("Config Set Name"), ReadOnly(true)]
            public string ConfigSetName { get; set; }

            [Category("Generic")]
            [DisplayName("Parameter Name")]
            public string ParameterName
            {
                get { return _parameterName; }
                set
                {
                    _parameterName = value;
                    OnPropertyChanged("ParameterName");
                }
            }

            [Category("Generic")]
            [DisplayName("Key")]
            public object Key
            {
                get { return _key; }
                set
                {
                    _key = value;
                    OnPropertyChanged("Str1");
                }
            }

            [Category("Generic")]
            [DisplayName("Value")]
            public string Value
            {
                get { return _value; }
                set
                {
                    _value = value;
                    OnPropertyChanged("Str2");
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged(string propertyName)
            {
                var handler = PropertyChanged;
                if (handler != null)
                    handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
