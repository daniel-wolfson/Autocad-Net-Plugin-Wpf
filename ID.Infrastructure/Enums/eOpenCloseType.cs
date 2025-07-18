using System.ComponentModel.DataAnnotations;

namespace ID.Infrastructure.Enums
{
    public enum eOpenCloseType
    {
        [Display(Name = "Open"), DataInfo("Open", "BUILDING PARTNER", -1, 7)]
        Open = 0,

        [Display(Name = "Close"), DataInfo("Close", "CLOSURE BEZEK", 3, 7)]
        Close = 1
    }
}