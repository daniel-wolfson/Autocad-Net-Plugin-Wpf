using System;

namespace ID.Infrastructure.Interfaces
{
    public interface IUsers
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string UserTypeName { get; set; }
    }
}
