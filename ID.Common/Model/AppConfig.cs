using General.Interfaces;

namespace General.Models
{
    public class BaseAppConfig : IAppConfig
    {
        public string ClientName { get; set; }
        public string DalApi { get; set; }
        public string DalDb { get; set; }
        public string TMSCE { get; set; }
        public string GeneralVersion { get; set; }
        public string SapTmsInterServer { get; set; }
        public string ActiveTestDayApp { get; set; }
        public string EventGeneratorApp { get; set; }
        public string DefaultCultureName { get; set; }
        public string ButtonsIcon { get; set; }
        public string DefaultImage { get; set; }
        public string Image { get; set; }
        public string Multimedia { get; set; }
        public string ActiveDirectorySearchFilterMemberOf { get; set; }
        public string ActiveDirectoryEntryPath { get; set; }
        public string Domain { get; set; }
        public string GroupName { get; set; }
        public string CreateManualDbForSortingDay { get; set; }
        public string UseActiveDirectoryVerification { get; set; }
        public string UserActiveDirectoryDefaultPassword { get; set; }
        public string AddNewCandidate { get; set; }
        public string AddNewTest { get; set; }
        public string SQL_Error { get; set; }
        public string InterfaceOnline { get; set; }
        public string InsertDataSystemLog { get; set; }
        public string Site { get; set; }
        public string ApiResponseTimeout { get; set; }

        public string ActiveEventsDalApi { get; set; }
    }
}
