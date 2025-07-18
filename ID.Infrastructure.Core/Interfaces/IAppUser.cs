using System;

namespace ID.Infrastructure.Interfaces
{
    public interface IAppUser : IUsers
    {
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
        bool LockoutEnabled { get; set; }
        int AccessFailedCount { get; set; }
        bool IsAuthenticated { get; }        
    }
}
