using System.ComponentModel.DataAnnotations;

namespace Intellidesk.Common.Enums
{
    public enum eTitleType
    {
        [Display(Name = "Add title type..."), DataInfo("0", 255, 7)]
        NewTitle = 0,

        [Display(Name = "TitleDefault"), DataInfo("Default", 2, 7)]
        Default = 1,
    }
}