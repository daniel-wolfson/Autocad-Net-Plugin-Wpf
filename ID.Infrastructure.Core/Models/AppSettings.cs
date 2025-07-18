namespace ID.Infrastructure.Models
{
    public class AppSettings
    {
        public string Link { get; set; }
        public string ServerUrls { get; set; }
        public string MapitMainUrl { get; set; }
        public string MapitServiceUrl { get; set; }
        public string PostgreSqlPort { get; set; }
        public string HostEmailUrl { get; set; }
        public int HostEmailPort { get; set; }
        public string OrgId { get; set; }
        public string AppServiceSigningKey { get; set; }
        public int AppUserTokenExpires { get; set; }
    }
}

