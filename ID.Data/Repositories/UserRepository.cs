using Intellidesk.Data.Models.Cad;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intellidesk.Data.Repositories
{
    public class UserRepository : BaseRepository<User, int>, IUserRepository
    {
        public UserRepository() : base(null)
        {
        }

        public User GetUserById(int id)
        {
            return Find(x => x.UserId == id);
        }
        public async Task<User> GetUserByIdAsync(int id)
        {
            return await FindAsync(x => x.UserId == id);
        }

        public List<User> GetUsers()
        {
            return GetAll().ToList();
        }
        public Task<List<User>> GetUsersAsync()
        {
            return GetAsync();
        }

        public User GetUserByName(string userName)
        {
            var user = Find(u => u.Name == userName);
            return user;
        }
        public User FindUser(string userName, string password)
        {
            string psw = password.Replace(" ", "+");
            var user = GetAll().Where(u => u.Name == userName && u.Password == psw).FirstOrDefault();
            return user;
        }

        public void Add(User user)
        {
            //_dbContext.User.Add(user);
        }
    }
}
