﻿using System;
using System.Windows.Input;

namespace Intellidesk.AcadNet.ViewModels
{
    public class DelegateCommand : ICommand
    {

        private Action<object> _executeMethod;
        private Func<object, bool> _canExecuteMethod;

        public DelegateCommand(Action<object> executeMethod, Func<object, bool> canExecuteMethod = null)
        {
            if (executeMethod == null)
            {
                throw new ArgumentNullException("executeMethod");
            }
            _executeMethod = executeMethod;
            _canExecuteMethod = canExecuteMethod ?? CanExecute;
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecuteMethod == null)
            {
                return true;
            }
            return _canExecuteMethod(parameter);
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _executeMethod(parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            EventHandler handler = CanExecuteChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
    }
}
