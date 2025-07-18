using AspNet.Identity.PostgreSQL;
using ID.Infrastructure;
using ID.Infrastructure.Models;
using Intellidesk.AcadNet.Data.Repositories.EF6;
using Intellidesk.Data.Auth;
using Intellidesk.Data.Repositories.EF6.UnitOfWork;
using System;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Intellidesk.Data.Services
{
    public abstract class BaseService : IUnitOfWork
    {
        public AppDbContext DbContext = null;

        private AppUserManager _userManager = null;
        public AppUserManager UserManager
        {
            get
            {
                if (_userManager == null)
                {
                    if (HttpContext.Current != null)
                        _userManager = HttpContext.Current.GetOwinContext().Get<AppUserManager>("AppUserManager");
                    else
                    {
                        IConfigurationBuilder configuration = Plugin.GetService<IConfigurationBuilder>();
                        var connectionStrings = configuration.GetSection<ConnectionStrings>();
                        DbContext = AppDbContext.Create();
                        _userManager = new AppUserManager(new UserStore<IdentityUserDetails>(new PostgreSQLDatabase(connectionStrings.DbConn))); ;
                    }
                }
                return _userManager;
            }
        }

        public virtual int SaveChanges(CancellationToken cancellationToken = default)
        {
            return DbContext.SaveChanges();
        }

        public virtual Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return DbContext.SaveChangesAsync();
        }

        public object GetHttpClient()
        {
            HttpClientHandler handler = new HttpClientHandler()
            {
                UseDefaultCredentials = true
            };

            HttpClient client = new HttpClient(handler);

            ServicePointManager.ServerCertificateValidationCallback +=
                delegate (object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                                        System.Security.Cryptography.X509Certificates.X509Chain chain,
                                        System.Net.Security.SslPolicyErrors sslPolicyErrors)
                {
                    return true; // **** Always accept
                };

            return client;
        }

        public int SaveChanges()
        {
            throw new NotImplementedException();
        }

        IRepository<TEntity> IUnitOfWork.Repository<TEntity>()
        {
            throw new NotImplementedException();
        }

        public void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified)
        {
            throw new NotImplementedException();
        }

        public bool Commit()
        {
            throw new NotImplementedException();
        }

        public void Rollback()
        {
            throw new NotImplementedException();
        }

        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing)
        //    {
        //        this.Dispose();
        //    }

        //    base.Dispose(disposing);
        //}

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed;
        public void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                // free other managed objects that implement
                // IDisposable only

                try
                {
                }
                catch (ObjectDisposedException)
                {
                    // do nothing, the objectContext has already been disposed
                }

                if (DbContext != null)
                {
                    DbContext.Dispose();
                    DbContext = null;
                }
            }

            // release any unmanaged objects
            // set the object references to null

            _disposed = true;
        }
    }
}
