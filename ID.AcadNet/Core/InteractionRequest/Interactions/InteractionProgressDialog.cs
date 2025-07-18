//AUTHOR: GERARD CASTELLO
//DATE: 09/25/2011

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using Intellidesk.AcadNet.Infrastructure.InteractionRequest.Requests;
using Intellidesk.AcadNet.Infrastructure.InteractionRequest.Views;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;

namespace Intellidesk.AcadNet.Infrastructure.InteractionRequest.Interactions
{
    public class InteractionProgressDialog : TriggerAction<Grid>
    {
        #region Properties

        public static readonly DependencyProperty DialogProperty =
            DependencyProperty.Register("Dialog", typeof(InteractionDialogBase), typeof(InteractionProgressDialog), new PropertyMetadata(null));

        public InteractionDialogBase Dialog
        {
            get { return (InteractionDialogBase)GetValue(DialogProperty); }
            set { SetValue(DialogProperty, value); }
        }

        #endregion

        #region Methods

        protected override void Invoke(object parameter)
        {
            var args = parameter as InteractionRequestedEventArgs;
            if (args != null)
            {
                Notification notification = args.Context as Notification;                
                UIElement element = InteractionDialogBase.FindDialog(this.AssociatedObject);
                this.SetDialog(notification, args.Callback, element);
            }
        }

        private void SetDialog(Notification notification, Action callback, UIElement element)
        {
            ProgressRequest.ProgressMessage msg = notification.Content as ProgressRequest.ProgressMessage;
            if (this.Dialog is IProgressbarView)
            {
                IProgressbarView view = (IProgressbarView)this.Dialog;                
                view.SetProggessStep(msg.Step);
                view.SetProgressMessage(msg.Message);
                if (msg.Title != null)
                    view.SetTitle(msg.Title);
            }
            if (element == null)
            {
                EventHandler handler = null;
                handler = (s, e) =>
                {
                    this.Dialog.Closed -= handler;
                    this.AssociatedObject.Children.Remove(this.Dialog);                    
                    callback();
                };

                if (msg.Initialize)
                {
                    this.Dialog.Closed += handler;
                    this.Dialog.SetValue(Grid.RowSpanProperty, this.AssociatedObject.RowDefinitions.Count == 0 ? 1 : this.AssociatedObject.RowDefinitions.Count);
                    this.Dialog.SetValue(Grid.ColumnSpanProperty, this.AssociatedObject.ColumnDefinitions.Count == 0 ? 1 : this.AssociatedObject.ColumnDefinitions.Count);
                    this.AssociatedObject.Children.Add(this.Dialog);
                }
            }
        }

        #endregion
    }
}
