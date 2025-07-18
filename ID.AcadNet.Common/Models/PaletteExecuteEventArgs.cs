namespace Intellidesk.AcadNet.Common.Models
{
    public class PaletteExecuteEventArgs : PaletteEventArgs
    {
        public PaletteExecuteEventArgs(string paletteName, CommandArgs commandArgs)
            : base(paletteName)
        {
            _commandArgs = commandArgs;
        }

        private CommandArgs _commandArgs;
        public CommandArgs CommandArgs
        {
            get { return _commandArgs; }
        }
    }
}
