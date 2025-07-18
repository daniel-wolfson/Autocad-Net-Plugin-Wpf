using System;
using System.Diagnostics;
using System.Windows.Input;
using Intellidesk.Common.Interfaces;

namespace Intellidesk.Common.Commands
{
    public class CommandLineModel : ICommandLineModel
    {
        public CommandLineModel(Action<object> execute, Predicate<object> canExecute)
        {
            //if (execute == null)
            //execute = o => (new Intellidesk.AcadNet.Services.CommandService()).SendToExecute(o + " ");
            //throw new ArgumentNullException("execute");

            this._execute = execute;
            this._canExecute = canExecute;
        }

        public CommandLineModel(Action<object> execute) : this(execute, null) { }

        #region ICommand Members

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return this._canExecute == null || this._canExecute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(string command, object parameter = null)
        {
            this._execute(command);
        }

        #endregion

        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;
    }
}