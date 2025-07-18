using System.ComponentModel.DataAnnotations;

namespace ID.Infrastructure.Enums
{
    public enum eCabinetType
    {
        [Display(Name = "New cabinet ..."), DataInfo("Cabinet", "Cabinet", 255, 255)]
        NewCabinet = 0,

        [Display(Name = "AGC"), DataInfo("AGC", "Cabinet", 255, 255)]
        AGC = 1,

        [Display(Name = "FDH"), DataInfo("FDH", "Cabinet", 255, 255)]
        HFD = 2
    }
}