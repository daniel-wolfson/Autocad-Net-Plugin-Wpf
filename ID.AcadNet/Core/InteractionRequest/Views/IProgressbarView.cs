//AUTHOR: GERARD CASTELLO
//DATE: 09/25/2011

namespace Intellidesk.AcadNet.Infrastructure.InteractionRequest.Views
{
    public interface IProgressbarView
    {
        void SetProggessStep(int step);
        void SetProgressMessage(string message);
        void SetTitle(string title);
    }
}
