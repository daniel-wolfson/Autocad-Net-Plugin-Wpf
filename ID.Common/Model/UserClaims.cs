using General.Interfaces;
using General.Models;
using System;
using System.Collections.Generic;

namespace Mapit.WebApi.Model
{
    public partial class UserClaims : IUserClaims
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string ClaimType { get; set; }
        public string ClaimValue { get; set; }

        public virtual IUserDetails User { get; set; }
    }
}
