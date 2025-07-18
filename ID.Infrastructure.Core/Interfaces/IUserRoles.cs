namespace ID.Infrastructure.Interfaces
{
    public interface IUserRoles
    {
        IUserRoles Role { get; set; }
        string RoleId { get; set; }
        IUserDetails User { get; set; }
        string UserId { get; set; }
    }
}