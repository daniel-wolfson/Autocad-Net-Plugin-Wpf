using System;
using System.Collections.Generic;

namespace ID.WebApi.Models.Identity
{
    public partial class AspNetUserLogins
    {
        public string UserId { get; set; }
        public string LoginProvider { get; set; }
        public string ProviderKey { get; set; }

        public virtual AspNetUsers User { get; set; }
    }
}
