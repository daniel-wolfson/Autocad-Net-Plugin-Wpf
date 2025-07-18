using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Windows;
using App = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Db = Autodesk.AutoCAD.DatabaseServices;
using Ed = Autodesk.AutoCAD.EditorInput;

namespace Intellidesk.AcadNet.Common.Extensions
{
    /// <summary> General </summary>
    public static class LineTypeExtensions
    {
        public static Editor Ed => Doc.Editor;
        public static Database Db => Doc.Database;
        public static Document Doc => App.DocumentManager.MdiActiveDocument;

        //The current LineType
        private static ObjectId _lineTypeDefaultId;

        /// <summary> LineType </summary>
        public static ObjectId LineType(string value)
        {
            var retValue = Db.Celtype;
            using (var tr = Db.TransactionManager.StartTransaction())
            {
                var lt = (LinetypeTable)tr.GetObject(Db.LinetypeTableId, OpenMode.ForRead);
                //Dim acLineTypTbl As LinetypeTable = WorkSpace.Db.LinetypeTableId.Open(OpenMode.ForRead)
                if (lt != null)
                    if (lt.Has(value) == false)
                    {
                        //Log.Add("Warning", "LineType " + value + " not found. Will be used Solid type");
                    }
                    else
                    {
                        retValue = lt[value];
                    }
                tr.Commit();
                return retValue;
            }
        }

        /// <summary> LineType </summary>
        public static ObjectId LineType(int value = 0)
        {
            using (var tr = Db.TransactionManager.StartTransaction())
            {
                LinetypeTable lt;
                string ltNameDefault = "Continuous";
                switch (value)
                {
                    case -2:
                        //Load All Linetypes
                        using (Doc.LockDocument())
                        {
                            string path = HostApplicationServices.Current.FindFile("acad.lin", Db, FindFileHint.Default);
                            lt = tr.GetObject(Db.LinetypeTableId, OpenMode.ForRead) as LinetypeTable; //LinetypeTable lt = WorkSpace.Db.LinetypeTableId.Open(OpenMode.ForRead)
                            if (lt != null && lt.Has("DASHED") == false)
                            {
                                Db.LoadLineTypeFile("DASHED", path);
                            }
                            //Db.Celtype = lt(ltNameDefault)  '' Set the current linetype
                        }

                        break;
                    case -1:
                        //Dialog mode
                        var ltDlg = new LinetypeDialog { IncludeByBlockByLayer = true };
                        ltDlg.ShowModal();
                        var ltr = tr.GetObject(ltDlg.Linetype, OpenMode.ForRead) as LinetypeTableRecord;
                        if (ltr != null) ltNameDefault = ltr.Name;
                        _lineTypeDefaultId = ltDlg.Linetype;
                        break;
                    case 0:
                        ltNameDefault = "Continuous";
                        break;
                    case 1:
                        ltNameDefault = "DASHED";
                        break;
                    case 2:
                        ltNameDefault = "Center";
                        break;
                    case 3:
                        ltNameDefault = "DOT";
                        break;
                    default:
                        ltNameDefault = "Dashed";
                        break;
                }
                lt = tr.GetObject(Db.LinetypeTableId, OpenMode.ForRead) as LinetypeTable;
                //Dim acLineTypTbl As LinetypeTable = WorkSpace.Db.LinetypeTableId.Open(OpenMode.ForRead)
                if (lt != null && lt.Has(ltNameDefault) == false)
                {
                    Ed.WriteMessage("LineType " + ltNameDefault + " not found");
                    ltNameDefault = "Continuous";
                }
                tr.Commit();
                if (lt != null) _lineTypeDefaultId = lt[ltNameDefault];
            }
            return _lineTypeDefaultId;
        }

        /// <summary> CreateComplexLinetype </summary>
        public static void CreateComplexLinetype()
        {
            var tr = Db.TransactionManager.StartTransaction();
            using (tr)
            {
                // We'll use the textstyle table to access
                // the "Standard" textstyle for our text
                // segment

                var tt = (TextStyleTable)tr.GetObject(Db.TextStyleTableId, OpenMode.ForRead);

                // Get the linetype table from the drawing
                var lt = (LinetypeTable)tr.GetObject(Db.LinetypeTableId, OpenMode.ForWrite);

                // Create our new linetype table record...
                var ltr = new LinetypeTableRecord // ... and set its properties
                {
                    Name = "COLD_WATER_SUPPLY",
                    AsciiDescription = "Cold water supply ---- CW ---- CW ---- CW ----",
                    PatternLength = 0.9,
                    NumDashes = 3
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
                ltr.SetTextAt(1, "CW");

                // Dash #3
                ltr.SetDashLengthAt(2, -0.2);

                // Add the new linetype to the linetype table
                ObjectId ltId = lt.Add(ltr);

                tr.AddNewlyCreatedDBObject(ltr, true);

                // Create a test line with this linetype
                var bt = (BlockTable)tr.GetObject(Db.BlockTableId, OpenMode.ForRead);
                var btr = (BlockTableRecord)tr.GetObject(bt[App.DocumentManager.GetCurrentSpace()], OpenMode.ForWrite);

                var ln = new Line(new Point3d(0, 0, 0), new Point3d(10, 10, 0));

                ln.SetDatabaseDefaults(Db);
                ln.LinetypeId = ltId;

                btr.AppendEntity(ln);
                tr.AddNewlyCreatedDBObject(ln, true);

                tr.Commit();
            }

        }
    }
}