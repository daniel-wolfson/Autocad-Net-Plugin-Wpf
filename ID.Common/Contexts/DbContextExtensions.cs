using General.Core;
using General.Enums;
using General.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace General.Contexts
{
    public static partial class DbContextExtensions
    {
        public static IEnumerable<KeyValuePair<TEntity, EntityState>> GetTrackEntries<TEntity>(this DbContext dbContext) where TEntity : class
        {
            var entries = dbContext.ChangeTracker.Entries()
                .Where(x => x.Entity.GetType() == typeof(TEntity))
                .Select(x => new KeyValuePair<TEntity, EntityState>(x.Entity as TEntity, x.State));

            return entries;
        }

        public static async Task<int> SaveChangesAsync(this DbContext context, 
            Func<IEnumerable<EntityEntry>, Task> resolveConflictsAsync, int retryCount = 3)
        {
            if (retryCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(retryCount));
            }

            for (int retry = 1; retry < retryCount; retry++)
            {
                try
                {
                    return await context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException exception) when (retry < retryCount)
                {
                    await resolveConflictsAsync(exception.Entries);
                }
            }
            return await context.SaveChangesAsync();
        }

        public static void AttachEnsured<TEntity>(this IDBContext dbContext, TEntity entity) where TEntity : class
        {
            // get entity Keys
            var dbSet = dbContext.Set<TEntity>();
            var entityUniqueKeys = dbSet.GetKeys().ToList();
            Type entityType = typeof(TEntity);

            // existing entities in local context
            var localEntities = dbSet.Local.Where(localEntity =>
            {
                bool compareResult = entityUniqueKeys.All(pi =>
                {
                    object localValue = entityType.GetProperty(pi.Name).GetValue(localEntity, null);
                    object inputValue = entityType.GetProperty(pi.Name).GetValue(entity, null);
                    return localValue != null && (localValue == inputValue || localValue.Equals(inputValue));
                });
                return compareResult;
            }).ToList();

            // Detach exist entities
            if (localEntities.Any())
                localEntities.ForEach(p => dbContext.Entry(p).State = EntityState.Detached);

            // Attach the detached entity
            if (dbContext.Entry(entity).State == EntityState.Detached)
                dbSet.Attach(entity);
        }

        public static void AttachEnsured<TEntity>(this IDBContext dbContext, IEnumerable<TEntity> entities) where TEntity : class
        {
            foreach (var entity in entities)
                dbContext.AttachEnsured(entity);
        }

        public static string ResolveConnectionString(this IServiceProvider provider)
        {
            // Get context
            IHttpContextAccessor httpContextAccessor = (IHttpContextAccessor)provider.GetService(typeof(IHttpContextAccessor));
            HttpContext httpContext = httpContextAccessor.HttpContext;

            var items = httpContext.Items;
            string clientName = items[SessionKeys.ClientName.ToString()].ToString();
            string dbName = items[SessionKeys.DbName.ToString()].ToString();

            LogContext.PushProperty("Client", $"Client={items[SessionKeys.ClientName.ToString()]}");

            //string appUserClaimsDb = httpContextAccessor.HttpContext.User.Claims
            //    .Where(c => c.Type.ToLower() == tmsSystem).Select(c => c.Value).FirstOrDefault();

            // Get connectionString
            IConfiguration configuration = (IConfiguration)provider.GetService(typeof(IConfiguration));
            string connectionString = string.Format(configuration.GetConnectionString("TMSDBConnection"), dbName);

            //Services.AddSingleton(x => new Lazy<Serilog.ILogger>(() => Log.Logger));

            IHostingEnvironment env = (IHostingEnvironment)provider.GetService(typeof(IHostingEnvironment));
            if (env.IsDevelopment() && GeneralContext.RequestTraceId != httpContext.TraceIdentifier) // TraceIdentifier preventing calling twice
            {
                var args = connectionString.Split(";");
                var server = args[0];
                var db = args[1];
                Log.Logger.Information($"connected to {server}, {db}");
                GeneralContext.RequestTraceId = httpContext.TraceIdentifier;
            }

            return connectionString;
        }

        public static TDBContext CreateDBContext<TDBContext>(this IServiceProvider provider) where TDBContext : DbContext, IDBContext
        {
            var connectionString = provider.ResolveConnectionString();

            var dbContext = new DbContextFactory<TDBContext>()
                    .CreateDbContext(new[] { connectionString });

            return dbContext;
        }

        //public static async Task<int> SaveChangesAsync(this DbContext context, RefreshConflict refreshMode, RetryStrategy retryStrategy) =>
        //        await context.SaveChangesAsync(
        //            async conflicts => await Task.WhenAll(conflicts.Select(async tracking => await tracking.RefreshAsync(refreshMode))),
        //            retryStrategy);

        public static async Task<EntityEntry> RefreshAsync(this EntityEntry tracking, RefreshConflict refreshMode)
        {
            switch (refreshMode)
            {
                case RefreshConflict.StoreWins:
                    {
                        await tracking.ReloadAsync();
                        break;
                    }
                case RefreshConflict.ClientWins:
                    {
                        PropertyValues databaseValues = await tracking.GetDatabaseValuesAsync();
                        if (databaseValues == null)
                        {
                            tracking.State = EntityState.Detached;
                        }
                        else
                        {
                            tracking.OriginalValues.SetValues(databaseValues);
                        }
                        break;
                    }
                case RefreshConflict.MergeClientAndStore:
                    {
                        PropertyValues databaseValues = await tracking.GetDatabaseValuesAsync();
                        if (databaseValues == null)
                        {
                            tracking.State = EntityState.Detached;
                        }
                        else
                        {
                            PropertyValues originalValues = tracking.OriginalValues.Clone();
                            tracking.OriginalValues.SetValues(databaseValues);
#if EF
                databaseValues.PropertyNames
                    .Where(property => !object.Equals(originalValues[property], databaseValues[property]))
                    .ForEach(property => tracking.Property(property).IsModified = false);
#else
                            databaseValues.Properties
                                .Where(property => !Equals(originalValues[property.Name], databaseValues[property.Name]));
                            //.ForEach(property => tracking.Property(property.Name).IsModified = false);
#endif
                        }
                        break;
                    }
            }
            return tracking;
        }

        public static IEnumerable<KeyValuePair<TEntity, EntityState>> GetTrackEntries<TEntity>(this IDBContext dbContext) where TEntity : class
        {
            var entries = dbContext.ChangeTracker.Entries()
                .Where(x => x.Entity.GetType() == typeof(TEntity))
                .Select(x => new KeyValuePair<TEntity, EntityState>(x.Entity as TEntity, x.State));

            return entries;
        }
    }

    public enum RefreshConflict
    {
        StoreWins,
        ClientWins,
        MergeClientAndStore
    }
}
