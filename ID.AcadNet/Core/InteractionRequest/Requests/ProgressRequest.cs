//AUTHOR: GERARD CASTELLO
//DATE: 09/25/2011

using System;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;

namespace Intellidesk.AcadNet.Infrastructure.InteractionRequest.Requests
{
    public class ProgressRequest : InteractionRequest<Notification>
    {
        #region Params

        public class ProgressMessage
        {
            public string Title { get; set; }
            public string Message { get; set; }
            public int Step { get; set; }
            public bool Initialize { get; set; }
        }

        #endregion

        #region Methods

        public void ShowDialog(string title, string message, Action<Notification> callback)
        {
            this.SetProgressStep(0, message, title, callback, true);
        }

        public void SetProgressStep(int step)
        {
            this.SetProgressStep(step, null);
        }

        public void SetProgressStep(int step, string messgage)
        {
            this.SetProgressStep(step, messgage, null, null, false);
        }

        public void SetProgressStep(int step, string messgage, string title, Action<Notification> callback, bool init)
        {
            ProgressMessage msg = new ProgressMessage();
            msg.Title = title;
            msg.Message = messgage;
            msg.Step = step;
            msg.Initialize = init;
            this.Raise(new Notification() { Content = msg, Title = title }, callback);
        }

        #endregion
    }
}
