using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace General.Contexts
{
    /// <summary> Default dbContext factory by connectionString </summary>
    internal class DbContextFactory<TContext> : IDesignTimeDbContextFactory<TContext> where TContext : DbContext
    {
        public TContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<TContext>();
            var connectionString = args[0];
            builder.UseSqlServer(connectionString);

            var dbContext = (TContext)Activator.CreateInstance(typeof(TContext), builder.Options);
            return dbContext;
        }
    }
}
