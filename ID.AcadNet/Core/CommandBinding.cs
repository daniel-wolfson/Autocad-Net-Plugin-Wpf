using System;
using System.Windows.Input;

namespace Intellidesk.AcadNet.Core
{
    public class ButtonCommandBinding : ICommand
    {
        //delegate command to register method to be executed
        private readonly Action handler;
        private bool isEnabled;

        /// <summary>
        /// Bind method to be executed to the handler
        /// So that it can direct on event execution
        /// </summary>
        /// <param name="handler"></param>
        public ButtonCommandBinding(Action handler)
        {
            //Specify the method name to the handler
            this.handler = handler;
        }

        public bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                if (value != isEnabled)
                {
                    isEnabled = value;
                    if (CanExecuteChanged != null)
                    {
                        CanExecuteChanged(this, EventArgs.Empty);
                    }
                }
            }
        }

        /// <summary>
        /// method to specify if the event will execute
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public bool CanExecute(object parameter)
        {
            return IsEnabled;
        }

        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// This method is called when the event occurs which transfers the 
        /// controll to the method that has been registered
        /// </summary>
        /// <param name="parameter"></param>
        public void Execute(object parameter)
        {
            //call the method using the handler
            handler();
        }
    }
}

