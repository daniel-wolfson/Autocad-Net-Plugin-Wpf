using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Intellidesk.Data.Models.Entities;
using Intellidesk.Data.Models.EntityMetaData;

namespace Intellidesk.Data.Models.Cad
{
    [MetadataType(typeof(ConfigMetaData))]
    public partial class Config : BaseEntity
    {
        public int ConfigId { get; set; }
        public string ConfigSetName { get; set; }
        public string ParameterName { get; set; }
        public Nullable<int> Int1 { get; set; }
        public Nullable<int> Int2 { get; set; }
        public Nullable<int> Int3 { get; set; }
        public Nullable<int> Int4 { get; set; }
        public Nullable<double> Float1 { get; set; }
        public Nullable<double> Float2 { get; set; }
        public Nullable<double> Float3 { get; set; }
        public Nullable<double> Float4 { get; set; }
        public Nullable<System.DateTime> Date1 { get; set; }
        public Nullable<System.DateTime> Date2 { get; set; }
        public Nullable<System.DateTime> Date3 { get; set; }
        public Nullable<System.DateTime> Date4 { get; set; }
        public string Str1 { get; set; }
        public string Str2 { get; set; }
        public string Str3 { get; set; }
        public string Str4 { get; set; }
        public string LongStr { get; set; }
        
        public Config()
        {
            this.LayoutOptions = new List<LayoutDictionary>();
        }

        [Browsable(false)]
        public List<LayoutDictionary> LayoutOptions { get; set; }


    }
}
