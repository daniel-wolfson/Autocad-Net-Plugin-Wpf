//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Intellidesk.AcadNet.Data.Models.Intel
{
    using System;
    using System.Collections.Generic;
    
    public partial class LO_Bay
    {
        public decimal LayoutID { get; set; }
        public string BayName { get; set; }
        public string BaySide { get; set; }
        public string BayPart { get; set; }
        public double Xmin { get; set; }
        public double Ymin { get; set; }
        public double Xmax { get; set; }
        public double Ymax { get; set; }
    
        public virtual LO_Layout LO_Layouts { get; set; }
    }
}
