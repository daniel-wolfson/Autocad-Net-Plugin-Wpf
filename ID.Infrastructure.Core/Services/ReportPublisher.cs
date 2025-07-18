using ID.Infrastructure.Interfaces;
using System.Threading.Tasks;
using Serilog.Extensions.Logging;
using ID.Infrastructure.Models;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace ID.Infrastructure.Services
{
    public class ReportPublisher : IReportPublisher
    {
        private readonly ILogger<ReportPublisher> _logger;

        public ReportPublisher(ILogger<ReportPublisher> logger)
        {
            _logger = logger;
        }

        public async Task Publish(object report, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Doing heavy publishing logic ...");

            await Task.Delay(2500, cancellationToken);

            //_logger.LogInformation("\"{Name} by {Author}\" has been published!", report.Name, report.Author);
        }

        public Task Publish(ApiReportItem book, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }
    }
}
