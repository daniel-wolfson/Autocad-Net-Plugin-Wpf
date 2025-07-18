using General.Infrastructure.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace General.Infrastructure.Models
{
    public class UserDetails : IUserDetails
    {
        public UserDetails()
        {
            UserClaims = new HashSet<UserClaims>();
            UserLogins = new HashSet<UserLogins>();
            UserRoles = new HashSet<UserRoles>();
        }
        [Required]
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        [Required]
        public string PasswordHash { get; set; }
        [Required]
        public string PasswordSalt { get; set; }
        public string SecurityStamp { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public DateTime? LockoutEndDateUtc { get; set; }
        public bool LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }
        [Required]
        public string UserName { get; set; }
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public int RoleId { get; set; }
        public Guid? OrgId { get; set; }

        //bool? EmailConfirmed { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        //bool? LockoutEnabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        //bool? IUserDetails.PhoneNumberConfirmed { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        //int? IUserDetails.RoleId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        //bool? IUserDetails.TwoFactorEnabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public virtual ICollection<UserClaims> UserClaims { get; set; }
        public virtual ICollection<UserLogins> UserLogins { get; set; }
        public virtual ICollection<UserRoles> UserRoles { get; set; }

    }
}
