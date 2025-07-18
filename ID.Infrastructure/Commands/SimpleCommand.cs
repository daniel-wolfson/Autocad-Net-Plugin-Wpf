using System;
using System.Windows.Input;

namespace ID.Infrastructure.Commands
{
    public class SimpleCommand : ICommand
    {
        public Predicate<object> CanExecuteDelegate { get; set; }
        public Action<object> ExecuteDelegate { get; set; }

        #region ICommand Members

        public bool CanExecute(object parameter)
        {
            if (CanExecuteDelegate != null)
                return CanExecuteDelegate(parameter);
            return true;// if there is no can execute default to true
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            if (ExecuteDelegate != null)
                ExecuteDelegate(parameter);
        }

        #endregion
    }
}

//using (new SysVarOverride("VTENABLE", 7))
//{
//    context.CommandLine.ZoomView(
//        ent.GeometricExtents.MinPoint,
//        ent.GeometricExtents.MaxPoint,
//        (Point3d)parentContext.Parameters["BasePoint"]);
//}
