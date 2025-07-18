using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using ID.Infrastructure;
using Intellidesk.AcadNet.Common.Enums;
using Intellidesk.AcadNet.Common.Extensions;
using Intellidesk.AcadNet.Common.Interfaces;
using Serilog;
using System.Collections.Generic;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Exception = Autodesk.AutoCAD.Runtime.Exception;

namespace Intellidesk.AcadNet.Common.Jig
{
    public class BlockAttributeJig : EntityJig
    {
        #region Fields

        private int _curJigFactor = 1;
        private double _angleOffset = 0.0;
        private Point3d _position = new Point3d(0, 0, 0); // Factor #1
        private double _rotation = 0.0;                  // Factor #2
        private double _scaleFactor = 1.0;              // Factor #3
        private Dictionary<AttributeReference, AttributeDefinition> _ref2DefMap;

        #endregion

        #region Constructors

        public BlockAttributeJig(BlockReference ent, Dictionary<AttributeReference, AttributeDefinition> dict)
            : base(ent)
        {
            _angleOffset = ent.Rotation;
            _ref2DefMap = dict;
        }

        #endregion

        #region Properties

        protected static Matrix3d UCS
        {
            get { return Application.DocumentManager.MdiActiveDocument.Editor.CurrentUserCoordinateSystem; }
        }

        protected new BlockReference Entity
        {
            get { return (BlockReference)base.Entity; }
        }

        #endregion

        #region Overrides

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            var commandLine = Plugin.GetService<ICommandLine>();
            switch (_curJigFactor)
            {
                case 1:
                    JigPromptPointOptions prOptions1 =
                        new JigPromptPointOptions(commandLine.Current() + "Block insertion point | Esc:");
                    PromptPointResult prResult1 = prompts.AcquirePoint(prOptions1);
                    if (prResult1.Status == PromptStatus.Cancel)
                        return SamplerStatus.Cancel;

                    Point3d tempPt = prResult1.Value.TransformBy(UCS.Inverse());
                    if (tempPt.IsEqualTo(_position))
                    {
                        return SamplerStatus.NoChange;
                    }
                    _position = tempPt;
                    return SamplerStatus.OK;

                case 2:
                    JigPromptAngleOptions prOptions2 = new JigPromptAngleOptions(commandLine.Current() + "Block rotation angle | Esc:");
                    prOptions2.BasePoint = _position.TransformBy(UCS);
                    prOptions2.UseBasePoint = true;
                    PromptDoubleResult prResult2 = prompts.AcquireAngle(prOptions2);
                    if (prResult2.Status == PromptStatus.Cancel)
                        return SamplerStatus.Cancel;

                    if (prResult2.Value.Equals(_rotation))
                    {
                        return SamplerStatus.NoChange;
                    }
                    _rotation = prResult2.Value;
                    return SamplerStatus.OK;

                case 3:
                    JigPromptDistanceOptions prOptions3 = new JigPromptDistanceOptions(commandLine.Current() + "Block scale factor:");
                    prOptions3.BasePoint = _position.TransformBy(UCS);
                    prOptions3.UseBasePoint = true;
                    PromptDoubleResult prResult3 = prompts.AcquireDistance(prOptions3);
                    if (prResult3.Status == PromptStatus.Cancel)
                        return SamplerStatus.Cancel;

                    if (prResult3.Value.Equals(_scaleFactor))
                    {
                        return SamplerStatus.NoChange;
                    }
                    _scaleFactor = prResult3.Value;
                    return SamplerStatus.OK;

                default:
                    break;
            }

            return SamplerStatus.OK;
        }

        protected override bool Update()
        {
            switch (_curJigFactor)
            {
                case 1:
                    Entity.Position = _position.TransformBy(UCS);
                    break;
                case 2:
                    Entity.Rotation = _rotation + _angleOffset; ;
                    break;
                case 3:
                    Entity.ScaleFactors = new Scale3d(_scaleFactor);
                    break;
                default:
                    break;
            }

            try
            {
                //foreach (KeyValuePair<AttributeReference, AttributeDefinition> ar2ad in _ref2DefMap)
                //{
                //    string value = ar2ad.Key.TextString;
                //    ar2ad.Key.SetAttributeFromBlock(ar2ad.Value, Entity.BlockTransform);
                //    ar2ad.Key.TextString = value;
                //    ar2ad.Key.XData = ar2ad.Value.XData;
                //    ar2ad.Key.AdjustAlignment(Entity.Database);
                //}
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex.InnerException?.Message ?? ex.Message);
            }

            return true;
        }
        #endregion

        public static bool CreateJig(BlockReference ent, Dictionary<AttributeReference, AttributeDefinition> dict, eJigPrompt jigPrompt = eJigPrompt.PromptInsertRotateScale)
        {
            try
            {
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
                BlockAttributeJig jigger = new BlockAttributeJig(ent, dict);
                PromptResult pr;
                do
                {
                    pr = ed.Drag(jigger);
                } while (pr.Status != PromptStatus.Cancel && pr.Status != PromptStatus.Error && ++jigger._curJigFactor <= (int)jigPrompt);

                return pr.Status == PromptStatus.OK;
            }
            catch
            {
                return false;
            }
        }

        public static string CollectAttributeText(AttributeDefinition attDef)
        {
            string ret = string.Empty;

            PromptStringOptions prStrOpt = new PromptStringOptions("");
            prStrOpt.AllowSpaces = true;
            prStrOpt.DefaultValue = attDef.TextString;
            prStrOpt.UseDefaultValue = true;
            prStrOpt.Message = attDef.Prompt;
            PromptResult pr = Application.DocumentManager.MdiActiveDocument.Editor.GetString(prStrOpt);
            if (pr.Status == PromptStatus.OK)
            {
                ret = pr.StringResult;
            }

            return ret;
        }

        //[CommandMethod("BlockAttributeJig")]
        public static Point3d InsertBlockJig(string blockName, Point3d? insertPoint = null, eJigPrompt jigPrompt = eJigPrompt.PromptInsertRotateScale)
        {
            Point3d pointResult = Point3d.Origin;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            Database db = HostApplicationServices.WorkingDatabase;
            Document doc = Application.DocumentManager.GetDocument(db);

            try
            {
                using (doc.LockDocument())
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                    if (!bt.Has(blockName))
                        db.ImportBlock(blockName);

                    BlockTableRecord btr = tr.GetObject(bt[blockName], OpenMode.ForRead) as BlockTableRecord;

                    if (btr != null)
                    {
                        BlockReference ent = new BlockReference(insertPoint ?? Point3d.Origin, btr.ObjectId);
                        ent.TransformBy(UCS);
                        //ent.Transparency = new Transparency(30);
                        BlockTableRecord modelspace = (BlockTableRecord)tr.GetObject(
                            bt[Application.DocumentManager.GetCurrentSpace()], OpenMode.ForWrite);
                        modelspace.AppendEntity(ent);
                        tr.AddNewlyCreatedDBObject(ent, true);

                        if (btr.Annotative == AnnotativeStates.True)
                        {
                            ObjectContextManager ocm = db.ObjectContextManager;
                            ObjectContextCollection occ = ocm.GetContextCollection("ACDB_ANNOTATIONSCALES");
                            ent.AddContext(occ.CurrentContext);
                        }

                        Dictionary<AttributeReference, AttributeDefinition> dict = new Dictionary<AttributeReference, AttributeDefinition>();
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
                                    ar.SetAttributeFromBlock(ad, ent.BlockTransform);
                                    ar.TextString = CollectAttributeText(ad);

                                    ent.AttributeCollection.AppendAttribute(ar);
                                    tr.AddNewlyCreatedDBObject(ar, true);

                                    dict.Add(ar, ad);
                                }
                            }
                        }

                        if (CreateJig(ent, dict))
                        {
                            tr.Commit();
                            pointResult = ent.Position;
                        }
                        else
                        {
                            ent.Dispose();
                            foreach (KeyValuePair<AttributeReference, AttributeDefinition> entry in dict)
                            {
                                entry.Value.Dispose();
                            }
                            tr.Abort();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ed.WriteMessage(ex.Message);
            }
            return pointResult;
        }
    }
}