using System;
using System.Collections.Generic;

namespace ID.Api.Models
{
    public partial class UserClaims
    {
        public int Id { get; set; }
        public string ClaimType { get; set; }
        public string ClaimValue { get; set; }
        public Guid UserId { get; set; }

        public virtual Users User { get; set; }
    }
}
