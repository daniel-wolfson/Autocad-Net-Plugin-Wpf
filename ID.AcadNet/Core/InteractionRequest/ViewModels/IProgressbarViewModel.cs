//AUTHOR: GERARD CASTELLO
//DATE: 09/25/2011

using System.ComponentModel;
using Intellidesk.AcadNet.Infrastructure.InteractionRequest.Views;

namespace Intellidesk.AcadNet.Infrastructure.InteractionRequest.ViewModels
{
    public interface IProgressbarViewModel : IProgressbarView, INotifyPropertyChanged
    {
        int Step { get; set; }
        string Message { get; }
        string Title { get; }
    }
}
