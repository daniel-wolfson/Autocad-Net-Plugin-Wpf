using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Intellidesk.Data.Repositories.Infrastructure
{
    public enum ObjectState
    {
        [Display(Name = "Unchanged")]
        Unchanged,
        [Display(Name = "New")]
        Added,
        [Display(Name = "Modified")]
        Modified,
        [Display(Name = "Deleted")]
        Deleted,
        [Display(Name = "Edit")]
        Edit
    }
}