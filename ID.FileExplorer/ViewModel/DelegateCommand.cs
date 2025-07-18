using System;
using System.Diagnostics;
using System.Windows.Input;

namespace FileExplorer.ViewModel
{
    public class DelegateCommand1 : ICommand
    {
        #region Fields 
        readonly Action<object> _execute;
        readonly Predicate<object> _canExecute;

        #endregion

        // Fields 
        #region Constructors 
        public DelegateCommand1(Action<object> execute) : this(execute, null) { }

        public DelegateCommand1(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null) throw new ArgumentNullException("execute");
            _execute = execute;
            _canExecute = canExecute;
        }


        #endregion

        // Constructors 
        #region ICommand Members 

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute(parameter);
        }
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        #endregion // ICommand Members }
    }

    public class DelegateCommand1<T> : ICommand where T : class 
    {
        #region Fields 
        readonly Action<T> _execute;
        readonly Predicate<T> _canExecute;

        #endregion

        // Fields 
        #region Constructors 
        public DelegateCommand1(Action<T> execute) : this(execute, null) { }

        public DelegateCommand1(Action<T> execute, Predicate<object> canExecute)
        {
            if (execute == null) throw new ArgumentNullException("execute");
            _execute = execute;
            _canExecute = canExecute;
        }


        #endregion

        // Constructors 
        #region ICommand Members 

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute((T)parameter);
        }
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }

        #endregion // ICommand Members }
    }
}