using System;
using System.Collections.Generic;

namespace ID.Infrastructure.Interfaces
{
    public interface IUserDetails
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string PasswordHash { get; set; }
        public string SecurityStamp { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }

        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public bool LockoutEnabled { get; set; }
        public string PasswordSalt { get; set; }

        public ICollection<IUserClaims> AspNetUserClaims { get; set; }
        public ICollection<IUserRoles> AspNetUserRoles { get; set; }
    }
}