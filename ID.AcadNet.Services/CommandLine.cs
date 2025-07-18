using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Customization;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using ID.Infrastructure.Enums;
using ID.Infrastructure.Extensions;
using ID.Infrastructure.Interfaces;
using ID.Infrastructure.Models;
using Intellidesk.AcadNet.Common.Core;
using Intellidesk.AcadNet.Common.Enums;
using Intellidesk.AcadNet.Common.Extensions;
using Intellidesk.AcadNet.Common.Interfaces;
using Intellidesk.AcadNet.Common.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Exception = System.Exception;

namespace Intellidesk.AcadNet.Services
{
    public class CommandLine : CommandDocumentBase, ICommandLine
    {
        public void Ok(string commandName, string text = null)
        {
            text = text ?? "";
            Notifications.SendNotifyMessageAsync(new NotifyArgs(NotifyStatus.Ready, text, commandName + ": success", commandName));
        }

        #region <Commands: SendToExecute, LoadCuixMenu, ...>

        public static void SendCancel()
        {
            acadApp.DocumentManager.MdiActiveDocument.SendStringToExecute(Convert.ToString(Convert.ToChar(27)), true, false, false); //"\x1B"
        }

        public void Cancel()
        {
            Services.CommandLine.SendCancel();
        }

        public void Error(string commandName, string message)
        {
            var inProcessList = Doc.GetInProcessList();
            if (inProcessList.ContainsKey(commandName))
            {
                ITaskArguments commandArgs = inProcessList[commandName];
                //args.CancellationToken = new CancellationToken(true);
                if (!String.IsNullOrEmpty(message))
                    commandArgs.CommandInfo.Add(NotifyStatus.Error, message);
                commandArgs.IsCanceled = true;
            }
            //Close(commandName, new NotifyArgs(NotifyImageStatus.Error, text, toolTip, commandName));
            //Cancellation.Cancel();
        }

        public string Current()
        {
            var inProcessList = Doc.GetInProcessList();
            return inProcessList != null && inProcessList.Count > 0
                ? inProcessList.Last().Value.DisplayName
                : "";
        }

        public void Enter()
        {
            Doc.SendStringToExecute(Convert.ToString(Convert.ToChar(13)), true, false, false);
        }

        public void Redraw()
        {
            Doc.SendStringToExecute(".redraw ", true, false, false);
        }

        public void Execute(string commandName, ITaskArguments commandArgs)
        {
            if (Doc.UserData.ContainsKey(commandName))
                Doc.UserData.Remove(commandName);

            Doc.UserData.Add(commandName, commandArgs);
            this.SendToExecute(commandName);
        }

        /// <summary> SendToExecute method call it is new mechanism from version 2015 </summary>
        public void Execute(params object[] commandArgs)
        {
            Editor ed = acadApp.DocumentManager.MdiActiveDocument.Editor;
            ed.Command(commandArgs); //new Object[] { "_.LINE", "0,0,0", "10,10,0", "" }
        }

        public void SendToExecute(string commandName, string commandParams = "", int tCmdEcho = 0,
            int tFileDia = 0, bool tActivate = true, bool tWrapUpInactiveDoc = false, bool tEchoCommand = false)
        {
            var oldCmdEcho = acadApp.GetSystemVariable("CMDECHO");
            var oldFileDia = acadApp.GetSystemVariable("FILEDIA");

            acadApp.SetSystemVariable("FILEDIA", tFileDia);
            acadApp.SetSystemVariable("CMDECHO", tCmdEcho);

            //var manualResetEvent = new ManualResetEvent(false);

            if (commandParams == "")
                Doc.SendStringToExecute(commandName.TrimEnd() + " ", tActivate, tWrapUpInactiveDoc, tEchoCommand);
            else
            {
                //Commands.InProcessList.Add(commandName, new TaskArguments() { Command = commandParams });
                Doc.SendStringToExecute("(command \"" + commandName + "\" \" " + commandParams + "\") ",
                    tActivate, tWrapUpInactiveDoc, tEchoCommand);
                //manualResetEvent.WaitOne();
            }

            acadApp.SetSystemVariable("FILEDIA", oldFileDia);
            acadApp.SetSystemVariable("CMDECHO", oldCmdEcho);
        }

        public void ReloadLinetype()
        {
            var db = acadApp.DocumentManager.MdiActiveDocument.Database;
            var trans = db.TransactionManager.StartTransaction();
            var bReload = false;

            using (trans)
            {
                var table = trans.GetObject(db.LinetypeTableId, OpenMode.ForRead) as LinetypeTable;
                if (table != null && table.Has("CENTER")) bReload = true;
            }

            var fileDia = (Int16)acadApp.GetSystemVariable("FILEDIA");
            acadApp.SetSystemVariable("FILEDIA", 0);

            //reload using linetype command...
            var acadObject = Application.AcadApplication;
            var activeDocument = acadObject.GetType()
                .InvokeMember("ActiveDocument", BindingFlags.GetProperty, null, acadObject, null);

            var dataArry = new object[1];
            if (bReload)
                dataArry[0] = "-linetype Load CENTER\nacad.lin\nYes\n ";
            else
                dataArry[0] = "-linetype Load CENTER\nacad.lin\n ";

            activeDocument.GetType()
                .InvokeMember("SendCommand", BindingFlags.InvokeMethod, null, activeDocument, dataArry);
            acadApp.SetSystemVariable("FILEDIA", fileDia);
        }

        public ResultBuffer InvokeLisp(ResultBuffer args)
        {
            //IntPtr rb = IntPtr.Zero;
            //ResultBuffer stat = Application.Invoke(args); //(args.UnmanagedObject, out rb)
            //if (stat == (int)PromptStatus.OK && rb != IntPtr.Zero)
            //    return (ResultBuffer)DisposableWrapper.Create(typeof(Resu​ltBuffer), rb, true);
            return null;
        }

        public void Regen()
        {
            // Redraw the drawing
            acadApp.UpdateScreen();

            Ed.UpdateScreen();

            // Regenerate the drawing
            Ed.Regen();
        }

        public void WriteMessage(string s)
        {
            // A simple helper to write to the command-line
            Ed.WriteMessage(s);
        }

        public void Alert(string alert)
        {
            acadApp.ShowAlertDialog(alert);
        }

        public void LoadCuixMenu(ref Assembly assem, string sectionName = "")
        {
            if (String.IsNullOrEmpty(sectionName))
                sectionName = Path.GetFileNameWithoutExtension(assem.Location);
            var myCuiFile = (Path.GetDirectoryName(assem.Location) + "\\" + sectionName + ".cuix").Replace("\\", "\\\\");
            var myCuiFileToSend = sectionName + ".cuix";
            //Dim currentWorkSpace As String = CType(Application.GetSystemVariable("WSCURRENT"), String)
            var mainCui = Application.GetSystemVariable("MENUNAME") + ".cuix";
            var cs = new CustomizationSection(mainCui);

            PartialCuiFileCollection pcfc = cs.PartialCuiFiles;
            if (pcfc.Contains(myCuiFile) | pcfc.Contains(myCuiFileToSend.ToLower()))
            {
                Ed.WriteMessage(Convert.ToChar(10).ToString(CultureInfo.InvariantCulture) + "Customization file \"" +
                                myCuiFileToSend + "\" already loaded.");
            }
            else
            {
                if (File.Exists(sectionName + ".cuix"))
                {
                    try
                    {
                        SendToExecute("_CUILOAD", myCuiFileToSend);
                        //LoadLispCmd("_WSCURRENT", "AutoCAD Classic")
                    }
                    catch (Exception)
                    {
                        //Log.Add(ex);
                        //Log.Add("Error: " + MethodBase.GetCurrentMethod().GetFullNameBundle(), "\nCustomization file \"" + myCuiFile + "\" error.");
                    }
                }
                else
                {
                    //var msg = "Error: " + MethodBase.GetCurrentMethod().GetFullNameBundle() + "\nCustomization file \"" + myCuiFile + "\" does not exist - building it.";
                    //Log.Add(new Exception(msg));
                    //Log.Add("Error: " + MethodBase.GetCurrentMethod().GetFullNameBundle(), "\nCustomization file \"" + myCuiFile + "\" does not exist - building it.");
                }
            }
        }

        //[CommandMethod("MyGroup", "AppInters", "AppIntersLocal", CommandFlags.UsePickSet)]
        public void AppInters() // This method can have any name
        {
            //_geomService.GetIntersect();
        }

        #endregion"

        #region <Commands: View, Zoom>

        /// <summary> Zoom Ext </summary>
        public void Zoom()
        {
            SendToExecute("._zoom _e ");
        }

        /// <summary> Zoom by Point3d[] </summary>
        public void Zoom(Point3d[] points)
        {
            var min2D = new Point2d(points[0].X - (points[1].X - points[0].X) / 5, points[0].Y);
            var max2D = new Point2d(points[1].X + (points[1].X - points[0].X) / 5, points[1].Y);
            var view = new ViewTableRecord
            {

                CenterPoint = new Point2d(min2D.X + (max2D.X - min2D.X) / 2, min2D.Y + (max2D.Y - min2D.Y) / 2),
                Height = max2D.Y - min2D.Y,
                Width = max2D.X - min2D.X
            };
            Ed.SetCurrentView(view);
        }

        /// <summary> Zoom by window </summary>
        public void Zoom(double tXmin, double tYmin, double tXmax, double tYmax)
        {
            var min2D = new Point2d(tXmin - (tXmax - tXmin) / 5, tYmin);
            var max2D = new Point2d(tXmax + (tXmax - tXmin) / 5, tYmax);
            var view = new ViewTableRecord
            {
                CenterPoint = new Point2d(min2D.X + (max2D.X - min2D.X) / 2, min2D.Y + (max2D.Y - min2D.Y) / 2),
                Height = max2D.Y - min2D.Y,
                Width = max2D.X - min2D.X
            };
            Ed.SetCurrentView(view);
            Thread.Sleep(500);
        }

        /// <summary> Zoom List of entity </summary>
        public void Zoom(List<Entity> tObj, double dFactor = 1)
        {
            Zoom(new ObjectIdCollection(tObj.Select(x => x.ObjectId).ToArray()), dFactor);
        }

        /// <summary> Zoom entity </summary>
        public void Zoom(Entity ent, double dFactor = 1)
        {
            ZoomWindow(ent.GeometricExtents.MinPoint, ent.GeometricExtents.MinPoint);
            //Zoom(new ObjectIdCollection { ent.ObjectId }, dFactor);
        }

        /// <summary> Zoom objectId </summary>
        public void Zoom(ObjectId objectId, double dFactor = 1)
        {
            Zoom(new ObjectIdCollection { objectId }, dFactor);
        }

        /// <summary> Zoom ObjectIdCollection </summary>
        public void Zoom(ObjectIdCollection tObjIds, double dFactor = 1)
        {
            var pntMin = Point3d.Origin;
            var pntMax = Point3d.Origin;

            if (tObjIds.Count > 0)
            {
                try
                {
                    using (Doc.LockDocument())
                    {
                        using (var tr = Db.TransactionManager.StartTransaction())
                        {
                            var flg = true;
                            foreach (ObjectId objId in tObjIds)
                            {
                                if (objId != ObjectId.Null)
                                {
                                    DBObject obj = tr.GetObject(objId, OpenMode.ForRead);
                                    if (flg)
                                    {
                                        if (obj.Bounds != null)
                                        {
                                            pntMin = obj.Bounds.Value.MinPoint;
                                            pntMax = obj.Bounds.Value.MaxPoint;
                                        }
                                        else if (obj is BlockReference)
                                        {
                                            Extents3d ext = ((BlockReference)obj).XGeometricExtents();
                                            var deltaX = (ext.MaxPoint.X - ext.MinPoint.X) / 2;
                                            var deltaY = (ext.MaxPoint.Y - ext.MinPoint.Y) / 2;

                                            var xMin = ((BlockReference)obj).Position.X - deltaX;
                                            var yMin = ((BlockReference)obj).Position.Y - deltaY;
                                            var xMax = ((BlockReference)obj).Position.X + deltaX;
                                            var yMax = ((BlockReference)obj).Position.Y + deltaY;

                                            pntMin = new Point3d(xMin, yMin, 0);
                                            pntMax = new Point3d(xMax, yMax, 0);
                                            dFactor = (dFactor == 1) ? 17 : dFactor + 17;
                                        }
                                        else if (obj is Entity)
                                        {
                                            Extents3d ext = ((Entity)obj).GeometricExtents;
                                            ext.TransformBy(Ed.CurrentUserCoordinateSystem.Inverse());
                                            pntMin = ext.MinPoint;
                                            pntMax = ext.MaxPoint;
                                        }
                                        flg = false;
                                    }

                                    if (obj.Bounds != null)
                                    {
                                        pntMin = new Point3d(Math.Min(obj.Bounds.Value.MinPoint.X, pntMin.X),
                                            Math.Min(obj.Bounds.Value.MinPoint.Y, pntMin.Y), 0);
                                        pntMax = new Point3d(Math.Max(obj.Bounds.Value.MaxPoint.X, pntMax.X),
                                            Math.Max(obj.Bounds.Value.MaxPoint.Y, pntMax.Y), 0);
                                    }
                                }
                            }

                        }
                    }

                    //CommandLine.ZoomWindow(pntMin, pntMax, dFactor);
                    this.ZoomView(pntMin, pntMax, new Point3d(), dFactor);
                }
                catch (Exception)
                {
                    //Log.Add(ex);
                    //Log.Add("Error", ProjectManager.NameMsg + "<XAddNew> {0}, {1}", ex.Message, ex.Source);
                }
            }
        }

        public void ZoomWindow(Point3d pntMin, Point3d pntMax, double dFactor = 1)
        {
            if (dFactor != 1)
            {
                pntMin = new Point3d(pntMin.X - (pntMax.X - pntMin.X) * dFactor, pntMin.Y - (pntMax.Y - pntMin.Y) * dFactor, pntMin.Z);
                pntMax = new Point3d(pntMax.X + (pntMax.X - pntMin.X) * dFactor, pntMax.Y + (pntMax.Y - pntMin.Y) * dFactor, pntMin.Z);
            }

            Point2d pMin = new Point2d(pntMin.X, pntMin.Y);
            Point2d pMmax = new Point2d(pntMax.X, pntMax.Y);

            using (Transaction acTrans = Db.TransactionManager.StartTransaction())
            {
                using (ViewTableRecord view = new ViewTableRecord())
                {
                    view.CenterPoint = pMin + ((pMmax - pMin) / 2.0);
                    view.Height = pMmax.Y - pMin.Y;
                    view.Width = pMmax.X - pMin.X;
                    view.IsPaperspaceView = !Db.TileMode && Db.PaperSpaceVportId == Ed.CurrentViewportObjectId;
                    Ed.SetCurrentView(view);
                }
                acTrans.Commit();
            }
        }

        public void ZoomWindowSend(Point3d pntMin, Point3d pntMax, double dFactor = 1)
        {
            SendToExecute("._zoom _w " +
                                  Convert.ToString(pntMin.X - (pntMax.X - pntMin.X) * dFactor) + "," +
                                  Convert.ToString(pntMin.Y - (pntMax.Y - pntMin.Y) * dFactor) + " " +
                                  Convert.ToString(pntMax.X + (pntMax.X - pntMin.X) * dFactor) + "," +
                                  Convert.ToString(pntMax.Y + (pntMax.Y - pntMin.Y) * dFactor) + " ");
        }

        public void ZoomView(Point3d pntMin, Point3d pntMax, Point3d? pntCenter, double dFactor = 1)
        {
            var pCenter = pntCenter == null ? new Point3d() : (Point3d)pntCenter;

            // Get the extents of the current space no points 
            // or only a center point is provided
            // Check to see if Model space is current
            if (Db.TileMode)
            {
                if (pntMin.Equals(new Point3d()) && pntMax.Equals(new Point3d()))
                {
                    pntMin = Db.Extmin;
                    pntMax = Db.Extmax;
                }
            }
            else
            {
                int nCurVport = Convert.ToInt32(acadApp.GetSystemVariable("CVPORT"));
                // Check to see if Paper space is current
                if (nCurVport == 1)
                {
                    // Get the extents of Paper space
                    if (pntMin.Equals(new Point3d()) && pntMax.Equals(new Point3d()))
                    {
                        pntMin = Db.Pextmin;
                        pntMax = Db.Pextmax;
                    }
                }
                else
                {
                    // Get the extents of Model space
                    if (pntMin.Equals(new Point3d()) && pntMax.Equals(new Point3d()))
                    {
                        pntMin = Db.Extmin;
                        pntMax = Db.Extmax;
                    }
                }
            }

            // Start a transaction
            using (Transaction acTrans = Db.TransactionManager.StartTransaction())
            {
                // Get the current view
                using (ViewTableRecord acView = Doc.Editor.GetCurrentView())
                {
                    Extents3d extents;

                    // Translate WCS coordinates to DCS
                    Matrix3d matWCS2DCS;
                    matWCS2DCS = Matrix3d.PlaneToWorld(acView.ViewDirection);
                    matWCS2DCS = Matrix3d.Displacement(acView.Target - Point3d.Origin) * matWCS2DCS;
                    matWCS2DCS = Matrix3d.Rotation(-acView.ViewTwist, acView.ViewDirection, acView.Target) * matWCS2DCS;

                    // If a center point is specified, define the min and max 
                    // point of the extents
                    // for Center and Scale modes
                    if (pCenter.DistanceTo(Point3d.Origin) != 0)
                    {
                        pntMin = new Point3d(pCenter.X - (acView.Width / 2), pCenter.Y - (acView.Height / 2), 0);
                        pntMax = new Point3d((acView.Width / 2) + pCenter.X, (acView.Height / 2) + pCenter.Y, 0);
                    }

                    // Create an extents object using a line
                    using (Line acLine = new Line(pntMin, pntMax))
                    {
                        extents = new Extents3d(acLine.Bounds.Value.MinPoint, acLine.Bounds.Value.MaxPoint);
                    }

                    // Calculate the ratio between the width and height of the current view
                    double dViewRatio;
                    dViewRatio = (acView.Width / acView.Height);

                    // Tranform the extents of the view
                    matWCS2DCS = matWCS2DCS.Inverse();
                    extents.TransformBy(matWCS2DCS);

                    double dWidth;
                    double dHeight;
                    Point2d pNewCentPt;

                    // Check to see if a center point was provided (Center and Scale modes)
                    if (pCenter.DistanceTo(Point3d.Origin) != 0)
                    {
                        dWidth = acView.Width;
                        dHeight = acView.Height;

                        if (dFactor == 1)
                            pCenter = pCenter.TransformBy(matWCS2DCS);

                        pNewCentPt = new Point2d(pCenter.X, pCenter.Y);
                    }
                    else // Working in Window, Extents and Limits mode
                    {
                        // Calculate the new width and height of the current view
                        dWidth = extents.MaxPoint.X - extents.MinPoint.X;
                        dHeight = extents.MaxPoint.Y - extents.MinPoint.Y;

                        // Get the center of the view
                        pNewCentPt = new Point2d(((extents.MaxPoint.X + extents.MinPoint.X) * 0.5),
                            ((extents.MaxPoint.Y + extents.MinPoint.Y) * 0.5));
                    }

                    // Check to see if the new width fits in current window
                    if (dWidth > (dHeight * dViewRatio)) dHeight = dWidth / dViewRatio;

                    // Resize and scale the view
                    if (dFactor != 1)
                    {
                        acView.Height = dHeight * dFactor;
                        acView.Width = dWidth * dFactor;
                    }

                    // Set the center of the view
                    acView.CenterPoint = pNewCentPt;

                    // Set the current view
                    Doc.Editor.SetCurrentView(acView);
                }

                // Commit the changes
                acTrans.Commit();
            }
        }

        public void ZoomLimits()
        {
            // Zoom to the limits of Model space
            ZoomView(
                new Point3d(Db.Limmin.X, Db.Limmin.Y, 0),
                new Point3d(Db.Limmax.X, Db.Limmax.Y, 0),
                new Point3d(), 1
            );
        }

        /// <summary> Zoom by db extents </summary>
        public void ZoomExtents(Database db)
        {
            db.TileMode = true;
            Point2d scrSize = (Point2d)acadApp.GetSystemVariable("screensize");
            double ratio = scrSize.X / scrSize.Y;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            using (Line line = new Line(db.Extmin, db.Extmax))
            {
                ViewportTable vpt = (ViewportTable)tr.GetObject(db.ViewportTableId, OpenMode.ForRead);
                ViewportTableRecord vptr = (ViewportTableRecord)tr.GetObject(vpt["*Active"], OpenMode.ForWrite);
                Extents3d ext = line.GeometricExtents;

                using (ViewTableRecord view = Ed.GetCurrentView())
                {
                    Matrix3d worldToEye = (Matrix3d.Rotation(-view.ViewTwist, view.ViewDirection, view.Target) *
                    Matrix3d.Displacement(view.Target - Point3d.Origin) * Matrix3d.PlaneToWorld(view.ViewDirection)).Inverse();
                    ext.TransformBy(worldToEye);
                }

                Point2d pmin = new Point2d(ext.MinPoint.X, ext.MinPoint.Y);
                Point2d pmax = new Point2d(ext.MaxPoint.X, ext.MaxPoint.Y);
                double height = pmax.Y - pmin.Y;
                double width = pmax.X - pmin.X;
                if (width / height < ratio)
                {
                    vptr.Height = height;
                    vptr.Width = height * ratio;
                }
                else
                {
                    vptr.Width = width;
                    vptr.Height = width / ratio;
                }
                vptr.CenterPoint = pmin + (pmax - pmin) / 2.0;
                tr.Commit();
            }
        }

        public void ZoomExtents()
        {
            ((dynamic)Application.AcadApplication).ZoomExtents();

            //Document yourACDoc = acApp.DocumentManager.MdiActiveDocument;

            ////When your plug-in is invoked the first thing I'd do is make sure they're
            ////in PaperSpace
            //yourACDoc.SendStringToExecute("tilemode 0 ", true, false, false);

            ////Next enable ModelSpace through PaperSpace.
            //yourACDoc.SendStringToExecute("_mspace ", true, false, false);

            ////Now we zoom to the extents of the drawing.
            ////Not sure about the bools at the end. The AC documentation has it there for a
            ////zoom all example but AC doesn't ask any further questions after selecting the 
            ////all or extents zoom types and doesn't elaborate on it?
            //yourACDoc.SendStringToExecute("._zoom _extents ", true, false, false);

            ////Head back to PaperSpace
            //yourACDoc.SendStringToExecute("_pspace ", true, false, false);

            //Document curDoc = acApp.DocumentManager.MdiActiveDocument;
            //Extents3d allEntsExtents = new Extents3d();
            //using (Transaction tr = curDoc.TransactionManager.StartTransaction())
            //{
            //    BlockTable bt = tr.GetObject(curDoc.Database.BlockTableId, OpenMode.ForRead, false) as BlockTable;
            //    BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead, false) as BlockTableRecord;
            //    allEntsExtents.AddBlockExtents(btr);
            //    tr.Commit();
            //}
            //Plane plane = new Plane();
            //Extents2d window = new Extents2d(allEntsExtents.MinPoint.Convert2d(plane), allEntsExtents.MaxPoint.Convert2d(plane));

            //ZoomWindow(new Point3d(window.MinPoint.X, window.MinPoint.Y, 0),
            //    new Point3d(window.MaxPoint.X, window.MaxPoint.Y, 0));
        }

        public void AddScale()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            // next get the objectContextManager           
            try
            {
                ObjectContextManager contextManager = db.ObjectContextManager;
                if (contextManager != null)
                {
                    // now get the Annotation Scaling context collection
                    // (named ACDB_ANNOTATIONSCALES_COLLECTION)                   
                    ObjectContextCollection contextCollection = contextManager.GetContextCollection("ACDB_ANNOTATIONSCALES");
                    if (contextCollection != null)
                    {
                        // create a brand new scale context                       
                        AnnotationScale annotationScale = new AnnotationScale();
                        annotationScale.Name = "WBScale2 1:28";
                        annotationScale.PaperUnits = 1;
                        annotationScale.DrawingUnits = 28;
                        // now add to the drawing's context collection                       
                        contextCollection.AddContext(annotationScale);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Editor ed = doc.Editor;
                ed.WriteMessage(ex.ToString());
            }
        }

        /// <summary> ViewIsometric </summary>
        public void ViewIsometric(int iJob)
        {
            var optionsViews = Enum.GetNames(typeof(ViewOptions)).ToList();
            if (acadApp.Version.Major > 14)
            {
                if (iJob <= optionsViews.Count())
                {
                    Doc.SendStringToExecute(
                        "_regenauto" + "\rON" + Convert.ToChar(13).ToString(CultureInfo.InvariantCulture), false, false,
                        false);
                    // IM, 2007.02.27
                    Doc.SendStringToExecute("_-view" + "\r_" + optionsViews[iJob].ToLower() + "\r", false, false, false);
                }
            }
        }

        /// <summary> Viewport </summary>
        public void Viewport(Vector3d vec)
        {
            using (var tr = Db.TransactionManager.StartTransaction())
            {
                using (Doc.LockDocument())
                {
                    // Open active view window
                    var acVportTblRec = tr.GetObject(Ed.ActiveViewportId, OpenMode.ForWrite) as ViewportTableRecord;

                    // return direction view of current view window
                    if (acVportTblRec != null) acVportTblRec.ViewDirection = vec; // new Vector3d(-1, -1, 1);
                    Ed.UpdateTiledViewportsFromDatabase();
                }
            }
        }

        /// <summary> UpdateScreen </summary>
        public void UpdateScreen()
        {
            acadApp.UpdateScreen();
        }

        public async Task<bool> PageExists(string url)
        {
            // First check whether the URL is valid
            Uri uriResult;
            if (!Uri.TryCreate(url, UriKind.Absolute, out uriResult) || uriResult.Scheme != Uri.UriSchemeHttp)
                return false;

            // Then we try to peform a HEAD request on the page
            // (a WebException will be fired if it doesn't exist)

            try
            {
                using (var client = new HeadClient())
                {
                    await client.DownloadStringTaskAsync(url);
                }
                return true;
            }
            catch (WebException ex)
            {
                Ed.WriteMessage(ex.ToString());
                return false;
            }
        }

        #endregion

        private bool SendToMapHttpClient(string commnadName, string requestUrl, CommandCancellationTokenSource cts)
        {
            bool actionResult = true;
            try
            {
                Ed.WriteMessage(PluginSettings.Prompt + commnadName + " get " + requestUrl + "...");
                HttpClient client = new HttpClient();
                HttpResponseMessage wcfResponse = client.GetAsync(requestUrl).GetAwaiter().GetResult();

                if ((int)wcfResponse.StatusCode >= 500)
                {
                    cts.OperationCanceledException = new OperationCanceledException(
                        $"Command {commnadName} response.StatusCode: {wcfResponse.StatusCode}");
                    cts.Cancel(false);
                    Ed.WriteMessage(PluginSettings.Prompt + cts.OperationCanceledException.Message);
                    actionResult = false;
                }

                if (!cts.IsCancellationRequested)
                {
                    HttpContent stream = wcfResponse.Content;
                    var data = stream.ReadAsStringAsync();
                    Ed.WriteMessage(PluginSettings.Prompt + data.Result);
                    cts.Cancel(false);
                }
            }
            catch (Exception ex)
            {
                this.Error(commnadName, ex.ToStringMessage());
                cts.Cancel();
                actionResult = false;
            }
            return actionResult;
        }

        public void Dispose()
        {
        }
    }
}