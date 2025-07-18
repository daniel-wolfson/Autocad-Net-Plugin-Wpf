using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Intellidesk.AcadNet.Common.Internal;

namespace Intellidesk.AcadNet.Common.Jig
{
    internal class PositionJig : EntityJig
    {
        private Point3d _pos = Point3d.Origin;
        private Vector3d _move;
        private readonly string _message;

        public Entity Ent => Entity;

        public PositionJig(Entity entity, string message)
            : base(entity)
        {
            _message = message;
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
            var pos = prompts.AcquirePoint(jppo).Value;
            if (pos.IsNull())
            {
                return SamplerStatus.Cancel;
            }
            else if (pos != _pos)
            {
                _move = pos - _pos;
                _pos = pos;
                return SamplerStatus.OK;
            }

            return SamplerStatus.NoChange;
        }

        protected override bool Update()
        {
            try
            {
                Entity.TransformBy(Matrix3d.Displacement(_move));
            }
            catch
            {
            }
            return true;
        }
    }
}
