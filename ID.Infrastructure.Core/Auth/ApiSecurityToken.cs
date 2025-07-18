using Microsoft.IdentityModel.Tokens;
using System;

namespace ID.Infrastructure.Auth
{
    public class ApiSecurityToken : SecurityToken
    {
        public ApiSecurityToken(Guid id, string issuer, SecurityKey securityKey, SecurityKey signingKey, DateTime validFrom, DateTime validTo)
        {
            Id = id.ToString();
            Issuer = issuer;
            SecurityKey = securityKey;
            SigningKey = signingKey;
            ValidFrom = validFrom;
            ValidTo = validTo;
        }

        public override string Id { get; }
        public override string Issuer { get; }
        public override SecurityKey SecurityKey { get; }
        public override SecurityKey SigningKey { get; set; }
        public override DateTime ValidFrom { get; }
        public override DateTime ValidTo { get; }
    }
}
