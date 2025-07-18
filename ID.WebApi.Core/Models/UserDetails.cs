using General.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace General.Infrastructure.WebApi.Core.Models
{
    public partial class UserDetails : IUserDetails
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
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public int RoleId { get; set; }
        public int? DefaultOrganizationId { get; set; }
        public int? SelectedOrganizationId { get; set; }
        public string SelectedOrganizationName { get; set; }

        public virtual ICollection<UserClaims> UserClaims { get; set; }
        public virtual ICollection<UserLogins> UserLogins { get; set; }
        public virtual ICollection<UserRoles> UserRoles { get; set; }
        int IUserDetails.AccessFailedCount { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        int? IUserDetails.DefaultOrganizationId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        string IUserDetails.Email { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        bool IUserDetails.EmailConfirmed { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        string IUserDetails.FirstName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        string IUserDetails.Id { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        string IUserDetails.LastName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        bool IUserDetails.LockoutEnabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        DateTime? IUserDetails.LockoutEndDateUtc { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        string IUserDetails.PasswordHash { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        string IUserDetails.PasswordSalt { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        string IUserDetails.PhoneNumber { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        bool IUserDetails.PhoneNumberConfirmed { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        int IUserDetails.RoleId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        string IUserDetails.SecurityStamp { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        int? IUserDetails.SelectedOrganizationId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        string IUserDetails.SelectedOrganizationName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        bool IUserDetails.TwoFactorEnabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        ICollection<IUserClaims> IUserDetails.UserClaims { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        ICollection<IUserLogins> IUserDetails.UserLogins { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        string IUserDetails.UserName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        ICollection<IUserRoles> IUserDetails.UserRoles { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
