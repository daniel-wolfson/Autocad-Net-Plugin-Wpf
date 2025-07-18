using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using ID.Infrastructure.Commands;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using static Autodesk.AutoCAD.Windows.OpenFileDialog;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Transaction = Autodesk.AutoCAD.DatabaseServices.Transaction;

namespace Intellidesk.AcadNet.Common.Extensions
{
    public static class XrefExtensions
    {
        #region Helpers

        public delegate void ProcessSingleXref(BlockTableRecord btr);

        public delegate void ProcessMultipleXrefs(ObjectIdCollection xrefIds);

        public static void DetachXref(this BlockTableRecord btr)
        {
            acadApp.DocumentManager.MdiActiveDocument.Database.DetachXref(btr.ObjectId);
        }

        public static void OpenXref(BlockTableRecord btr)
        {
            string xrefPath = btr.PathName;
            if (xrefPath.Contains(".\\"))
            {
                string hostPath = acadApp.DocumentManager.MdiActiveDocument.Database.Filename;
                Directory.SetCurrentDirectory(Path.GetDirectoryName(hostPath));

                xrefPath = Path.GetFullPath(xrefPath);
            }

            if (!File.Exists(xrefPath)) return;

            Document doc = acadApp.DocumentManager.Open(xrefPath, false);
            if (doc.IsReadOnly)
            {
                System.Windows.Forms.MessageBox.Show(
                    doc.Name + " opened in read-only mode.",
                    "OpenXrefs",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Warning);
            }
        }

        public static ObjectIdCollection XGetXrefs(this Database db)
        {
            var xrefCollection = new ObjectIdCollection();
            using (var xg = db.GetHostDwgXrefGraph(false))
            {
                int numOfNodes = xg.NumNodes;
                for (int cnt = 0; cnt < xg.NumNodes; cnt++)
                {
                    var xNode = xg.GetXrefNode(cnt);
                    if (!xNode.Database.Filename.Equals(db.Filename))
                    {
                        if (xNode.XrefStatus == XrefStatus.Resolved)
                        {
                            xrefCollection.Add(xNode.BlockTableRecordId);
                        }
                    }
                }
            }
            return xrefCollection;
        }

        public static void XBindXrefs(this Database db)
        {
            //var Doc = Application.DocumentManager.MdiActiveDocument;
            //var Db = Doc.Database;
            var xrefCollection = new ObjectIdCollection();

            using (var xg = db.GetHostDwgXrefGraph(false))
            {
                int numOfNodes = xg.NumNodes;
                for (int cnt = 0; cnt < xg.NumNodes; cnt++)
                {
                    var xNode = xg.GetXrefNode(cnt);
                    if (!xNode.Database.Filename.Equals(db.Filename))
                    {
                        if (xNode.XrefStatus == XrefStatus.Resolved)
                        {
                            xrefCollection.Add(xNode.BlockTableRecordId);
                        }
                    }
                }
            }
            if (xrefCollection.Count != 0)
                db.BindXrefs(xrefCollection, true);
        }

        public static List<KeyValuePair<BlockTableRecord, XrefGraphNode>> GetAllXref(this Database db)
        {
            List<KeyValuePair<BlockTableRecord, XrefGraphNode>> results = new List<KeyValuePair<BlockTableRecord, XrefGraphNode>>();
            var doc = acadApp.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;

            XrefGraph xrGraph = db.GetHostDwgXrefGraph(false);
            using (var tr = doc.TransactionManager.StartTransaction())
            {
                for (int i = 1; i < xrGraph.NumNodes; i++)
                {
                    XrefGraphNode xrNode = xrGraph.GetXrefNode(i);

                    BlockTableRecord btr = tr.GetObject(xrNode.BlockTableRecordId, OpenMode.ForWrite) as BlockTableRecord;
                    results.Add(new KeyValuePair<BlockTableRecord, XrefGraphNode>(btr, xrNode));
                }
                tr.Commit();
            }
            return results;
        }

        public static bool BindAllXref(this Database db, ObjectIdCollection btrCol)
        {
            var doc = acadApp.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            try
            {
                using (var tr = doc.TransactionManager.StartTransaction())
                {
                    try
                    {
                        System.Windows.Forms.Application.DoEvents();
                        Thread.Sleep(500);

                        if (btrCol.Count > 0)
                        {
                            ed.WriteMessage($"\n{CommandNames.UserGroup} Begin bind...");
                            db.BindXrefs(btrCol, true);
                            ed.WriteMessage($"\n{CommandNames.UserGroup} End bind");
                        }
                    }
                    catch (Autodesk.AutoCAD.Runtime.Exception ex)
                    {
                        ed.WriteMessage("\n Error bind Xref!" + ex);
                    }

                    tr.Commit();
                }
                ed.WriteMessage("\n bind commplete");
                return true;
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage("\n Error bind Xref!" + ex);
                return false;
            }
        }

        public static void BindXrefs(ObjectIdCollection xrefIds)
        {
            acadApp.DocumentManager.MdiActiveDocument.Database.BindXrefs(xrefIds, false);
        }

        public static void ReloadXrefs(ObjectIdCollection xrefIds)
        {
            acadApp.DocumentManager.MdiActiveDocument.Database.ReloadXrefs(xrefIds);
        }

        public static void UnloadXrefs(ObjectIdCollection xrefIds)
        {
            acadApp.DocumentManager.MdiActiveDocument.Database.UnloadXrefs(xrefIds);
        }

        public static void ProcessXrefs(string promptMessage, ProcessSingleXref process)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            TypedValue[] filterList = { new TypedValue(0, "INSERT") };
            ed.WriteMessage(promptMessage);
            PromptSelectionResult result = ed.GetSelection(new SelectionFilter(filterList));
            if (result.Status != PromptStatus.OK) return;

            ObjectId[] ids = result.Value.GetObjectIds();
            Database db = Application.DocumentManager.MdiActiveDocument.Database;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                ObjectIdCollection xrefIds = new ObjectIdCollection();
                foreach (ObjectId id in ids)
                {
                    BlockReference blockRef = (BlockReference)tr.GetObject(id, OpenMode.ForRead, false, true);
                    ObjectId bId = blockRef.BlockTableRecord;
                    if (!xrefIds.Contains(bId))
                    {
                        xrefIds.Add(bId);
                        BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bId, OpenMode.ForRead);
                        if (btr.IsFromExternalReference)
                            process(btr);
                    }
                }
                tr.Commit();
            }
        }

        public static void processXrefs(string promptMessage, ProcessMultipleXrefs process)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            TypedValue[] filterList = { new TypedValue(0, "INSERT") };
            ed.WriteMessage(promptMessage);
            PromptSelectionResult result = ed.GetSelection(new SelectionFilter(filterList));
            if (result.Status != PromptStatus.OK) return;

            ObjectId[] ids = result.Value.GetObjectIds();
            Database db = Application.DocumentManager.MdiActiveDocument.Database;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                ObjectIdCollection blockIds = new ObjectIdCollection();
                foreach (ObjectId id in ids)
                {
                    BlockReference blockRef = (BlockReference)tr.GetObject(id, OpenMode.ForRead, false, true);
                    blockIds.Add(blockRef.BlockTableRecord);
                }
                ObjectIdCollection xrefIds = filterXrefIds(blockIds);
                if (xrefIds.Count != 0)
                    process(xrefIds);
                tr.Commit();
            }
        }

        public static void attachXrefs(string[] fileNames)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            Array.Sort(fileNames);
            Database db = Application.DocumentManager.MdiActiveDocument.Database;
            double dimScale = db.Dimscale;
            foreach (string fileName in fileNames)
            {
                PromptPointOptions options = new PromptPointOptions("Pick insertion point for " + fileName + ": ");
                options.AllowNone = false;
                PromptPointResult pt = ed.GetPoint(options);
                if (pt.Status != PromptStatus.OK) continue;

                double xrefScale = getDwgScale(fileName);
                double scaleFactor = dimScale / xrefScale;
                using (Transaction tr = Application.DocumentManager.MdiActiveDocument.TransactionManager.StartTransaction())
                {
                    ObjectId xrefId = db.AttachXref(fileName, Path.GetFileNameWithoutExtension(fileName));
                    BlockReference blockRef = new BlockReference(pt.Value, xrefId);
                    BlockTableRecord layoutBlock = (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);
                    blockRef.ScaleFactors = new Scale3d(scaleFactor, scaleFactor, scaleFactor);
                    blockRef.Layer = "0";
                    layoutBlock.AppendEntity(blockRef);
                    tr.AddNewlyCreatedDBObject(blockRef, true);
                    tr.Commit();
                }
            }
        }

        public static double getDwgScale(string fileName)
        {
            using (Database db = new Database(false, true))
            {
                db.ReadDwgFile(fileName, FileOpenMode.OpenForReadAndAllShare, false, string.Empty);
                return db.Dimscale;
            }
        }

        public static ObjectIdCollection filterXrefIds(ObjectIdCollection blockIds)
        {
            ObjectIdCollection xrefIds = new ObjectIdCollection();
            foreach (ObjectId bId in blockIds)
            {
                if (!xrefIds.Contains(bId))
                {
                    BlockTableRecord btr = (BlockTableRecord)bId.GetObject(OpenMode.ForRead);
                    if (btr.IsFromExternalReference)
                        xrefIds.Add(bId);
                }
            }
            return xrefIds;
        }

        /// <summary>
        /// Attaches the specified Xref to the current space in the current drawing.
        /// </summary>
        /// <param name="context">current command context</param>
        /// <param name="path">Path to the drawing file to attach as an Xref.</param>
        /// <param name="position">Position of Xref in WCS coordinates.</param>
        /// <param name="name">Optional name for the Xref.</param>
        /// <returns>Whether the attach operation succeeded.</returns>
        public static ObjectId XrefAttachAndInsert(this Database db, string path, Point3d position, string name = null)
        {
            var ret = ObjectId.Null;

            if (!File.Exists(path))
                return ret;

            if (string.IsNullOrEmpty(name))
                name = Path.GetFileNameWithoutExtension(path);

            try
            {
                var doc = acadApp.DocumentManager.MdiActiveDocument;
                using (doc.LockDocument())
                {
                    using (var tr = db.TransactionManager.StartOpenCloseTransaction())
                    {
                        // Attach the Xref - add it to the database's block table
                        var xBtrId = db.AttachXref(path, name);
                        if (xBtrId.IsValid)
                        {
                            // Open the newly created block, so we can get its units
                            var xbtr = (BlockTableRecord)tr.GetObject(xBtrId, OpenMode.ForRead);

                            // Determine the unit conversion between the xref and the target
                            // database
                            var sf = UnitsConverter.GetConversionFactor(xbtr.Units, db.Insunits);

                            // Create the block reference and scale it accordingly
                            var br = new BlockReference(position, xBtrId);
                            br.ScaleFactors = new Scale3d(sf);

                            // Add the block reference to the current space and the transaction
                            var btr = (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);
                            btr.AppendEntity(br);
                            tr.AddNewlyCreatedDBObject(br, true);

                            ret = br.ObjectId;
                        }
                        tr.Commit();
                    }
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                Log.Logger.Error(ex.InnerException?.Message ?? ex.Message);
                //context.Logger.Error(ex.Message, ex);
            }

            return ret;
        }

        public static void ChangeXref(this Database db)
        {
            // Get the database associated with each xref in the
            // drawing and change all of its circles to be dashed

            using (var tr = db.TransactionManager.StartTransaction())
            {
                var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);

                // Loop through the contents of the modelspace
                foreach (var id in ms)
                {
                    // We only care about BlockReferences
                    var br = tr.GetObject(id, OpenMode.ForRead) as BlockReference;
                    if (br != null)
                    {
                        // Check whether the associated BlockTableRecord is
                        // an external reference
                        var bd = (BlockTableRecord)tr.GetObject(br.BlockTableRecord, OpenMode.ForRead);
                        if (bd.IsFromExternalReference)
                        {
                            // If so, get its Database and call the function
                            // to change the linetype of its Circles

                            var xdb = bd.GetXrefDatabase(false);
                            if (xdb != null)
                            {

                                using (var xf = XrefFileLock.LockFile(xdb.XrefBlockId))
                                {
                                    // Make sure the original symbols are loaded
                                    xdb.RestoreOriginalXrefSymbols();

                                    xdb.RestoreForwardingXrefSymbols();
                                }

                            }
                        }
                    }
                }
                tr.Commit();
            }
        }

        public static void ConvertBlocksToXrefs(this Database db)
        {
            Dictionary<ObjectId, ObjectId> _map = new Dictionary<ObjectId, ObjectId>();
            //_map.Clear();
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var bt = (BlockTable)tr.GetObject(
                    db.BlockTableId, OpenMode.ForRead
               );

                var ms = (BlockTableRecord)tr.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db), OpenMode.ForRead);

                GenerateXrefForBlock(tr, null, ms, _map);
                tr.Commit();
            }
        }

        private static void GenerateXrefForBlock(Transaction tr, BlockReference br, BlockTableRecord btr, Dictionary<ObjectId, ObjectId> _map)
        {
            var db = btr.Database;

            // Iterate the block table record, looking for blocks, then recurse
            foreach (var id in btr)
            {
                var br2 = tr.GetObject(id, OpenMode.ForRead) as BlockReference;
                if (br2 != null)
                {
                    // If we've found a BlockReference, check whether we've seen it
                    if (_map.ContainsKey(br2.BlockTableRecord))
                    {
                        // If we already have a replacement Xref, use that
                        br2.UpgradeOpen();
                        br2.BlockTableRecord = _map[br2.BlockTableRecord];
                    }
                    else
                    {
                        // If not, we need to process the block by recursing
                        var btr2 = (BlockTableRecord)tr.GetObject(br2.BlockTableRecord, OpenMode.ForRead);

                        // GenerateXrefForBlock(tr, br2, btr2);
                    }
                }
            }

            // After we've done our depth-first replacement of our block contents,
            // wblock ourselves out and reattach as an Xref
            // Only do this for nested calls (this won't happen for the initial
            // run on the modelspace block)

            if (!btr.IsLayout && br != null)
            {
                // Use the block name as the filename, and the name of our Xref
                var blkName = btr.Name;
                var outName = "c:\\temp\\" + blkName + ".dwg";

                // Wblock out the block
                var destDb = db.Wblock(btr.ObjectId);
                destDb.SaveAs(outName, DwgVersion.Current);

                // Erase the original block definition

                btr.UpgradeOpen();
                btr.Erase();

                // Reattach the Xref
                var xid = db.AttachXref(outName, blkName);

                // Point the BlockReference to the newly created Xref
                br.UpgradeOpen();
                br.BlockTableRecord = xid;

                // Add the entry to our map
                _map.Add(btr.ObjectId, xid);
            }
        }

        #endregion

        #region Commands

        //[CommandMethod("XrefTools", "AttachXrefs", CommandFlags.Modal | CommandFlags.DocExclusiveLock)]
        public static void XrefAttach()
        {
            string initFolder = acadApp.DocumentManager.MdiActiveDocument.Database.Filename.ToUpper();
            if (initFolder.Contains("PLOT"))
            {
                initFolder = initFolder.Replace("-PLOT.DWG", "");
                initFolder = initFolder.Replace("PLOT\\", "");
                initFolder = initFolder.Replace("PLOTS\\", "");
                if (!Directory.Exists(initFolder))
                    initFolder = acadApp.DocumentManager.MdiActiveDocument.Database.Filename;
            }

            OpenFileDialogFlags flags = OpenFileDialogFlags.DefaultIsFolder | OpenFileDialogFlags.AllowMultiple;
            OpenFileDialog dlg = new OpenFileDialog("Select Drawings to Attach", initFolder, "dwg", "Select Xrefs", flags);
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                attachXrefs(dlg.GetFilenames());
        }

        //[CommandMethod("XrefTools", "BindXrefs", CommandFlags.Modal | CommandFlags.DocExclusiveLock)]
        public static void XrefBind()
        {
            processXrefs("\nSelect xrefs to bind: ", XrefExtensions.BindXrefs);
        }

        //[CommandMethod("XrefTools", "DetachXrefs", CommandFlags.Modal | CommandFlags.DocExclusiveLock)]
        public static void XrefDetach()
        {
            ProcessXrefs("\nSelect xrefs to detach: ", XrefExtensions.DetachXref);
        }

        //[CommandMethod("XrefTools", "OpenXrefs", CommandFlags.Session)]
        public static void XrefOpen()
        {
            ProcessXrefs("\nSelect xrefs to open: ", XrefExtensions.OpenXref);
        }

        //[CommandMethod("XrefTools", "ReloadXrefs", CommandFlags.Modal | CommandFlags.DocExclusiveLock)]
        public static void XrefReload()
        {
            processXrefs("\nSelect xrefs to reload: ", XrefExtensions.ReloadXrefs);
        }

        //[CommandMethod("XrefTools", "ReloadAllXrefs", CommandFlags.Modal | CommandFlags.DocExclusiveLock)]
        public static void XrefReloadAll()
        {
            Database db = acadApp.DocumentManager.MdiActiveDocument.Database;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable blockTbl = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                ObjectIdCollection blockIds = new ObjectIdCollection();
                foreach (ObjectId bId in blockTbl)
                    blockIds.Add(bId);
                ObjectIdCollection xrefIds = filterXrefIds(blockIds);
                if (xrefIds.Count != 0)
                    db.ReloadXrefs(xrefIds);
                tr.Commit();
            }
        }

        [CommandMethod("XrefTools", "UnloadXrefs", CommandFlags.Modal | CommandFlags.DocExclusiveLock)]
        public static void XrefUnload()
        {
            processXrefs("\nSelect xrefs to unload: ", XrefExtensions.UnloadXrefs);
        }

        //[CommandMethod("freezeXrefLayer")]
        public static void CmdFreezeXrefLayer()
        {
            Editor ed = acadApp.DocumentManager.MdiActiveDocument.Editor;
            Database db = acadApp.DocumentManager.MdiActiveDocument.Database;

            // select an entity
            PromptEntityOptions pr = new PromptEntityOptions("Select a xref: ");
            PromptEntityResult res = ed.GetEntity(pr);

            if (res.Status != PromptStatus.OK) return;

            // start the transaction
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockReference blockRef = trans.GetObject(res.ObjectId, OpenMode.ForRead) as BlockReference;

                // is a block reference?
                if (blockRef == null) return;

                // open the block definition?
                BlockTableRecord blockDef = trans.GetObject(blockRef.BlockTableRecord, OpenMode.ForRead) as BlockTableRecord;

                // is not from external reference, exit
                if (!blockDef.IsFromExternalReference) return;

                // open the xref database
                Database xRefDB = new Database(false, true);
                xRefDB.ReadDwgFile(blockDef.PathName, System.IO.FileShare.Read, false, string.Empty);

                using (Transaction xRefTrans = xRefDB.TransactionManager.StartTransaction())
                {
                    // open the block definition and its model space
                    BlockTable xRefBT = xRefTrans.GetObject(xRefDB.BlockTableId, OpenMode.ForRead) as BlockTable;
                    BlockTableRecord xRefBTR = xRefTrans.GetObject(xRefBT[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;

                    // iterate through entities on the xref model space
                    foreach (ObjectId xRefEntId in xRefBTR)
                    {
                        Entity ent = xRefTrans.GetObject(xRefEntId, OpenMode.ForRead) as Entity;
                        LayerTable lt = trans.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;

                        // get the drawing name
                        string dwgName = xRefDB.OriginalFileName.Substring(xRefDB.OriginalFileName.LastIndexOf("\\") + 1,
                            xRefDB.OriginalFileName.Length - xRefDB.OriginalFileName.LastIndexOf("\\") - 5);

                        // now set the layer as frozen
                        if (lt.Has(dwgName + "|" + ent.Layer))
                        {
                            ObjectId lyrId = lt[dwgName + "|" + ent.Layer];
                            LayerTableRecord ltr = trans.GetObject(lyrId,
                                OpenMode.ForWrite) as LayerTableRecord;
                            ltr.IsFrozen = true;
                        }
                    }
                    xRefTrans.Commit();
                }
                trans.Commit();
            }
        }

        #endregion
    }
}


//Sample GetData
//BlockReference[] selectionIds = Db.GetData<BlockReference>((Tr, id) =>
//{
//    if (Tr == null || Tr.IsDisposed || id == ObjectId.Null || !id.IsValid || id.IsErased)
//        return false;

//    Database _db = id.Database;
//    DBObject item = Tr.GetObject(id, OpenMode.ForRead);

//    bool result = item is BlockReference;
//    if (!result) return false;

//    ObjectId ownerId = _db.CurrentSpaceId;
//    Entity entity = item as Entity;

//    if (entity.BlockId != ownerId) return false;

//    BlockReference br = (BlockReference)item;
//    if (br.Position != new Point3d(5.0, 5.0, 0.0))
//    {
//        return true;
//    }
//    return false;
//},
//(Tr, X) => (BlockReference)Tr.GetObject(X, OpenMode.ForRead));