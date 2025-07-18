using ID.Infrastructure.Models;
using System.Threading;
using System.Threading.Tasks;

namespace ID.Infrastructure.Interfaces
{
    public interface IReportPublisher
    {
        Task Publish(ApiReportItem reportItem, CancellationToken cancellationToken = default);
    }
}
