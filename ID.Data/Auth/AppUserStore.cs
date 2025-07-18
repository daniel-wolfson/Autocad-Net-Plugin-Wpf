using ID.Infrastructure.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Threading.Tasks;

namespace Intellidesk.Data.Auth
{
    public class AppUserStore : UserStore<AppUser>, IUserStore<AppUser>
    {
        private readonly AuthRepository _authRepository;

        public AppUserStore(AuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        public override async Task CreateAsync(AppUser user)
        {
            await _authRepository.CreateAsync(user);
        }
        //public async Task CreateAsync(AppUser user)
        //{
        //    return await Task.Factory.StartNew(() => Users.Add(user));
        //}

        //public Task UpdateAsync(AppUser user)
        //{
        //    throw new NotImplementedException();
        //}

        //public Task DeleteAsync(AppUser user)
        //{
        //    throw new NotImplementedException();
        //}

        //public Task<AppUser> FindByIdAsync(string userId)
        //{
        //    throw new NotImplementedException();
        //}

        override public async Task<AppUser> FindByNameAsync(string userName)
        {
            //return Task<ApplicationUser>.Factory.StartNew(() => Users.FirstOrDefault(u => u.UserName == userName));
            //return _authRepository.FindAsync(x => x.UserName == userName);
            //return Task<ApplicationUser>.Factory.StartNew(() => await _authRepository.FindAsync(x => x.UserName == userName));
            return await _authRepository.FindAsync(x => x.UserName == userName);
        }
    }
}
