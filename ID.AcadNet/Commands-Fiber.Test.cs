#define TEST
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using Autodesk.Windows;
using ID.Infrastructure;
using ID.Infrastructure.Commands;
using ID.Infrastructure.Enums;
using ID.Infrastructure.Extensions;
using ID.Infrastructure.Interfaces;
using Intellidesk.AcadNet.Common.Core;
using Intellidesk.AcadNet.Common.Drawing;
using Intellidesk.AcadNet.Common.Enums;
using Intellidesk.AcadNet.Common.Extensions;
using Intellidesk.AcadNet.Common.Interfaces;
using Intellidesk.AcadNet.Common.Internal;
using Intellidesk.AcadNet.Common.Jig;
using Intellidesk.AcadNet.Common.Models;
using Intellidesk.AcadNet.Services;
using Intellidesk.AcadNet.Services.Commands;
using Intellidesk.AcadNet.Services.Core;
using Intellidesk.AcadNet.Services.Extentions;
using Intellidesk.AcadNet.Services.Interfaces;
using Intellidesk.AcadNet.Views;
using Intellidesk.Data.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Resources;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Permissions;
using System.ServiceModel;
using System.Windows.Controls;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using Color = Autodesk.AutoCAD.Colors.Color;
using Exception = System.Exception;
using TextHorizontalMode = Autodesk.AutoCAD.DatabaseServices.TextHorizontalMode;

#if (TEST)
[assembly: CommandClass(typeof(Intellidesk.AcadNet.CommandFiberTest))]
#endif
namespace Intellidesk.AcadNet
{
    public class CommandFiberTest : CommandLineBase
    {
        /// <summary> Application Session Command with localized name </summary>
        [CommandMethod("MapView", CommandFlags.NoHistory)]
        public void MapPalette()
        {
            using (var context = new CommandContext("MapView", "Loading"))
            {
                var tabs = ToolsManager.PaletteTabs;
                var pset = tabs["MapIt"];
                if (pset == null)
                {
                    MapView view = Plugin.GetService<MapView>();
                    //new ParameterOverrides { { "dataContext", PluginBuilder.ProjectExplorerViewModel } });

                    tabs.AddTab(view, true);
                    pset = tabs[view.Name];
                    pset.MinimumSize = new Size(300, 300);
                    pset.MaximumSize = new Size(900, 800);
                    //pset.Size = new Size(ProjectExplorerViewModel.CurrentUserSetting.MinWidth, 300);
                }
                pset.Visible = true;

                context.Clean();
            }
        }

        [CommandMethod("testRectanglejig", CommandFlags.NoHistory)]
        public void RectangleJig()
        {
            IDrawService draw = Plugin.GetService<IDrawService>();
            var rectangle = Drawing.Rectangle(12, 6);
            //RectangleJig jig = new RectangleJig(Point3d.Origin);

            ElementEntityJig jig = new ElementEntityJig(rectangle, new AcadCabinet());

            PromptResult promptResult = jig.Drag(eDragType.Location);

            if (promptResult.Status == PromptStatus.Cancel | promptResult.Status == PromptStatus.Error)
                return;

            var basePoint = jig.BasePoint;

            using (var tr = Db.TransactionManager.StartTransaction())
            {
                jig.GetEntity().XSaveChanges();
                tr.Commit();
            }
        }

        [CommandMethod("testSnapablePolylineJig", CommandFlags.NoHistory)]
        public void SnapablePolylineJig()
        {
            Polyline polyline;

            var service = new PolylineJigService();

            polyline = service.Draw(true, true, 2);

            if (polyline != null)
            {
                // Use a for loop to get each vertex, one by one
                var vn = polyline.NumberOfVertices;

                var list = new List<Point2d>();

                for (var i = 0; i < vn; i++)
                {
                    // Could also get the 3D point here
                    var pt = polyline.GetPoint2dAt(i);
                    list.Add(new Point2d(pt.X, pt.Y));
                }
            }
        }

        [CommandMethod("TestMYPOLY", CommandFlags.NoHistory)]
        public void MyPoly()
        {
            Document doc =
              Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            // Get the current color, for our temp graphics
            Color col = Color.FromColorIndex(ColorMethod.ByAci, 2); //doc.Database.Cecolor;

            // Create a point collection to store our vertices
            Point3dCollection pts = new Point3dCollection();

            // Set up the selection options
            // (used for all vertices)
            PromptPointOptions opt = new PromptPointOptions("\nSelect polyline vertex: ");
            opt.AllowNone = true;

            // Get the start point for the polyline
            PromptPointResult res = ed.GetPoint(opt);
            while (res.Status == PromptStatus.OK)
            {
                // Add the selected point to the list
                pts.Add(res.Value);

                // Drag a temp line during selection
                // of subsequent points
                opt.UseBasePoint = true;
                opt.BasePoint = res.Value;
                res = ed.GetPoint(opt);
                if (res.Status == PromptStatus.OK)
                {
                    // For each point selected,
                    // draw a temporary segment
                    ed.DrawVector(
                      pts[pts.Count - 1], // start point
                      res.Value,          // end point
                      col.ColorIndex,     // current color
                      false);             // highlighted?
                }
            }
            if (res.Status == PromptStatus.None)
            {
                // Get the current UCS
                Matrix3d ucs =
                  ed.CurrentUserCoordinateSystem;
                Point3d origin = new Point3d(0, 0, 0);
                Vector3d normal = new Vector3d(0, 0, 1);
                normal = normal.TransformBy(ucs);

                // Create a temporary plane, to help with calcs
                Plane plane = new Plane(origin, normal);

                // Create the polyline, specifying
                // the number of vertices up front
                Polyline pline = new Polyline(pts.Count);
                pline.Normal = normal;
                foreach (Point3d pt in pts)
                {
                    Point3d transformedPt =
                      pt.TransformBy(ucs);
                    pline.AddVertexAt(
                      pline.NumberOfVertices,
                      plane.ParameterOf(transformedPt),
                      0, 0, 0
                    );
                }

                // Now let's add the polyline to the modelspace
                Transaction tr =
                  db.TransactionManager.StartTransaction();
                using (tr)
                {
                    BlockTable bt =
                      (BlockTable)tr.GetObject(
                        db.BlockTableId,
                        OpenMode.ForRead
                      );
                    BlockTableRecord btr =
                      (BlockTableRecord)tr.GetObject(
                        bt[BlockTableRecord.ModelSpace],
                        OpenMode.ForWrite
                      );
                    ObjectId plineId = btr.AppendEntity(pline);
                    tr.AddNewlyCreatedDBObject(pline, true);
                    tr.Commit();
                    ed.WriteMessage("\nPolyline entity is: " +
                      plineId.ToString()
                    );
                }
            }
            // Clear the temp graphics (polyline should be in
            // the same location, if selection was not cancelled)
            // We could "redraw" instead of "regen" here
            ed.Regen();
        }

        [CommandMethod("TestStationing", CommandFlags.NoHistory)]
        public void Stationing()
        {

            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;

            Database db = doc.Database;

            Editor ed = doc.Editor;

            Matrix3d ucs = ed.CurrentUserCoordinateSystem;

            //TypedValue[] filter = { new TypedValue((int)DxfCode.Start, "LWPOLYLINE") };

            //SelectionFilter filterSset = new SelectionFilter(filter);

            //PromptSelectionOptions pso = new PromptSelectionOptions();

            //pso.SingleOnly = true;//comment this if you want to label multiple polylines

            //pso.MessageForAdding = "Select a single polyline >> ";

            //PromptSelectionResult psr = ed.GetSelection(pso, filterSset);

            //if (psr.Status != PromptStatus.OK)
            //    return;
            double fuzz = (double)Autodesk.AutoCAD.ApplicationServices.Application.GetSystemVariable("dimtxt");
            if (fuzz == 0) fuzz = 0.1;
            //-------------------------------------------------------------------
            PromptPointOptions ppo = new PromptPointOptions("\nPick a point near to the start of curve: ");
            PromptPointResult ppr = default(PromptPointResult);
            ppr = ed.GetPoint(ppo);
            if (ppr.Status != PromptStatus.OK)
            {
                ed.WriteMessage("\nwrong point specification!");
                return;
            }
            Point3d p = ppr.Value;
            //------------------------------------------------------------''
            Vector3d vd = new Vector3d(fuzz, fuzz, 0);
            Point3d pMin = p - vd;
            Point3d pMax = p + vd;
            TypedValue[] tvs = new TypedValue[] { new TypedValue(0, "lwpolyline") };
            Point3dCollection points = new Point3dCollection();
            points.Add(pMin);
            points.Add(pMax);
            SelectionFilter sf = new SelectionFilter(tvs);
            PromptSelectionResult sres = ed.SelectFence(points, sf);
            if (sres.Status != PromptStatus.OK)
            {
                ed.WriteMessage("\nWrong selection!");
                return;
            }

            if (sres.Value.Count == 0)
            {
                ed.WriteMessage("\nNothing selected!");
                return;
            }

            //-------------------------------------------------------------------

            Transaction tr = db.TransactionManager.StartTransaction();

            using (tr)
            {
                try
                {

                    BlockTableRecord btr = (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);
                    //' cast entity as Line
                    ObjectId eid = sres.Value.GetObjectIds()[0];

                    Entity ent = tr.GetObject(eid, OpenMode.ForRead) as Entity;
                    //'-----------------------------------------------------------''
                    Polyline pline = ent as Polyline;

                    if (pline != null)
                    {
                        ed.WriteMessage("\n{0},{1}", pline.StartPoint.X, pline.StartPoint.Y);
                    }
                    //ObjectId[] ids = psr.Value.GetObjectIds();
                    Point3d px = pline.GetClosestPointTo(pMin, false);

                    double leng = pline.Length;

                    bool endclick = false;

                    if ((pline.GetDistAtPoint(px) > leng / 2))
                    {
                        endclick = true;
                    }

                    double theight = db.Textsize;

                    if (theight == 0) theight = db.Dimtxt;

                    //for (int i = 0; i < ids.Length; i++)
                    //{
                    // Autodesk.AutoCAD.DatabaseServices.Polyline pline = tr.GetObject(ids[i], OpenMode.ForRead) as Autodesk.AutoCAD.DatabaseServices.Polyline;

                    double step = 50.0;// <---  change the step to your needs

                    int num = (int)(leng / step);

                    double ang;

                    int n = 1;

                    Plane plan = pline.GetPlane();

                    Line ln = null;

                    DBText txt = null;

                    Vector3d vect = new Vector3d();

                    Vector3d vec = new Vector3d();

                    Point3d ptxt = new Point3d();

                    Point3d ppt = new Point3d();

                    Point3d pptx = new Point3d();

                    if (!endclick)
                    {
                        for (n = 0; n < num + 1; n++)
                        {
                            ptxt = pline.GetPointAtDist(step * n);

                            vec = pline.GetFirstDerivative(ptxt).GetPerpendicularVector();

                            ppt = (ptxt + vec * theight * 8).TransformBy(ucs);

                            pptx = (ptxt + vec * theight * 5).TransformBy(ucs);

                            ln = new Line(ptxt, ppt);

                            btr.AppendEntity(ln);

                            tr.AddNewlyCreatedDBObject(ln, true);

                            vect = ln.GetFirstDerivative(ppt).GetPerpendicularVector();

                            txt = new DBText();

                            txt.Position = pptx;

                            txt.TextString = string.Format("{0:f2}", n * step);

                            txt.HorizontalMode = Autodesk.AutoCAD.DatabaseServices.TextHorizontalMode.TextCenter;

                            txt.VerticalMode = TextVerticalMode.TextBottom;

                            txt.AlignmentPoint = pptx;

                            ang = ln.Angle + Math.PI;

                            if ((ang > Math.PI / 2) || (ang < Math.PI * 1.5))

                                ang = ang + Math.PI;
                            txt.Rotation = ang;

                            btr.AppendEntity(txt);

                            tr.AddNewlyCreatedDBObject(txt, true);

                        }

                        ptxt = pline.EndPoint;

                        vec = pline.GetFirstDerivative(ptxt).GetPerpendicularVector();

                        ppt = (ptxt + vec * theight * 8).TransformBy(ucs);

                        pptx = (ptxt + vec * theight * 5).TransformBy(ucs);

                        ln = new Line(ptxt, ppt);

                        btr.AppendEntity(ln);

                        tr.AddNewlyCreatedDBObject(ln, true);

                        vect = ln.GetFirstDerivative(ppt).GetPerpendicularVector();

                        txt = new DBText();

                        txt.Position = pptx;

                        txt.TextString = string.Format("{0:f2}", leng);

                        txt.HorizontalMode = TextHorizontalMode.TextCenter;

                        txt.VerticalMode = TextVerticalMode.TextBottom;

                        txt.AlignmentPoint = pptx;

                        ang = ln.Angle + Math.PI;

                        if ((ang > Math.PI / 2) || (ang < Math.PI * 1.5))

                            ang = ang + Math.PI;

                        txt.Rotation = ang;

                        btr.AppendEntity(txt);

                        tr.AddNewlyCreatedDBObject(txt, true);
                    }
                    else
                    {
                        //---------------------------------------------------
                        for (n = num; n >= 0; n--)
                        {
                            ptxt = pline.GetPointAtDist(step * n);

                            vec = pline.GetFirstDerivative(ptxt).GetPerpendicularVector();

                            ppt = (ptxt + vec * theight * 8).TransformBy(ucs);

                            pptx = (ptxt + vec * theight * 5).TransformBy(ucs);

                            ln = new Line(ptxt, ppt);

                            btr.AppendEntity(ln);

                            tr.AddNewlyCreatedDBObject(ln, true);

                            vect = ln.GetFirstDerivative(ppt).GetPerpendicularVector();

                            txt = new DBText();

                            txt.Position = pptx;

                            txt.TextString = string.Format("{0:f2}", n * step);

                            txt.HorizontalMode = TextHorizontalMode.TextCenter;

                            txt.VerticalMode = TextVerticalMode.TextBottom;

                            txt.AlignmentPoint = pptx;

                            ang = ln.Angle + Math.PI;

                            if ((ang > Math.PI / 2) || (ang < Math.PI * 1.5))

                                ang = ang + Math.PI;

                            txt.Rotation = ang;

                            btr.AppendEntity(txt);

                            tr.AddNewlyCreatedDBObject(txt, true);

                        }

                        ptxt = pline.EndPoint;

                        vec = pline.GetFirstDerivative(ptxt).GetPerpendicularVector();

                        ppt = (ptxt + vec * theight * 8).TransformBy(ucs);

                        pptx = (ptxt + vec * theight * 5).TransformBy(ucs);

                        ln = new Line(ptxt, ppt);

                        btr.AppendEntity(ln);

                        tr.AddNewlyCreatedDBObject(ln, true);

                        vect = ln.GetFirstDerivative(ppt).GetPerpendicularVector();

                        txt = new DBText();

                        txt.Position = pptx;

                        txt.TextString = string.Format("{0:f2}", leng);

                        txt.HorizontalMode = TextHorizontalMode.TextCenter;

                        txt.VerticalMode = TextVerticalMode.TextBottom;

                        txt.AlignmentPoint = pptx;

                        ang = ln.Angle + Math.PI;

                        if ((ang > Math.PI / 2) || (ang < Math.PI * 1.5))

                            ang = ang + Math.PI;

                        txt.Rotation = ang;

                        btr.AppendEntity(txt);

                        tr.AddNewlyCreatedDBObject(txt, true);
                        //---------------------------------------------------

                    }

                    db.TransactionManager.QueueForGraphicsFlush();

                    //}

                    tr.Commit();
                }
                catch (Autodesk.AutoCAD.Runtime.Exception ex)
                {
                    //MessageBox.Show(string.Format("Error:\n{0}\nTrace:\n{1}", ex.Message, ex.StackTrace));
                }
            }
        }

        [CommandMethod("Testpreg", CommandFlags.UsePickSet | CommandFlags.NoHistory)]
        public void testPolygonRegular()
        {
            Document doc = acadApp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            PromptIntegerOptions pio = new PromptIntegerOptions("");
            pio.Message = "\nNumber of sides: ";

            // Restrict input to positive and non-negative values
            pio.AllowZero = false;
            pio.AllowNegative = false;
            // Add default value
            pio.DefaultValue = 3;
            pio.AllowNone = true;
            // Get the value entered by the user
            PromptIntegerResult ires = ed.GetInteger(pio);
            if (ires.Status != PromptStatus.OK) return;
            int n = ires.Value;

            PromptDoubleOptions pdo = new PromptDoubleOptions("\nEnter radius: ");
            pdo.AllowZero = false;
            pdo.AllowNegative = false;
            pdo.AllowNone = true;
            pdo.UseDefaultValue = true;
            pdo.DefaultValue = 3.0;

            PromptDoubleResult res;
            res = ed.GetDouble(pdo);
            if (res.Status != PromptStatus.OK) return;

            double rad = res.Value;

            double ang = Math.PI * 2 / n;
            Point3d cp, p1, p2;
            // Center point
            cp = new Point3d(0, 0, 0);

            Polyline poly = new Polyline(n);
            Polyline poly2 = new Polyline(n);
            for (int th = 0; th < n; th++)
            {
                // Circumscribed
                p1 = cp.XGetPolarPoint(th * ang, rad / Math.Cos(ang / 2)).TransformBy(Matrix3d.Identity);
                poly.AddVertexAt(th, new Point2d(p1.X, p1.Y), 0, 0, 0);
                // Inscribed
                p2 = cp.XGetPolarPoint(th * ang, rad).TransformBy(Matrix3d.Identity);
                poly2.AddVertexAt(th, new Point2d(p2.X, p2.Y), 0, 0, 0);
            }
            poly.Closed = true;
            poly.ColorIndex = 1;
            poly2.Closed = true;
            poly2.ColorIndex = 3;
            // Add circle for imagination only
            Circle circ = new Circle(new Point3d(0, 0, 0), Vector3d.ZAxis, rad);
            circ.ColorIndex = 5;
            Transaction tr = doc.TransactionManager.StartTransaction();

            using (tr)
            {
                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);
                btr.AppendEntity(poly);
                tr.AddNewlyCreatedDBObject(poly, true);
                btr.AppendEntity(poly2);
                tr.AddNewlyCreatedDBObject(poly2, true);
                btr.AppendEntity(circ);
                tr.AddNewlyCreatedDBObject(circ, true);
                tr.Commit();
            }
        }

        [CommandMethod("testellipsejig", CommandFlags.NoHistory)]
        public static void DoIt()
        {
            Editor ed = acadApp.DocumentManager.MdiActiveDocument.Editor;
            PromptPointOptions opts = new PromptPointOptions("\nEnter Ellipse Center Point:");
            PromptPointResult res = ed.GetPoint(opts);

            Vector3d x = acadApp.DocumentManager.MdiActiveDocument.Database.Ucsxdir;
            Vector3d y = acadApp.DocumentManager.MdiActiveDocument.Database.Ucsydir;
            Vector3d NormalVec = x.CrossProduct(y);

            Database db = acadApp.DocumentManager.MdiActiveDocument.Database;
            Autodesk.AutoCAD.DatabaseServices.TransactionManager tm = db.TransactionManager;

            //Create Ellipsejig
            EllipseJig jig = new EllipseJig(res.Value, NormalVec.GetNormal());
            //first call drag to get the major axis
            jig.setPromptCounter(0);
            acadApp.DocumentManager.MdiActiveDocument.Editor.Drag(jig);
            // Again call drag to get minor axis					
            jig.setPromptCounter(1);
            acadApp.DocumentManager.MdiActiveDocument.Editor.Drag(jig);

            //Append entity.
            using (Transaction myT = tm.StartTransaction())
            {
                BlockTable bt = (BlockTable)tm.GetObject(db.BlockTableId, OpenMode.ForRead, false);
                BlockTableRecord btr = (BlockTableRecord)tm.GetObject(
                    bt[acadApp.DocumentManager.GetCurrentSpace()], OpenMode.ForWrite, false);
                btr.AppendEntity(jig.GetEntity());
                tm.AddNewlyCreatedDBObject(jig.GetEntity(), true);
                myT.Commit();
            }
        }

        [CommandMethod("testDUMPTT", CommandFlags.NoHistory)]
        public void DumpTooltips()
        {
            Document doc = acadApp.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;

            Autodesk.Windows.ComponentManager.ToolTipOpened +=
            (s, e) =>
            {
                Autodesk.Internal.Windows.ToolTip tt = s as Autodesk.Internal.Windows.ToolTip;
                if (tt != null)
                {
                    Autodesk.Windows.RibbonToolTip rtt = tt.Content as Autodesk.Windows.RibbonToolTip;
                    if (rtt == null)
                    {
                        // Basic tooltip
                        ed.WriteMessage("\nTooltip containing basic content \"{0}\".\n", tt.Content);
                    }
                    else
                    {
                        // Enhanced tooltips

                        ed.WriteMessage("\nTooltip containing enhanced content \"{0}\".\n", rtt.Content);
                    }
                }
            };
        }

        private static MyDrawOverrule _drawOverrule;
        [CommandMethod("testTOG", CommandFlags.NoHistory)]
        public static void ToggleOverrule()
        {
            // Initialize Overrule if first time run
            if (_drawOverrule == null)
            {
                _drawOverrule = new MyDrawOverrule();
                Overrule.AddOverrule(RXObject.GetClass(typeof(Line)), _drawOverrule, false);
                Overrule.Overruling = true;
            }
            else
            {
                // Toggle Overruling on/off
                Overrule.Overruling = !Overrule.Overruling;
            }
            // Regen is required to update changes on screen
            Application.DocumentManager.MdiActiveDocument.Editor.Regen();
        }

        [CommandMethod("TestInsertBlock", CommandFlags.NoHistory)]
        public void InsertBlock()
        {
            var blockName = "gis_point";
            var appSettings = Plugin.GetService<IPluginSettings>();
            var fullPath = blockName.Contains("\\") ? blockName : appSettings.ResourceLib;
            //Db.ImportBlock(blockName);
            Db.InsertBlock(blockName, Point3d.Origin, new BlockOptions(null, 1, eJigPrompt.PromptInsert));
        }

        [CommandMethod("TestAddCustomData", CommandFlags.NoHistory)]
        public void AddCustomData()
        {
            Document doc = acadApp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            PromptResult pr = Ed.GetString("\nBlock name: ");
            if (pr.Status != PromptStatus.OK) return;
            string blockName = pr.StringResult;

            using (Transaction tr = Db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)tr.GetObject(Db.BlockTableId, OpenMode.ForRead);
                if (!bt.Has(blockName))
                {
                    Ed.WriteMessage("\nNone block '{0}' in the document block table.", blockName);
                    return;
                }
                BlockTableRecord curSpace =
                    (BlockTableRecord)tr.GetObject(Db.CurrentSpaceId, OpenMode.ForWrite);

                // Add the block reference to Database first
                BlockReference br = new BlockReference(Point3d.Origin, bt[blockName]);
                br.TransformBy(Ed.CurrentUserCoordinateSystem);
                curSpace.AppendEntity(br);
                tr.AddNewlyCreatedDBObject(br, true);

                // Get the block definition
                BlockTableRecord btr =
                    (BlockTableRecord)tr.GetObject(bt[blockName], OpenMode.ForRead);
                BlockJig jig;
                if (btr.HasAttributeDefinitions)
                {
                    // Add attribute references to the block reference
                    foreach (ObjectId id in btr)
                    {
                        if (id.ObjectClass.Name == "AcDbAttributeDefinition")
                        {
                            AttributeDefinition attDef =
                                (AttributeDefinition)tr.GetObject(id, OpenMode.ForRead);
                            AttributeReference attRef = new AttributeReference();
                            attRef.SetAttributeFromBlock(attDef, br.BlockTransform);
                            ObjectId attId = br.AttributeCollection.AppendAttribute(attRef);
                            tr.AddNewlyCreatedDBObject(attRef, true);
                        }
                    }
                    // Create a BlockAttribJig instance
                    jig = new BlockAttribJig(br);
                }
                else
                {
                    // Create a BlockJig instance
                    jig = new BlockJig(br);
                }
                // Drag the block reference
                pr = Ed.Drag(jig);
                if (pr.Status != PromptStatus.OK) br.Erase();
                tr.Commit();
            }
        }

        [CommandMethod("TestAddCustomData2", CommandFlags.NoHistory)]
        public void AddCustomData2()
        {
            Document doc = acadApp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            PromptEntityResult per = Ed.GetEntity("\nSelect an entity: ");
            if (per.Status != PromptStatus.OK)
                return;
            AddCustomData(per.ObjectId,
                "CustomData",
                new TypedValue((int)DxfCode.Thickness, 5.0),
                new TypedValue((int)DxfCode.Real, 35.0));
        }

        [CommandMethod("TestAddCustomData3", CommandFlags.NoHistory)]
        public void AddCustomData3()
        {
            Document doc = acadApp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            PromptEntityResult per = Ed.GetEntity("\nSelect an entity: ");
            if (per.Status != PromptStatus.OK)
                return;
            using (Transaction tr = Db.TransactionManager.StartTransaction())
            {
                DBObject obj = tr.GetObject(per.ObjectId, OpenMode.ForRead);
                obj.XAddXrecord(ObjectId.Null, "CustomData",
                    new TypedValue((int)DxfCode.Thickness, 5.0),
                    new TypedValue((int)DxfCode.Real, 35.0));
                tr.Commit();
            }
        }

        private void AddCustomData(ObjectId id, string key, params TypedValue[] values)
        {
            Database db = id.Database;
            using (Transaction tr = Db.TransactionManager.StartTransaction())
            {
                DBObject obj = tr.GetObject(id, OpenMode.ForRead);
                ObjectId dictId = obj.ExtensionDictionary;
                if (dictId == ObjectId.Null)
                {
                    obj.UpgradeOpen();
                    obj.CreateExtensionDictionary();
                    dictId = obj.ExtensionDictionary;
                }
                DBDictionary xdict = (DBDictionary)tr.GetObject(dictId, OpenMode.ForRead);
                Xrecord xrec;
                if (xdict.Contains(key))
                {
                    xrec = (Xrecord)tr.GetObject((ObjectId)xdict[key], OpenMode.ForWrite);
                }
                else
                {
                    xdict.UpgradeOpen();
                    xrec = new Xrecord();
                    xdict.SetAt(key, xrec);
                    tr.AddNewlyCreatedDBObject(xrec, true);
                }
                xrec.Data = new ResultBuffer(values);
                tr.Commit();
            }
        }

        [CommandMethod("TestBLOG", CommandFlags.NoHistory)]
        public async void OpenBlog()
        {
            const string url = "http://www.google.co.il"; //file://C:\Projects\IntelliDesk\ID.AcadNet\Views\MapIt.html

            // As we're calling an async function, we need to await
            // (and mark the command itself as async)

            //if (await CommandLine.PageExists(url))
            //{
            //    // Now that we've validated the URL, we can call the
            //    // new API in AutoCAD 2015 to load our page

            //    Application.DocumentWindowCollection.AddDocumentWindow("Kean's blog", new System.Uri(url));
            //}
            //else
            //{
            //    Ed.WriteMessage("\nCould not load url: \"{0}\".", url);
            //}
        }


        [CommandMethod("TestLV", CommandFlags.NoHistory)]
        public void ListVertices()
        {
            IEnumerable<Entity> entities = Selects.GetEntities(GetSelectOptions.GetObjects, "Select path of polyline:", null, EntityTypes.LWPOLYLINE);
            List<Point3d> points = entities.SelectMany(x => x.XGetPoints()).ToList();

            //Entity ent = Draw.Polyline(new Point3dCollection(points.ToArray()), "0", 1, 2, 0);

            new Polyline { Layer = "0", ColorIndex = 1, Closed = false, LinetypeScale = 0 }
                    .XTransform(new Point3dCollection(points.ToArray()), 2, Selects.Db.ByLayerLinetype)
                    .XSaveChangesCommit();
        }

        [CommandMethod("TESTTestPolylineJig", CommandFlags.NoHistory)]
        public void TestPolylineJig()
        {
            Document doc = acadApp.DocumentManager.MdiActiveDocument;
            try
            {
                PromptResult jigRes;
                eCableType cableType = eCableType.Cable144x12x12;
                PlineJig jigger = new PlineJig(doc.Database.GetUcsMatrix());

                do
                {
                    jigRes = doc.Editor.Drag(jigger);
                    if (jigRes.Status == PromptStatus.OK)
                        jigger.AllVertexes.Add(jigger.LastPoint);
                } while (jigRes.Status == PromptStatus.OK);

                using (Transaction tr = Db.TransactionManager.StartTransaction())
                {
                    BlockTableRecord btr = (BlockTableRecord)tr.GetObject(Db.CurrentSpaceId, OpenMode.ForWrite);
                    Polyline ent = new Polyline();
                    ent.SetDatabaseDefaults();

                    ObjectId linetypeId;
                    Linetypes.CableTypeCache.TryGetValue(cableType, out linetypeId);
                    if (linetypeId != ObjectId.Null)
                    {
                        ent.LinetypeId = linetypeId;
                        ent.Color = Color.FromColorIndex(ColorMethod.ByAci, (short)cableType.GetAttribute<DataInfoAttribute>().ColorIndex);
                    }

                    for (int i = 0; i < jigger.AllVertexes.Count; i++)
                    {
                        Point3d pt3D = jigger.AllVertexes[i];
                        Point2d pt2D = new Point2d(pt3D.X, pt3D.Y);
                        ent.AddVertexAt(i, pt2D, 0, Db.Plinewid, Db.Plinewid);
                    }

                    ent.TransformBy(jigger.Ucs);
                    btr.AppendEntity(ent);
                    tr.AddNewlyCreatedDBObject(ent, true);
                    tr.Commit();
                }
            }
            catch (System.Exception ex)
            {
                Doc.Editor.WriteMessage(ex.ToString());
            }
        }

        [CommandMethod("TestPolylineAddVertex", CommandFlags.NoHistory)]
        public void TestPolylineAddVertex()
        {
            PromptEntityOptions peo = new PromptEntityOptions("\nSelect a segment where to add a vertex: ");
            peo.SetRejectMessage("\nIncorrect entity");
            peo.AllowNone = false;
            peo.AllowObjectOnLockedLayer = false;
            peo.AddAllowedClass(typeof(Polyline), true);
            PromptEntityResult per = Ed.GetEntity(peo);
            if (per.Status == PromptStatus.OK)
            {
                Matrix3d UCS = Ed.CurrentUserCoordinateSystem;
                ObjectId objId = per.ObjectId;
                try
                {
                    using (Transaction tr = Db.TransactionManager.StartTransaction())
                    {
                        Polyline pline = tr.GetObject(objId, OpenMode.ForRead, false) as Polyline;
                        if (pline != null)
                        {
                            Point3d pickPt = PickPointOnPline(pline, per.PickedPoint);
                            double param = pline.GetParameterAtPoint(pickPt);
                            int index = (int)param;

                            Matrix3d OCS = Matrix3d.PlaneToWorld(pline.Normal);
                            Point3d transPt = pickPt.TransformBy(OCS);

                            if (!OCS.CoordinateSystem3d.Zaxis.IsEqualTo(UCS.CoordinateSystem3d.Zaxis))
                            {
                                Ed.CurrentUserCoordinateSystem = plineUCS(pline, index);
                            }

                            Int16 aperture = (Int16)acadApp.GetSystemVariable("APERTURE");
                            double viewsize = (double)acadApp.GetSystemVariable("VIEWSIZE");
                            Point2d screensize = (Point2d)acadApp.GetSystemVariable("SCREENSIZE");
                            double tol = 2 * aperture * viewsize / screensize.Y;
                            Tolerance tolerance = new Tolerance(tol, tol);

                            int endParam = pline.Closed == true ? pline.NumberOfVertices : pline.NumberOfVertices - 1;
                            Vector3d vec = new Vector3d();

                            using (Polyline ghost = new Polyline())
                            {
                                ghost.ColorIndex = 7;

                                if ((pline.Closed == false) && pickPt.IsEqualTo(pline.GetPoint3dAt(0), tolerance))
                                {
                                    vec = pline.GetFirstDerivative(0);
                                    double bulge = pline.GetBulgeAt(0);
                                    double width = pline.GetStartWidthAt(0);
                                    Point2d p0 = new Point2d(transPt.X, transPt.Y);
                                    Point2d p1 = pline.GetPoint2dAt(0);

                                    ghost.AddVertexAt(0, p0, bulge, width, width);
                                    ghost.AddVertexAt(1, p1, bulge, width, width);
                                    ghost.Normal = pline.Normal;
                                    ghost.Elevation = pline.Elevation;

                                    VertexJig jig = new VertexJig(ghost, pickPt, 0, vec, bulge, width, width);

                                    PromptResult res = Ed.Drag(jig);
                                    if (res.Status == PromptStatus.OK)
                                    {
                                        pline.UpgradeOpen();
                                        pline.AddVertexAt(index, ghost.GetPoint2dAt(0), ghost.GetBulgeAt(0), width, width);
                                    }
                                }
                                else if ((pline.Closed == false) && pickPt.IsEqualTo(pline.GetPoint3dAt(endParam), tolerance))
                                {
                                    vec = pline.GetFirstDerivative(endParam);
                                    double bulge = pline.GetBulgeAt(index);
                                    double width = pline.GetEndWidthAt(endParam);
                                    Point2d p0 = pline.GetPoint2dAt(endParam);
                                    Point2d p1 = new Point2d(transPt.X, transPt.Y);

                                    ghost.AddVertexAt(0, p0, bulge, width, width);
                                    ghost.AddVertexAt(1, p1, bulge, width, width);
                                    ghost.Normal = pline.Normal;
                                    ghost.Elevation = pline.Elevation;

                                    VertexJig jig = new VertexJig(ghost, pickPt, 1, vec, bulge, width, width);

                                    PromptResult res = Ed.Drag(jig);
                                    if (res.Status == PromptStatus.OK)
                                    {
                                        pline.UpgradeOpen();
                                        pline.AddVertexAt(endParam + 1, ghost.GetPoint2dAt(1), ghost.GetBulgeAt(0), width, width);
                                        pline.SetBulgeAt(endParam, ghost.GetBulgeAt(0));
                                    }
                                }

                                else
                                {
                                    double bulge = pline.GetBulgeAt(index);
                                    double sWidth = pline.GetStartWidthAt(index);
                                    double eWidth = pline.GetEndWidthAt(index);
                                    Point2d p0 = pline.GetPoint2dAt(index);
                                    Point2d p1 = new Point2d(transPt.X, transPt.Y);
                                    Point2d p2;
                                    if (pline.Closed == false)
                                        p2 = pline.GetPoint2dAt(index + 1);
                                    else
                                    {
                                        try { p2 = pline.GetPoint2dAt(index + 1); }
                                        catch { p2 = pline.GetPoint2dAt(0); }
                                    }

                                    ghost.AddVertexAt(0, p0, bulge, sWidth, 0.0);
                                    ghost.AddVertexAt(1, p1, bulge, 0.0, eWidth);
                                    ghost.AddVertexAt(2, p2, 0.0, 0.0, 0.0);
                                    ghost.Normal = pline.Normal;
                                    ghost.Elevation = pline.Elevation;

                                    VertexJig jig = new VertexJig(ghost, pickPt, 1, vec, bulge, sWidth, eWidth);

                                    PromptResult res = Ed.Drag(jig);
                                    if (res.Status == PromptStatus.OK)
                                    {
                                        pline.UpgradeOpen();
                                        pline.SetEndWidthAt(index, ghost.GetStartWidthAt(1));
                                        pline.AddVertexAt(index + 1, ghost.GetPoint2dAt(1), ghost.GetBulgeAt(1), ghost.GetStartWidthAt(1), eWidth);
                                        pline.SetBulgeAt(index, ghost.GetBulgeAt(0));
                                    }
                                }
                            }
                            Ed.CurrentUserCoordinateSystem = UCS;
                        }
                        tr.Commit();
                    }
                }
                catch (System.Exception ex)
                {
                    Ed.CurrentUserCoordinateSystem = UCS;
                    Ed.WriteMessage("Error: " + ex.Message);
                }
            }
        }

        [CommandMethod("TestPolylineRemoveVertex", CommandFlags.NoHistory)]
        public void TestPolylineRemoveVertex()
        {
            PromptEntityOptions peo = new PromptEntityOptions("\nSelect the vertex to remove: ");
            peo.SetRejectMessage("\nIncorrect entity");
            peo.AllowNone = false;
            peo.AllowObjectOnLockedLayer = false;
            peo.AddAllowedClass(typeof(Polyline), true);
            PromptEntityResult per = Ed.GetEntity(peo);
            if (per.Status == PromptStatus.OK)
            {
                ObjectId objId = per.ObjectId;
                try
                {
                    using (Transaction tr = Db.TransactionManager.StartTransaction())
                    {
                        Polyline pline = tr.GetObject(objId, OpenMode.ForWrite, false) as Polyline;
                        if (pline != null)
                        {
                            if (pline.NumberOfVertices > 2)
                            {
                                Point3d pickPt = PickPointOnPline(pline, per.PickedPoint);
                                double param = pline.GetParameterAtPoint(pickPt);
                                int index = (int)param;
                                if ((param - Math.Truncate(param)) > 0.5)
                                    index += 1;
                                pline.RemoveVertexAt(index);
                            }
                            else
                                Ed.WriteMessage("\nOnly two vertices left");
                        }
                        tr.Commit();
                    }
                }
                catch (System.Exception ex)
                {
                    Ed.WriteMessage("Error: " + ex.Message);
                }
            }
        }

        [CommandMethod("TestPickPointOnPline", CommandFlags.NoHistory)]
        private Point3d PickPointOnPline(Polyline pl, Point3d pt)
        {
            pt = pt.TransformBy(Ed.CurrentUserCoordinateSystem);
            Vector3d vdir = Ed.GetCurrentView().ViewDirection;
            pt = pt.Project(pl.GetPlane(), vdir);
            return pl.GetClosestPointTo(pt, false);
        }

        private Matrix3d plineUCS(Polyline pline, int param)
        {
            Point3d origin = pline.GetPoint3dAt(param);
            Vector3d xDir = origin.GetVectorTo(pline.GetPoint3dAt(param + 1)).GetNormal();
            Vector3d zDir = pline.Normal;
            Vector3d yDir = zDir.CrossProduct(xDir).GetNormal();
            return new Matrix3d(new double[16]{
        xDir.X, yDir.X, zDir.X, origin.X,
        xDir.Y, yDir.Y, zDir.Y, origin.Y,
        xDir.Z, yDir.Z, zDir.Z, origin.Z,
        0, 0, 0, 1});
        }

        [CommandMethod("SaveClassToEntityXData", CommandFlags.NoHistory)]
        public void SaveClassToEntityXData()
        {
            Database db = acadApp.DocumentManager.MdiActiveDocument.Database;
            Editor ed = acadApp.DocumentManager.MdiActiveDocument.Editor;

            PromptEntityResult per =
             ed.GetEntity("Select entity to save class to:\n");
            if (per.Status != PromptStatus.OK)
                return;

            // Create an object

            MyClass mc = new MyClass
            {
                myDouble = 1.2345,
                myString = "Some text"
            };

            // Save it to the document
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                Entity ent = (Entity)tr.GetObject(per.ObjectId, OpenMode.ForWrite);
                mc.SaveToEntity(ent);
                tr.Commit();
            }

            // Write some info about the results
            ed.WriteMessage("Content of MyClass we serialized:\n {0} \n", mc.ToString());
        }

        [CommandMethod("GetClassFromEntityXData", CommandFlags.NoHistory)]
        public void GetClassFromEntityXData()
        {
            Database db = acadApp.DocumentManager.MdiActiveDocument.Database;
            Editor ed = acadApp.DocumentManager.MdiActiveDocument.Editor;

            PromptEntityResult per =
             ed.GetEntity("Select entity to get class from:\n");
            if (per.Status != PromptStatus.OK)
                return;

            // Get back the class 

            using (
          Transaction tr = db.TransactionManager.StartTransaction())
            {
                Entity ent =
                 (Entity)tr.GetObject(per.ObjectId, OpenMode.ForRead);

                MyClass mc = (MyClass)MyClass.NewFromEntity(ent);

                // Write some info about the results

                ed.WriteMessage(
             "Content of MyClass we deserialized:\n {0} \n",
             mc.ToString());

                tr.Commit();
            }
        }

        [CommandMethod("ADNPLUGINS", "3PCIR", CommandFlags.Modal | CommandFlags.NoHistory)]
        public void CreateCircle()
        {
            Document doc = acadApp.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;

            // Ask the user to select the first two points outside the jig
            PromptPointOptions ppo = new PromptPointOptions("\nSelect first point") { AllowNone = false };
            PromptPointResult ppr = ed.GetPoint(ppo);
            if (ppr.Status != PromptStatus.OK)
                return;
            Point3d first = ppr.Value;

            ppo.Message = "\nSelect second point";
            ppr = ed.GetPoint(ppo);
            if (ppr.Status != PromptStatus.OK)
                return;
            Point3d second = ppr.Value;

            // Pass the points into the jig
            ThreePtCircleJig cj = new ThreePtCircleJig(first, second);

            // Then execute it
            PromptResult pr = ed.Drag(cj);

            if (pr.Status != PromptStatus.OK)
                return;

            Transaction tr = doc.TransactionManager.StartTransaction();
            using (tr)
            {
                // Add our circle to the current space

                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(doc.Database.CurrentSpaceId, OpenMode.ForWrite);

                Entity ent = cj.GetEntity();
                btr.AppendEntity(ent);
                tr.AddNewlyCreatedDBObject(ent, true);

                tr.Commit();
            }
        }

        private PaletteSet _ps;
        ResourceManager _resourceManager;

        [CommandMethod("Test")]
        public void ShowWpfPalette()
        {
            _resourceManager = new ResourceManager("Intellidesk.AcadNet.Properties.Resources", this.GetType().Assembly);
            if (Application.Version.Major == 17)
            {
                if (Application.Version.Minor == 1)
                    return; //AutoCAD 2008
            }

            if (_ps == null)
            {
                _ps = new PaletteSet("WPF Palette")
                {
                    Size = new Size(400, 600),
                    DockEnabled = (DockSides)((int)DockSides.Left + (int)DockSides.Right)
                };
                //var uc = new MyWPFUserControl();

                //Autodesk.Windows.RibbonControl ribControl = Autodesk.Windows.ComponentManager.Ribbon;

                var ribControl = new RibbonControl();

                var ribTab = new RibbonTab { Title = "Test", Id = "Test" };
                ribControl.Tabs.Add(ribTab);

                var ribSourcePanel = new Autodesk.Windows.RibbonPanelSource
                {
                    Title = "My Tools",
                    DialogLauncher = new RibbonCommandItem { CommandHandler = new AdskCommandHandler() }
                };

                //Add a Panel
                var ribPanel = new RibbonPanel { Source = ribSourcePanel };
                ribTab.Panels.Add(ribPanel);

                //Create button
                var ribButton1 = new RibbonButton
                {
                    Text = "Line\nGenerator",
                    CommandParameter = "Line ",
                    ShowText = true,
                    //LargeImage = ImageHelper.GetBitmap((Bitmap)_resourceManager.GetObject("shield_16")),
                    //Image = ImageHelper.GetBitmap((Bitmap)_resourceManager.GetObject("shield_16")),
                    Size = RibbonItemSize.Large,
                    Orientation = Orientation.Vertical,
                    ShowImage = true
                };
                ribButton1.ShowText = true;
                ribButton1.CommandHandler = new AdskCommandHandler();
                ribSourcePanel.Items.Add(ribButton1);

                //uc.Content = ribControl;

                //_ps.AddVisual("Test", uc);
            }

            _ps.KeepFocus = true;
            _ps.Visible = true;
        }

        //[CommandMethod("Intellidesk", "IDPOINTONMAPURI", CommandFlags.Session)]
        public void MapInfoGetDataUri(string uri)
        {
            using (var context = new CommandContext("IDPOINTONMAPURI", "GetData"))
            {
                // Create a request using a URL that can receive a post. 
                var request = WebRequest.Create("http://vmmapinfo.partnergsm.co.il/AcadNetGis/PartnerGis/");
                // Set the Method property of the request to POST.
                request.Method = "POST";
                // Create POST data and convert it to a byte array.
                string postData = "This is a test that posts this string to a Web server.";
                byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(postData);
                // Set the ContentType property of the WebRequest.
                request.ContentType = "application/X-www-form-urlencoded";
                // Set the ContentLength property of the WebRequest.
                request.ContentLength = byteArray.Length;
                // Get the request stream.
                System.IO.Stream dataStream = request.GetRequestStream();
                // Write the data to the request stream.
                dataStream.Write(byteArray, 0, byteArray.Length);
                // Close the Stream object.
                dataStream.Close();
                // Get the response.
                WebResponse response = request.GetResponse();
                // Display the status.
                context.CommandLine.WriteMessage(((HttpWebResponse)response).StatusDescription);
                // Get the stream containing content returned by the server.
                dataStream = response.GetResponseStream();
                // Open the stream using a StreamReader for easy access.
                System.IO.StreamReader reader = new System.IO.StreamReader(dataStream);
                // Read the content.
                string responseFromServer = reader.ReadToEnd();

                //var jsonSerializer = new System.Web.Serialization.JavaScriptSerializer.JavaScriptSerializer();
                //var arguments = (IDictionary<string, object>)jsonSerializer.DeserializeObject(e.Argument);
                //var current = jsonSerializer.Deserialize<StateItem>(responseFromServer);

                // Display the content.
                context.CommandLine.WriteMessage(responseFromServer);
                // Clean up the streams.
                reader.Close();
                if (dataStream != null) dataStream.Close();
                response.Close();

                context.Clean();
            }
        }

        private void SendToMapByChannel(double x, double y)
        {
            try
            {
                //using (var cmd = new CommandContext("IDPOINTONMAPCHANNEL", "Sending"))
                //{
                Point3d pnt = Point3d.Origin;
                var ids = Ed.GetSelectImplied();

                if (ids.Count == 0)
                {
                    PromptPointsResult promptPointsResult = Ed.GetPoints("\nEnter the point to send to map", 1);
                    if (promptPointsResult.Status == PromptStatus.OK && promptPointsResult.Value.Any())
                        pnt = promptPointsResult.Value.FirstOrDefault();
                    else
                    {
#if DEBUG
                        //cmd.Cancel("point not selected");
                        //throw new CommandCanceledException("Cancel", new Exception("point not selected"));
#else
                        //throw new OperationCanceledException("Cancel", this.CommandInProcessList[CommandNames.].CancellationToken);
#endif
                    }
                }

                var config = ConfigurationManager<IMapInfoAcmv>.GetConfiguration();
                var group = config.GetSectionGroup();
                var url = group.Client.Endpoints[0].Address;
                var binding = new BasicHttpBinding(); //group.Client.Endpoints[0].Binding;
                var endpoint = new EndpointAddress(url);
                var channelFactory = new ChannelFactory<IMapInfoAcmv>(binding, endpoint);

                IMapInfoAcmv client = null;
                try
                {
                    client = channelFactory.CreateChannel();
                    client.SetViewrXY(pnt.X, pnt.Y);
                    ((ICommunicationObject)client).Close();
                }
                catch (Exception ex)
                {
                    if (client != null)
                        ((ICommunicationObject)client).Abort();
                    //Logger.Error("channelFactory error: ", ex);

#if DEBUG
                    //CommandLine.InProcessList[CommandNames.PointOnMap].IsCanceled = true;
                    throw new CommandCanceledException("Error", ex);
#else
                    throw new OperationCanceledException(ex.ToMessage(CommandNames.PointOnMap),
                        this.CommandInProcessList[CommandNames.PointOnMap].CancellationToken);
#endif
                }
                //}
            }
            catch (OperationCanceledException ex)
            {
                Log.Logger.Error(ex.InnerException.InnerException?.Message ?? ex.Message);
                //this.CommandClose(ex, CommandNames.PointOnMap);
            }
            catch (CommandCanceledException ex)
            {
                Log.Logger.Error(ex.InnerException.InnerException?.Message ?? ex.Message);
                //this.CommandClose(ex, CommandNames.PointOnMap);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex.InnerException.InnerException?.Message ?? ex.Message);
                //this.CommandClose("Error", ex.ToNotifyMessage(), CommandNames.PointOnMap);
            }

        }

        public void MiNote(int miWindowID, string message)
        {
            //System.IntPtr MIDispatchPtr = new IntPtr(MIWindowID);
            //DMapInfo MIConnection = (DMapInfo)Marshal.GetObjectForIUnknown(MIDispatchPtr);
            //MIConnection.Do(String.Format("Note \"Note from CSharp: {0}\"", Message));
            //DMBApplications Applications = (DMBApplications)MIConnection.MBApplications;
            //foreach (DMapBasicApplication mbApp in Applications)
            //{
            //    MIConnection.Do(String.Format("Note \"MB App. running in this MapInfo instance: {0}\"", mbApp.Name));
            //}

            //Create a command string that contains the path to our mbx to be run.
            //string appcommand = "Run Application " + "\"" + @"RemoteTest.MBX" + "\"";
            //mapinfoinstance.Do("Open Table \"" + pathName + mapName + ".tab\" Interactive");
            //mapinfoinstance.Do("Open Table " + mapName + " Interactive");
            //name - вызываемый метод, BindingFlags.InvokeMethod - способ связывания, null - связывающий класс, comMapIObject - СОМ объект, Args - аргументы метода
            //comMapIType.InvokeMember("do", BindingFlags.InvokeMethod, null, comMapIObject, new object[] { "Open Table \"" + pathName + mapName + ".tab\"" });

            //Create a instance of Mapinfo
            //string evalResult = mapinfoinstance.Eval("FrontWindow()");
            //var windowId = Int32.Parse(evalResult);
            //var centerX = 5;
            //var centerY = 5;
            //var mapperZoom = 1;

            //var mapinfo = (MapInfoApplication)Marshal.GetActiveObject("Mapinfo.Application");
            //mapinfo.Do("Print \"Hello Partner\" ");
            ////mapinfo.Do("Add Map Auto Layer KAV0912");
            ////mapinfo.Do("Close Table KAV0912 Interactive
            //mapinfo.Do("Open Table \"" + pathName + mapName + ".tab\" Interactive "); //Open Table "C:\Users\Daniel\My Projects\Parner AutoCAD\Y-009-012\GIS\KAV0912.tab" Interactive
            //mapinfo.Do("Map From " + mapName + " ");

            //mapinfo.Do("SearchRect(FrontWindow( ), 0, 0, 2, 2) ");
            //var oType = Type.GetTypeFromProgID("MapInfo.Application");
            //Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(oType.FullName);
            //object o = Activator.CreateInstance(oType);
            //oType.InvokeMember("do", BindingFlags.InvokeMethod, null, o, new object[] { "Set Application Window " + UIManager.PaletteSetCurrent.Handle });
            //oType.InvokeMember("do", BindingFlags.InvokeMethod, null, o, new object[] { "Set Next Document Parent " + UIManager.PaletteSetCurrent.Handle + " Style 1" });
            //oType.InvokeMember("do", BindingFlags.InvokeMethod, null, o, new object[] { "Open Table \"" + pathName + mapName + ".tab\"" });

            //MapInfoApplication _objMI = new MapInfoApplicationClass();
            //_objMI = (MapInfoApplication)Marshal.GetActiveObject("MapInfo.Application");
            //string returnValue = mapinfo.Eval("Set Application Window " + UIManager.PaletteSetCurrent.Handle);
            //returnValue = mapinfo.Eval("Set Next Document Parent " + UIManager.PaletteSetCurrent.Handle + " Style 1");
        }

        public void XrefAttachAtOrigin()
        {
            using (CommandContext context = new CommandContext(CommandNames.PasteAsBlock, "Paste"))
            {
                CommandContext parentContext;
                CommandContext.Cache.TryGetValue(CommandNames.CopyAsBlock, out parentContext);
                if (parentContext != null)
                {
                    var refId = Db.InsertBlock(
                        parentContext.Parameters["BlockName"].ToString(),
                        (Point3d)parentContext.Parameters["BasePoint"],
                        new BlockOptions("Temp", 1, eJigPrompt.PromptInsert));


                    //// Ask the user to specify a file to attach
                    //var opts = new PromptOpenFileOptions("Select Reference File");
                    //opts.Filter = "Drawing (*.dwg)|*.dwg";
                    //var pr = Ed.GetFileNameForOpen(opts);
                    //if (pr.Status == PromptStatus.OK)

                    // Attach the specified file and insert it at the origin

                    //Xref
                    //ObjectId xRefId = context.XrefAttachAndInsert(parentContext.Parameters["BlockName"].ToString(), Point3d.Origin); //pr.StringResult
                    //Ed.WriteMessage("External reference {0}attached at the origin.", xRefId == ObjectId.Null ? "" : "not ");

                    Entity ent = refId.XCast<BlockReference>();
                    if (ent != null)
                    {
                        if (refId != ObjectId.Null)
                        {
                            using (new SysVarOverride("VTENABLE", 7))
                            {
                                context.CommandLine.Zoom(refId);
                            }
                        }
                        context.Ed.WriteMessage(PluginSettings.Prompt + "block inserted successfully");
                        CommandContext value2;
                        CommandContext.Cache.TryRemove(parentContext.CommandName, out value2);
                    }
                }
            }

        }
    }

    // We need it to help with deserialization
    public sealed class MyBinder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            return Type.GetType($"{typeName}, {assemblyName}");
        }
    }

    // Helper class to write to and from ResultBuffer
    [Serializable]
    public abstract class MyBaseClass : ISerializable
    {
        public const string appName = "MyApp";

        public static object NewFromResBuf(ResultBuffer resBuf)
        {
            BinaryFormatter bf = new BinaryFormatter();
            bf.Binder = new MyBinder();

            MemoryStream ms = resBuf.ResBufToStream();
            MyBaseClass mbc = (MyBaseClass)bf.Deserialize(ms);
            return mbc;
        }

        public static object NewFromEntity(Entity ent)
        {
            using (ResultBuffer resBuf = ent.GetXDataForApplication(appName))
            {
                return NewFromResBuf(resBuf);
            }
        }

        public ResultBuffer SaveToResBuf()
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, this);
            ms.Position = 0;

            ResultBuffer resBuf = ms.StreamToResBuf(appName);
            return resBuf;
        }

        public void SaveToEntity(Entity ent)
        {
            // Make sure application name is registered
            // If we were to save the ResultBuffer to an Xrecord.Data,
            // then we would not need to have a registered application name

            Transaction tr = ent.Database.TransactionManager.TopTransaction;

            RegAppTable regTable = (RegAppTable)tr
                .GetObject(ent.Database.RegAppTableId, OpenMode.ForWrite);

            if (!regTable.Has(MyClass.appName))
            {
                RegAppTableRecord app = new RegAppTableRecord { Name = MyClass.appName };
                regTable.Add(app);
                tr.AddNewlyCreatedDBObject(app, true);
            }

            using (ResultBuffer resBuf = SaveToResBuf())
            {
                ent.XData = resBuf;
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public abstract void GetObjectData(SerializationInfo info, StreamingContext context);
    }

    [Serializable]
    public class MyClass : MyBaseClass
    {
        public string myString;
        public double myDouble;

        public MyClass()
        {
        }

        protected MyClass(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new System.ArgumentNullException("info");

            myString = (string)info.GetValue("MyString", typeof(string));
            myDouble = (double)info.GetValue("MyDouble", typeof(double));
        }

        [SecurityPermission(SecurityAction.LinkDemand,
         Flags = SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(
         SerializationInfo info, StreamingContext context)
        {
            info.AddValue("MyString", myString);
            info.AddValue("MyDouble", myDouble);
        }

        // Just for testing purposes

        public override string ToString()
        {
            return base.ToString() + "," +
             myString + "," + myDouble.ToString();
        }
    }

}
