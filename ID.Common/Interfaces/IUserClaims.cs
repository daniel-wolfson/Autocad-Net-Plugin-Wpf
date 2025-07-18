namespace General.Infrastructure.Interfaces
{
    public interface IUserClaims
    {
        string ClaimType { get; set; }
        string ClaimValue { get; set; }
        int Id { get; set; }
        IUserDetails User { get; set; }
        string UserId { get; set; }
    }
}