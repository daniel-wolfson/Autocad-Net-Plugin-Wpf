using General.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace General.Models
{
    public class UserDetails : IUserDetails
    {
        public UserDetails()
        {
            UserClaims = new HashSet<IUserClaims>();
            UserLogins = new HashSet<IUserLogins>();
            UserRoles = new HashSet<IUserRoles>();
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
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public int RoleId { get; set; }
        public int? DefaultOrganizationId { get; set; }
        public int? SelectedOrganizationId { get; set; }
        public string SelectedOrganizationName { get; set; }

        public virtual ICollection<IUserClaims> UserClaims { get; set; }
        public virtual ICollection<IUserLogins> UserLogins { get; set; }
        public virtual ICollection<IUserRoles> UserRoles { get; set; }
    }
}
