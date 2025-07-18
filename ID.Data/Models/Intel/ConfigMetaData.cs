using System.Collections.Generic;
using System.ComponentModel;
using Intellidesk.Data.Models.Cad;

namespace Intellidesk.Data.Models.EntityMetaData
{
    public class ConfigMetaData : BaseEntity
    {
        [Category("Generic")]
        [DisplayName("ConfigSet Name")]
        public string ConfigSetName { get; set; }

        [Category("Generic")]
        [DisplayName("Int1")]
        public int? Int1 { get; set; }

        [Category("Generic")]
        [DisplayName("Str1")]
        public string Str1 { get; set; }

        [Category("Generic")]
        [DisplayName("Str2")]
        public string Str2 { get; set; }

        [Category("Generic")]
        [DisplayName("LongStr")]
        public string LongStr { get; set; }

        [Category("Generic")]
        [DisplayName("Layout Options")]
        public List<LayoutDictionary> LayoutOptions { get; set; }
    }
}