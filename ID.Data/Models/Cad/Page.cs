using System;

namespace Intellidesk.Data.Models
{
    public partial class Page
    {
        public int ID_Page { get; set; }
        public string Title { get; set; }
        public string FullName { get; set; }
        public string URL { get; set; }
        public string AttributeURL { get; set; }
        public string Tooltip { get; set; }
        public bool isActive { get; set; }
        public bool isPrinted { get; set; }
        public Nullable<int> Ordered { get; set; }
        public int LevelAgent { get; set; }
        public string ID_System { get; set; }
    }
}
