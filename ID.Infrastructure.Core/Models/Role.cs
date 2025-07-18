using ID.Infrastructure.Interfaces;

namespace ID.Infrastructure.Models
{
    public class Role : IRoles
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
