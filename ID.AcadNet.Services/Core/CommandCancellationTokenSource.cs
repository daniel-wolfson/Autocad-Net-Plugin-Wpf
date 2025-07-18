using System;
using System.Threading;
using Intellidesk.Infrastructure.Core;

namespace Intellidesk.AcadNet.Services.Core
{
    public class CommandCancellationTokenSource : CancellationTokenSource
    {
        public OperationCanceledException OperationCanceledException { get; set; }
        public NotifyArgs NotifyArgs { get; set; }

        public void CancelCommand(NotifyArgs notifyArgs)
        {
            NotifyArgs = notifyArgs;
            this.Cancel(false);
        }
    }
}
