using ID.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace ID.Infrastructure.Auth
{
    public class TokenRequirement : IAuthorizationRequirement
    {
        public IAuthOptions AuthOptions { get; }

        public TokenRequirement(IAuthOptions authOptions)
        {
            AuthOptions = authOptions;
        }
    }
}