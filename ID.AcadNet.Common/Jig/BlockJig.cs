using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

namespace Intellidesk.AcadNet.Common.Jig
{
    public class BlockJig : EntityJig
    {
        protected BlockReference _br;
        protected Point3d _pos;
        protected double _rot, _ucsRot;

        public BlockJig(BlockReference br) : base(br)
        {
            _br = br;
            _pos = _br.Position;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            CoordinateSystem3d ucs = ed.CurrentUserCoordinateSystem.CoordinateSystem3d;
            Matrix3d ocsMat = Matrix3d.WorldToPlane(new Plane(Point3d.Origin, ucs.Zaxis));
            _ucsRot = Vector3d.XAxis.GetAngleTo(ucs.Xaxis.TransformBy(ocsMat), ucs.Zaxis);
            _rot = _br.Rotation - _ucsRot;
        }

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            System.Windows.Forms.Keys mods = System.Windows.Forms.Control.ModifierKeys;
            if ((mods & System.Windows.Forms.Keys.Control) > 0)
            {
                JigPromptAngleOptions jpao =
                    new JigPromptAngleOptions("\nSpecify the rotation: ");
                jpao.UseBasePoint = true;
                jpao.BasePoint = _br.Position;
                jpao.Cursor = CursorType.RubberBand;
                jpao.UserInputControls = (
                    UserInputControls.Accept3dCoordinates |
                    UserInputControls.UseBasePointElevation);
                PromptDoubleResult pdr = prompts.AcquireAngle(jpao);

                if (_rot == pdr.Value)
                {
                    return SamplerStatus.NoChange;
                }
                else
                {
                    _rot = pdr.Value;
                    return SamplerStatus.OK;
                }
            }
            else
            {
                JigPromptPointOptions jppo =
                    new JigPromptPointOptions("\nSpecify insertion point (or press Ctrl for rotation): ");
                jppo.UserInputControls =
                    (UserInputControls.Accept3dCoordinates | UserInputControls.NullResponseAccepted);
                PromptPointResult ppr = prompts.AcquirePoint(jppo);
                if (_pos.DistanceTo(ppr.Value) < Tolerance.Global.EqualPoint)
                {
                    return SamplerStatus.NoChange;
                }
                else
                {
                    _pos = ppr.Value;
                }
                return SamplerStatus.OK;
            }
        }

        protected override bool Update()
        {
            _br.Position = _pos;
            _br.Rotation = _rot + _ucsRot;
            return true;
        }
    }
}