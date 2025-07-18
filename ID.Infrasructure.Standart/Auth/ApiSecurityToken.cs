using System;
using System.Collections.ObjectModel;
using System.IdentityModel.Tokens;

namespace ID.Infrastructure.Auth
{
    public class ApiSecurityToken : SecurityToken
    {
        public ApiSecurityToken(string id, string issuer, SecurityKey securityKey, SecurityKey signingKey, DateTime validFrom, DateTime validTo)
        {
            Id = id;
            Issuer = issuer;
            SecurityKey = securityKey;
            SigningKey = signingKey;
            ValidFrom = validFrom;
            ValidTo = validTo;
        }

        public override string Id { get; }
        public string Issuer { get; }
        public SecurityKey SecurityKey { get; }
        public SecurityKey SigningKey { get; set; }
        public override DateTime ValidFrom { get; }
        public override DateTime ValidTo { get; }

        public override ReadOnlyCollection<SecurityKey> SecurityKeys => throw new NotImplementedException();
    }
}
