using ID.Infrastructure.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Diagnostics;

namespace Intellidesk.Data.Auth
{
    /// <summary> Configure the db context and user manager to use a single instance per request </summary>
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext() : base("AuthContext", throwIfV1Schema: false)
        {
            Configuration.ProxyCreationEnabled = false;
            Configuration.LazyLoadingEnabled = false;

            AppDomain.CurrentDomain.SetData("DataDirectory", "D:\\IntelliDesk\\IntelliDesk.bundle.2019\\Contents\\WebApi");

            // Log all the database calls when in Debug.
            this.Database.Log = (message) =>
            {
                Debug.Write(message);
            };
        }

        public static AppDbContext Create()
        {
            AppDbContext appDbContext = new AppDbContext();
            //Plugin.Container.RegisterInstance<AppDbContext>(appDbContext);
            return appDbContext;
        }

    }
}