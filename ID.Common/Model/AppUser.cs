using General.Infrastructure.Core;
using General.Infrastructure.Interfaces;
using Intellidesk.Common.Commands;
using Intellidesk.Common.Enums;
using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace General.Infrastructure.Models
{
    public class AppUser : IdentityUser, IUserDetails
    {
        private static AppUser Instance;
        private AppUser()
        { }

        public static AppUser Create(IUserDetails user)
        {
            Instance = new AppUser(user);
            return Instance;
        }

        public static AppUser Get()
        {
            return Instance;
        }

        public int UserTypeId { get; set; }
        //public string PasswordHash { get; private set; }
        public bool InputInMassPermission { get; set; }

        public int OrganizationId { get; set; }
        public string OrganizationName { get; set; }

        public bool IsLogged
        {
            get { return !string.IsNullOrEmpty(SecurityStamp) && UserName != null && !string.IsNullOrEmpty(Id); }
        }

        private AppUser(IUserDetails user)
        {
            Id = user.Id;
            FirstName = user.FirstName;
            LastName = user.LastName;
            UserName = user.UserName;
            UserTypeId = user.RoleId;
            PasswordHash = user.PasswordHash;
            OrganizationName = user.OrganizationName;
            OrganizationId = user.OrganizationId;
        }

        public DateTimeOffset? LockoutEnd { get; set; }
        public string ConcurrencyStamp { get; set; }
        public virtual string PasswordSalt { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int RoleId { get; set; }
        public ICollection<IUserClaims> UserClaims { get; set; }
        public ICollection<IUserLogins> UserLogins { get; set; }
        public ICollection<IUserRoles> UserRoles { get; set; }


        //[JsonConverter(typeof(ConcreteTypeConverter<IEnumerable<UserOrganizationsRolesExtended>>))]
        private IEnumerable<IAppRole> _roles;
        public IEnumerable<IAppRole> Roles
        {
            get
            {
                if (_roles == null)
                    _roles = GeneralContext.GetClaimRoles();
                return _roles;
            }
            set
            {
                _roles = value;
            }
        }

        public static readonly AppUser Default = AppUser.Create(
                new AppUserDetails()
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000000").ToString(),
                    FirstName = "user",
                    LastName = "user",
                    UserName = CommandNames.UserGroup + "User",
                    PasswordHash = "Aa123456",
                    DefaultOrganizationId = 1
                }
            );

        public string ToStringJson()
        {
            var userData = new
            {
                Id,
                UserName,
                UserTypeId,
                OrganizationId,
                OrganizationName, // = SelectedOrganizationName.ToBase64String(),
                FirstName, // = !FirstName.Contains("base64_") ? "base64_"+FirstName.ToBase64String() : , 
                LastName // = LastName.ToBase64String() 
            };
            var result = JsonConvert.SerializeObject(userData);
            return result;
        }

        public bool IsAuthenticated
        {
            get
            {
                //var httpContext = GeneralContext.GetService<IHttpContextAccessor>().HttpContext;
                //ClaimsPrincipal currentUser = httpContext.User;
                //return currentUser.Identity.IsAuthenticated;
                return false;
            }
        }

        public bool IsPermitted(string tabName, PermissionTypes permissionType)
        {

            //var exist = this.Roles?.FirstOrDefault()?.Permissions?.
            //    Any(x => x.TabName == tabName && x.PermissionTypeId == (int)permissionType);
            //return exist ?? false;
            return false;
        }

        public int GetPermissionType(string tabName)
        {
            //var result = this.Roles?.FirstOrDefault()?.Permissions?
            //    .FirstOrDefault(x => x.TabName == tabName);
            //return result?.PermissionTypeId ?? 0;
            return 0;
        }

        public IAppRole GetRole()
        {
            var result = this.Roles?.FirstOrDefault();
            return result;
        }
    }
}
