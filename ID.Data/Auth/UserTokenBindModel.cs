using System.ComponentModel.DataAnnotations;

namespace Intellidesk.Data.Auth
{
    public class UserAccessBindModel
    {
        [Required]
        public string UserName { get; set; }

        [Display(Name = "Role Name")]
        public string grant_type { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
    }
}