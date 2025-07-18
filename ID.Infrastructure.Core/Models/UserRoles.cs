using System;
using System.Collections.Generic;

namespace General.Api.Models
{
    public partial class UserRoles
    {
        public string RoleId { get; set; }
        public Guid UserId { get; set; }

        public virtual Roles Role { get; set; }
        public virtual Users User { get; set; }
    }
}
