using System.ComponentModel.DataAnnotations;

namespace Intellidesk.Data.Auth
{
    public class UserData : UserAccessBindModel
    {
        //[Key]
        //[ScaffoldColumn(false)]
        //public string Id { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
    }
}