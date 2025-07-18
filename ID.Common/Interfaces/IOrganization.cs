namespace WebInfrastructure.Interfaces
{
    public interface IOrganization
    {
        int OrgId { get; set; }
        string OrgName { get; set; }
        string Description { get; set; }
        bool IsActive { get; set; }
        bool IsSite { get; set; }
        int Level { get; set; }
        int ParentId { get; set; }
    }
}