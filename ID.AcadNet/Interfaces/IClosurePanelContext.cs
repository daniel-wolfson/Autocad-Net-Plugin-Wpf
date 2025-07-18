using Intellidesk.AcadNet.Common.Interfaces;
using Intellidesk.AcadNet.ViewModels;

namespace Intellidesk.AcadNet.Interfaces
{
    public interface IClosurePanelContext : IPanelContext
    {
        ClosurePanelElementContext BodyElementDataContext { get; }
        ISearchViewModel SearchViewModel { get; }
    }
}