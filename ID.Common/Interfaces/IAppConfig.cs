namespace General.Infrastructure.Interfaces
{
    public interface IAppConfig
    {
        string ActiveDirectoryEntryPath { get; set; }
        string ActiveDirectorySearchFilterMemberOf { get; set; }
        string DefaultCultureName { get; set; }
        string DefaultImage { get; set; }
        string Domain { get; set; }
        string GroupName { get; set; }
        string InsertDataSystemLog { get; set; }
        string InterfaceOnline { get; set; }
        string SQL_Error { get; set; }
        string ClientName { get; set; }
        string DalApi { get; set; }
        string DalDb { get; set; }
        string GeneralVersion { get; set; }
        string UseActiveDirectoryVerification { get; set; }
        string UserActiveDirectoryDefaultPassword { get; set; }
        string Site { get; set; }
        string ApiResponseTimeout { get; set; }
    }
}