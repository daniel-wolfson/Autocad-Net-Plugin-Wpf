using System;
using System.Collections.Generic;

namespace ID.Infrastructure.Interfaces
{
    public interface IUserDetails
    {
        int AccessFailedCount { get; set; }
        string Email { get; set; }
        bool EmailConfirmed { get; set; }
        string FirstName { get; set; }
        Guid Id { get; set; }
        string LastName { get; set; }
        bool LockoutEnabled { get; set; }
        DateTime? LockoutEndDateUtc { get; set; }
        Guid? OrgId { get; set; }
        string PasswordHash { get; set; }
        string PasswordSalt { get; set; }
        string PhoneNumber { get; set; }
        bool PhoneNumberConfirmed { get; set; }
        string SecurityStamp { get; set; }
        bool TwoFactorEnabled { get; set; }
        ICollection<IUserClaims> UserClaims { get; set; }
        ICollection<IUserLogins> UserLogins { get; set; }
        string UserName { get; set; }
        ICollection<IUserRoles> UserRoles { get; set; }
    }
}