using System;
using System.Diagnostics;
using System.Windows.Input;

namespace ID.Infrastructure.Commands
{
    public class CommandModel : ICommand
    {
        public CommandModel(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public CommandModel(Action<object> execute) : this(execute, null) { }

        #region ICommand Members

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return canExecute == null ? true : canExecute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { System.Windows.Input.CommandManager.RequerySuggested += value; }
            remove { System.Windows.Input.CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            execute(parameter);
        }

        #endregion

        private readonly Action<object> execute;
        private readonly Predicate<object> canExecute;
    }

    //searchByNameCommand = new SimpleCommand
    //{
    //     CanExecuteDelegate = X => !String.IsNullOrEmpty(SearchText),
    //     ExecuteDelegate = X => myDataView.Filter = stateObj => ((Person)stateObj).Name.Contains(SearchText)
    //};
}
