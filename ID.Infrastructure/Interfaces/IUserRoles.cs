namespace ID.Infrastructure.Interfaces
{
    public interface IUserRoles1
    {
        string RoleId { get; set; }
        IAppRole Role { get; set; }

        string UserId { get; set; }
        IUserDetails1 User { get; set; }

    }
}