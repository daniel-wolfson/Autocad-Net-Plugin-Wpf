using ID.Infrastructure.Interfaces;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;

namespace ID.Infrastructure.Models
{
    public class AppUserDetails : IdentityUser, IAppUser
    {

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int OrganizationId { get; set; }
        public string OrganizationName { get; set; }

        public int RoleId { get; set; }
        public string PasswordSalt { get; set; }

        //public ICollection<IUserClaims> UserClaims { get; set; }
        //public ICollection<IUserLogins> UserLogins { get; set; }
        //public ICollection<IUserRoles> UserRoles { get; set; }

        public Guid? OrgId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool IsLogged => throw new NotImplementedException();

        public DateTimeOffset? LockoutEnd { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string ConcurrencyStamp { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string NormalizedEmail { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string NormalizedUserName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool IsAuthenticated => throw new NotImplementedException();

        Guid IAppUser.Id { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        IEnumerable<IUserOrganizationsRole> IAppUser.Roles => throw new NotImplementedException();

        public object ToStringJson()
        {
            return new { FirstName, Email, Id, LastName, PhoneNumber, RoleId, SecurityStamp };
        }

        string IAppUser.ToStringJson()
        {
            throw new NotImplementedException();
        }
    }
}
