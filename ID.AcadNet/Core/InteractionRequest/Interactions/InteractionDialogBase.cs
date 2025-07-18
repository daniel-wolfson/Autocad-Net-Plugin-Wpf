//AUTHOR: GERARD CASTELLO
//DATE: 09/25/2011

using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using Intellidesk.AcadNet.Infrastructure.InteractionRequest.Views;

namespace Intellidesk.AcadNet.Infrastructure.InteractionRequest.Interactions
{
    public class InteractionDialogBase : UserControl
    {
        #region Events

        public event EventHandler Closed;

        #endregion

        #region Methods

        public void Close()
        {
            this.OnClose(EventArgs.Empty);
        }

        protected virtual void OnClose(EventArgs e)
        {
            var handler = this.Closed;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public static UIElement FindDialog(Grid parent)
        {
            IEnumerator en = parent.Children.GetEnumerator();
            UIElement element = null;
            while (en.MoveNext() && element == null)
            {
                if (en.Current is IProgressbarView)
                    element = en.Current as UIElement;
            }
            return element;
        }

        #endregion

    }
}
