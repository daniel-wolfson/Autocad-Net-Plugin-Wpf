using AspNet.Identity.PostgreSQL;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using System;
using System.Threading.Tasks;

namespace Intellidesk.Data.Auth
{
    public class AppUserManager : UserManager<IdentityUserDetails>
    {
        public static AppUserManager Create(IdentityFactoryOptions<AppUserManager> options, IOwinContext context)
        {
            //var appDbContext = context.Get<AppDbContext>();
            //var appUserManager = context.Get<AppUserManager>();

            var manager = new AppUserManager(new UserStore<IdentityUserDetails>(new PostgreSQLDatabase()));

            manager.Init();

            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider = new DataProtectorTokenProvider<IdentityUserDetails>(dataProtectionProvider.Create("ASP.NET Identity"))
                {
                    //Code for email confirmation and reset password life time
                    TokenLifespan = TimeSpan.FromHours(6)
                };
            }
            context.Set("AppUserManager", manager);

            return manager;
        }

        public void Init()
        {
            //var appDbContext = context.Get<AppDbContext>();
            //var appUserManager = context.Get<AppUserManager>();

            //var manager = new AppUserManager(new UserStore<IdentityUserDetails>(new PostgreSQLDatabase()));
            //var manager1 = new AppUserManager(new AppUserStore(new AuthRepository(appDbContext)));
            //var appUserManager = new AppUserManager(new AppUserStore(new AuthRepository(appDbContext)));

            // Configure validation logic for usernames
            this.UserValidator = new UserValidator<IdentityUserDetails>(this)
            {
                AllowOnlyAlphanumericUserNames = true,
                RequireUniqueEmail = true
            };

            // Configure validation logic for passwords
            this.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = true,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
            };

            // Configure user lockout defaults
            this.UserLockoutEnabledByDefault = true;
            this.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            this.MaxFailedAccessAttemptsBeforeLockout = 5;

            // Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
            // You can write your own provider and plug it in here.
            this.RegisterTwoFactorProvider("Phone Code",
                new PhoneNumberTokenProvider<IdentityUserDetails>
                {
                    MessageFormat = "Your security code is {0}"
                });

            this.RegisterTwoFactorProvider("Email Code",
                new EmailTokenProvider<IdentityUserDetails>
                {
                    Subject = "Security Code",
                    BodyFormat = "Your security code is {0}"
                });

            this.EmailService = new EmailService();
            this.SmsService = new SmsService();

            this.UserTokenProvider = new MyUserTokenProvider<IdentityUserDetails>();
        }

        public AppUserManager(UserStore<IdentityUserDetails> store) : base(store) { }

        public override async Task<IdentityUserDetails> FindAsync(string userName, string password)
        {
            IdentityUserDetails user = await this.Store.FindByNameAsync(userName);
            if (user != null && this.CheckPassword(user, password))
                return user;

            return default;
            //var result = this.PasswordHasher.VerifyHashedPassword(user.PasswordHash, password);
            //if (!(result == PasswordVerificationResult.Success))
            //    return null;

            //var taskInvoke = Task.Factory.StartNew(async () =>
            //{
            //    PasswordVerificationResult result = this.PasswordHasher.VerifyHashedPassword(user.PasswordHash, password);
            //    if (result == PasswordVerificationResult.SuccessRehashNeeded)
            //    {
            //        return await Store.FindByNameAsync(userName);
            //    }
            //    return null;
            //});
            //return taskInvoke.Result.Result;
        }

        public override async Task<IdentityResult> CreateAsync(IdentityUserDetails user, string password)
        {
            //await this.Store.CreateAsync(new IdentityUserDetails(user));
            //var userDetails = await this.Store.FindByNameAsync(user.UserName);
            IdentityResult identityResult = await base.CreateAsync(new IdentityUserDetails(user), password);
            return identityResult;
        }

        public override Task<string> GeneratePasswordResetTokenAsync(string userId)
        {
            return base.GeneratePasswordResetTokenAsync(userId);
        }
    }

    public class MyUserTokenProvider<TUser> : IUserTokenProvider<IdentityUserDetails, string> where TUser : class, IUser
    {
        public Task<string> GenerateAsync(string purpose, UserManager<IdentityUserDetails, string> manager, IdentityUserDetails user)
        {
            Guid resetToken = Guid.NewGuid();
            user.PasswordHash = resetToken.ToString();
            //manager.UpdateAsync(user);
            return Task.FromResult<string>(resetToken.ToString());
        }

        public Task<bool> IsValidProviderForUserAsync(UserManager<IdentityUserDetails, string> manager, IdentityUserDetails user)
        {
            if (manager == null) throw new ArgumentNullException();
            else
            {
                return Task.FromResult<bool>(manager.SupportsUserPassword);
            }
        }

        public Task NotifyAsync(string token, UserManager<IdentityUserDetails, string> manager, IdentityUserDetails user)
        {
            return Task.FromResult<int>(0);
        }

        public Task<bool> ValidateAsync(string purpose, string token, UserManager<IdentityUserDetails, string> manager, IdentityUserDetails user)
        {
            return Task.FromResult<bool>(user.PasswordHash.ToString() == token);
        }
    }
}
