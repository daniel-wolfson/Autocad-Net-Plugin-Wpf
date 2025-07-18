using ID.Infrastructure.Models;
using Intellidesk.Data.Repositories;
using System;

namespace Intellidesk.Data.Auth
{
    public class AuthRepository : BaseRepository<AppUser, int>, IDisposable
    {
        public AuthRepository(AppDbContext authContext) : base(authContext)
        {
            //_userManager = new AppUserManager(new AppUserStore(new AuthRepository(authContext)));
        }

        //public async Task<IdentityResult> RegisterUser(UserData userModel)
        //{
        //    //ApplicationUser user = new ApplicationUser(userModel.UserName, userModel.Password);
        //    //var result = await _userManager.CreateAsync(user, userModel.Password);
        //    return null;
        //}

        //public async Task<IdentityUser> FindUser(string userName, string password)
        //{
        //    //var user = await _userManager.FindByNameAsync(userName);
        //    //if (user != null && await _userManager.CheckPasswordAsync(user, password))
        //    //{
        //    //    // user is valid do whatever you want
        //    //}

        //    //IdentityUser user = await _userManager.FindAsync(userName, password);
        //    return null;
        //}

        public void Dispose()
        {
            Context.Dispose();
            //_userManager.Dispose();

        }
    }
}