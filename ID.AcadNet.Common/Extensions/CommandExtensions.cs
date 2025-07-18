using Intellidesk.AcadNet.Common.Core;
using Intellidesk.AcadNet.Common.Interfaces;
using Intellidesk.AcadNet.Common.Models;

namespace Intellidesk.AcadNet.Common.Extensions
{
    public static class CommandExtensions
    {
        public static ICommandArgs CreateCommand(this BaseViewModel baseViewModel, string commandName, object commandParameter, bool isGroupCommand = true)
        {
            return new CommandArgs(baseViewModel, commandName, commandParameter, isGroupCommand);
        }
    }
}
