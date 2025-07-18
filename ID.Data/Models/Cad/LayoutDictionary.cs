using System.ComponentModel;
using Intellidesk.Data.Models.Entities;

namespace Intellidesk.Data.Models.Cad
{
    public class LayoutDictionary : BaseEntity
    {
        private string _parameterName;
        private string _key;
        private string _value;

        [Browsable(false)]
        public string LayoutDicId { get; set; }

        [Category("Generic")]
        [DisplayName("Config Set Name"), ReadOnly(true)]
        public string ConfigSetName { get; set; }

        [Category("Generic")]
        [DisplayName("Parameter Name")]
        public string ParameterName
        {
            get { return _parameterName; }
            set { Set(ref _parameterName, value); }
        }

        [Category("Generic")]
        [DisplayName("Key")]
        public string Key
        {
            get { return _key; }
            set
            { Set(ref _key, value); }
        }

        [Category("Generic")]
        [DisplayName("Value")]
        public string Value
        {
            get { return _value; }
            set
            { Set(ref _value, value); }
        }
        //public int Config_ConfigID { get; set; }
        //public virtual Config Config { get; set; }
    }
}
