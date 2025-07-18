namespace ID.Infrastructure.Models
{
    public class BaseUser
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int UserTypeId { get; set; }
        public int OrganizationId { get; set; }
        public string Units { get; set; }
    }
}