using System;

namespace ID.Infrastructure.Interfaces
{
    public interface IUserLogins1
    {
        string LoginProvider { get; set; }
        string ProviderKey { get; set; }
        IUserDetails1 User { get; set; }
        Guid UserId { get; set; }
    }
}