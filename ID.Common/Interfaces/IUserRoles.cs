namespace General.Infrastructure.Interfaces
{
    public interface IUserRoles
    {
        IRoles Role { get; set; }
        string RoleId { get; set; }
        IUserDetails User { get; set; }
        string UserId { get; set; }
    }
}