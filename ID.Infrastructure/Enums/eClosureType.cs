using System.ComponentModel.DataAnnotations;

namespace ID.Infrastructure.Enums
{
    public enum eClosureType
    {
        [Display(Name = "Add closure type..."), DataInfo("NewClosure", "0", 255, 7)]
        NewClosure = 0,

        [Display(Name = "CL"), DataInfo("Cl", "Closure", 2, 7)]
        Cl = 1
    }
}