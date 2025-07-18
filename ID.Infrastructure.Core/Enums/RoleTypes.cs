namespace ID.Infrastructure.Enums
{
    public enum RoleTypes
    {
        // from table Users UserTypeId
        SystemAdmin = -1,
        OrganizationalAdmin = -2,
        RegularUser = -3,

        // from table roles
        None = 0,
        Interviewer = 1,
        DiagnosticManager = 2,
        SiteManager = 3,
        ConstitutionsManager = 4,
        PickerManager = 5,
        TestManager = 7,

    }
}
