using Microsoft.AspNet.Identity;

namespace AspNet.Identity.PostgreSQL
{
    public class User : IUser
    {
        public string Id { get; }

        public string UserName { get; set; }
    }
}
