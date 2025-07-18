using General.Interfaces;

namespace Mapit.WebApi.Model
{
    public class UserRoles : IUserRoles
    {
        public string UserId { get; set; }
        public string RoleId { get; set; }

        public virtual IRoles Role { get; set; }
        public virtual IUserDetails User { get; set; }
    }
}
