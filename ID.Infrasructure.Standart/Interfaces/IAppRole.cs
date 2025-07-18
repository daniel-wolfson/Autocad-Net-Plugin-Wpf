using System;

namespace ID.Infrastructure.Interfaces
{
    public interface IAppRole
    {
        string Description { get; set; }
        int RoleId { get; set; }
        string RoleName { get; set; }
        DateTime? UpdateDate { get; set; }
    }
}