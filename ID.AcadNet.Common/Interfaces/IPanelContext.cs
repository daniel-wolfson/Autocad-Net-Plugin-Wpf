using Intellidesk.AcadNet.Common.Core;
using System;
using System.Threading.Tasks;

namespace Intellidesk.AcadNet.Common.Interfaces
{
    public interface IPanelDataContext
    {
        Action<object> Load { get; set; }
        void ExecuteRefresh(object value);
        Task<CommandArgs> ExecuteCommand(CommandArgs command);
    }
}