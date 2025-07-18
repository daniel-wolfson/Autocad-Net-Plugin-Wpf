namespace ID.Infrastructure.Interfaces
{
    public interface IAppPermissionsInfo
    {
        int PermissionTypeId { get; set; }
        string PermissionTypeName { get; set; }
        int? RoleId { get; set; }
        int? ParentId { get; set; }

        int TabId { get; set; }
        string TabName { get; set; }
        string Description { get; set; }
        bool? IsActive { get; set; }
        int TabOrder { get; set; }


    }
}