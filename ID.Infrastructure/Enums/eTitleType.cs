using System.ComponentModel.DataAnnotations;

namespace ID.Infrastructure.Enums
{
    public enum eTitleType
    {
        [Display(Name = "Add title type..."), DataInfo("NewTitle", "Title", 255, 7)]
        NewTitle = 0,

        [Display(Name = "TitleDefault"), DataInfo("Default", "Title", 2, 7)]
        Default = 1,
    }
}