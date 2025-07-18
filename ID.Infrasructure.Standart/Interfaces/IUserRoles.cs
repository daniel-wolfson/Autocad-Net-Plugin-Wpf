using ID.Infrastructure.Models;

namespace ID.Infrastructure.Interfaces
{
    public interface IUserRoles
    {
        string RoleId { get; set; }
        Role Role { get; set; }

        string UserId { get; set; }
        IUserDetails User { get; set; }

    }
}