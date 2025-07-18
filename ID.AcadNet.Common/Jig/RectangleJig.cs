using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Intellidesk.AcadNet.Common.Extensions;
using Intellidesk.AcadNet.Common.Extentions;

namespace Intellidesk.AcadNet.Common.Jig
{
    public class RectangleJig : EntityJig
    {
        private readonly Point2d _p1;
        private Point2d _p2;
        private Point2d _p3;
        private Point2d _p4;
        public Point3d Corner1;
        public Point3d Corner2;
        private PromptPointResult _pres;
        private const double tan30 = 0; //Math.Tan((30.0 / 180.0) * Math.PI)

        public Polyline Polyline;
        public List<Point3d> pointsList = new List<Point3d>();
        public Point3d LastPoint { get; private set; }

        public RectangleJig(Point3d corner1) : base(new Polyline(3))
        {
            Corner1 = corner1;
            pointsList.Add(Corner1);

            Polyline = (Polyline)Entity;
            Polyline.SetDatabaseDefaults();

            _p1 = new Point2d(Corner1.X, Corner1.Y);
            Polyline.AddVertexAt(0, _p1, 0, 0, 0);
            Polyline.AddVertexAt(1, _p1, 0, 0, 0);
            Polyline.AddVertexAt(2, _p1, 0, 0, 0);
            Polyline.AddVertexAt(3, _p1, 0, 0, 0);
            Polyline.Closed = true;
        }

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            var jigPointOpts = new JigPromptPointOptions("\nSpecify other corner point");

            jigPointOpts.UseBasePoint = true;
            jigPointOpts.BasePoint = Corner1;
            jigPointOpts.UserInputControls = (UserInputControls.Accept3dCoordinates) | UserInputControls.NullResponseAccepted;
            _pres = prompts.AcquirePoint(jigPointOpts);

            var endPointTemp = _pres.Value;
            if (endPointTemp != Corner2)
            {
                Corner2 = endPointTemp;
            }
            else
            {
                return SamplerStatus.NoChange;
            }

            if (_pres.Status == PromptStatus.Cancel)
            {
                return SamplerStatus.Cancel;
            }

            pointsList.Add(Corner2);
            return SamplerStatus.OK;
        }

        protected override bool Update()
        {
            _p3 = new Point2d(Corner2.X, Corner2.Y);
            double y = tan30 * (Corner2.X - Corner1.X);
            _p2 = new Point2d(Corner2.X, Corner1.Y + y);
            _p4 = new Point2d(Corner1.X, Corner2.Y - y);
            Polyline.SetPointAt(1, _p2);
            Polyline.SetPointAt(2, _p3);
            Polyline.SetPointAt(3, _p4);

            LastPoint = _p4.XGetPoint3d();
            return true;
        }

        public Entity GetEntity()
        {
            return this.Entity;
        }
    }
}