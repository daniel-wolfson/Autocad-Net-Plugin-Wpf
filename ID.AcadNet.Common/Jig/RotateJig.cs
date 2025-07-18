using System;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

namespace Intellidesk.AcadNet.Common.Jig
{
    public class RotateJig : EntityJig
    {
        // Declare some internal state
        private double _baseAngle;
        private double _deltaAngle;
        private Point3d _rotationPoint;
        private Matrix3d _ucs;
        // Constructor sets the state and clones
        // the entity passed in
        // (adequate for simple entities)

        public RotateJig(Entity ent, Point3d rotationPoint, double baseAngle, Matrix3d ucs)
            : base(ent.Clone() as Entity)
        {
            _rotationPoint = rotationPoint;
            _baseAngle = baseAngle;
            _ucs = ucs;
        }

        protected override SamplerStatus Sampler(JigPrompts jp)
        {
            // We acquire a single angular value
            var jo = new JigPromptAngleOptions("\nAngle of rotation: ") { BasePoint = _rotationPoint, UseBasePoint = true };
            //Dim jo As New JigPromptDistanceOptions(vbLf & "Distance: ")
            var pdr = jp.AcquireAngle(jo);
            //Dim pdr As PromptDoubleResult = jp.AcquireDistance(jo)
            if (pdr.Status == PromptStatus.OK)
            {
                // Check if it has changed or not
                // (reduces flicker)
                if (Math.Abs(_baseAngle - pdr.Value) < 0.001)
                {
                    return SamplerStatus.NoChange;
                }
                // Set the change in angle to
                // the new value
                _deltaAngle = pdr.Value;
                return SamplerStatus.OK;
            }
            return SamplerStatus.Cancel;
        }

        protected override bool Update()
        {
            // Filter out the case where a zero delta is provided
            if (_deltaAngle > Tolerance.Global.EqualPoint)
            {
                // We rotate the polyline by the change
                // minus the base angle
                var trans = Matrix3d.Rotation(_deltaAngle - _baseAngle, _ucs.CoordinateSystem3d.Zaxis, _rotationPoint);
                Entity.TransformBy(trans);
                // The base becomes the previous delta
                // and the delta gets set to zero
                _baseAngle = _deltaAngle;
                _deltaAngle = 0.0;
            }
            return true;
        }

        public Entity GetEntity()
        {
            return Entity;
        }

        public double GetRotation()
        {
            // The overall rotation is the
            // base plus the delta
            return _baseAngle + _deltaAngle;
        }
    }
}