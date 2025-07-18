using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interactivity;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;

namespace Intellidesk.AcadNet.Views
{
    class ConfirmationDialog
    {
    }
}

namespace Mm.ConfirmationDialog.Dialog
{
    public class ConfirmationWindowAction : TriggerAction<FrameworkElement>
    {
        protected override void Invoke(object parameter)
        {
            InteractionRequestedEventArgs args = parameter as InteractionRequestedEventArgs;
            if (args != null)
            {
                Confirmation confirmation = args.Context as Confirmation;
                if (confirmation != null)
                {
                    ConfirmationWindow window = new ConfirmationWindow(confirmation);
                    EventHandler closeHandler = null;
                    closeHandler = (sender, e) =>
                    {
                        window.Closed -= closeHandler;
                        args.Callback();
                    };
                    window.Closed += closeHandler;
                    window.Show();
                }
            }

        }
    }
}

