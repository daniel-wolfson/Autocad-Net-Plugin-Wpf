using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Intellidesk.AcadNet.Commands.Draw;
using Intellidesk.AcadNet.Commands.Jig;

[assembly: CommandClass(typeof(CommandsJig1))]
namespace Intellidesk.AcadNet.Commands.Draw
{
    /// <summary>
    /// https://spiderinnet1.typepad.com/blog/2012/05/autocad-net-use-drawjig-to-drag-and-create-rectangles.html
    /// </summary>
    public class CommandsJig1
    {
        public RectangleJig jigger;

        [CommandMethod("PARTNER", "PARTNERDRAWRECTANGLE2", CommandFlags.Session)]
        public void RectangleDrawJig_Method()
        {
            try
            {
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                PromptPointOptions prOpt = new PromptPointOptions("\nCorner1:");
                PromptPointResult pr = ed.GetPoint(prOpt);

                if (pr.Status != PromptStatus.OK) return;

                jigger = new RectangleJig(pr.Value);
                ed.Drag(jigger);

                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    BlockTableRecord btr = (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);

                    Polyline ent = new Polyline();
                    ent.SetDatabaseDefaults();
                    for (int i = 0; i < jigger.Corners.Count; i++)
                    {
                        Point3d pt3d = jigger.Corners[i];
                        Point2d pt2d = new Point2d(pt3d.X, pt3d.Y);
                        ent.AddVertexAt(i, pt2d, 0, db.Plinewid, db.Plinewid);
                    }
                    ent.Closed = true;
                    ent.TransformBy(jigger.UCS);
                    btr.AppendEntity(ent);
                    tr.AddNewlyCreatedDBObject(ent, true);

                    tr.Commit();
                }
            }
            catch (System.Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(ex.ToString());
            }
        }
    }
}
