//AUTHOR: GERARD CASTELLO
//DATE: 09/25/2011

using System.ComponentModel;

namespace Intellidesk.AcadNet.Infrastructure.InteractionRequest.ViewModels
{
    public class ProgressbarViewModel : IProgressbarViewModel
    {
        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Properties

        public int Step
        {
            get;
            set;
        }

        public string Message
        {
            get;
            private set;
        }

        public string Title
        {
            get;
            private set;
        }

        #endregion

        #region Methods

        public void SetProggessStep(int step)
        {
            this.Step = step;
            this.OnPropertyChanged("Step");
        }

        public void SetProgressMessage(string message)
        {
            this.Message = message;
            this.OnPropertyChanged("Message");
        }

        public void SetTitle(string title)
        {
            this.Title = title;
            this.OnPropertyChanged("Title");
        }        

        public void OnPropertyChanged(string property)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        #endregion
    }
}
