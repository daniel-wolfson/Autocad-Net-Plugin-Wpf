using System;
using System.Collections.Generic;

namespace General.Infrastructure.Interfaces
{
    public interface IUserDetails
    {
        int AccessFailedCount { get; set; }
        string Email { get; set; }
        bool EmailConfirmed { get; set; }
        string FirstName { get; }
        string Id { get; set; }
        string LastName { get; }
        bool LockoutEnabled { get; set; }
        DateTime? LockoutEndDateUtc { get; set; }
        string PasswordHash { get; set; }
        string PasswordSalt { get; set; }
        string PhoneNumber { get; set; }
        bool PhoneNumberConfirmed { get; set; }
        int RoleId { get; set; }
        string SecurityStamp { get; set; }
        bool TwoFactorEnabled { get; set; }
        ICollection<IUserClaims> UserClaims { get; set; }
        ICollection<IUserLogins> UserLogins { get; set; }
        string UserName { get; set; }
        ICollection<IUserRoles> UserRoles { get; set; }

        int OrganizationId { get; set; }
        string OrganizationName { get; set; }
    }
}