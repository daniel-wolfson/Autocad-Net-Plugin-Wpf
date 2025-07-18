using System;
using System.Windows.Input;
using Autodesk.AutoCAD.ApplicationServices;

namespace Intellidesk.AcadNet.Core
{
    class TextboxCommandHandler : ICommand
    {
#pragma warning disable 67
        public event EventHandler CanExecuteChanged;
#pragma warning restore 67

        public bool CanExecute(object parameter)
        {
            // Yes, we can execute
            return true;
        }

        public void Execute(object parameter)
        {
            // Dump the textbox contents to the command-line
            var tb = parameter as WpfTextBox;
            if (tb != null)
            {
                Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nRibbon Textbox: " + tb.GetTextWithoutNewlines() + "\n");
                tb.ClearText();
            }
        }
    }
}