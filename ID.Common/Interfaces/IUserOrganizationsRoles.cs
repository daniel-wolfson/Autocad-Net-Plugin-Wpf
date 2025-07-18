using System.Collections.Generic;

namespace General.Infrastructure.Interfaces
{
    public interface IUserOrganizationsRole
    {
        int UserId { get; set; }
        int OrganizationId { get; set; }
        string OrganizationName { get; set; }
        int RoleId { get; set; }
        string RoleName { get; set; }

        IEnumerable<IAppPermissionsInfo> Permissions { get; set; }
    }
}
