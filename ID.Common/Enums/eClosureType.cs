using System.ComponentModel.DataAnnotations;

namespace Intellidesk.Common.Enums
{
    public enum eClosureType
    {
        [Display(Name = "Add closure type..."), DataInfo("0", 255, 7)]
        NewClosure = 0,

        [Display(Name = "CL"), DataInfo("Closure", 2, 7)]
        Cl = 1
    }
}