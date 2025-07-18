using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Intellidesk.AcadNet.Common.Internal;
using System;

namespace Intellidesk.AcadNet.Common.Jig
{
    internal class RotationJig : EntityJig
    {
        private Point3d _end;
        private double _angle;
        private readonly Point3d _center;
        private readonly string _message;

        public Entity Ent => Entity;

        public RotationJig(Entity entity, Point3d center, string message)
            : base(entity)
        {
            _center = center;
            _message = message;
        }

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            var jppo = new JigPromptPointOptions(_message)
            {
                Cursor = CursorType.EntitySelect,
                BasePoint = _center,
                UseBasePoint = true,
                UserInputControls = UserInputControls.NullResponseAccepted
            };
            jppo.Keywords.Add(""); // mod 20140527
            var end = prompts.AcquirePoint(jppo).Value;
            if (end.IsNull())
            {
                return SamplerStatus.Cancel;
            }
            else if (end != _end)
            {
                _end = end;
                return SamplerStatus.OK;
            }

            return SamplerStatus.NoChange;
        }

        protected override bool Update()
        {
            try
            {
                var dir = _end - _center;
                double angle = dir.GetAngleTo(Vector3d.YAxis);
                if (dir.X > 0)
                {
                    angle = Math.PI * 2 - angle;
                }
                Entity.TransformBy(Matrix3d.Rotation(angle - _angle, Vector3d.ZAxis, _center));
                _angle = angle;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
