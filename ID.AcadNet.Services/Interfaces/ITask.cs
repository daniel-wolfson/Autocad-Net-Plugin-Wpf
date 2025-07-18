using System;

namespace Intellidesk.AcadNet.Services.Interfaces
{
    public interface ITask
    {
        string ProcessName { get; set; }
        bool IsExecuted { get; set; }
        bool IsStarted { get; set; }
        bool IsSuccessed { get; set; }
        byte RunOrder { get; set; }
        Func<bool> Action { get; set; }
    }
}