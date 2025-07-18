using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ID.Infrastructure.Enums
{
    public enum SessionKeys
    {
        [Display(Name = "AppTimeStamp", Description = "AppTimeStamp", ResourceType = typeof(DateTime))]
        AppTimeStamp,
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
        [Display(Name = "_float", Description = "left", ResourceType = typeof(string))]
        _float,
        [Display(Name = "_!float", Description = "right", ResourceType = typeof(string))]
        _notfloat,
        [Display(Name = "_dir", Description = "true", ResourceType = typeof(string))]
        _dir,
        [Display(Name = "_rtl", Description = "true", ResourceType = typeof(string))]
        _rtl,
        [Display(Name = "OrganizationalAdminOrgIds", Description = "", ResourceType = typeof(List<object>))] //OrganizationalStructure
        OrganizationalAdminOrgIds,
        [Display(Name = "RequestVerificationToken", Description = "", ResourceType = typeof(string))]
        RequestVerificationToken,
        [Display(Name = "LastProcessedToken", Description = "", ResourceType = typeof(string))]
        LastProcessedToken,
        [Display(Name = "RoleName", Description = "", ResourceType = typeof(string))]
        RoleName,
        [Display(Name = "EditMode", Description = "", ResourceType = typeof(bool))]
        EditMode,
        [Display(Name = "CurrentTestOrganization", Description = "", ResourceType = typeof(int))]
        CurrentTestOrganization,
        [Display(Name = "DbName", Description = "db name", ResourceType = typeof(string))]
        DbName,
        [Display(Name = "ContextName", Description = "api | mvc", ResourceType = typeof(string))]
        ContextName,
        [Display(Name = "ClientName", Description = "Client Name", ResourceType = typeof(string))]
        ClientName,
        [Display(Name = "Candidate", Description = "", ResourceType = typeof(object))]
        Candidate,
        [Display(Name = "SiteId", Description = "Atd", ResourceType = typeof(int))]
        SiteId,
        [Display(Name = "IsActiveCurrentTestDay", Description = "Atd", ResourceType = typeof(int))]
        IsActiveCurrentTestDay,
        [Display(Name = "Tabs", Description = "", ResourceType = typeof(Dictionary<int, List<object>>))]
        RolePermissionTabs,
        [Display(Name = "ReturnUrl", Description = "", ResourceType = typeof(string))]
        ReturnUrl,
        [Display(Name = "TokenData", Description = "encrypted string", ResourceType = typeof(string))]
        TokenData,
        [Display(Name = "RoleData", Description = "json role of type RolePermissions", ResourceType = typeof(string))]
        RoleData
    }
}
