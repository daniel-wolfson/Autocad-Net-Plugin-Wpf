using ID.Infrastructure.Interfaces;

namespace ID.Infrastructure.Models
{
    public class AuthOptions : IAuthOptions
    {
        public string ISSUER { get; set; }
        public string AUDIENCE { get; set; }
        public int LIFETIME { get; set; }
        public string SALT { get; set; }
        public string KEY { get; set; }
        public string PASSWORDKEY { get; set; }
        public string AuthenticationType { get; set; }
    }
}
