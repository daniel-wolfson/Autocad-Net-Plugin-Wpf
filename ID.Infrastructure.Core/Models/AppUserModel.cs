using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ID.Infrastructure.Models
{
    public class AppUserModel
    {
        public AppUserModel()
        {

        }
        public AppUserModel(AppUserModel user) : this()
        {
            this.DefaultOrganizationId = user.DefaultOrganizationId;
            this.FirstName = user.FirstName;
            this.InputInMassPermission = user.InputInMassPermission;
            this.LastName = user.LastName;
            this.Password = user.Password;
            this.UserId = user.UserId;
            this.UserName = user.UserName;
            this.UserTypeId = user.UserTypeId;
        }

        [Key]
        [Column(Order = 0)]
        //[DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int UserId { get; set; }

        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int UserTypeId { get; set; }
        public int? DefaultOrganizationId { get; set; }
        public int? SelectedOrganizationId { get; set; }
        public string SelectedOrganizationName { get; set; }
        public bool InputInMassPermission { get; set; } //only tms
    }
}
