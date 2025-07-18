using AspNet.Identity.PostgreSQL;
using ID.Infrastructure;
using ID.Infrastructure.Helpers;
using ID.Infrastructure.Interfaces;
using ID.Infrastructure.Models;
using Intellidesk.Data.Auth;
using Intellidesk.Data.Services;
using Microsoft.AspNet.Identity;
using Prism.Ioc;
using System;
using System.Threading;
using System.Threading.Tasks;
using Unity;

namespace Intellidesk.AcadNet
{
    /// <summary> UIBuildService </summary>
    public class AcadNetDataManager : IRegisterModule
    {
        public static Thread _waitWindowThread;

        #region <IRegisterModule>

        public async Task<bool> Register(IUnityContainer container)
        {
            await Initialize(container);
            return await Task.FromResult(true);
        }

        public async Task<bool> Initialize(IUnityContainer container)
        {
            var pluginSettings = container.Resolve<IPluginSettings>();
            var authOptions = container.Resolve<IAuthOptions>();
            var configuration = container.Resolve<IConfigurationBuilder>();
            var connectionStrings = configuration.GetSection<ConnectionStrings>();

            string machineName = Environment.MachineName
                ?? System.Net.Dns.GetHostName()
                ?? Environment.GetEnvironmentVariable("COMPUTERNAME")
                ?? Guid.NewGuid().ToString();

            var userService = Plugin.GetService<IAdminService<IdentityUserDetails>>();
            string token = await userService.ReadLocalTokenAsync();

            var isTokenValid = true; //token != null && Util.VerifyToken(token, authOptions.KEY);
            if (!isTokenValid)
            {
                Plugin.Logger.Information($"{nameof(AcadNetManager)}.{nameof(Initialize)} called!");

                token = await userService.AuthenticateAsync(authOptions.USERNAME, authOptions.USERPASSWORD);
                if (!string.IsNullOrEmpty(token))
                {
                    await userService.WriteLocalTokenAsync(token);
                    isTokenValid = true;
                }
            }

            Plugin.InitilizedmoduleTypes.Add(this.GetType().Name, isTokenValid);
            return isTokenValid;
        }

        public async Task<bool> Initialize2Async(IUnityContainer container)
        {
            var configuration = container.Resolve<IConfigurationBuilder>();
            var connectionStrings = configuration.GetSection<ConnectionStrings>();

            var appUserManager = new AppUserManager(new UserStore<IdentityUserDetails>(
                    new PostgreSQLDatabase(connectionStrings.DbConn)));
            appUserManager.Init();

            string machineName = Environment.MachineName
                ?? System.Net.Dns.GetHostName()
                ?? Environment.GetEnvironmentVariable("COMPUTERNAME")
                ?? Guid.NewGuid().ToString();

            AppUser appUser;
            AppDomain.CurrentDomain.AssemblyResolve += AppAssemblyResolver.OnCurrentDomainOnAssemblyResolve;
            IdentityUserDetails identityUserDetails = await appUserManager.FindByNameAsync(machineName.ToLower());
            AppDomain.CurrentDomain.AssemblyResolve -= AppAssemblyResolver.OnCurrentDomainOnAssemblyResolve;

            if (identityUserDetails != null)
            {
                var psw_token = appUserManager.GeneratePasswordResetTokenAsync(identityUserDetails.Id);
                appUser = AppUser.Create(identityUserDetails);
            }
            else
            {
                var userAdminService = container.Resolve<IAdminService<IdentityUserDetails>>();
                var passwordHash = Util.EncryptPassword("123456", "authOptions");

                var userDetails = new IdentityUserDetails()
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = machineName.ToLower(),
                    PasswordHash = passwordHash
                };

                IdentityResult identityResult = await appUserManager.CreateAsync(userDetails);
                if (identityResult.Succeeded)
                    appUser = AppUser.Create(new AppUserDetails()
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserName = "Daniel",
                        PasswordHash = passwordHash
                    });
            }

            //var tokenResult = await InitializeUserAsync().ConfigureAwait(false);
            //if (tokenResult != null)
            //{
            //    //var user = await UserAdminService.AuthenticateAsync("admin", "123456").ConfigureAwait(false);
            //    //return user != null;
            //}

            return await Task.FromResult(true);
        }

        #endregion
    }
}