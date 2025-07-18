using System.Drawing;

namespace Intellidesk.AcadNet.Interfaces
{
    public interface IPaletteSetControl
    {
        /// <summary> Minimize size of palette </summary>
        Size SizeMin { get; set; }

        /// <summary> Maximize size of palette </summary>
        Size SizeMax { get; set; }

        /// <summary> State of palette </summary>
        Autodesk.AutoCAD.Windows.StateEventIndex PaletteState { get; set; }

        /// <summary> occur on event the State changed of palette </summary>
        //void OnStateChanged(object Sender, Autodesk.AutoCAD.Windows.PaletteSetStateEventArgs e);
    }
}