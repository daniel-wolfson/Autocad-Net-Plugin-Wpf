//AUTHOR: GERARD CASTELLO
//DATE: 09/25/2011

using Intellidesk.AcadNet.Infrastructure.InteractionRequest.Views;

namespace Intellidesk.AcadNet.Infrastructure.InteractionRequest.ViewModels
{
    public interface IProgressbarAdapter : IProgressbarView
    {
        IProgressbarViewModel ViewModel { get; }
    }
}
