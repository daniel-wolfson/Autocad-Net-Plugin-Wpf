using System;
using System.Collections.Generic;

namespace ID.Interfaces
{
    public interface IAppRole
    {
        int UserId { get; set; }
        int OrganizationId { get; set; }
        string OrganizationName { get; set; }
        int RoleId { get; set; }
        string RoleName { get; set; }
        DateTime? UpdateDate { get; set; }
        string Description { get; set; }

        IEnumerable<IAppPermissionsInfo> Permissions { get; set; }
    }
}