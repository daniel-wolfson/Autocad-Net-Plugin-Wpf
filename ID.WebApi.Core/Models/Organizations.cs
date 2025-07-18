using System;
using System.Collections.Generic;

namespace ID.Api.Models
{
    public partial class Organizations
    {
        public Guid OrgId { get; set; }
        public string OrgName { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
