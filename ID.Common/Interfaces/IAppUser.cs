using System;
using System.Collections.Generic;

namespace General.Infrastructure.Interfaces
{
    public interface IAppUser_
    {
        // TmsUser implementation
        string FirstName { get; set; }
        string LastName { get; set; }
        int? DefaultOrganizationId { get; set; }
        int SelectedOrganizationId { get; set; }
        string SelectedOrganizationName { get; set; }
        bool InputInMassPermission { get; set; }
        int UserTypeId { get; set; }
        bool IsLogged { get; }

        // IdentityUser implementation
        DateTimeOffset? LockoutEnd { get; set; }
        bool TwoFactorEnabled { get; set; }
        bool PhoneNumberConfirmed { get; set; }
        string PhoneNumber { get; set; }
        string ConcurrencyStamp { get; set; }
        string SecurityStamp { get; set; }
        string PasswordHash { get; set; }
        bool EmailConfirmed { get; set; }
        string NormalizedEmail { get; set; }
        string Email { get; set; }
        string NormalizedUserName { get; set; }
        string UserName { get; set; }
        string Id { get; set; }
        bool LockoutEnabled { get; set; }
        int AccessFailedCount { get; set; }

        //[JsonConverter(typeof(ConcreteTypeConverter<IEnumerable<UserOrganizationsRolesExt>>))]
        IEnumerable<IAppRole> Roles { get; }

        bool IsAuthenticated { get; }

        string ToStringJson();
    }
}