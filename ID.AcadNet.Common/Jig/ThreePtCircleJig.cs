using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

namespace Intellidesk.AcadNet.Common.Jig
{
    public class ThreePtCircleJig : EntityJig
    {
        private Point3d _first;
        private Point3d _second;
        private Point3d _third;
        
        public ThreePtCircleJig(Point3d first, Point3d second)
            : base(new Polyline(2))
        {
            _first = first;
            _second = second;
        }
        protected override SamplerStatus Sampler(JigPrompts jp)
        {
            // We acquire a single 3D point
            var jo = new JigPromptPointOptions("\nSelect third point") { UserInputControls = UserInputControls.Accept3dCoordinates };
            var ppr = jp.AcquirePoint(jo);
            if (ppr.Status == PromptStatus.OK)
            {
                // Check whether it's basically unchanged
                if (_third.DistanceTo(ppr.Value) < Tolerance.Global.EqualPoint)
                {
                    return SamplerStatus.NoChange;
                }
                // Otherwise just set the jig's state
                _third = ppr.Value;
                return SamplerStatus.OK;
            }
            return SamplerStatus.Cancel;
        }
        protected override bool Update()
        {
            // Create a temporary CircularArc3d by three points
            // and use it to create our Circle
            var ca = new CircularArc3d(_first, _second, _third);
            var cir = (Circle)Entity;
            cir.Center = ca.Center;
            cir.Normal = ca.Normal;
            cir.Radius = ca.Radius;
            return true;
        }
        public Entity GetEntity()
        {
            return Entity;
        }
    }
}