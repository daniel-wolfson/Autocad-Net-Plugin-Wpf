using ID.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ID.Infrastructure.Exrtensions
{
    public static partial class DbContextExtensions
    {
        // Resolve DBContext connectionString
        public static string ResolveConnectionString(this IServiceProvider provider)
        {
            IConfiguration configuration = (IConfiguration)provider.GetService(typeof(IConfiguration));
            string connectionString = configuration.GetConnectionString("CRPMDatabase");
            return connectionString;
        }

        // Get changed entries
        public static IEnumerable<KeyValuePair<TEntity, EntityState>> GetTrackEntries<TEntity>(this IDBContext dbContext) where TEntity : class
        {
            var entries = dbContext.ChangeTracker.Entries()
                .Where(x => x.Entity.GetType() == typeof(TEntity))
                .Select(x => new KeyValuePair<TEntity, EntityState>(x.Entity as TEntity, x.State));

            return entries;
        }
    }
}
