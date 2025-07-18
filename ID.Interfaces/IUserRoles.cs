namespace ID.Interfaces
{
    public interface IUserRoles
    {
        string RoleId { get; set; }
        IAppRole Role { get; set; }

        string UserId { get; set; }
        IUserDetails User { get; set; }

    }
}