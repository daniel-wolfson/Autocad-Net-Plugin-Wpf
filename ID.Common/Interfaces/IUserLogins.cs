namespace General.Infrastructure.Interfaces
{
    public interface IUserLogins
    {
        string LoginProvider { get; set; }
        string ProviderKey { get; set; }
        IUserDetails User { get; set; }
        string UserId { get; set; }
    }
}