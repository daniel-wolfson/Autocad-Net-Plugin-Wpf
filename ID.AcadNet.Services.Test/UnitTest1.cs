using System;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Intellidesk.AcadNet.Services.Jig;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ID.AcadNet.Services.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {

        }

        [CommandMethod("PaintTest")]
        public static void PaintTest()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            using (Transaction t = db.TransactionManager.StartTransaction())
            {
                LinetypeTable ltype = t.GetObject(db.LinetypeTableId, OpenMode.ForWrite) as LinetypeTable;
                LinetypeTableRecord ltTblRec = t.GetObject(ltype["ZIGZAG"], OpenMode.ForRead) as LinetypeTableRecord;
                //ConvertTextToImage(ltTblRec.Comments);

                t.Commit();

            }

        }

        [CommandMethod("DL")]
        public void DirectionalLeader()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            var db = doc.Database;

            // Ask the user for the string and the start point of the leader

            var pso = new PromptStringOptions("\nEnter text");
            pso.AllowSpaces = true;
            var pr = ed.GetString(pso);

            if (pr.Status != PromptStatus.OK)
                return;

            var ppr = ed.GetPoint("\nStart point of leader");
            if (ppr.Status != PromptStatus.OK)
                return;

            // Start a transaction, as we'll be jigging a db-resident object

            using (var tr = db.TransactionManager.StartTransaction())
            {
                var bt =
                (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead, false);
                var btr =
                (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite, false);

                // Create and pass in an invisible MLeader
                // This helps avoid flickering when we start the jig

                var ml = new MLeader();
                ml.Visible = false;

                // Create jig

                var jig = new DirectionalLeaderJig(pr.StringResult, ppr.Value, ml);

                // Add the MLeader to the drawing: this allows it to be displayed

                btr.AppendEntity(ml);
                tr.AddNewlyCreatedDBObject(ml, true);

                // Set end point in the jig

                var res = ed.Drag(jig);

                // If all is well, commit

                if (res.Status == PromptStatus.OK)
                {
                    tr.Commit();
                }
            }
        }
    }
}

