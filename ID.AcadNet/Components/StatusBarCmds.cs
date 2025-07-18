using Autodesk.AutoCAD.Runtime;
using Intellidesk.AcadNet.Common.Utils;
using Intellidesk.AcadNet.Components;

[assembly: CommandClass(typeof(StatusBarCmds))]

namespace Intellidesk.AcadNet.Components
{
    public class StatusBarCmds
    {
        public StatusBarCmds() { }

        [CommandMethod("ShowDrawingStatusBars")]
        public static void ShowDrawingStatusBars()
        {
            Utils.ShowDrawingStatusBars(true);
        }

        [CommandMethod("HideDrawingStatusBars")]
        public static void HideDrawingStatusBars()
        {
            Utils.ShowDrawingStatusBars(false);
        }
    }
}
