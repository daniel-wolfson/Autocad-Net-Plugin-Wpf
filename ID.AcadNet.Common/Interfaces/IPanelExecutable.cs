using System;
using System.Threading.Tasks;

namespace Intellidesk.AcadNet.Common.Interfaces
{
    public interface IBasePanelContext
    {
        Action<object> Load { get; set; }
        void ExecuteRefreshCommand(object value);
        Task<ICommandArgs> ExecuteCommand(ICommandArgs command);
    }
}