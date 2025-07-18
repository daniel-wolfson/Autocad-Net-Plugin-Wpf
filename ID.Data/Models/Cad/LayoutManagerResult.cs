using System;

namespace Intellidesk.Data.Models.Cad
{
    public partial class LayoutManagerResult
    {
        public string LAYOUT_COMMAND { get; set; }
        public Nullable<int> LAYOUT_ID { get; set; }
        public Nullable<int> RETURN_CODE { get; set; }
        public string RETURN_MESSAGE { get; set; }
    }
}