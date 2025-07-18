namespace ID.Infrastructure.Interfaces
{
    public interface IAuthOptions
    {
        string AUDIENCE { get; set; }
        string ISSUER { get; set; }
        string KEY { get; set; }
        int LIFETIME { get; set; }
        string SALT { get; set; }
        string USERNAME { get; set; }
        string USERPASSWORD { get; set; }
    }
}