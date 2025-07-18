using Intellidesk.AcadNet.Common.Interfaces;
using Intellidesk.AcadNet.ViewModels;

namespace Intellidesk.AcadNet.Interfaces
{
    public interface ICablePanelContext : IPanelContext
    {
        CablePanelElementContext ElementDataContext { get; }
        ISearchViewModel SearchViewModel { get; }
    }
}