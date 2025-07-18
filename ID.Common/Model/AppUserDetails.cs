using General.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;

namespace General.Infrastructure.Models
{
    public class AppUserDetails : IUserDetails
    {
        public int AccessFailedCount { get; set; }
        public int? DefaultOrganizationId { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string FirstName { get; set; }
        public string Id { get; set; }
        public string LastName { get; set; }
        public bool LockoutEnabled { get; set; }
        public DateTime? LockoutEndDateUtc { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public int RoleId { get; set; }
        public string SecurityStamp { get; set; }
        public int? OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public ICollection<IUserClaims> UserClaims { get; set; }
        public ICollection<IUserLogins> UserLogins { get; set; }
        public string UserName { get; set; }
        public ICollection<IUserRoles> UserRoles { get; set; }
        int IUserDetails.OrganizationId { get; set; }
    }
}
