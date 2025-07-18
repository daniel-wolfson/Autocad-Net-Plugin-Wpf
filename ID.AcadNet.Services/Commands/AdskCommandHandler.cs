using Autodesk.Windows;
using System;
using System.Windows.Input;

namespace Intellidesk.AcadNet.Services.Commands
{
    public class AdskCommandHandler : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return true;
        }
        event EventHandler ICommand.CanExecuteChanged
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }
        public void Execute(object parameter)
        {
            var ribBtn = parameter as RibbonButton;
            if (ribBtn != null)
            {
                string sCmd = string.Empty;
                string sSubCmd = ribBtn.CommandParameter.ToString();
                Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.SendStringToExecute(sSubCmd, true, false, true);
            }
        }
    }
}