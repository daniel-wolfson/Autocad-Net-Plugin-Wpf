
using Microsoft.AspNet.Identity.EntityFramework;

namespace Intellidesk.Data.Models.Securiry
{
    public class OwinAuthDbContext : IdentityDbContext
    {
        public OwinAuthDbContext()
            : base("OwinAuthDbContext")
        {
        }
    }
}
