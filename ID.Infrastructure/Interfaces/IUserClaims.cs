using System;

namespace ID.Infrastructure.Interfaces
{
    public interface IUserClaims1
    {
        string ClaimType { get; set; }
        string ClaimValue { get; set; }
        int Id { get; set; }
        IUserDetails1 User { get; set; }
        Guid UserId { get; set; }
    }
}