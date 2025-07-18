using System.ComponentModel.DataAnnotations;

namespace Intellidesk.Data.Auth
{
    public class UserRegisterBindModel : UserData
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}