using ID.Infrastructure;
using Intellidesk.AcadNet.Common.Interfaces;

namespace Intellidesk.AcadNet.Common.Models
{
    public class CommandLineBase : CommandBase
    {
        public ICommandLine CommandLine => Plugin.GetService<ICommandLine>();

        public CommandLineBase() : base(null, null)
        {
        }
        public CommandLineBase(string globalCommandName, object commandArgs) : base(globalCommandName, commandArgs)
        {
        }
    }
}
