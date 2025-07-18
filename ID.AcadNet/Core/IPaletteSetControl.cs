using System.Drawing;
using Autodesk.AutoCAD.Windows;

namespace Intellidesk.AcadNet.Helpers
{
    public interface IPaletteSetControl
    {
        /// <summary> Minimize size of palette </summary>
        Size SizeMin { get; set; }

        /// <summary> Maximize size of palette </summary>
        Size SizeMax { get; set; }

        /// <summary> State of palette </summary>
        StateEventIndex PaletteState { get; set; }

        /// <summary> occur on event the State changed of palette </summary>
        void OnStateChanged(object sender, Autodesk.AutoCAD.Windows.PaletteSetStateEventArgs e);
    }
}