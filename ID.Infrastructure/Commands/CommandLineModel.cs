using ID.Infrastructure.Interfaces;
using System;
using System.Diagnostics;
using System.Windows.Input;

namespace ID.Infrastructure.Commands
{
    public class CommandLineModel : ICommandLineModel
    {
        public CommandLineModel(Action<object> execute, Predicate<object> canExecute)
        {
            //if (execute == null)
            //execute = o => (new Intellidesk.AcadNet.Services.CommandService()).SendToExecute(o + " ");
            //throw new ArgumentNullException("execute");

            _execute = execute;
            _canExecute = canExecute;
        }

        public CommandLineModel(Action<object> execute) : this(execute, null) { }

        #region ICommand Members

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(string command, object parameter = null)
        {
            _execute(command);
        }

        #endregion

        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;
    }
}