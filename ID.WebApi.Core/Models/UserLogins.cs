using System;
using System.Collections.Generic;

namespace ID.Api.Models
{
    public partial class UserLogins
    {
        public string LoginProvider { get; set; }
        public string ProviderKey { get; set; }
        public Guid UserId { get; set; }

        public virtual Users User { get; set; }
    }
}
