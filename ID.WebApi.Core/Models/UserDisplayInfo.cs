namespace ID.Api.Models
{
    public class UserDisplayInfo
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public int UserTypeId { get; set; }
        public string UserTypeName { get; set; }
        public int DefaultOrganizationId { get; set; }
        public string DefaultOrganizationName { get; set; }
        public bool InputInMassPermission { get; set; }
    }
}