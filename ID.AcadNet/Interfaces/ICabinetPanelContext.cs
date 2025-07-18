using Intellidesk.AcadNet.Common.Interfaces;
using Intellidesk.AcadNet.ViewModels;

namespace Intellidesk.AcadNet.Interfaces
{
    public interface ICabinetPanelContext : IPanelContext
    {
        CabinetPanelElementContext BodyElementDataContext { get; }
        ISearchViewModel SearchViewModel { get; }
    }
}