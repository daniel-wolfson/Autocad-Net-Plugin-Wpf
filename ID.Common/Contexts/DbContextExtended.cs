using General.Core;
using General.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading;

namespace General.Contexts
{
    public partial class GeneralDBContext : DbContext, IDBContext
    {
        bool disposed = false;
        private static int _globalContextId;
        [NotMapped]
        /// <summary> auto increment number/order instnace of context /// </summary>
        public readonly int GlobalContextId = Interlocked.Increment(ref _globalContextId);

        /// <summary> total count of all savechanges within single transaction /// </summary>
        private int _changesCounter;
        public int GetChangesCounter()
        { return _changesCounter; }
        public void ClearChangesCounter()
        { _changesCounter = 0; }

        [Obsolete]
        public static readonly ILoggerFactory ConsoleLoggerFactory
            = new LoggerFactory(new[] {
                  new ConsoleLoggerProvider((category, level) =>
                    category == DbLoggerCategory.Database.Command.Name && level == LogLevel.Information, true)
                });

        //[Obsolete]
        //public ILoggerFactory ConsoleLoggerFactory
        //{
        //    get
        //    {
        //        var factory = (ILoggerFactory)GeneralContext.ServiceProvider.GetService(typeof(ILoggerFactory));
        //        factory.AddProvider(new ConsoleLoggerProvider((category, level) => category == DbLoggerCategory.Database.Command.Name && level == LogLevel.Information, true));
        //        return factory;
        //    }
        //}

        public override int SaveChanges()
        {
            //Log.Logger.Warning("Not use strange calling of SaveChanges from context, use the SaveChanges of service");
            return _changesCounter += base.SaveChanges();
        }

        protected void CustomConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = GeneralContext.ServiceProvider.ResolveConnectionString();
                optionsBuilder.UseSqlServer(connectionString);

                var env = GeneralContext.GetService<IHostingEnvironment>();
                if (env.IsDevelopment())
                    optionsBuilder.UseLoggerFactory(ConsoleLoggerFactory); // Warning: Do not create a new ILoggerFactory instance each time
            }
        }

        protected void CustomModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<RolePermissions>(entity =>
            //{
            //entity.HasKey(e => new { e.RoleId, e.TabId })
            //    .HasName("tabroles_pkey");

            //entity.Property(e => e.TabId);

            //entity.Property(e => e.RoleId);

            //entity
            //    .HasOne(d => d)
            //    .WithMany(p => p.RoleId)
            //    .HasForeignKey(d => d.RoleId)
            //    .OnDelete(DeleteBehavior.ClientSetNull)
            //    .HasConstraintName("UserRoles_RoleId_fkey");

            //entity.HasOne(d => d.User)
            //    .WithMany(p => p.UserRoles)
            //    .HasForeignKey(d => d.UserId)
            //    .HasConstraintName("UserRoles_UserId_fkey");
            //});
        }

        public override void Dispose()
        {
            if (disposed)
                return;

            base.Dispose();
            disposed = true;
        }
    }

    //public partial class Users : IUsers
    //{
    //    //public ICollection<RolePermissions> RolePermissions { get; set; }
    //}

    //public partial class RolePermissions
    //{
    //    //public ICollection<Roles> Roles { get; set; }
    //}

    //public partial class Roles : IRoles
    //{
    //}
}