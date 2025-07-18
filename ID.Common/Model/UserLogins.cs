using General.Interfaces;
using System;
using System.Collections.Generic;

namespace Mapit.WebApi.Model
{
    public partial class UserLogins : IUserLogins
    {
        public string LoginProvider { get; set; }
        public string ProviderKey { get; set; }
        public string UserId { get; set; }

        public virtual IUserDetails User { get; set; }
    }
}
