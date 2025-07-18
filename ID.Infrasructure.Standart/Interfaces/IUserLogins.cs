using System;

namespace ID.Infrastructure.Interfaces
{
    public interface IUserLogins
    {
        string LoginProvider { get; set; }
        string ProviderKey { get; set; }
        IUserDetails User { get; set; }
        Guid UserId { get; set; }
    }
}