using ID.Infrastructure.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;

namespace Intellidesk.Data.Auth
{
    // Configure the application sign-in manager which is used in this application.
    public class AppSignInManager : SignInManager<AppUser, string>
    {
        public AppSignInManager(UserManager<AppUser> userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }

        //public override Task<ClaimsIdentity> CreateUserIdentityAsync(AppUser user)
        //{
        //    return user.GenerateUserIdentityAsync((UserManager<AppUser>)UserManager);
        //}

        public static AppSignInManager Create(IdentityFactoryOptions<AppSignInManager> options,
            IOwinContext context)
        {
            return new AppSignInManager(context.GetUserManager<UserManager<AppUser>>(),
                context.Authentication);
        }
    }
}
