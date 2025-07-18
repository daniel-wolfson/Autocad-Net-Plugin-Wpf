using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using ID.Infrastructure.Enums;
using ID.Infrastructure.Extensions;
using System.Collections.Generic;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Intellidesk.AcadNet.Common.Internal
{
    public class Linetypes
    {
        public static Dictionary<eCableType, ObjectId> CableTypeCache = new Dictionary<eCableType, ObjectId>();
        public static Dictionary<eClosureType, ObjectId> ClosureTypeCache = new Dictionary<eClosureType, ObjectId>();

        public static void AddCableLinetype(eCableType cabletype)
        {
            if (CableTypeCache.ContainsKey(cabletype)) return;

            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            using (doc.LockDocument())
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                // We'll use the textstyle table to access the "Standard" textstyle for our text segment
                TextStyleTable tt = (TextStyleTable)tr.GetObject(db.TextStyleTableId, OpenMode.ForRead);

                // Get the linetype table from the drawing
                LinetypeTable lt = (LinetypeTable)tr.GetObject(db.LinetypeTableId, OpenMode.ForWrite);

                // Create our new linetype table record...
                var cableDisplayName = cabletype.GetDisplayName();

                if (!lt.Has(cableDisplayName))
                {

                    LinetypeTableRecord ltr = new LinetypeTableRecord
                    {
                        Name = cableDisplayName,
                        AsciiDescription =
                            $"{cableDisplayName} ---- {cableDisplayName} ---- {cableDisplayName} ---- {cableDisplayName} ----",
                        PatternLength = 0.9,
                        NumDashes = 9
                    };

                    // Dash #1
                    ltr.SetDashLengthAt(0, 0.5);

                    // Dash #2
                    ltr.SetDashLengthAt(1, -0.2);
                    ltr.SetShapeStyleAt(1, tt["Standard"]);
                    ltr.SetShapeNumberAt(1, 0);
                    ltr.SetShapeOffsetAt(1, new Vector2d(-0.1, -0.05));
                    ltr.SetShapeScaleAt(1, 0.1);
                    ltr.SetShapeIsUcsOrientedAt(1, false);
                    ltr.SetShapeRotationAt(1, 0);
                    ltr.SetTextAt(1, cableDisplayName);

                    // Dash #3
                    ltr.SetDashLengthAt(2, -0.2);

                    // Add the new linetype to the linetype table
                    ObjectId ltId = lt.Add(ltr);
                    tr.AddNewlyCreatedDBObject(ltr, true);

                    CableTypeCache.Add(cabletype, ltId);

                    // Create a test line with this linetype
                    //BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                    //BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                    //Line ln = new Line(new Point3d(0, 0, 0), new Point3d(10, 10, 0));
                    //ln.SetDatabaseDefaults(db);
                    //ln.LinetypeId = ltId;
                    //btr.AppendEntity(ln);
                    //tr.AddNewlyCreatedDBObject(ln, true);

                    tr.Commit();
                }
            }
        }

        public static void CurrentLineType()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                ObjectId id = db.Celtype;
                LinetypeTableRecord linetype = tr.GetObject(id, OpenMode.ForRead) as LinetypeTableRecord;

                ed.WriteMessage("Previous database line type is " + linetype.Name + "\n");

                LinetypeTable tbl = (LinetypeTable)tr.GetObject(db.LinetypeTableId, OpenMode.ForRead);
                if (!tbl.Has("DASHED"))
                    db.LoadLineTypeFile("DASHED", "acad.lin");

                //now make the linetype  current.
                db.Celtype = tbl["DASHED"];

                linetype = tr.GetObject(db.Celtype, OpenMode.ForRead) as LinetypeTableRecord;

                ed.WriteMessage("Current database line type is " + linetype.Name + "\n");
                tr.Commit();
            }
        }
    }
}
