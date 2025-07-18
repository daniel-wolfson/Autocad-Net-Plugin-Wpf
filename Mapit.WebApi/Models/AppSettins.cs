namespace General.Api.Models
{
    public class AppSettings
    {
        public string ServerUrls { get; set; }
        public string PostgreSqlPort { get; set; }
        public string HostEmailUrl { get; set; }
        public int HostEmailPort { get; set; }
        public string OrgId { get; set; }
        public string AppServiceSigningKey { get; set; }
        public int AppUserTokenExpires { get; set; }
    }
}