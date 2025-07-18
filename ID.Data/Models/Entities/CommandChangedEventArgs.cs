using System.ComponentModel;

namespace Intellidesk.Data.Models.Entities
{
    public class CommandChangedEventArgs : PropertyChangedEventArgs
    {
        public string CommandName => PropertyName;
        public object CommandArgs { get; set; }

        public CommandChangedEventArgs(string commandName, object commandArgs) : base(commandName)
        {
            CommandArgs = commandArgs;
        }
    }
}
