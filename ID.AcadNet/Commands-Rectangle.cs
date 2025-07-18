using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using ID.Infrastructure.Commands;
using Intellidesk.AcadNet;
using Intellidesk.AcadNet.Common.Drawing;
using Intellidesk.AcadNet.Common.Extensions;
using Intellidesk.AcadNet.Common.Jig;
using Intellidesk.AcadNet.Common.Models;

[assembly: CommandClass(typeof(CommandsJig))]
namespace Intellidesk.AcadNet
{
    /// <summary>
    /// https://spiderinnet1.typepad.com/blog/2012/05/autocad-net-use-drawjig-to-drag-and-create-rectangles.html
    /// </summary>

    public class CommandsJig : CommandLineBase
    {
        [CommandMethod(CommandNames.UserGroup, "PARTNERDRAWRECTANGLE", CommandFlags.UsePickSet)]
        [CommandMethod("PARTNERDRAWRECTANGLE")]

        public void RectangleDrawJig_Method()
        {
            try
            {
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                PromptPointOptions prOpt = new PromptPointOptions("\nCorner1:");
                PromptPointResult pr = ed.GetPoint(prOpt);

                if (pr.Status != PromptStatus.OK) return;

                RectangleDrawJig jigger = new RectangleDrawJig(pr.Value);
                ed.Drag(jigger);

                var objectId = Drawing.Pline(jigger.Corners);
                objectId.XOpenForWrite<Polyline>(poly =>
                {
                    poly.Closed = true;
                    poly.TransformBy(jigger.UCS);
                });
            }
            catch (System.Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(ex.ToString());
            }
        }
    }
}
