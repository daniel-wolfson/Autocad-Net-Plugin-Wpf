using System;

namespace ID.Infrastructure.Interfaces
{
    public interface IUserClaims
    {
        int Id { get; set; }
        string ClaimType { get; set; }
        string ClaimValue { get; set; }
        Guid UserId { get; set; }
        IUserDetails User { get; set; }
    }
}