namespace WebInfrastructure.Interfaces
{
    public interface IAppUserModel
    {
        //int? SelectedOrganizationId { get; set; }
        //string SelectedOrganizationName { get; set; }
        int? DefaultOrganizationId { get; set; }
        string FirstName { get; set; }
        string LastName { get; set; }
        string Password { get; set; }
        int UserId { get; set; }
        string UserName { get; set; }
        int UserTypeId { get; set; }
        bool InputInMassPermission { get; set; }
    }
}