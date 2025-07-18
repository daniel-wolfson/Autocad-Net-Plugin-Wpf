using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ID.Infrastructure.Enums
{
    public enum ItemsKeys
    {
        [Display(Name = "ApplicationUser", Description = "ApplicationUser", ResourceType = typeof(object))] //Users
        AppUser,
        [Display(Name = "UserRolePermissions", Description = "", ResourceType = typeof(List<object>))] //RolePermissionsTabsInfo
        UserRolePermissions,
        [Display(Name = "ControlPanelSiteId", Description = "", ResourceType = typeof(int))]
        ControlPanelSiteId,
        [Display(Name = "SelectedOrganizationId", Description = "1", ResourceType = typeof(int))]
        SelectedOrganizationId,
        [Display(Name = "SelectedOrganizationName", Description = "", ResourceType = typeof(string))]
        SelectedOrganizationName,
        [Display(Name = "SelectedCultureName", Description = "he", ResourceType = typeof(string))]
        SelectedCultureName,
        [Display(Name = "DefaultCultureName", Description = "he", ResourceType = typeof(string))]
        DefaultCultureName,
        [Display(Name = "DbName", Description = "db name", ResourceType = typeof(string))]
        DbName,
        [Display(Name = "RoleName", Description = "", ResourceType = typeof(string))]
        RoleName,
        [Display(Name = "ClientName", Description = "Client Name", ResourceType = typeof(string))]
        ClientName,
        [Display(Name = "Candidate", Description = "", ResourceType = typeof(object))]
        Candidate,
        [Display(Name = "SiteId", Description = "Atd", ResourceType = typeof(int))]
        SiteId,
        [Display(Name = "IsActiveCurrentTestDay", Description = "Atd", ResourceType = typeof(int))]
        IsActiveCurrentTestDay,
        [Display(Name = "OrganizationalAdminOrgIds", Description = "", ResourceType = typeof(List<int>))]
        OrganizationalAdminOrgIds,
    }
}
