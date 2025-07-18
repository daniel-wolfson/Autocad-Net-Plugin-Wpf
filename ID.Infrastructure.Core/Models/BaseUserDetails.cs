namespace ID.Infrastructure.Models
{
    public class BaseUserDetails : BaseUser
    {
        public string FullName { get; set; }
        public string UserTypeName { get; set; }
        public string OrganizationName { get; set; }
    }
}