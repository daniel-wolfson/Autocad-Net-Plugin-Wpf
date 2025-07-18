using System.Collections.Generic;
using System.Threading.Tasks;
using Intellidesk.Data.Models.Cad;

namespace Intellidesk.Data.Repositories
{
    public interface IUserRepository
    {
        void Add(User user);
        User GetUserById(int id);
        Task<User> GetUserByIdAsync(int id);
        User GetUserByName(string userName);
        User FindUser(string userName, string password);
        List<User> GetUsers();
        Task<List<User>> GetUsersAsync();
    }
}