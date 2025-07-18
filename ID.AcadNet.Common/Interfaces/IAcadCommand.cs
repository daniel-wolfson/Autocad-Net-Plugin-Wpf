using System.Windows.Input;

namespace Intellidesk.AcadNet.Common.Interfaces
{
    public interface IAcadCommand : ICommand
    {
        /// <summary> command </summary>
        object AcadCommand { get; set; }
        /// <summary> command arguments </summary>
        object AcadCommandArgs { get; set; }
    }
}
