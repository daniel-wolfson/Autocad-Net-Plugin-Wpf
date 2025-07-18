using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Intellidesk.AcadNet.Common.Extensions;
using Intellidesk.AcadNet.Common.Internal;
using Intellidesk.Data.Models.Entities;
using Serilog;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Intellidesk.AcadNet.Common.Jig
{
    public class ElementJig : EntityJig
    {
        #region Fields
        private readonly Circle _entity;
        private readonly PaletteElement _element;
        private DBText _text = null;
        private int _curJigFactorNumber = 1;
        private Point3d _center = Point3d.Origin;    // Factor #1
        private double _radius = 0.0001;            // Factor #2 : It is critical to set it as none zero to avoid the degeneration problem.
        private readonly string _title = "";
        private double _titleHeight = 0.85;
        private string _promptHeader = "";
        #endregion

        #region ctor
        public ElementJig(PaletteElement element, Entity ent) : base(ent)
        {
            _element = element;
            _title = element.Title;
            _promptHeader = element.GetType().Name.Replace("Acad", "");

            _entity = ent as Circle;
            if (_entity != null)
            {
                _entity.Center = _center;
                _entity.Radius = _radius;
            }
        }
        #endregion

        protected override bool Update()
        {
            switch (_curJigFactorNumber)
            {
                case 1:
                    (Entity as Circle).Center = _center;
                    break;
                case 2:
                    (Entity as Circle).Radius = _radius;
                    break;
                default:
                    return false;
            }
            return true;
        }

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            switch (_curJigFactorNumber)
            {
                case 1:
                    JigPromptPointOptions prOptions1 = new JigPromptPointOptions($"\n{_promptHeader} center:");
                    PromptPointResult prResult1 = prompts.AcquirePoint(prOptions1);
                    if (prResult1.Status == PromptStatus.Cancel) return SamplerStatus.Cancel;
                    if (prResult1.Value.Equals(_center))
                    {
                        return SamplerStatus.NoChange;
                    }
                    _center = prResult1.Value;
                    return SamplerStatus.OK;

                case 2:
                    if (_entity.Radius > 0) break;

                    JigPromptDistanceOptions prOptions2 =
                        new JigPromptDistanceOptions($"\n{_promptHeader} radius:") { BasePoint = _center };

                    PromptDoubleResult prResult2 = prompts.AcquireDistance(prOptions2);
                    if (prResult2.Status == PromptStatus.Cancel) return SamplerStatus.Cancel;
                    if (prResult2.Value.Equals(_radius))
                        return SamplerStatus.NoChange;

                    if (prResult2.Value < 0.0001)  // To avoid the degeneration problem!
                        return SamplerStatus.NoChange;

                    _radius = prResult2.Value;
                    return SamplerStatus.OK;

                case 3:
                    if (string.IsNullOrEmpty(_title)) break;

                    _text = new DBText
                    {
                        Position = new Point3d(_center.X, _center.Y, 0),
                        Height = _titleHeight < 0.8 ? 0.8 : _titleHeight,
                        TextString = _title
                    };
                    _text.SetDatabaseDefaults();

                    return SamplerStatus.OK;
            }
            return SamplerStatus.OK;
        }

        public static bool Jig(PaletteElement element)
        {
            try
            {
                Document doc = acadApp.DocumentManager.MdiActiveDocument;
                Editor ed = doc.Editor;
                Database db = doc.Database;

                var ent = new Circle
                {
                    LinetypeScale = 1,
                    Radius = 3.5,
                    Color = Colors.GetColorFromIndex((short)element.ColorIndex)
                };
                ent.SetDatabaseDefaults();

                ElementJig jigger = new ElementJig(element, ent);

                PromptResult pr;
                do
                {
                    pr = ed.Drag(jigger);
                    jigger._curJigFactorNumber++;
                } while (pr.Status != PromptStatus.Cancel && jigger._curJigFactorNumber <= 3);

                if (pr.Status != PromptStatus.Cancel)
                {
                    using (Transaction tr = db.TransactionManager.StartTransaction())
                    {
                        BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                        BlockTableRecord btr = (BlockTableRecord)tr.GetObject(
                            bt[acadApp.DocumentManager.GetCurrentSpace()], OpenMode.ForWrite);

                        btr.AppendEntity(jigger.Entity);
                        tr.AddNewlyCreatedDBObject(jigger.Entity, true);
                        jigger._element.ObjectId = jigger.Entity.ObjectId;

                        btr.AppendEntity(jigger._text);
                        tr.AddNewlyCreatedDBObject(jigger._text, true);

                        tr.Commit();
                    }
                }
                else
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex.InnerException?.Message ?? ex.Message);
                return false;
            }
        }
    }
}