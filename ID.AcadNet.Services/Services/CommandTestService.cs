using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;

using Intellidesk.Infrastructure;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Exception = System.Exception;

// This line is not mandatory, but improves loading performances
//[assembly: CommandClass(typeof(CmdManager))]
namespace Intellidesk.AcadNet.Services
{
    public partial class CommandTestService //TEST
    {
        //public static LinkingCommands()
        //{
        //  Tools.Db.ObjectModified += new ObjectEventHandler(OnObjectModified);
        //  Tools.Db.ObjectErased += new ObjectErasedEventHandler(OnObjectErased);
        //  Tools.Doc.CommandEnded += new CommandEventHandler(OnCommandEnded);
        //  m_linkManager = new LinkedObjectManager();
        //  m_entitiesToUpdate = new ObjectIdCollection();
        //}

        //static ~LinkingCommands()
        //{
        //  try
        //  {
        //    Tools.Db.ObjectModified -= new ObjectEventHandler(OnObjectModified);
        //    Tools.Db.ObjectErased -= new ObjectErasedEventHandler(OnObjectErased);
        //    Tools.Doc.CommandEnded += new CommandEventHandler(OnCommandEnded);
        //  }
        //  catch(System.Exception)
        //  {
        //    // The document or database may no longer
        //    // be available on unload
        //  }
        //}

        //    private Document _doc;
        //    private Database _db;

        //    private ObjectIdCollection _blkDefs = new ObjectIdCollection();

        //    private ObjectIdCollection _blkRefs = new ObjectIdCollection();

        //    private ObjectIdCollection _blkConts = new ObjectIdCollection();

        //    private bool _handlers = false;

        //    private bool _exploding = false;


        //    [CommandMethod("STOPEX")]
        //    public void StopBlockFromExploding()
        //    {
        //        _doc = Application.DocumentManager.MdiActiveDocument;
        //        _db = _doc.Database;

        //        if (!_handlers)
        //        {
        //            AddEventHandlers();
        //            _handlers = true;
        //        }

        //        // Get the name of the block to protect
        //        var pso = new PromptStringOptions("\nEnter block name: ");
        //        pso.AllowSpaces = false;
        //        var pr = _doc.Editor.GetString(pso);

        //        if (pr.Status != PromptStatus.OK)
        //            return;

        //        Transaction tr = _db.TransactionManager.StartTransaction();
        //        using (tr)
        //        {
        //            // Make sure the block definition exists
        //            var bt = (BlockTable)tr.GetObject(_db.BlockTableId,OpenMode.ForRead);
        //            if (bt.Has(pr.StringResult))
        //            {
        //                // Collect information about the block...
        //                // 1. the block definition
        //                ObjectId blkId = bt[pr.StringResult];
        //                _blkDefs.Add(blkId);

        //                var btr = (BlockTableRecord)tr.GetObject(blkId,OpenMode.ForRead);

        //                // 2. the block's contents
        //                foreach (ObjectId id in btr)
        //                    _blkConts.Add(id);

        //                // 3. the block's references
        //                var blkRefs = btr.GetBlockReferenceIds(true, true);
        //                foreach (ObjectId id in blkRefs)
        //                    _blkRefs.Add(id);
        //            }
        //            tr.Commit();
        //        }
        //    }

        //    private void AddEventHandlers()
        //    {
        //        // When a block reference is added, we need to
        //        // check whether it's for a block we care about
        //        // and add it to the list, if so
        //        _db.ObjectAppended += delegate(object sender, ObjectEventArgs e)
        //          {
        //              var br = (BlockReference)e.DBObject;
        //              if (br != null)
        //              {
        //                  if (_blkDefs.Contains(br.BlockTableRecord))
        //                      _blkRefs.Add(br.ObjectId);
        //              }
        //          };

        //        // Conversely we need to remove block references
        //        // that as they're erased
        //        _db.ObjectErased += delegate(object sender, ObjectErasedEventArgs e)
        //          {
        //              // This is called during as part of the cloning 
        //              // process, so let's check that's not happening

        //              if (!_exploding)
        //              {
        //                  var br = (BlockReference)e.DBObject;
        //                  if (br != null)
        //                  {
        //                      // If we're erasing, remove this block
        //                      // reference from the list, otherwise if
        //                      // we're unerasing we will want to add it
        //                      // back in

        //                      if (e.Erased)
        //                      {
        //                          if (_blkRefs.Contains(br.ObjectId))
        //                              _blkRefs.Remove(br.ObjectId);
        //                      }
        //                      else
        //                      {
        //                          if (_blkDefs.Contains(br.BlockTableRecord))
        //                              _blkRefs.Add(br.ObjectId);
        //                      }
        //                  }
        //              }
        //          };

        //        // This is where we fool AutoCAD into thinking the
        //        // block contents have already been cloned

        //        _db.BeginDeepClone += delegate(object sender, IdMappingEventArgs e)
        //          {
        //              // Only for the explode context
        //              if (e.IdMapping.DeepCloneContext !=
        //                  DeepCloneType.Explode)
        //                  return;

        //              // We add IDs to the map to stop the
        //              // block contents from being cloned

        //              foreach (ObjectId id in _blkConts)
        //                  e.IdMapping.Add(
        //                    new IdPair(id, id, true, true, true)
        //                  );
        //          };

        //        // And this is where we remove the mapping entries
        //        _db.BeginDeepCloneTranslation += delegate(object sender, IdMappingEventArgs e)
        //          {
        //              // Only for the explode context
        //              if (e.IdMapping.DeepCloneContext !=
        //                  DeepCloneType.Explode)
        //                  return;

        //              // Set the flag for our CommandEnded handler
        //              _exploding = true;

        //              // Remove the entries we added on BeginDeepClone
        //              foreach (ObjectId id in _blkConts)
        //                  e.IdMapping.Delete(id);
        //          };

        //        // As the command ends we unerase the block references
        //        _doc.CommandEnded += delegate(object sender, CommandEventArgs e)
        //          {
        //              if (e.GlobalCommandName == "EXPLODE" && _exploding)
        //              {
        //                  // By this point the block contents should not have
        //                  // been cloned, but the blocks have been erased

        //                  Transaction tr = _db.TransactionManager.StartTransaction();
        //                  using (tr)
        //                  {
        //                      // So we need to unerase each of the erased
        //                      // block references

        //                      foreach (ObjectId id in _blkRefs)
        //                      {
        //                          DBObject obj = tr.GetObject(id, OpenMode.ForRead, true);
        //                          // Only unerase it if it's needed

        //                          if (obj.IsErased)
        //                          {
        //                              obj.UpgradeOpen();
        //                              obj.Erase(false);
        //                          }
        //                      }
        //                      tr.Commit();
        //                  }
        //                  _exploding = false;
        //              }
        //          };
        //    }

        [CommandMethod("XDataDirWrite")]
        public static void XDataDirWriteMethod()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            try
            {
                string TestAppName = "XDataDir_AppName";
                PromptEntityResult prEntRes = ed.GetEntity("Select an Entity to attach XDATA");
                if (prEntRes.Status == PromptStatus.OK)
                {
                    using (Transaction tr = db.TransactionManager.StartTransaction())
                    {
                        RegAppTable regAppTable = (RegAppTable)tr.GetObject(db.RegAppTableId, OpenMode.ForRead);
                        if (!regAppTable.Has(TestAppName))
                        {
                            using (RegAppTableRecord regAppRecord = new RegAppTableRecord())
                            {
                                regAppRecord.Name = TestAppName;
                                regAppTable.UpgradeOpen();
                                regAppTable.Add(regAppRecord);
                                regAppTable.DowngradeOpen();
                                tr.AddNewlyCreatedDBObject(regAppRecord, true);
                            }
                        }

                        Entity ent = (Entity)tr.GetObject(prEntRes.ObjectId, OpenMode.ForWrite);
                        ent.XData = new ResultBuffer
                                    (
                                        new TypedValue((int)DxfCode.ExtendedDataRegAppName, TestAppName),   //1001
                            //new TypedValue((int)DxfCode.ExtendedDataWorldXCoordinate, new Point3d(1.2, 2.2, 3.2)), //1011
                                        new TypedValue((int)DxfCode.ExtendedDataWorldXDir, new Point3d(1.4, 2.4, 3.4)) //1013
                            //new TypedValue((int)DxfCode.ExtendedDataWorldXDir, new Vector3d(1.4, 2.4, 3.4)) //1013
                                    );

                        tr.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Add(ex);
            }
        }

        [CommandMethod("XDataDirRead")]
        public static void XDataDirReadMethod()
        {
            string TestAppName = "XDataDir_AppName";

            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            try
            {
                PromptEntityResult prEntRes = ed.GetEntity("Select an Entity");
                if (prEntRes.Status == PromptStatus.OK)
                {
                    using (Transaction tr = db.TransactionManager.StartTransaction())
                    {
                        Entity ent = (Entity)tr.GetObject(prEntRes.ObjectId, OpenMode.ForRead);
                        ResultBuffer rb = ent.GetXDataForApplication(TestAppName);
                        if (rb != null)
                        {
                            TypedValue[] rvArr = rb.AsArray();
                            foreach (TypedValue tv in rvArr)
                            {
                                switch ((DxfCode)tv.TypeCode)
                                {
                                    case DxfCode.ExtendedDataWorldXDir:
                                        Point3d dir = (Point3d)tv.Value;
                                        //Vector3d dir = (Vector3d)tv.Value;
                                        ed.WriteMessage("\nDirection: {0}", dir.ToString());
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                        else
                            ed.WriteMessage("The entity does not have the {0} XData.", TestAppName);

                        tr.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Add(ex);
            }
        }

        [CommandMethod("tlo")]
        public static void TestLayerOff() //TEST
        {
            //foreach (AcadEntity ent in Application.DocumentManager.MdiActiveDocument.Database.ObjectErased.ModelSpace)
            //{
            //    //... get xData from the entity.
            //    object xdata, xdataType;
            //    ent.GetXData("MyRegisteredAppName", out xdataType, out xdata);
            //    //... read and spit out the xdata to the command line to see what we got
            //}
        }
    }

    public class Session
    {
        public Document[] Documents;
    }

    public class SessionManager
    {
        public ObjectIdCollection _ids = null;


    }
}

namespace BackgroundProcess
{
    public class Commands : IExtensionApplication
    {
        delegate void FinishedProcessingDelegate();
        static Control _syncCtrl;

        public void Initialize()
        {
            // The control created to help with marshaling 
            // needs to be created on the main thread
            _syncCtrl = new Control();
            _syncCtrl.CreateControl();
        }

        public void Terminate()
        {
        }

        void BackgroundProcess()
        {
            // This is to represent the background process
            Thread.Sleep(5000);

            // Now we need to marshall the call to the main thread
            // I don't see how this could ever be false in this context, 
            // but I check it anyway
            if (_syncCtrl.InvokeRequired)
                _syncCtrl.Invoke(new FinishedProcessingDelegate(FinishedProcessing));
            else
                FinishedProcessing();
        }

        void FinishedProcessing()
        {
            // If we want to modify the database, then we need to lock 
            // the document since we are in session/application context
            Document doc = Application.DocumentManager.MdiActiveDocument;
            using (doc.LockDocument())
            {
                using (Transaction tr = doc.Database.TransactionManager.StartTransaction())
                {
                    BlockTable bt = (BlockTable)tr.GetObject(doc.Database.BlockTableId, OpenMode.ForRead);
                    BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                    Line line = new Line(new Point3d(0, 0, 0), new Point3d(10, 10, 0));

                    ms.AppendEntity(line);
                    tr.AddNewlyCreatedDBObject(line, true);
                    tr.Commit();
                }
            }

            // Also write a message to the command line
            // Note: using AutoCAD notification bubbles would be 
            // a nicer solution :)
            // TrayItem/TrayItemBubbleWindow
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ed.WriteMessage("Finished the background process!\n");
        }

        [CommandMethod("ProcessInBackground")]
        public void ProcessBackground()
        {
            // Let's say we got some data from the drawing and
            // now we want to process it in a background thread
            var thread = new Thread(new ThreadStart(BackgroundProcess));
            thread.Start();

            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ed.WriteMessage("Started background processing. " + "You can keep working as usual.\n");
        }
    }
}
