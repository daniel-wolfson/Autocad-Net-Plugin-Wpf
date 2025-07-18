using ID.Infrastructure.Models;
using System;
using System.Threading;

namespace Intellidesk.AcadNet.Common.Interfaces
{
    public interface ICommandArgs : IDisposable
    {
        //CancellationTokenSource CancelTokenSource { get; }
        CancellationToken CancelToken { get; }
        Action<ICommandArgs> CommandCallBack { get; set; }
        ICommandLine CommandLine { get; }
        string CommandName { get; set; }
        string CommandGroup { get; set; }
        object CommandParameter { get; set; }
        NotifyArgs NotifyArgs { get; set; }
        object Sender { get; set; }

        void Cancel(NotifyArgs notifyArgs = null);
        void Clean();
        void SendToExecute(Action<ICommandArgs> commandCallBack = null);
        void RunIdle(Action<object> commandCallBack = null);
    }
}