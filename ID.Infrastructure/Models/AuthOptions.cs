using ID.Infrastructure.Interfaces;

namespace Intellidesk.Data.General
{
    public class AuthOptions : IAuthOptions
    {
        public string ISSUER { get; set; }
        public string AUDIENCE { get; set; }
        public string KEY { get; set; }
        public int LIFETIME { get; set; }
        public string SALT { get; set; }
        public string USERNAME { get; set; }
        public string USERPASSWORD { get; set; }
    }
}
