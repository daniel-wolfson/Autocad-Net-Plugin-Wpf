using ID.Infrastructure.Interfaces;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;

namespace AspNet.Identity.PostgreSQL
{
    /// <summary>
    /// Class that implements the ASP.NET Identity IUser interface.
    /// </summary>
    public class IdentityUserDetails : IdentityUser, IAppUser, IUser
    {
        /// <summary>
        /// Default constructor. 
        /// </summary>
        public IdentityUserDetails()
        {
            this.Id = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Constructor that takes user name as argument.
        /// </summary>
        /// <param name="userName"></param>
        public IdentityUserDetails(string userName)
            : this()
        {
            this.UserName = userName;
        }

        public IdentityUserDetails(IAppUser userDetails)
            : this()
        {
            this.AccessFailedCount = userDetails.AccessFailedCount;
            this.Email = userDetails.Email;
            this.EmailConfirmed = userDetails.EmailConfirmed;
            this.FirstName = userDetails.FirstName;
            //this.Id = userDetails.Id;
            this.LastName = userDetails.LastName;
            this.LockoutEnabled = userDetails.LockoutEnabled;
            //this.LockoutEndDateUtc = userDetails.LockoutEndDateUtc;
            //this.OrganizationId = userDetails.OrganizationId;
            //this.OrganizationName = userDetails.OrganizationName;
            //this.OrganizationName = userDetails.OrganizationName;
            this.PasswordHash = userDetails.PasswordHash;
            //this.PasswordSalt = userDetails.PasswordSalt;
            this.PhoneNumber = userDetails.PhoneNumber;
            this.PhoneNumberConfirmed = userDetails.PhoneNumberConfirmed;
            //this.RoleId = userDetails.RoleId;
            this.SecurityStamp = userDetails.SecurityStamp;
            //this.UserClaims = userDetails.UserClaims;
        }

        public string FirstName { get; }

        public string LastName { get; }

        public string PasswordSalt { get; set; }
        public int RoleId { get; set; }
        //public ICollection<IUserClaims> UserClaims { get; set; }
        //public ICollection<IUserLogins> UserLogins { get; set; }
        //public ICollection<IUserRoles> UserRoles { get; set; }
        public int OrganizationId { get; set; }
        public string OrganizationName { get; set; }

        /// TODO:
        public Guid? OrgId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool IsLogged => throw new NotImplementedException();
        public DateTimeOffset? LockoutEnd { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string ConcurrencyStamp { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string NormalizedEmail { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string NormalizedUserName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool IsAuthenticated => throw new NotImplementedException();

        string IAppUser.FirstName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        string IAppUser.LastName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        Guid IAppUser.Id { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        IEnumerable<IUserOrganizationsRole> IAppUser.Roles => throw new NotImplementedException();

        public object ToStringJson()
        {
            throw new NotImplementedException();
        }

        string IAppUser.ToStringJson()
        {
            throw new NotImplementedException();
        }
    }
}
