using System;
using System.Windows.Input;
using Autodesk.Windows;

namespace Intellidesk.AcadNet.Core
{
    /// <summary>  Command Sample  </summary>
    public class CommandSample : ICommand
    {
        #region Constructor

        public CommandSample(Action<RibbonButton> action, Predicate<RibbonButton> canExecutean)
        {
            ExecuteDelegate = action;
            CanExecuteDelegate = CanExecute;
        }

        #endregion

        #region Properties

        public Predicate<RibbonButton> CanExecuteDelegate { get; set; }
        public Action<RibbonButton> ExecuteDelegate { get; set; }

        #endregion

        #region ICommand Members

        public bool CanExecute(object parameter)
        {
            if (CanExecuteDelegate != null)
            {
                return CanExecuteDelegate(parameter as RibbonButton);
            }

            return true;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            if (ExecuteDelegate != null)
            {
                ExecuteDelegate(parameter as RibbonButton);
            }
        }

        #endregion
    }
}