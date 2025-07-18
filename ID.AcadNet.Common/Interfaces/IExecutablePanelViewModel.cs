using Intellidesk.AcadNet.Common.Core;

namespace Intellidesk.AcadNet.Common.Interfaces
{
    public interface IExecutablePanelViewModel : IBasePanelContext
    {
        event PaletteExecuteStartEventhandler PaletteExecuteStarted;
        event PaletteExecuteCompleteEventhandler PaletteExecuteCompleted;
        //event PropertyChangedEventHandler PropertyChanged;
    }
}