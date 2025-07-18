using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using ID.Infrastructure;
using ID.Infrastructure.Commands;
using ID.Infrastructure.Extensions;
using ID.Infrastructure.Interfaces;
using Intellidesk.AcadNet.Common.Enums;
using Intellidesk.AcadNet.Common.Interfaces;
using Intellidesk.AcadNet.Common.Internal;
using Intellidesk.AcadNet.Common.Jig;
using Intellidesk.AcadNet.Common.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Document = Autodesk.AutoCAD.ApplicationServices.Document;
using Exception = System.Exception;
using TransactionManager = Autodesk.AutoCAD.DatabaseServices.TransactionManager;

namespace Intellidesk.AcadNet.Common.Extensions
{
    public static class BlocksExtensions
    {
        #region <Get>

        public static DBObject XGetObject(this ObjectId id)
        {
            using (var tr = id.Database.TransactionManager.StartTransaction())
            {
                return tr.GetObject(id, OpenMode.ForRead);
            }
        }

        public static Entity XGetEntity<T>(this ObjectId id) where T : Entity
        {
            using (var tr = id.Database.TransactionManager.StartTransaction())
            {
                return tr.GetObject(id, OpenMode.ForRead) as T;
            }
        }

        public static BlockTableRecord XGetBlockTableRecord(this ObjectId id)
        {
            using (var tr = id.Database.TransactionManager.StartTransaction())
            {
                BlockReference br = tr.GetObject(id, OpenMode.ForRead) as BlockReference;
                return tr.GetObject(br.BlockTableRecord, OpenMode.ForRead) as BlockTableRecord;
            }
        }

        public static List<ObjectId> XGetObjects(this ObjectId id, Transaction tr, Type[] filterTypes = null, bool isParentFilterTypes = false, string[] layerPatterns = null)
        {
            var readArgs = new ActionArguments
            {
                FilterTypesOn = filterTypes,
                LayerPatternOn = layerPatterns,
                IsParentFilterTypes = isParentFilterTypes
            };
            return ((BlockReference)id.GetObject(OpenMode.ForRead)).XGetObjects(readArgs);
        }

        /// <summary> Get Objects from Model Space </summary>
        public static List<ObjectId> XGetObjects(this BlockReference br, Transaction tr, Type[] filterTypes = null, bool isParentFilterTypes = false, string[] layerPatterns = null)
        {
            var readArgs = new ActionArguments
            {
                FilterTypesOn = filterTypes,
                IsParentFilterTypes = isParentFilterTypes,
                LayerPatternOn = layerPatterns
            };
            return br.XGetObjects(readArgs);
        }

        /// <summary> Get Objects from Model Space  </summary>
        public static List<ObjectId> XGetObjects(this BlockReference br, ActionArguments readArgs)
        {
            IEnumerable<ObjectId> btr;
            Document doc = acadApp.DocumentManager.MdiActiveDocument;
            using (var tr = doc.TransactionManager.StartTransaction())
            {
                btr = ((BlockTableRecord)tr.GetObject(br.IsDynamicBlock ? br.DynamicBlockTableRecord : br.BlockTableRecord, OpenMode.ForRead))
                    .Cast<ObjectId>()
                    .Where(objId => ((Entity)tr.GetObject(objId, OpenMode.ForRead)).Visible == readArgs.FilterVisible);

                if (readArgs.FilterTypesOn != null && readArgs.FilterTypesOn.Any())
                {
                    var rxClasses = readArgs.FilterTypesOn.Select(RXObject.GetClass).ToList();
                    btr = btr.Where(x => rxClasses.Contains(readArgs.IsParentFilterTypes ? x.ObjectClass.MyParent : x.ObjectClass));
                }

                if (readArgs.LayerPatternOn != null && readArgs.LayerPatternOn.Length > 0)
                    btr = btr.Where(x =>
                    {
                        var ent = (Entity)tr.GetObject(x, OpenMode.ForRead);
                        return ent.Layer.XIsMatchFor(readArgs.LayerPatternOn);
                    });
            }

            return btr.ToList();
        }

        /// <summary> XGetObjects </summary>
        public static List<dynamic> XGetObjectsDynamic(this BlockReference br, Type[] filterTypes = null, bool isParentFilterTypes = false, string[] layerPatterns = null)
        {
            var btr = ((BlockTableRecord)(br.IsDynamicBlock ? br.DynamicBlockTableRecord : br.BlockTableRecord).GetObject(OpenMode.ForRead))
                .Cast<ObjectId>().Where(objId => ((Entity)objId.GetObject(OpenMode.ForRead)).Visible);

            if (filterTypes != null && filterTypes.Length > 0)
            {
                var rxClasses = filterTypes.Select(RXObject.GetClass).ToList();
                btr = btr.Where(x => (rxClasses.Contains((isParentFilterTypes ? x.ObjectClass.MyParent : x.ObjectClass))));
            }

            if (layerPatterns != null && layerPatterns.Length > 0)
                btr = btr.Where(x =>
                {
                    var ent = ((Entity)x.GetObject(OpenMode.ForRead));
                    return ent.Layer.XIsMatchFor(layerPatterns);
                });

            return btr.Cast<dynamic>().ToList();
        }

        public static IEnumerable<KeyValuePair<string, BlockReference>> XGetBlockReferences(this BlockTableRecord btr,
            string blockNamePattern = null, string attrPattern = null, string layerPattern = null)
        {
            Document doc = acadApp.DocumentManager.MdiActiveDocument;
            using (Transaction tr = doc.TransactionManager.StartTransaction())
            {
                if (btr.IsDynamicBlock)
                {
                    var anonymousBlockIds = btr.GetAnonymousBlockIds();
                    if (anonymousBlockIds.Count > 0)
                        foreach (ObjectId parentId in anonymousBlockIds)
                        {
                            var btr2 = (BlockTableRecord)tr.GetObject(parentId, OpenMode.ForRead, false, false);
                            var blockReferenceIds = btr2.GetBlockReferenceIds(true, true);
                            if (blockReferenceIds.Count > 0)
                            {
                                var res = ResolveBlock(btr2, blockReferenceIds, blockNamePattern, attrPattern, layerPattern);
                                if (res != null && res.Any())
                                    return res;
                            }
                        }
                }
                else
                {
                    var blockReferenceIds = btr.GetBlockReferenceIds(true, true);
                    if (blockReferenceIds.Count > 0)
                    {
                        var res = ResolveBlock(btr, blockReferenceIds, blockNamePattern, attrPattern, layerPattern);
                        if (res != null && res.Any())
                            return res;
                    }
                }
            }
            return null;
        }

        public static IEnumerable<T> XReadBlockReferences<T>(this BlockTableRecord btr,
            string blockNamePattern = null, string layerPattern = null,
            string attrTagPattern = null, string attrValuePattern = null)
            where T : BlockReference
        {
            var blockReferenceItems = btr.XGetBlockReferences(null, attrTagPattern);
            if (blockReferenceItems != null)
            {
                foreach (var blockReferenceItem in blockReferenceItems.ToList())
                {
                    var br = blockReferenceItem.Value as T; //scope.tr.GetObject(id, OpenMode.ForRead) as T;
                    if (br != null && !br.IsErased)
                    {
                        acadApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage($"\n{typeof(T).Name}: {br.Name}");

                        var ltr = br.LayerId.XAsLayer();
                        if (ltr != null && ltr.XValidate(layerPattern))
                        {
                            if (br.XValidate(blockNamePattern))
                            {
                                if (!br.XAttrValidate(attrTagPattern, attrValuePattern))
                                    continue;
                            }
                            else
                                continue;
                        }
                        else
                            continue;
                    }
                    yield return br;
                }
            }
        }

        private static List<KeyValuePair<string, BlockReference>> ResolveBlock(BlockTableRecord btr,
            ObjectIdCollection blockReferenceIds, string blockNamePattern, string attrTagPattern,
            string layerPattern)
        {
            List<KeyValuePair<string, BlockReference>> results = new List<KeyValuePair<string, BlockReference>>();
            if (blockReferenceIds.Count > 0)
            {
                var doc = acadApp.DocumentManager.MdiActiveDocument;
                using (var tr = doc.TransactionManager.StartTransaction())
                {
                    foreach (ObjectId btrId in blockReferenceIds)
                    {
                        var br = (BlockReference)tr.GetObject(btrId, OpenMode.ForRead, false, false);
                        var baseBtr = (BlockTableRecord)tr.GetObject(br.BlockTableRecord, OpenMode.ForRead);

                        var ltr = br.LayerId.XAsLayer();
                        if (ltr != null)
                        {
                            if (layerPattern.IsSearchPatternValid(() => br.Layer.Contains(layerPattern)))
                                if (blockNamePattern.IsSearchPatternValid(() => br.Name == blockNamePattern))
                                {
                                    if (btr.HasAttributeDefinitions && attrTagPattern.IsSearchPatternValid())
                                    {
                                        var attrs = br.XGetAttributes(attrTagPattern.Split(',')).OrderBy(x => x.Tag)
                                            .Select(x => x.Tag).ToList();

                                        if (!attrs.Any()) break;

                                        var blockAggrAttrName = GetBlockAttrNameAggregated(attrs);

                                        if (String.IsNullOrEmpty(blockAggrAttrName)) break;

                                        results.Add(
                                            new KeyValuePair<string, BlockReference>(
                                                $"{baseBtr.Name}|{blockAggrAttrName}", br));
                                    }
                                    else if (!attrTagPattern.IsSearchPatternValid())
                                    {
                                        results.Add(new KeyValuePair<string, BlockReference>(baseBtr.Name, br));
                                    }
                                }
                        }
                    }
                };
            }
            return results;
        }

        public static bool XValidate(this BlockReference br, string blockNamePattern, string attrTagPattern = null, string attrValuePattern = null)
        {
            if (string.IsNullOrEmpty(blockNamePattern) || blockNamePattern == "*")
                blockNamePattern = ".*";

            return Regex.IsMatch(br.Name, blockNamePattern) &&
                br.XAttrValidate(attrTagPattern, attrValuePattern);

            //return attrTagPattern == null || blockNamePattern.IsSearchPatternValid(() =>
            //           Regex.IsMatch(br.Name, blockNamePattern) &&
            //           br.XAttrValidate(attrTagPattern, attrValuePattern));
        }

        public static bool XAttrValidate(this BlockReference br, string attrTagPattern = null, string attrValuePattern = null)
        {
            if (string.IsNullOrEmpty(attrTagPattern) || attrTagPattern == "*")
                attrTagPattern = ".*";

            bool isValid = false;
            var hasAttrs = br.AttributeCollection != null && br.AttributeCollection.Count > 0;
            if (hasAttrs)
            {
                var attrs = br.XGetAttributes(new[] { attrTagPattern }, new[] { attrValuePattern });
                isValid = attrs.Any();
            }
            else
            {
                isValid = attrTagPattern == ".*";
            }

            return isValid;
        }

        public static IEnumerable<AttributeReference> XGetAttributes(this BlockReference br,
            string[] filterTagNames = null, string[] filterTagValues = null)
        {
            //var scope = acadApp.DocumentManager.MdiActiveDocument.TransactionManager.TransactionScope();
            var db = acadApp.DocumentManager.MdiActiveDocument.Database ?? HostApplicationServices.WorkingDatabase;
            List<AttributeReference> attrResults = new List<AttributeReference>();

            var isTop = db.TransactionManager.TopTransaction != null;
            var tr = db.TransactionManager.TopTransaction ?? db.TransactionManager.StartTransaction();
            //using (var tr = db.TransactionManager.StartTransaction())
            {
                foreach (ObjectId id in br.AttributeCollection)
                    attrResults.Add((AttributeReference)tr.GetObject(id, OpenMode.ForRead));

                //if (!tr.ExistTopTransaction) 
                if (!isTop) tr.Commit();
            }

            return attrResults.Where(attr =>
                   filterTagNames.PatternNormalize().Any(pattern => Regex.IsMatch(attr.Tag, pattern)) &&
                   filterTagValues.PatternNormalize().Any(pattern => Regex.IsMatch(attr.TextString, pattern)));
        }

        private static string GetBlockAttrNameAggregated(List<string> attrs)
        {
            var attrSb = new StringBuilder();
            var attrTags = attrs.Select(x => x).ToList();
            attrTags.AsParallel().ForEach(x => attrSb.Append(attrTags.FirstOrDefault(a => a == x) ?? ""));
            return attrSb.ToString();
        }

        public static IEnumerable<KeyValuePair<string, BlockReference>> XGetBlockReferencesByModelSpace(
            this BlockTableRecord btr, string attrTagPattern = null, string attrValuePattern = null,
            string spaceName = null)
        {
            Document doc = acadApp.DocumentManager.MdiActiveDocument;
            using (var tr = doc.TransactionManager.StartTransaction())
            {
                RXClass objectClass = RXObject.GetClass(typeof(BlockReference));
                DBDictionary layouts = (DBDictionary)tr.GetObject(btr.Database.LayoutDictionaryId, OpenMode.ForRead);

                foreach (var entry in layouts)
                {
                    if (entry.Key == spaceName)
                    {
                        foreach (ObjectId id in btr)
                        {
                            if (id.ObjectClass == objectClass)
                            {
                                var br = (BlockReference)tr.GetObject(id, OpenMode.ForRead);
                                BlockTableRecord btr2 = (BlockTableRecord)tr.GetObject(br.BlockTableRecord, OpenMode.ForRead);

                                var ltr = br.LayerId.XAsLayer();
                                if (ltr != null)
                                    if (attrTagPattern != null && br.AttributeCollection != null && br.AttributeCollection.Count > 0)
                                    {
                                        var attrs = br.XGetAttributes(new[] { attrTagPattern }, new[] { attrValuePattern });
                                        if (attrs.Any())
                                        {
                                            var attrText = attrs.FirstOrDefault(at => at.Tag == attrTagPattern).TextString;
                                            yield return new KeyValuePair<string, BlockReference>($"{btr2.Name}|{attrTagPattern}={attrText}", br);
                                        }
                                    }
                                    else
                                        yield return new KeyValuePair<string, BlockReference>(btr2.Name, br);
                            }
                        }
                    }
                }
            }
        }

        public static ObjectIdCollection XGetDynamicBlocksByName(this Database db, string blockName)
        {
            ObjectIdCollection res = new ObjectIdCollection();
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                //get the blockTable and iterate through all block Defs
                BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                foreach (ObjectId btrId in bt)
                {
                    //get the block Def and see if it is anonymous
                    BlockTableRecord btr = (BlockTableRecord)trans.GetObject(btrId, OpenMode.ForRead);
                    if (btr.IsDynamicBlock && btr.Name.Equals(blockName))
                    {
                        //get all anonymous blocks from this dynamic block
                        ObjectIdCollection anonymousIds = btr.GetAnonymousBlockIds();
                        ObjectIdCollection dynBlockRefs = new ObjectIdCollection();
                        foreach (ObjectId anonymousBtrId in anonymousIds)
                        {
                            //get the anonymous block
                            BlockTableRecord anonymousBtr = (BlockTableRecord)trans.GetObject(anonymousBtrId, OpenMode.ForRead);
                            //and all references to this block
                            ObjectIdCollection blockRefIds = anonymousBtr.GetBlockReferenceIds(true, true);
                            foreach (ObjectId id in blockRefIds) dynBlockRefs.Add(id);
                        }
                        res = dynBlockRefs;
                        break;
                    }
                }
                trans.Commit();
            }
            return res;
        }

        /// <summary> XGetObjects </summary>
        public static string XGetTrueName(this BlockReference br, Transaction tr = null)
        {
            var retValue = br.Name;
            BlockTableRecord btr;
            if (tr != null)
            {
                btr = tr.GetObject(br.DynamicBlockTableRecord, OpenMode.ForRead) as BlockTableRecord;
            }
            else
            {
                btr = br.DynamicBlockTableRecord.GetObject(OpenMode.ForRead) as BlockTableRecord;
            }

            if (btr != null)
            {
                var xdata = btr.XData.AsArray().FirstOrDefault(x => x.TypeCode == 1000 && !x.Value.ToString().StartsWith("{"));
                if (!String.IsNullOrEmpty(xdata.ToString()))
                    retValue = xdata.Value.ToString();
            }
            return retValue;
        }

        public static bool Duplicate(this BlockReference blk1, BlockReference blk2)
        {
            Tolerance tol = new Tolerance(1e-6, 1e-6);
            return
                blk1.OwnerId == blk2.OwnerId &&
                blk1.Name == blk2.Name &&
                blk1.Layer == blk2.Layer &&
                Math.Round(blk1.Rotation, 5) == Math.Round(blk2.Rotation, 5) &&
                blk1.Position.IsEqualTo(blk2.Position, tol) &&
                blk1.ScaleFactors.IsEqualTo(blk2.ScaleFactors, tol);
        }

        // Gets the block effective name (anonymous dynamic blocs).
        public static string XGetEffectiveName(this BlockReference br)
        {
            if (br.IsDynamicBlock)
                return br.DynamicBlockTableRecord.GetObject<BlockTableRecord>().Name;
            return br.Name;
        }

        public static Extents3d XGeometricExtents(this BlockReference br)
        {
            Document doc = acadApp.DocumentManager.MdiActiveDocument;
            //BlockReference block = (BlockReference)res.ObjectId.GetObject(OpenMode.ForRead);
            BlockTableRecord btr = (BlockTableRecord)br.BlockTableRecord.GetObject(OpenMode.ForRead);

            Extents3d extents = new Extents3d();
            try
            {
                foreach (ObjectId id in btr)
                {
                    Entity ent = (Entity)id.GetObject(OpenMode.ForRead);
                    if (ent.Bounds != null)
                    {
                        Extents3d ex = ent.GeometricExtents;
                        extents.AddExtents(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                doc.Editor.WriteMessage($"{CommandNames.UserGroup} error: {ex}");
            }

            //Point2dCollection pts2d = new Point2dCollection(5);
            //pts2d.Add(new Point2d(extents.MinPoint.X, extents.MinPoint.Y));
            //pts2d.Add(new Point2d(extents.MinPoint.X, extents.MaxPoint.Y));
            //pts2d.Add(new Point2d(extents.MaxPoint.X, extents.MaxPoint.Y));
            //pts2d.Add(new Point2d(extents.MaxPoint.X, extents.MinPoint.Y));
            //pts2d.Add(new Point2d(extents.MinPoint.X, extents.MinPoint.Y));

            return extents;
        }

        public static Extents3d XGeometricExtents(this BlockTableRecord btr)
        {
            var minX = btr.Cast<ObjectId>()
                .Select(n => n.GetObject(OpenMode.ForRead) as Entity)
                .Where(n => n != null && !(n is AttributeDefinition))
                .Min(n => n.GeometricExtents.MinPoint.X);

            var maxX = btr.Cast<ObjectId>()
                .Select(n => n.GetObject(OpenMode.ForRead) as Entity)
                .Where(n => n != null && !(n is AttributeDefinition))
                .Max(n => n.GeometricExtents.MaxPoint.X);

            var minY = btr.Cast<ObjectId>()
                .Select(n => n.GetObject(OpenMode.ForRead) as Entity)
                .Where(n => n != null && !(n is AttributeDefinition))
                .Min(n => n.GeometricExtents.MinPoint.Y);

            var maxY = btr.Cast<ObjectId>()
                .Select(n => n.GetObject(OpenMode.ForRead) as Entity)
                .Where(n => n != null && !(n is AttributeDefinition))
                .Max(n => n.GeometricExtents.MaxPoint.Y);

            return new Extents3d(new Point3d(minX, minY, 0), new Point3d(maxX, maxY, 0));
        }

        public static double XGetYMax(BlockTableRecord btr)
        {
            return btr.Cast<ObjectId>()
                .Select(n => n.GetObject(OpenMode.ForRead) as Entity)
                .Where(n => n != null && !(n is AttributeDefinition))
                .Max(n => n.GeometricExtents.MaxPoint.Y);
        }

        #endregion

        #region Attrs

        public static bool XIsMatchFor(this ObjectId id, Transaction tr, params string[] patterns)
        {
            bool retValue;

            if (tr != null)
            {
                retValue = ((BlockReference)tr.GetObject(id, OpenMode.ForRead)).Layer.XIsMatchFor(patterns);
            }
            else
            {
                using (tr = acadApp.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction())
                {
                    retValue = ((BlockReference)tr.GetObject(id, OpenMode.ForRead)).Layer.XIsMatchFor(patterns);
                }
            }

            return retValue;
        }

        public static bool XIsMatchFor(this BlockReference br, Transaction tr, params string[] patterns)
        {
            var retValue = br.Layer.XIsMatchFor(patterns);
            return retValue;
        }

        public static bool XContainsAttributeTags(this ObjectId id, Transaction tr, params string[] filterTags)
        {
            var retValue = XContainsAttributeTags((BlockReference)id.GetObject(OpenMode.ForRead), tr, filterTags);
            return retValue;
        }

        public static bool XContainsAttributeTags(this BlockReference br, Transaction tr, params string[] filterTags)
        {
            var retValue = br.AttributeCollection.Cast<ObjectId>()
                .Select(attr => (AttributeReference)tr.GetObject(attr, OpenMode.ForRead))
                .Any(x => filterTags.Contains(x.Tag));
            return retValue;
        }

        public static bool DynamicXContainsAttributeTags(this ObjectId id, params string[] filterTags)
        {
            return DynamicXContainsAttributeTags((BlockReference)id.GetObject(OpenMode.ForRead), filterTags);
        }

        public static bool DynamicXContainsAttributeTags(this BlockReference br, params string[] filterTags)
        {
            var retValue = br.AttributeCollection.Cast<ObjectId>()
                .Select(attr => (AttributeReference)attr.GetObject(OpenMode.ForRead))
                .Any(x => filterTags.Contains(x.Tag));
            return retValue;
        }

        public static IEnumerable<AttributeReference> XGetAttributes(this BlockReference br, Func<AttributeReference, bool> exp)
        {
            var tr = acadApp.DocumentManager.MdiActiveDocument.TransactionManager.StartTransaction();

            IEnumerable<AttributeReference> attrResults = new List<AttributeReference>();
            IEnumerable<ObjectId> attrList = br.AttributeCollection.Cast<ObjectId>().ToList();

            if (!attrList.Any())
                return attrResults;

            return attrList.Select(attr => (AttributeReference)tr.GetObject(attr, OpenMode.ForRead)).Where(exp);
        }

        //[CommandMethod("AppendAttributeToBlock", "appatt", CommandFlags.Session | CommandFlags.Modal | CommandFlags.UsePickSet | CommandFlags.Redraw)]
        public static void AppendAttributeTest()
        {
            Document doc = acadApp.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;

            try
            {
                using (doc.LockDocument())
                {
                    using (Transaction tr = db.TransactionManager.StartTransaction())
                    {
                        BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                        BlockTableRecord currSp = tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;

                        PromptNestedEntityOptions pno =
                            new PromptNestedEntityOptions("\nSelect source attribute to append new attribute below this one >>");

                        PromptNestedEntityResult nres =
                            ed.GetNestedEntity(pno);

                        if (nres.Status != PromptStatus.OK)

                            return;

                        ObjectId id = nres.ObjectId;
                        Entity ent = (Entity)tr.GetObject(id, OpenMode.ForRead);
                        Point3d pnt = nres.PickedPoint;
                        ObjectId owId = ent.OwnerId;
                        AttributeReference attref = null;

                        if (id.ObjectClass.IsDerivedFrom(RXClass.GetClass(typeof(AttributeReference))))
                        {
                            attref = tr.GetObject(id, OpenMode.ForWrite) as AttributeReference;
                        }

                        BlockTableRecord btr = null;
                        BlockReference bref = null;

                        if (owId.ObjectClass.IsDerivedFrom(RXClass.GetClass(typeof(BlockReference))))
                        {
                            bref = tr.GetObject(owId, OpenMode.ForWrite) as BlockReference;

                            if (bref.IsDynamicBlock)
                            {
                                btr = tr.GetObject(bref.DynamicBlockTableRecord, OpenMode.ForRead) as BlockTableRecord;
                            }
                            else
                            {
                                btr = tr.GetObject(bref.BlockTableRecord, OpenMode.ForRead) as BlockTableRecord;
                            }
                        }

                        Point3d insPt = attref.Position.TransformBy(bref.BlockTransform);
                        btr.UpgradeOpen();

                        ObjectIdCollection bids = new ObjectIdCollection();
                        AttributeDefinition def = null;

                        foreach (ObjectId defid in btr)
                        {
                            if (defid.ObjectClass.IsDerivedFrom(RXClass.GetClass(typeof(AttributeDefinition))))
                            {
                                def = tr.GetObject(defid, OpenMode.ForRead) as AttributeDefinition;

                                if (def.Tag == attref.Tag)
                                {
                                    def.UpgradeOpen();

                                    bids.Add(defid);

                                    break;
                                }
                            }
                        }

                        IdMapping map = new IdMapping();

                        db.DeepCloneObjects(bids, btr.ObjectId, map, true);

                        ObjectIdCollection coll = new ObjectIdCollection();
                        AttributeDefinition attDef = null;

                        foreach (IdPair pair in map)
                        {
                            if (pair.IsPrimary)
                            {
                                Entity oent = (Entity)tr.GetObject(pair.Value, OpenMode.ForWrite);

                                if (oent != null)
                                {

                                    if (pair.Value.ObjectClass.IsDerivedFrom(RXClass.GetClass(typeof(AttributeDefinition))))
                                    {
                                        attDef = oent as AttributeDefinition;

                                        attDef.UpgradeOpen();

                                        attDef.SetPropertiesFrom(def as Entity);
                                        // add other properties from source attribute definition to suit here:

                                        attDef.Justify = def.Justify;

                                        attDef.Position = btr.Origin.Add(

                                            new Vector3d(attDef.Position.X, attDef.Position.Y - attDef.Height * 1.25, attDef.Position.Z)).TransformBy(Matrix3d.Identity

                                        );

                                        attDef.Tag = "NEW_TAG";

                                        attDef.TextString = "New Prompt";

                                        attDef.TextString = "New Textstring";

                                        coll.Add(oent.ObjectId);


                                    }
                                }
                            }
                        }
                        btr.AssumeOwnershipOf(coll);

                        btr.DowngradeOpen();

                        attDef.Dispose();//optional

                        bref.RecordGraphicsModified(true);

                        tr.TransactionManager.QueueForGraphicsFlush();

                        doc.TransactionManager.FlushGraphics();//optional

                        ed.UpdateScreen();

                        tr.Commit();
                    }

                }
            }

            catch (Exception ex)
            {
                ed.WriteMessage(ex.Message + "\n" + ex.StackTrace);
            }
            finally
            {
                acadApp.ShowAlertDialog("Call command \"ATTSYNC\" manually");
            }

        }

        public static string XGetAttributesData(this BlockReference bref)
        {
            Document doc = acadApp.DocumentManager.MdiActiveDocument;
            bool existTopTransaction = doc.Database.TransactionManager.TopTransaction != null;
            Transaction tr = doc.Database.TransactionManager.TopTransaction ?? doc.Database.TransactionManager.StartTransaction();

            int mix = 24;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{"Block:".PadRight(mix - 6, ' ')}{bref.Name}\n");

            var info = (from ObjectId id in bref.AttributeCollection
                        let attref = (AttributeReference)tr.GetObject(id, OpenMode.ForRead, false)
                        select new { key = attref.Tag, value = attref.TextString }).ToArray();

            foreach (var item in info)
            {
                var left = item.key.Length;
                sb.AppendLine($"{item.key.PadRight(mix - left, ' ')}{item.value}");
            }

            if (!existTopTransaction)
                tr.Commit();

            return sb.ToString();
        }

        private static string CollectAttributeText(this AttributeDefinition attDef)
        {
            string ret = String.Empty;
            PromptStringOptions prStrOpt = new PromptStringOptions("");
            prStrOpt.AllowSpaces = true;
            prStrOpt.DefaultValue = attDef.TextString;
            prStrOpt.UseDefaultValue = true;
            prStrOpt.Message = attDef.Prompt;
            PromptResult pr = acadApp.DocumentManager.MdiActiveDocument.Editor.GetString(prStrOpt);
            if (pr.Status == PromptStatus.OK)
            {
                ret = pr.StringResult;
            }
            return ret;
        }

        #endregion

        #region <AddBlockReference>

        private static ObjectId AddBlockReference_(this Database db, ObjectId blockId, Point3d insertPoint,
            BlockOptions options)
        {
            try
            {
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                    BlockReference br;
                    //using (BlockTableRecord btr = (BlockTableRecord)db.CurrentSpaceId.GetObject(OpenMode.ForRead))
                    using (BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[acadApp.DocumentManager.GetCurrentSpace()], OpenMode.ForRead))
                    {
                        br = new BlockReference(insertPoint, blockId);
                        br.SetDatabaseDefaults();

                        if (Math.Abs(options.Scale - 1.0) > 0.01)
                            br.ScaleFactors = new Scale3d(options.Scale);

                        if (!String.IsNullOrEmpty(options.LayerName))
                        {
                            var layerService = Plugin.GetService<ILayerService>();
                            if (!layerService.Contains(options.LayerName))
                                layerService.Add(options.LayerName);
                            br.Layer = options.LayerName;
                        }

                        if (options.Transform != default(Matrix3d))
                            br.TransformBy(options.Transform);

                        btr.UpgradeOpen();
                        btr.AppendEntity(br);
                        tr.AddNewlyCreatedDBObject(br, true);
                        btr.DowngradeOpen();

                        // HasAttributeDefinitions
                        Dictionary<AttributeReference, AttributeDefinition> attrDict =
                            new Dictionary<AttributeReference, AttributeDefinition>();
                        if (btr.HasAttributeDefinitions)
                        {
                            RXClass rxClass = RXObject.GetClass(typeof(AttributeDefinition));
                            foreach (ObjectId id in btr)
                            {
                                if (id.ObjectClass == rxClass)
                                {
                                    DBObject obj = tr.GetObject(id, OpenMode.ForRead);
                                    AttributeDefinition ad = obj as AttributeDefinition;

                                    AttributeReference ar = new AttributeReference();
                                    ar.SetAttributeFromBlock(ad, br.BlockTransform);
                                    ar.TextString = ad.CollectAttributeText();

                                    br.AttributeCollection.AppendAttribute(ar);
                                    tr.AddNewlyCreatedDBObject(ar, true);

                                    attrDict.Add(ar, ad);
                                }
                            }
                        }

                        // JigPrompt
                        if (options.JigPrompt != eJigPrompt.NoPrompt)
                        {
                            if (!BlockAttributeJig.CreateJig(br, attrDict, options.JigPrompt))
                            {
                                br.Erase(true);
                                br.Dispose();
                                attrDict.ForEach(entry => entry.Value.Dispose());
                                return ObjectId.Null;
                            }
                        }
                    }
                    return br.ObjectId;
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex.InnerException?.Message ?? ex.Message);
                return ObjectId.Null;
            }
        }

        private static ObjectId AddBlockReference_(this Database db, string blockName, Point3d insertPoint,
            string layerName)
        {
            return db.AddBlockReference_(blockName, insertPoint, new BlockOptions(layerName));
        }

        /// <summary> Add BlockReference </summary>
        private static ObjectId AddBlockReference_(this Database db, string blockName, Point3d insertPoint,
            BlockOptions options)
        {
            ObjectId result = ObjectId.Null;
            Document doc = acadApp.DocumentManager.GetDocument(db);

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                if (bt.Has(blockName))
                {
                    ObjectId blockId = bt[blockName];
                    var br = new BlockReference(insertPoint, blockId);

                    // Scale
                    if (Math.Abs(options.Scale - 1.0) > 0.01)
                        br.ScaleFactors = new Scale3d(options.Scale);

                    // Layer
                    if (!String.IsNullOrEmpty(options.LayerName))
                    {
                        ILayerService layerService = Plugin.GetService<ILayerService>();
                        if (!layerService.Contains(options.LayerName))
                            layerService.Add(options.LayerName);
                        br.Layer = options.LayerName;
                    }

                    // Hyperlinks
                    HyperLink hyperLink = new HyperLink
                    {
                        Description = CommandNames.UserGroup + " gis point",
                        Name = CommandNames.UserGroup + " gis point",
                        SubLocation = "http://vmmapinfo/AcadNetGis/PartnerGis?lon={0}&lat={1}"
                    };
                    br.Hyperlinks.Add(hyperLink);

                    // Transform
                    if (options.Transform != default(Matrix3d))
                        br.TransformBy(options.Transform);
                    //Matrix3d.Displacement(insertPoint - Point3d.Origin).PreMultiplyBy(doc.Editor.CurrentUserCoordinateSystem)

                    BlockTableRecord btr = (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);
                    //BlockTableRecord btr = (BlockTableRecord)Tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                    // Annotative
                    if (btr.Annotative == AnnotativeStates.True)
                    {
                        ObjectContextManager ocm = db.ObjectContextManager;
                        ObjectContextCollection occ = ocm.GetContextCollection("ACDB_ANNOTATIONSCALES");
                        br.AddContext(occ.CurrentContext);
                    }

                    btr.AppendEntity(br);
                    tr.AddNewlyCreatedDBObject(br, true);

                    // HasAttributeDefinitions
                    Dictionary<AttributeReference, AttributeDefinition> attrDict =
                        new Dictionary<AttributeReference, AttributeDefinition>();
                    if (btr.HasAttributeDefinitions)
                    {
                        RXClass rxClass = RXObject.GetClass(typeof(AttributeDefinition));
                        foreach (ObjectId id in btr)
                        {
                            if (id.ObjectClass == rxClass)
                            {
                                DBObject obj = tr.GetObject(id, OpenMode.ForRead);
                                AttributeDefinition ad = obj as AttributeDefinition;

                                AttributeReference ar = new AttributeReference();
                                ar.SetAttributeFromBlock(ad, br.BlockTransform);
                                ar.TextString = ad.CollectAttributeText();

                                br.AttributeCollection.AppendAttribute(ar);
                                tr.AddNewlyCreatedDBObject(ar, true);

                                attrDict.Add(ar, ad);
                            }
                        }
                    }

                    // JigPrompt
                    if (options.JigPrompt != eJigPrompt.NoPrompt)
                    {
                        if (BlockAttributeJig.CreateJig(br, attrDict, options.JigPrompt))
                        {
                            result = br.ObjectId;
                        }
                        else
                        {
                            br.Dispose();
                            foreach (KeyValuePair<AttributeReference, AttributeDefinition> entry in attrDict)
                            {
                                entry.Value.Dispose();
                            }
                            result = ObjectId.Null;
                        }
                    }
                    else
                    {
                        result = br.ObjectId;
                    }
                }
            }
            return result;
        }

        /// <summary> InsertDrawingAsBlock  </summary>
        /// <summary> Add BlockReference </summary>
        public static ObjectId AddBlockReference_(this Database db, Transaction tr, ObjectId btrId, Point3d insertPoint,
            string layerName, double scale = 1.0)
        {
            ObjectId brObjectId = ObjectId.Null;
            using (BlockTableRecord btr = (BlockTableRecord)db.CurrentSpaceId.GetObject(OpenMode.ForWrite))
            {
                using (BlockReference br = new BlockReference(insertPoint, btrId))
                {
                    Matrix3d transform = Matrix3d.Identity;
                    transform = transform * Matrix3d.Scaling(scale, insertPoint);
                    //br.ScaleFactors = new Scale3d(1);
                    //br.TransformBy(transform);
                    br.TransformBy(acadApp.DocumentManager.MdiActiveDocument.Editor.CurrentUserCoordinateSystem);
                    //br.Transparency = new Transparency(TransparencyMethod.ByAlpha);

                    // Hyperlinks
                    HyperLink hyperLink = new HyperLink
                    {
                        Description = CommandNames.UserGroup + " gis point",
                        Name = CommandNames.UserGroup + " gis point",
                        SubLocation = "http://vmmapinfo/AcadNetGis/PartnerGis?lon={0}&lat={1}"
                    };
                    br.Hyperlinks.Add(hyperLink);

                    // Layer
                    var layerService = Plugin.GetService<ILayerService>();
                    br.Layer = layerService.GetWorkLayerName(layerName);

                    //if (br.Hyperlinks.Count != 0)
                    //{
                    //    AttributeCollection attrs = br.AttributeCollection;
                    //    foreach (ObjectId id in attrs)
                    //    {
                    //        AttributeReference attRef = Tr.GetObject(id, OpenMode.ForWrite) as AttributeReference;
                    //        if (attRef != null) attRef.Hyperlinks.Add(hyperLink);
                    //    }
                    //}

                    // Annotative
                    if (btr.Annotative == AnnotativeStates.True)
                    {
                        ObjectContextManager ocm = db.ObjectContextManager;
                        ObjectContextCollection occ = ocm.GetContextCollection("ACDB_ANNOTATIONSCALES");
                        br.AddContext(occ.CurrentContext);
                    }

                    btr.AppendEntity(br);
                    tr.AddNewlyCreatedDBObject(br, true);

                    using (
                        BlockTableRecord btAttRec = (BlockTableRecord)br.BlockTableRecord.GetObject(OpenMode.ForRead))
                    {
                        AttributeCollection attrs = br.AttributeCollection;
                        foreach (ObjectId subid in btAttRec)
                        {
                            Entity ent = (Entity)subid.GetObject(OpenMode.ForRead);
                            AttributeDefinition attDef = ent as AttributeDefinition;

                            if (attDef != null)
                            {
                                // Ed.WriteMessage("\nValue: " + attDef.TextString);
                                using (AttributeReference attRef = new AttributeReference())
                                {
                                    attRef.SetPropertiesFrom(attDef);
                                    attRef.Visible = attDef.Visible;
                                    attRef.SetAttributeFromBlock(attDef, br.BlockTransform);
                                    attRef.HorizontalMode = attDef.HorizontalMode;
                                    attRef.VerticalMode = attDef.VerticalMode;
                                    attRef.Rotation = attDef.Rotation;
                                    attRef.TextStyleId = attDef.TextStyleId;
                                    attRef.Position = attDef.Position + insertPoint.GetAsVector();
                                    attRef.Tag = attDef.Tag;
                                    attRef.FieldLength = attDef.FieldLength;
                                    attRef.TextString = attDef.TextString;
                                    attRef.AdjustAlignment(db); //?

                                    attrs.AppendAttribute(attRef);
                                    tr.AddNewlyCreatedDBObject(attRef, true);
                                }
                            }
                        }
                    }
                    br.DowngradeOpen();
                    brObjectId = br.ObjectId;
                }
            }
            return brObjectId;
        }

        #endregion

        #region <Import and Insert>

        public static ObjectId ImportDwgAsBlock(this Database db, string sourceDrawing, Point3d insertionPoint)
        {
            ObjectId sourceBlockId = ObjectId.Null;
            Document doc = acadApp.DocumentManager.GetDocument(db);
            Editor ed = doc.Editor;
            Matrix3d ucs = ed.CurrentUserCoordinateSystem;

            string blockname = sourceDrawing.Remove(0, sourceDrawing.LastIndexOf("\\", StringComparison.Ordinal) + 1);
            blockname = blockname.Substring(0, blockname.Length - 4); // remove the extension

            try
            {
                using (doc.LockDocument())
                {
                    using (var inMemoryDb = new Database(false, true))
                    {
                        #region Load the drawing into temporary inmemory database

                        if (sourceDrawing.LastIndexOf(".dwg", StringComparison.Ordinal) > 0)
                        {
                            inMemoryDb.ReadDwgFile(sourceDrawing, FileShare.Read, false, "");
                        }
                        else if (sourceDrawing.LastIndexOf(".dxf", StringComparison.Ordinal) > 0)
                        {
                            //_logger.Error(MethodBase.GetCurrentMethod().DeclaringType.FullName + "." + MethodBase.GetCurrentMethod().Name + " : Tried to invoke the method with .dxf file.");
                            throw new NotImplementedException("Importing .dxf is not supported in this version.");
                            //inMemoryDb.DxfIn("@" + sourceDrawing, System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + "\\log\\import_block_dxf_log.txt");
                        }
                        else
                        {
                            throw new ArgumentException("This is not a valid drawing.");
                        }

                        #endregion

                        using (var transaction = db.TransactionManager.StartTransaction())
                        {
                            BlockTable destDbBlockTable =
                                (BlockTable)transaction.GetObject(db.BlockTableId, OpenMode.ForRead);
                            BlockTableRecord destDbCurrentSpace =
                                (BlockTableRecord)db.CurrentSpaceId.GetObject(OpenMode.ForWrite);

                            // If the destination DWG already contains this block definition
                            // we will create a block reference and not a copy of the same definition

                            if (destDbBlockTable.Has(blockname))
                            {

                                //BlockTableRecord destDbBlockDefinition = (BlockTableRecord)transaction.GetObject(destDbBlockTable[blockname], OpenMode.ForRead);
                                //sourceBlockId = destDbBlockDefinition.ObjectId;

                                sourceBlockId =
                                    transaction.GetObject(destDbBlockTable[blockname], OpenMode.ForRead).ObjectId;

                                // Create a block reference to the existing block definition
                                using (var blockReference = new BlockReference(insertionPoint, sourceBlockId))
                                {
                                    ed.CurrentUserCoordinateSystem = Matrix3d.Identity;
                                    blockReference.TransformBy(ucs);
                                    ed.CurrentUserCoordinateSystem = ucs;
                                    //var converter = new MeasurementUnitsConverter();
                                    var scaleFactor = 2.0; //converter.GetScaleRatio(inMemoryDb.Insunits, db.Insunits);
                                    blockReference.ScaleFactors = new Scale3d(scaleFactor);
                                    destDbCurrentSpace.AppendEntity(blockReference);
                                    transaction.AddNewlyCreatedDBObject(blockReference, true);
                                    ed.Regen();
                                    transaction.Commit();
                                    // At this point the Bref has become a DBObject and (can be disposed) and will be disposed by the transaction
                                }
                                return sourceBlockId;
                            }

                            //else // There is not such block definition, so we are inserting/creating new one

                            sourceBlockId = db.Insert(blockname, inMemoryDb, true);
                            BlockTableRecord sourceBlock = (BlockTableRecord)sourceBlockId.GetObject(OpenMode.ForRead);
                            sourceBlock.UpgradeOpen();
                            sourceBlock.Name = blockname;
                            destDbCurrentSpace.DowngradeOpen();
                            var sourceBlockMeasurementUnits = inMemoryDb.Insunits;
                            try
                            {
                                //db.AddBlockReference(transaction, sourceBlockId, insertionPoint, "Gis", 1);
                                db.CreateBlockReference(transaction, sourceBlockId,
                                    insertionPoint,
                                    destDbCurrentSpace,
                                    destDbBlockTable);
                            }
                            catch (ArgumentException ex)
                            {
                                Log.Logger.Error(ex.InnerException?.Message ?? ex.Message);
                            }

                            ed.Regen();
                            transaction.Commit();
                        }
                    }
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                Log.Logger.Error(ex.InnerException?.Message ?? ex.Message);
            }
            return sourceBlockId;
        }

        private static void CreateBlockReference(this Database db, Transaction transaction, ObjectId sourceBlockId,
            Point3d insertionPoint, BlockTableRecord destDbCurrentSpace, BlockTable destDbBlockTable)
        {
            Document doc = acadApp.DocumentManager.GetDocument(db);
            Editor ed = doc.Editor;
            Matrix3d ucs = ed.CurrentUserCoordinateSystem;

            using (var blockReference = new BlockReference(insertionPoint, sourceBlockId))
            {
                ed.CurrentUserCoordinateSystem = Matrix3d.Identity;
                blockReference.TransformBy(ucs);
                ed.CurrentUserCoordinateSystem = ucs;
                //var converter = new MeasurementUnitsConverter();
                var scaleFactor = 2.0; //converter.GetScaleRatio(inMemoryDb.Insunits, db.Insunits);
                blockReference.ScaleFactors = new Scale3d(scaleFactor);
                destDbCurrentSpace.AppendEntity(blockReference);
                transaction.AddNewlyCreatedDBObject(blockReference, true);
                ed.Regen();
                transaction.Commit();
                // At this point the Bref has become a DBObject and (can be disposed) and will be disposed by the transaction
            }
        }

        /// <summary> ImportBlock </summary>
        public static ObjectId Import_(this Database db, string blockName)
        {
            ObjectId blockId;
            //Document doc = App.DocumentManager.GetDocument(db);
            DocumentCollection dm = acadApp.DocumentManager;
            Document doc = dm.MdiActiveDocument;
            db = dm.MdiActiveDocument.Database;

            using (Database tmpDb = new Database(false, true))
            {
                try
                {
                    var appSettings = Plugin.GetService<IPluginSettings>();
                    var libName = blockName.Contains("\\") ? blockName : appSettings.ResourceLib;
                    tmpDb.ReadDwgFile(libName, FileShare.Read, true, "");
                    tmpDb.CloseInput(true);

                }
                catch (Exception ex)
                {
                    doc.Editor.WriteMessage("\nUnable to read drawing file." + ex);
                    return ObjectId.Null;
                }

                // Create a variable to store the list of block identifiers
                ObjectIdCollection blockIds = new ObjectIdCollection();
                using (Transaction tr = tmpDb.TransactionManager.StartTransaction())
                {
                    // Open the block table
                    BlockTable bt = (BlockTable)tr.GetObject(tmpDb.BlockTableId, OpenMode.ForRead, false);

                    if (!String.IsNullOrEmpty(blockName) && bt.Has(blockName))
                        blockIds.Add(bt[blockName]);
                    else
                    {
                        // Check each block in the block table
                        foreach (ObjectId btrId in bt)
                        {
                            BlockTableRecord btr = (BlockTableRecord)tr.GetObject(btrId, OpenMode.ForRead, false);
                            // Only add named & non-layout blocks to the copy list
                            if (!btr.IsAnonymous && !btr.IsLayout)
                                blockIds.Add(btrId);
                            btr.Dispose();
                        }
                    }
                    blockId = bt[blockName];
                    //Tr.Commit();

                    // Copy blocks from source to destination database
                    IdMapping mapping = new IdMapping();
                    tmpDb.WblockCloneObjects(blockIds, db.BlockTableId, mapping,
                        DuplicateRecordCloning.Replace, false);
                }
            }
            return blockId;
        }

        /// <summary> ImportBlock </summary>
        public static ObjectId ImportBlock(this Database db, string blockName)
        {
            ObjectId blockId = new ObjectId();
            var appSettings = Plugin.GetService<IPluginSettings>();
            var fullPath = blockName.Contains("\\") ? blockName : appSettings.ResourceLib;
            blockName = blockName.Contains("\\") ? Path.GetFileNameWithoutExtension(blockName) : blockName;

            Document doc = acadApp.DocumentManager.GetDocument(db);
            Editor ed = doc.Editor;
            Database tempDb = new Database(false, true);

            try
            {
                tempDb.ReadDwgFile(fullPath, FileShare.Read, true, "");

                // Create a variable to store the list of block identifiers
                ObjectIdCollection blockIds = new ObjectIdCollection();
                TransactionManager tm = tempDb.TransactionManager;

                using (Transaction tr = tempDb.TransactionManager.StartTransaction())
                {
                    BlockTable bt = (BlockTable)tm.GetObject(tempDb.BlockTableId, OpenMode.ForRead, false);

                    if (!String.IsNullOrEmpty(blockName) && bt.Has(blockName))
                    {
                        blockIds.Add(bt[blockName]);
                        blockId = bt[blockName];
                    }
                    else
                    {
                        foreach (ObjectId btrId in bt)
                        {
                            BlockTableRecord btr = (BlockTableRecord)tm.GetObject(btrId, OpenMode.ForRead, false);
                            if (!btr.IsAnonymous && !btr.IsLayout)
                                blockIds.Add(btrId);
                            btr.Dispose();
                            blockId = btrId;
                        }
                    }
                }

                // Copy blocks from source to destination database
                IdMapping mapping = new IdMapping();
                tempDb.WblockCloneObjects(blockIds, db.BlockTableId, mapping, DuplicateRecordCloning.Replace, false);
            }

            catch (Exception ex)
            {
                ed.WriteMessage("\nImportBlocks error: " + ex.Message);
                blockId = ObjectId.Null;
            }
            tempDb.Dispose();
            return blockId;
        }

        /// <summary> Insert Block start</summary>
        public static ObjectId InsertBlock(this Database db, string blockName, Point3d insertPoint, BlockOptions options)
        {
            ObjectId blockId = new ObjectId();
            Document doc = acadApp.DocumentManager.GetDocument(db);

            try
            {
                var appSettings = Plugin.GetService<IPluginSettings>();
                var fullPath = blockName.Contains("\\")
                    ? blockName
                    : appSettings.ResourceLib.Replace("lib", blockName);

                blockName = blockName.Contains("\\") ? Path.GetFileNameWithoutExtension(blockName) : blockName;

                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                    if (!bt.Has(blockName) || options.IsBlockExistCreateNew)//blockId = bt[blockName];
                    {
                        if (options.IsBlockExistCreateNew)
                            blockName = blockName + "_" + Path.GetFileNameWithoutExtension(Path.GetTempFileName());

                        var document = Documents.DocumentFind(fullPath);
                        if (document != null && document != doc)
                            Documents.DocumentAction(fullPath, DocumentOptions.CloseAndDiscard);

                        if (File.Exists(fullPath))
                        {
                            // add the block to the ActiveDrawing blockTable
                            Database tmpDb = new Database(false, true);
                            tmpDb.ReadDwgFile(fullPath, FileShare.Read, true, "");
                            tmpDb.CloseInput(true);

                            using (doc.LockDocument())
                            {
                                db.Insert(blockName, tmpDb, false);
                            }
                            tr.Commit();
                        }
                    }
                }

                blockId = db.AddBlockReference(blockName, insertPoint, options);
                if (options.IsRegen) doc.Editor.Regen();

            }
            catch (Exception ex)
            {
                doc.Editor.WriteMessage("\nImportBlocks error: " + ex.Message);
                blockId = ObjectId.Null;
            }

            return blockId;
        }

        /// <summary> Insert Drawing As Block </summary>
        public static ObjectId InsertDrawingAsBlock(this Document doc, string blockname, Point3d insertPoint)
        {
            Database curdb = doc.Database;
            Editor ed = doc.Editor;
            ObjectId blockId = ObjectId.Null;

            var appSettings = Plugin.GetService<IPluginSettings>();
            var path = blockname.Contains("\\") ? blockname : appSettings.ResourceLib.Replace("lib", blockname);
            blockname = blockname.Contains("\\") ? Path.GetFileNameWithoutExtension(blockname) : blockname;

            DocumentLock loc = doc.LockDocument();
            using (loc)
            {
                Database db = new Database(false, true);
                using (db)
                {
                    db.ReadDwgFile(path, FileShare.Read, true, "");
                    blockId = curdb.Insert(path, db, true);

                    using (Transaction tr = doc.TransactionManager.StartTransaction())
                    {
                        BlockTable bt = (BlockTable)tr.GetObject(curdb.BlockTableId, OpenMode.ForRead);
                        if (bt.Has(blockname))
                        {
                            //MessageBox.Show(string.Format("Block {0} does already exist\nTry another block or Exit", blockname));
                            blockId = db.Insert(blockname, db, true);
                            return blockId;
                        }

                        bt.UpgradeOpen();

                        BlockTableRecord btrec = (BlockTableRecord)blockId.GetObject(OpenMode.ForRead);
                        btrec.UpgradeOpen();
                        btrec.Name = blockname;
                        btrec.DowngradeOpen();

                        //---> debug only
                        // this code block is written in the good programming manner (remember that)
                        //foreach (ObjectId index in btrec)
                        //{
                        //    Entity en = (Entity) index.GetObject(OpenMode.ForRead);
                        //    AttributeDefinition adef = en as AttributeDefinition;
                        //    if (adef != null)
                        //    {
                        //        ed.WriteMessage("\n" + adef.Tag);
                        //    }
                        //} //<--- debug only

                        BlockTableRecord btr = (BlockTableRecord)curdb.CurrentSpaceId.GetObject(OpenMode.ForWrite);
                        using (btr)
                        {
                            using (BlockReference bref = new BlockReference(insertPoint, blockId))
                            {
                                Matrix3d mat = Matrix3d.Identity;
                                bref.TransformBy(mat);
                                bref.ScaleFactors = new Scale3d(1, 1, 1);
                                btr.AppendEntity(bref);
                                tr.AddNewlyCreatedDBObject(bref, true);

                                using (BlockTableRecord btAttRec = (BlockTableRecord)bref.BlockTableRecord.GetObject(OpenMode.ForRead))
                                {
                                    AttributeCollection atcoll = bref.AttributeCollection;
                                    foreach (ObjectId subid in btAttRec)
                                    {
                                        Entity ent = (Entity)subid.GetObject(OpenMode.ForRead);
                                        AttributeDefinition attDef = ent as AttributeDefinition;

                                        if (attDef != null)
                                        {
                                            // ed.WriteMessage("\nValue: " + attDef.TextString);
                                            AttributeReference attRef = new AttributeReference();
                                            attRef.SetPropertiesFrom(attDef);
                                            attRef.Visible = attDef.Visible;
                                            attRef.SetAttributeFromBlock(attDef, bref.BlockTransform);
                                            attRef.HorizontalMode = attDef.HorizontalMode;
                                            attRef.VerticalMode = attDef.VerticalMode;
                                            attRef.Rotation = attDef.Rotation;
                                            attRef.TextStyleId = attDef.TextStyleId;
                                            attRef.Position = attDef.Position + insertPoint.GetAsVector();
                                            attRef.Tag = attDef.Tag;
                                            attRef.FieldLength = attDef.FieldLength;
                                            attRef.TextString = attDef.TextString;
                                            attRef.AdjustAlignment(curdb); //?
                                            atcoll.AppendAttribute(attRef);
                                            tr.AddNewlyCreatedDBObject(attRef, true);
                                        }
                                    }
                                }
                                bref.DowngradeOpen();
                            }
                        }

                        btrec.DowngradeOpen();
                        bt.DowngradeOpen();
                        ed.Regen();
                        tr.Commit();
                    }
                }
            }

            return blockId;
        }

        /// <summary> InsertBlock  </summary>
        public static ObjectId Insert_(this Database db, string blockName, Point3d insertPoint)
        {
            ObjectId blockRefId;
            var appSettings = Plugin.GetService<IPluginSettings>();
            var fullPath = blockName.Contains("\\") ? blockName : appSettings.ResourceLib;
            blockName = blockName.Contains("\\") ? Path.GetFileNameWithoutExtension(blockName) : blockName;

            Document doc = acadApp.DocumentManager.GetDocument(db);
            Editor ed = doc.Editor;

            using (doc.LockDocument())
            {
                using (var tr = db.TransactionManager.StartTransaction())
                {
                    BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                    if (!bt.Has(blockName))
                    {
                        var document = Documents.DocumentFind(fullPath);
                        if (document != null && document != doc)
                            Documents.DocumentAction(fullPath, DocumentOptions.CloseAndDiscard);

                        Database tmpDb = new Database(false, true);
                        try
                        {
                            tmpDb.ReadDwgFile(fullPath, FileShare.Read, true, "");

                            // add the block to the ActiveDrawing blockTable
                            db.Insert(blockName, tmpDb, true);
                        }
                        catch (Exception ex)
                        {
                            ed.WriteMessage("Error: {0} method; message: {1}", "ReadDwgFile/db.Insert", ex.ToString());
                        }
                    }

                    //PromptPointResult ppr = ed.GetPoint("\nSpecify insertion point: ");
                    //if (ppr.Status != PromptStatus.OK)
                    //    return;
                    //insertion point: ppr.Value

                    blockRefId = db.AddBlockReference(blockName, insertPoint, new BlockOptions());

                    tr.Commit();
                }
            }
            return blockRefId;
        }

        public static void InsertDrawingAsBlock(this Database db, string path, string blockName, Point3d insertPoint,
            string layerName)
        {
            var doc = acadApp.DocumentManager.MdiActiveDocument;
            using (doc.LockDocument())
            using (Database tempDb = new Database(false, true))
            {

                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                    if (!bt.Has(blockName))
                    {
                        tempDb.ReadDwgFile(path, FileShare.Read, true, "");
                        db.Insert(path, blockName, tempDb, true);
                    }

                    //bt.UpgradeOpen();
                    //BlockTableRecord btrec = (BlockTableRecord)blockId.GetObject(OpenMode.ForRead);
                    //btrec.UpgradeOpen();
                    //btrec.Name = blockName;
                    //btrec.DowngradeOpen();

                    db.InsertBlock(blockName, insertPoint, new BlockOptions(layerName));

                    //bt.DowngradeOpen();
                    tr.Commit();
                }
            }
        }

        /// <summary> New AddBlockReference </summary>
        public static ObjectId AddBlockReference(this Database db, string blockName, Point3d insertPoint,
            BlockOptions options)
        {
            ObjectId result; //Database db = App.DocumentManager.MdiActiveDocument.Database;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                //Get the block definition "Check".
                BlockTable bt = (BlockTable)db.BlockTableId.GetObject(OpenMode.ForRead);
                BlockTableRecord blockDef = (BlockTableRecord)bt[blockName].GetObject(OpenMode.ForRead);

                //Also open modelspace - we'll be adding our BlockReference to it
                BlockTableRecord ms = (BlockTableRecord)bt[acadApp.DocumentManager.GetCurrentSpace()]
                    .GetObject(OpenMode.ForWrite);

                //Create new BlockReference, and link it to our block definition
                BlockReference blockRef = new BlockReference(insertPoint, blockDef.ObjectId);

                //Add the block reference to modelspace
                ms.AppendEntity(blockRef);
                tr.AddNewlyCreatedDBObject(blockRef, true);
                result = blockRef.ObjectId;

                if (Math.Abs(options.Scale - 1.0) > 0.01)
                    blockRef.ScaleFactors = new Scale3d(options.Scale);

                if (!String.IsNullOrEmpty(options.LayerName))
                {
                    ILayerService layerService = Plugin.GetService<ILayerService>();
                    if (!layerService.Contains(options.LayerName))
                        layerService.Add(options.LayerName);
                    blockRef.Layer = options.LayerName;
                }

                if (options.Transform != default(Matrix3d))
                    blockRef.TransformBy(options.Transform);

                var attrDict = new Dictionary<AttributeReference, AttributeDefinition>();

                // AttributeDefinitions
                if (blockDef.HasAttributeDefinitions)
                {
                    foreach (ObjectId id in blockDef)
                    {
                        DBObject obj = id.GetObject(OpenMode.ForRead);
                        AttributeDefinition attDef = obj as AttributeDefinition;
                        if ((attDef != null) && !attDef.Constant)
                        {
                            //This is a non-constant AttributeDefinition
                            //Create a new AttributeReference
                            using (AttributeReference attRef = new AttributeReference())
                            {
                                attRef.SetAttributeFromBlock(attDef, blockRef.BlockTransform);
                                //attRef.TextString = "Example";
                                //Add the AttributeReference to the BlockReference
                                blockRef.AttributeCollection.AppendAttribute(attRef);
                                tr.AddNewlyCreatedDBObject(attRef, true);

                                attrDict.Add(attRef, attDef);
                            }
                        }
                    }
                }

                if (options.JigPrompt != eJigPrompt.NoPrompt)
                {
                    if (BlockAttributeJig.CreateJig(blockRef, attrDict, options.JigPrompt))
                        result = blockRef.ObjectId;
                    else
                    {
                        blockRef.Erase(true);
                        blockRef.Dispose();
                        attrDict.ForEach(entry => entry.Value.Dispose());
                        result = ObjectId.Null;
                    }
                }
                tr.Commit();
            }
            return result;
        }

        public static void ImportToDB(string blockname)
        {
            Document doc = acadApp.DocumentManager.MdiActiveDocument;
            using (Database openDb = new Database(false, true))
            {
                ObjectIdCollection ids = new ObjectIdCollection();
                using (Transaction tr = openDb.TransactionManager.StartTransaction())
                {
                    BlockTable bt = (BlockTable)tr.GetObject(openDb.BlockTableId, OpenMode.ForRead);

                    if (bt.Has(blockname))
                        ids.Add(bt[blockname]);
                    tr.Commit();
                }

                //if found, add the block
                if (ids.Count != 0)
                {
                    //get the current drawing database
                    Database destdb = doc.Database;

                    IdMapping iMap = new IdMapping();
                    destdb.WblockCloneObjects(ids, destdb.BlockTableId, iMap, DuplicateRecordCloning.Ignore, false);
                }
            }
        }

        public static void ImportBlockTest(this Database db, string blockName, Point3d insertPoint)
        {
            try
            {
                using (var tr = db.TransactionManager.StartOpenCloseTransaction())
                {
                    string dwgName = HostApplicationServices.Current.FindFile(blockName, db, FindFileHint.Default);
                    Database tempDb = new Database(false, false);
                    tempDb.ReadDwgFile(dwgName, FileShare.Read, true, "");

                    ObjectId BlkId;
                    BlkId = db.Insert(dwgName, tempDb, false);

                    BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead, true);
                    BlockTableRecord btr =
                        (BlockTableRecord)tr
                            .GetObject(bt[acadApp.DocumentManager.GetCurrentSpace()], OpenMode.ForWrite);
                    BlockReference bref = new BlockReference(insertPoint, BlkId);

                    btr.AppendEntity(bref);
                    tr.AddNewlyCreatedDBObject(bref, true);
                    bref.ExplodeToOwnerSpace();
                    bref.Erase();
                    tr.Commit();
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex.InnerException?.Message ?? ex.Message);
            }
        }

        public static void XPurgeBlocks(this Database db)
        {
            Document doc = acadApp.DocumentManager.MdiActiveDocument;
            using (doc.LockDocument())
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                ObjectIdCollection blockIds = new ObjectIdCollection();

                do
                {
                    blockIds.Clear();
                    foreach (ObjectId id in bt)
                    {
                        if (id.IsErased)
                            blockIds.Add(id);
                    }
                    db.Purge(blockIds);

                    foreach (ObjectId id in blockIds)
                    {
                        DBObject obj = tr.GetObject(id, OpenMode.ForRead);
                        obj.UpgradeOpen();
                        obj.Erase();
                        obj.DowngradeOpen();
                    }

                } while (blockIds.Count != 0);
                tr.Commit();
            }
        }

        public static void XClearUnrefedBlocks(this Database db)
        {
            using (var tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                foreach (ObjectId oid in bt)
                {
                    BlockTableRecord btr = tr.GetObject(oid, OpenMode.ForRead) as BlockTableRecord;
                    if (btr != null && btr.GetBlockReferenceIds(true, true).Count == 0 && !btr.IsLayout)
                    {
                        btr.UpgradeOpen();
                        btr.Erase();
                        btr.DowngradeOpen();
                    }
                }
                //if (!scope.ExistTopTransaction)
                tr.Commit();
            }
        }
        #endregion
    }
}
