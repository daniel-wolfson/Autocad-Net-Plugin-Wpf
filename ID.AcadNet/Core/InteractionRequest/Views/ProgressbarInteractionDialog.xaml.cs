//AUTHOR: GERARD CASTELLO
//DATE: 09/25/2011

using Intellidesk.AcadNet.Infrastructure.InteractionRequest.Interactions;
using Intellidesk.AcadNet.Infrastructure.InteractionRequest.ViewModels;

namespace Intellidesk.AcadNet.Infrastructure.InteractionRequest.Views
{
    /// <summary>
    /// Interaction logic for ProgressbarInteractionDialog.xaml
    /// </summary>
    public partial class ProgressbarInteractionDialog : InteractionDialogBase, IProgressbarAdapter
    {
        private IProgressbarAdapter adapter;

        public ProgressbarInteractionDialog()
        {
            this.adapter = new ProgressbarAdapter();
            this.DataContext = this.ViewModel;
            InitializeComponent();
        }

        public void SetProggessStep(int step)
        {            
            this.adapter.SetProggessStep(step);            
        }

        public void SetProgressMessage(string message)
        {
            this.adapter.SetProgressMessage(message);
        }

        public void SetTitle(string title)
        {
            this.adapter.SetTitle(title);
        }

        public IProgressbarViewModel ViewModel
        {
            get { return this.adapter.ViewModel; }
        }
    }
}
