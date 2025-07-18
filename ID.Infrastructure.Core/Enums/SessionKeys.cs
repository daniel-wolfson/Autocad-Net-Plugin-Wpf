using System;
using System.ComponentModel.DataAnnotations;

namespace ID.Infrastructure.Enums
{
    public enum SessionKeys
    {
        [Display(Name = "AppTimeStamp", Description = "AppTimeStamp", ResourceType = typeof(DateTime))]
        AppTimeStamp,
        [Display(Name = "ApplicationUser", Description = "ApplicationUser", ResourceType = typeof(object))] //Users
        AppUser,
        [Display(Name = "UserOrganizationId", Description = "1", ResourceType = typeof(int))]
        SelectedOrganizationId,
        [Display(Name = "UserOrganizationName", Description = "", ResourceType = typeof(string))]
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
        [Display(Name = "RequestVerificationToken", Description = "", ResourceType = typeof(string))]
        RequestVerificationToken,
        [Display(Name = "LastProcessedToken", Description = "", ResourceType = typeof(string))]
        LastProcessedToken,
        [Display(Name = "CurrentUICulture", Description = "", ResourceType = typeof(string))]
        CurrentUICulture,
        [Display(Name = "User", Description = "", ResourceType = typeof(object))]
        User,
        [Display(Name = "ActiveMenu", Description = "", ResourceType = typeof(object))]
        ActiveMenu,
        [Display(Name = "ClientName", Description = "", ResourceType = typeof(object))]
        ClientName,
        [Display(Name = "DbName", Description = "", ResourceType = typeof(object))]
        DbName,
        [Display(Name = "SelectedMetricsForReportMapEvalNotes", Description = "", ResourceType = typeof(object))]
        SelectedMetricsForReportMapEvalNotes,
        [Display(Name = "SelectedMetricsForReportMapFocus", Description = "", ResourceType = typeof(object))]
        SelectedMetricsForReportMapFocus,
        [Display(Name = "UnitBindingTree", Description = "", ResourceType = typeof(object))]
        UnitBindingTree,
        [Display(Name = "WifLogIn", Description = "", ResourceType = typeof(object))]
        WifLogIn,
        [Display(Name = "activiteslist", Description = "", ResourceType = typeof(object))]
        activiteslist,
        [Display(Name = "FlagAlertIs10Open", Description = "", ResourceType = typeof(object))]
        FlagAlertIs10Open,
        [Display(Name = "RootIndexGuid", Description = "", ResourceType = typeof(object))]
        RootIndexGuid,
        [Display(Name = "RootIndexName", Description = "", ResourceType = typeof(object))]
        RootIndexName,
        [Display(Name = "RootIndexScore", Description = "", ResourceType = typeof(object))]
        RootIndexScore,
        [Display(Name = "RootIndexUnitGuid", Description = "", ResourceType = typeof(object))]
        RootIndexUnitGuid,
        [Display(Name = "RootIndexModelGuid", Description = "", ResourceType = typeof(object))]
        RootIndexModelGuid,
        [Display(Name = "RootLevelColor", Description = "", ResourceType = typeof(object))]
        RootLevelColor,
        [Display(Name = "CalcDate", Description = "", ResourceType = typeof(object))]
        CalcDate,
        [Display(Name = "MetricReportDetails", Description = "", ResourceType = typeof(object))]
        MetricReportDetails,
        [Display(Name = "BranchMetricGuid", Description = "", ResourceType = typeof(object))]
        BranchMetricGuid,
        [Display(Name = "BranchMetricScore", Description = "", ResourceType = typeof(object))]
        BranchMetricScore,
        [Display(Name = "BranchLevelColor", Description = "", ResourceType = typeof(object))]
        BranchLevelColor,
        [Display(Name = "RefMetricGuid", Description = "", ResourceType = typeof(object))]
        RefMetricGuid,
        [Display(Name = "BranchMetricName", Description = "", ResourceType = typeof(object))]
        BranchMetricName,
        [Display(Name = "RefParentGuid", Description = "", ResourceType = typeof(object))]
        RefParentGuid,
        [Display(Name = "ReportUnitModelTitle", Description = "", ResourceType = typeof(object))]
        ReportUnitModelTitle,
        [Display(Name = "ModelName", Description = "", ResourceType = typeof(object))]
        ModelName,
        [Display(Name = "UnitName", Description = "", ResourceType = typeof(object))]
        UnitName,
        [Display(Name = "ReportModelGuid", Description = "", ResourceType = typeof(object))]
        ReportModelGuid,
        [Display(Name = "ReportUnitGuid", Description = "", ResourceType = typeof(object))]
        ReportUnitGuid,
        [Display(Name = "rootReportMapData", Description = "", ResourceType = typeof(object))]
        rootReportMapData,
        [Display(Name = "SavedReportsList", Description = "", ResourceType = typeof(object))]
        SavedReportsList,
        [Display(Name = "CurrentUser", Description = "", ResourceType = typeof(object))]
        CurrentUser
    }
}
