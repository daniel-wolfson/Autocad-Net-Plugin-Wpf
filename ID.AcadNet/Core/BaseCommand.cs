using System;
using System.Windows.Input;

namespace Intellidesk.AcadNet.Core
{
    internal class BaseCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private readonly Action _command;
        private readonly Func<bool> _canExecute;

        public BaseCommand(Action command, Func<bool> canExecute = null)
        {
            if (command == null)
                throw new ArgumentNullException("command");
            _canExecute = canExecute;
            _command = command;
        }

        public void Execute(object parameter)
        {
            _command();
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute();
        }
    }
}