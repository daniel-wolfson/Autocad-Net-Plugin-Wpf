using System.ComponentModel;

namespace Intellidesk.Data.Models.Map
{
    public class MapMarkerElement : MapElement
    {
        [DisplayName("Data_Search")]
        public string ElementName { get; set; }

        [DisplayName("As_made_nu")]
        public string FolderName { get; set; }

        [DisplayName("Shem_autoc")]
        public string FileName { get; set; }

        public MapMarkerElement() : base(0, "GovPoint")
        {
        }
    }
}