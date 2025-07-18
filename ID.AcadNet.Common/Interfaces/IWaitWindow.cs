using System;

namespace Intellidesk.AcadNet.Common.Interfaces
{
    public interface IWaitWindow
    {
        void AddTask(string taskName, Func<bool> fn, string onSuccessMessage);
        void AddMessage(string message, int timeout = 0);
        void AddMessage(string message, Action action); //Expression<Func<object, bool>> exp
        void Complete();
        void StartAndWait();
        void Show();
    }
}