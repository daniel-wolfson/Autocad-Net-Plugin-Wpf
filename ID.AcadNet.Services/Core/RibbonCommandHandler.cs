using Autodesk.AutoCAD.Customization;
using System;
using System.Windows.Input;

namespace Intellidesk.AcadNet.Services.Core
{
    /// <summary> RibbonCommandHandler sample </summary>
    public class RibbonCommandHandler : ICommand
    {
        private string _layoutFileName = "";

        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter)
        {
            var rb = parameter as RibbonButton;
            if (rb != null)
            {
            }
            return true;
        }

        public void Execute(object parameter)
        {
            //Tools.DocumentAction(_layoutFileName, OptionsDocument.OpenAndActive);
        }
    }
}