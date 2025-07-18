using System.Collections.Generic;
using User = Intellidesk.Data.Models.Cad.User;

namespace Intellidesk.Data.Services
{
    /// <summary>
    ///     Add any custom business logic (methods) here
    /// </summary>
    public interface IUserService : IService<User>
    {
        User GetUserByName(string environmentUserName);
        IEnumerable<User> GetUsers(bool fromCache = true);
        User Authenticate(string email, string password, int role);
        string PasswordGenerate(PasswordOptions opts = null);
    }
}