namespace Intellidesk.Interfaces
{
    public interface IAuthOptions
    {
        string AUDIENCE { get; set; }
        string ISSUER { get; set; }
        string KEY { get; set; }
        int LIFETIME { get; set; }
    }
}