using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;

namespace ID.Infrastructure.Interfaces
{
    public interface IUserDetails1 : IUser
    {
        int AccessFailedCount { get; set; }
        string Email { get; set; }
        bool EmailConfirmed { get; set; }
        string FirstName { get; }
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

        ICollection<IUserClaims1> UserClaims { get; set; }
        ICollection<IUserLogins1> UserLogins { get; set; }
        ICollection<IUserRoles1> UserRoles { get; set; }

        int OrganizationId { get; set; }
        string OrganizationName { get; set; }

        object ToStringJson();
    }
}