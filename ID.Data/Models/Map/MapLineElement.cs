using System.ComponentModel;

namespace Intellidesk.Data.Models.Map
{
    public class MapLineElement : MapElement
    {
        [DisplayName("Len")]
        public double Length { get; set; }

        [DisplayName("Koter")]
        public string Diameter { get; set; }

        [DisplayName("Kamut")]
        public string FiberCount { get; set; }

        public string Owner { get; set; }

        [DisplayName("Shem_file")]
        public string FileName { get; set; }

        [DisplayName("Date")]
        public string EffecDate { get; set; }

        [DisplayName("Kablan")]
        public string Contractor { get; set; }

        public MapLineElement() : base(1, "KavHafira")
        {
        }

    }
}