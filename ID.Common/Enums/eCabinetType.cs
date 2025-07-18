using System.ComponentModel.DataAnnotations;

namespace Intellidesk.Common.Enums
{
    public enum eCabinetType
    {
        [Display(Name = "New cabinet ..."), DataInfo("Cabinet", 255, 255)]
        NewCabinet = 0,

        [Display(Name = "AGC"), DataInfo("CabinetAGC", 255, 255)]
        AGC = 1,

        [Display(Name = "FDH"), DataInfo("CabinetFDH", 255, 255)]
        HFD = 2
    }
}