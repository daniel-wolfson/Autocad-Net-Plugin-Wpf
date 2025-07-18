using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace ID.Model.Entities
{
    public partial class IdentityContext : DbContext
    {
        //static LoggerFactory object
        public static readonly ILoggerFactory CRPMLoggerFactory =
            LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter((category, level) =>
                        category == DbLoggerCategory.Database.Command.Name
                        && level == LogLevel.Information)
                    .AddConsole();
                //.AddProvider(new ColoredConsoleLoggerProvider(new ColoredConsoleLoggerConfiguration()));
            });

        /// <summary> total count of all savechanges within single transaction /// </summary>
        private int _changesCounter;
        public int GetChangesCounter() => _changesCounter;
        public void ClearChangesCounter() => _changesCounter = 0;

        public override int SaveChanges()
        {
            return _changesCounter += base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _changesCounter += await base.SaveChangesAsync(cancellationToken);
        }
    }
}
