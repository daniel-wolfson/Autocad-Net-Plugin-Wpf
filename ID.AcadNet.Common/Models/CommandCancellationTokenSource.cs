using System;
using System.Threading;

namespace Intellidesk.AcadNet.Common.Models
{
    public class CommandCancellationTokenSource : CancellationTokenSource
    {
        public OperationCanceledException OperationCanceledException { get; set; }
    }
}
