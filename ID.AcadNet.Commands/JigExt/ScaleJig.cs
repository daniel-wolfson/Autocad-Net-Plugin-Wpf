using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Intellidesk.AcadNet.Commands.Common;
using Intellidesk.AcadNet.Commands.Internal;
using System;

namespace Intellidesk.AcadNet.Commands
{
    internal class ScaleJig : EntityJig
    {
        private Point3d _pos = Point3d.Origin;
        private Vector3d _move;
        private double _mag;
        private readonly Point3d _basePoint;
        private readonly string _message;

        public Entity Ent => Entity;

        public ScaleJig(Entity entity, Point3d basePoint, string message)
            : base(entity)
        {
            _basePoint = basePoint;
            _message = message;
            _mag = 1;
        }

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            var jppo = new JigPromptPointOptions(_message)
            {
                Cursor = CursorType.EntitySelect,
                UseBasePoint = false,
                UserInputControls = UserInputControls.NullResponseAccepted
            };
            jppo.Keywords.Add(""); // mod 20140527
            var corner = prompts.AcquirePoint(jppo).Value;
            var pos = Point3d.Origin + 0.5 * (_basePoint.GetAsVector() + corner.GetAsVector());
            var extents = Entity.GeometricExtents;
            double scale = Math.Min(
                Math.Abs(corner.X - _basePoint.X) / (extents.MaxPoint.X - extents.MinPoint.X),
                Math.Abs(corner.Y - _basePoint.Y) / (extents.MaxPoint.Y - extents.MinPoint.Y));

            // NOTE: the scale is likely small at the beginning. Too small a scale leads to non-proportional scaling for matrix operation, and thus gets rejected by AutoCAD and causes exception.
            if (scale < Consts.Epsilon)
            {
                scale = Consts.Epsilon;
            }
            if (pos.IsNull())
            {
                return SamplerStatus.Cancel;
            }
            else if (pos != _pos)
            {
                _move = pos - _pos;
                _pos = pos;
                _mag = scale;
                return SamplerStatus.OK;
            }

            return SamplerStatus.NoChange;
        }

        protected override bool Update()
        {
            try
            {
                // NOTE: mind the order.
                Entity.TransformBy(Matrix3d.Displacement(_move));
                Entity.TransformBy(Matrix3d.Scaling(_mag, _pos));
            }
            catch
            {
            }
            return true;
        }
    }
}
