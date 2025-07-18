using Autodesk.AutoCAD.DatabaseServices;
using Intellidesk.AcadNet.Common.Core;
using Intellidesk.AcadNet.Common.Internal;
using System.Collections.Generic;
using System.ComponentModel;

namespace FileExplorer.ViewModel
{
    public class LayoutInfoDicViewModel : BaseViewModel, IDataErrorInfo
    {

        Dictionary<DxfCode, object> data = new Dictionary<DxfCode, object>();
        public IDictionary<DxfCode, object> Data
        {
            get { return this.data; }
        }

        private KeyValuePair<DxfCode, object>? selectedKey = null;
        public KeyValuePair<DxfCode, object>? SelectedKey
        {
            get { return this.selectedKey; }
            set
            {
                this.selectedKey = value;
                this.OnPropertyChanged("SelectedKey");
                this.OnPropertyChanged("SelectedValue");
            }
        }

        public object SelectedValue
        {
            get
            {
                if (null == this.SelectedKey)
                {
                    return string.Empty;
                }

                return this.data[this.SelectedKey.Value.Key];
            }
            set
            {
                this.data[this.SelectedKey.Value.Key] = value;
                this.OnPropertyChanged("SelectedValue");
            }
        }

        public bool IsValid { get; set; }

        public LayoutInfoDicViewModel()
        {
            this.data.Add(DxfCode.UcsOrg, "ITM");
            var extents = Geoms.Extents();
            this.data.Add(DxfCode.ExtendedDataWorldXCoordinate, extents.MinPoint);
            this.data.Add(DxfCode.ExtendedDataWorldYCoordinate, extents.MaxPoint);
        }

        #region IDataErrorInfo Members

        public string this[string propertyName]
        {
            get
            {
                return null;
            }
        }

        public string Error
        {
            get { return (IsValid) ? "Error" : ""; }
        }

        #endregion "IDataErrorInfo Members"
        public bool IsActive { get; set; }
        //public event EventHandler IsActiveChanged;
    }
}
