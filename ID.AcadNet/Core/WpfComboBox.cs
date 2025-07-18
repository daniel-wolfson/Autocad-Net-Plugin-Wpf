using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.Windows;

namespace Intellidesk.AcadNet.Core
{
    class WpfComboBox : RibbonCombo
    {
        public WpfComboBox() { }

        protected override void OnEditableTextChanged(RibbonPropertyChangedEventArgs args)
        {
            base.OnEditableTextChanged(args);
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nRibbon ComboBox new value: " + args.NewValue + "\n");
        }
    }
}