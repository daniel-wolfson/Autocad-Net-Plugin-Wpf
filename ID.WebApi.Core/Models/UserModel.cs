using System.ComponentModel.DataAnnotations;

namespace ID.Api.Models
{
    public class UserModel
    {
        //[Key]
        //[Range(100000000, 999999999)]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(6)]
        public string PasswordHash { get; set; }

        //public int Role { get; set; } // role = 1 is candidate

        //public UserModel()
        //{ }
    }
}

