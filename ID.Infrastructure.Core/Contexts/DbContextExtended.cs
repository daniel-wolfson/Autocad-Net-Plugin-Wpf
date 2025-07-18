using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading;

namespace ID.Infrastructure.Contexts
{
    public partial class GeneralDBContext : DbContext
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

        public static readonly ILoggerFactory ConsoleLoggerFactory = LoggerFactory.Create(builder =>
           {
               builder.AddFilter("Microsoft", LogLevel.Warning)
                      .AddFilter("System", LogLevel.Warning)
                      .AddFilter("DbLoggerCategory.Database.Command.Name", LogLevel.Information)
                      .AddConsole();
           }
        );

        public override int SaveChanges()
        {
            //Log.Logger.Warning("Not use strange calling of SaveChanges from context, use the SaveChanges of service");
            return _changesCounter += base.SaveChanges();
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
}