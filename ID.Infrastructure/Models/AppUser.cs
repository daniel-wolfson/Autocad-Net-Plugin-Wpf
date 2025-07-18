using ID.Infrastructure.Interfaces;
using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Security.Principal;

namespace ID.Infrastructure.Models
{
    public class AppUser : IdentityUser, IAppUser
    {
        private static AppUser instance;

        //public List<RolePermissionsInfo> RolePermissions { get; set; }
        //public List<RoleItems> RoleItems { get; set; }

        private AppUser()
        { }

        public static AppUser Create(IAppUser user)
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
            get { return !string.IsNullOrEmpty(SecurityStamp) && UserName != null; }
        }

        private AppUser(IAppUser user)
        {
            //Id = user.UserGuid;
            //FirstName = user.UserFirstName;
            //LastName = user.UserLastName;
            //UserName = user.UserName;
            //PasswordHash = user.Password;
            //UnitGuid = user.UnitGuid;
            //UnitName = user.UnitName;
            //RoleId = user.RoleId;
            SecurityStamp = Guid.NewGuid().ToString();

            //RolePermissions = user.RolePermissions;
            //RoleItems = user.RoleItems;
        }

        public bool IsAuthenticated
        {
            get
            {
                var httpContext = GeneralContext.HttpContext;
                IPrincipal currentUser = httpContext.User;
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
        public Guid? OrgId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DateTimeOffset? LockoutEnd { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string ConcurrencyStamp { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string NormalizedEmail { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string NormalizedUserName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        Guid IAppUser.Id { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        IEnumerable<IUserOrganizationsRole> IAppUser.Roles => throw new NotImplementedException();

        public string ToStringJson()
        {
            var userData = new
            {
                UserGuid = Id,
                UserId,
                UnitGuid,
                UserName,
                FirstName,
                LastName, // .ToBase64String(),
                UserFullName = FullName,
                RoleId,
            };
            var result = JsonConvert.SerializeObject(userData);
            return result;

        }

        public object this[string propertyName]
        {
            get
            {
                PropertyInfo property = GetType().GetProperty(propertyName);
                return property.GetValue(this, null);
            }
            set
            {
                PropertyInfo property = GetType().GetProperty(propertyName);
                Type propType = property.PropertyType;
                if (value == null)
                {
                    if (propType.IsValueType && Nullable.GetUnderlyingType(propType) == null)
                        throw new InvalidCastException();
                    else
                        property.SetValue(this, null, null);
                }
                else if (value.GetType() == propType)
                {
                    property.SetValue(this, value, null);
                }
                else
                {
                    TypeConverter typeConverter = TypeDescriptor.GetConverter(propType);
                    object propValue = typeConverter.ConvertFromString(value.ToString());
                    property.SetValue(this, propValue, null);
                }
            }
        }
    }
}
