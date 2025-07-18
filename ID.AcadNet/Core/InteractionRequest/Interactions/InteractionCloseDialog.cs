//AUTHOR: GERARD CASTELLO
//DATE: 09/25/2011

using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;

namespace Intellidesk.AcadNet.Infrastructure.InteractionRequest.Interactions
{
    public class InteractionCloseDialog : TriggerAction<Grid>
    {
        #region Methods

        protected override void Invoke(object parameter)
        {
            var args = parameter as InteractionRequestedEventArgs;
            if (args != null)
            {
                Notification notification = args.Context as Notification;
                UIElement element = InteractionDialogBase.FindDialog(this.AssociatedObject);
                if (element != null)
                    this.AssociatedObject.Children.Remove(element);
            }
        }

        #endregion
    }
}
