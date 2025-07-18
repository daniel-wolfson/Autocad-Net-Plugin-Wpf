namespace ID.Infrastructure
{
    public static class GeneralConsts
    {
        /// <summary> ["int32", "single", "double", "string", "boolean"] </summary>
        public static string[] SimpleTypes = { "int32", "single", "double", "string", "boolean" };

        public static string[] TmsClaimTypes = { "PermissionTabs" };
    }

    public static class ClaimTypesExt
    {
        public const string PermissionTabs = "PermissionTabs";
        public const string PermissionRoles = "PermissionRoles";
        public const string OrganizationId = "SelectedOrganizationId";
        public const string OrganizationName = "SelectedOrganizationName";
        public const string RoleData = "RoleData";
    }
}
