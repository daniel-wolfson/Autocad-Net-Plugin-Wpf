//AUTHOR: GERARD CASTELLO
//DATE: 09/25/2011

using Microsoft.Practices.Prism.Interactivity.InteractionRequest;

namespace Intellidesk.AcadNet.Infrastructure.InteractionRequest.Requests
{
    public class CloseRequest : InteractionRequest<Notification>
    {
        #region Methods

        public void Close()
        {
            this.Raise(new Notification());
        }

        #endregion
    }
}
