using System;
using System.Collections.Generic;

namespace ID.Api.Models
{
    public partial class Permissions
    {
        public int PermissionTypeId { get; set; }
        public string PermissionTypeName { get; set; }
        public DateTime UpdateDate { get; set; }
        public Guid UpdateUserId { get; set; }
        public int Status { get; set; }
    }
}
