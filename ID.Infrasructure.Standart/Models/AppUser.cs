using ID.Infrastructure.Core;
using ID.Infrastructure.Extensions;
using ID.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace ID.Infrastructure.Models
{
    public class AppUser : IdentityUser<Guid>, IAppUser
    {
        private static AppUser instance;

        private AppUser() { }

        public static AppUser Create(IUserDetails user)
        {
            instance = new AppUser(user);
            return instance;
        }

        public static AppUser Get()
        {
            return instance;
        }

        public bool IsLogged
        {
            get { return !string.IsNullOrEmpty(SecurityStamp) && UserName != null && !Id.IsGuidEmpty(); }
        }

        private AppUser(IUserDetails user)
        {
            Id = user.Id;
            FirstName = user.FirstName;
            LastName = user.LastName;
            UserName = user.UserName;
            PasswordHash = user.PasswordHash;
            OrgId = user.OrgId;
        }

        public bool IsAuthenticated
        {
            get
            {
                var httpContext = GeneralContext.HttpContext;
                ClaimsPrincipal currentUser = httpContext.User;
                return currentUser.Identity.IsAuthenticated;
            }
        }

        public int UserId { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UnitGuid { get; private set; }
        public string UnitName { get; private set; }
        public string Units { get; set; }
        public string FullName { get; set; }
        public string UserTypeName { get; set; }
        public int RoleId { get; set; }
        public Guid? OrgId { get; set; }

        public IEnumerable<IUserOrganizationsRole> Roles => throw new NotImplementedException();

        Guid IAppUser.Id { get; set; }

        public string ToStringJson()
        {
            var userData = new
            {
                UserGuid = Id,
                UserId,
                UnitGuid,
                UserName,
                FirstName,
                LastName,
                UserFullName = FullName,
                RoleId,
            };
            var result = JsonConvert.SerializeObject(userData);
            return result;

        }

        public static readonly AppUser Default = new AppUser()
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000000"),
            FirstName = "Noname",
            LastName = "Noname",
            UserName = "admineltel",
            PasswordHash = "Gkuser001",
            OrgId = null
        };
    }
}
