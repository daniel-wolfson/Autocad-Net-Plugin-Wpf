using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Linq;

namespace ID.Infrastructure.Logging
{
    internal class PostgreLoggerProvider : ILoggerProvider
    {
        private static readonly string[] _sqlGenerationComponents = new string[] 
        {
            typeof(BatchExecutor).FullName,
            typeof(IQueryContextFactory).FullName,
        };
        public ILogger CreateLogger(string categoryName)
        {
            if (_sqlGenerationComponents.Contains(categoryName)) {
                return new LoggerFactory().CreateLogger(categoryName);
            }
            return NullLogger.Instance;
        }

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }
    }
}