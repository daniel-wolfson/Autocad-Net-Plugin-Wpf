using Intellidesk.AcadNet.Common.Interfaces;
using Intellidesk.AcadNet.ViewModels;
using System.Windows.Input;

namespace Intellidesk.AcadNet.Interfaces
{
    public interface IClosureConnectPanelContext : IPanelContext
    {
        PaletteElementContext BodyElementDataContext { get; }
        PaletteElementContext MarkerElementDataContext { get; }
        ISearchViewModel SearchViewModel { get; }
        ICommand AddMarkerCommand { get; }
    }
}