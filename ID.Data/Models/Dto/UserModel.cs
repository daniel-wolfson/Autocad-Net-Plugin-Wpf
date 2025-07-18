using System.ComponentModel.DataAnnotations;

namespace Intellidesk.Data.Models.Dto
{
    public class UserLoginModel
    {
        [Key]
        [Required]
        public string Username { get; set; }

        [Required]
        [StringLength(8)]
        public string Password { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public int Role { get; set; } // role = 1 is candidate

        public UserLoginModel()
        { }

        public UserLoginModel(string username, string email, string password, int role)
        {
            Username = username;
            Email = email;
            Password = password;
            Role = role;
        }
    }
}
