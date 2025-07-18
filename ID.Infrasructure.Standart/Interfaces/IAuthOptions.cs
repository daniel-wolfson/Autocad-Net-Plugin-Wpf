namespace ID.Infrastructure.Interfaces
{
    public interface IAuthOptions
    {
        string AUDIENCE { get; set; }
        string ISSUER { get; set; }
        int LIFETIME { get; set; }
        string KEY { get; set; }
        string PASSWORDKEY { get; set; }
        string AuthenticationType { get; set; }
    }
}