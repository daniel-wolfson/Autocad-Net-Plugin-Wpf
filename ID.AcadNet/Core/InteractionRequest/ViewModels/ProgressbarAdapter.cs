//AUTHOR: GERARD CASTELLO
//DATE: 09/25/2011

namespace Intellidesk.AcadNet.Infrastructure.InteractionRequest.ViewModels
{
    public class ProgressbarAdapter : IProgressbarAdapter
    {
        #region Attributes

        private IProgressbarViewModel viewModel;

        #endregion

        #region Constructors

        public ProgressbarAdapter()
        {
            this.viewModel = new ProgressbarViewModel();
        }

        #endregion

        #region Properties

        public IProgressbarViewModel ViewModel
        {
            get { return this.viewModel; }
        }

        #endregion

        #region Methods

        public void SetProggessStep(int step)
        {
            this.ViewModel.SetProggessStep(step);
        }

        public void SetProgressMessage(string message)
        {
            this.ViewModel.SetProgressMessage(message);
        }

        public void SetTitle(string title)
        {
            this.ViewModel.SetTitle(title);
        }

        #endregion
    }
}
