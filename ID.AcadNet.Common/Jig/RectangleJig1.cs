using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

namespace Intellidesk.AcadNet.Common.Jig
{
    public class RectangleJig1 : EntityJig
    {
        private Point2d P1;
        private Point2d P2;
        private Point2d P3;
        private Point2d P4;
        private readonly Point3d _corner1;
        private Point3d _corner2;
        private PromptPointResult _pres;
        public Polyline Pline;
        private double _tan30 = Math.Tan((30.0 / 180.0) * Math.PI);
        private double _degAng2Use1;

        private double _degAng2Use2;

        public RectangleJig1()
            : this(new Point3d())
        {
        }

        public RectangleJig1(Point3d corner1)
            : base(new Polyline(3))
        {
            _corner1 = corner1;
            Pline = (Polyline)Entity;
            Pline.SetDatabaseDefaults();
            P1 = new Point2d(_corner1.X, _corner1.Y);
            Pline.AddVertexAt(0, P1, 0, 0, 0);
            Pline.AddVertexAt(1, P1, 0, 0, 0);
            Pline.AddVertexAt(2, P1, 0, 0, 0);
            Pline.AddVertexAt(3, P1, 0, 0, 0);
            Pline.Closed = true;
        }

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            var jigPointOpts = new JigPromptPointOptions("\nSpecify other corner point")
            {
                UseBasePoint = true,
                BasePoint = _corner1,
                UserInputControls =
                    (UserInputControls.Accept3dCoordinates) |
                    UserInputControls.NullResponseAccepted
            };

            _pres = prompts.AcquirePoint(jigPointOpts);
            var endPointTemp = _pres.Value;

            if (endPointTemp != _corner2)
            {
                _corner2 = endPointTemp;
            }
            else
            {
                return SamplerStatus.NoChange;
            }

            if (_pres.Status == PromptStatus.Cancel)
            {
                return SamplerStatus.Cancel;
            }
            return SamplerStatus.OK;
        }

        private bool IsometricSnapIsOn()
        {
            return Application.GetSystemVariable("Snapstyl").ToString() == "1";
        }

        private String GetIsoPlane()
        {
            var result = "Left";

            switch (Application.GetSystemVariable("Snapisopair").ToString())
            {
                case "1":
                    result = "Top";
                    break;
                case "2":
                    result = "Right";
                    break;
            }
            return result;
        }

        private Point2d PolarPoint(Point2d basepoint, double angle, double distance)
        {
            return new Point2d(basepoint.X + (distance * Math.Cos(angle)), basepoint.Y + (distance * Math.Sin(angle)));
        }

        private Point2d ImaginaryIntersect(Point2d line1Pt1, Point2d line1Pt2, Point2d line2Pt1, Point2d line2Pt2)
        {
            var line1 = new Line2d(line1Pt1, line1Pt2);
            var line2 = new Line2d(line2Pt1, line2Pt2);

            var line1Ang = line1.Direction.Angle;
            var line2Ang = line2.Direction.Angle;

            var line1ConAng = line1Ang + DegreesRadiansConversion(180, false);
            var line2ConAng = line2Ang + DegreesRadiansConversion(180, false);

            var rayLine1Pt1 = PolarPoint(line1Pt1, line1Ang, 10000);
            var rayLine1Pt2 = PolarPoint(line1Pt1, line1ConAng, 10000);
            var rayLine1 = new Line2d(rayLine1Pt1, rayLine1Pt2);

            var rayLine2Pt1 = PolarPoint(line2Pt1, line2Ang, 10000);
            var rayLine2Pt2 = PolarPoint(line2Pt1, line2ConAng, 10000);
            var rayLine2 = new Line2d(rayLine2Pt1, rayLine2Pt2);

            var col = rayLine1.IntersectWith(rayLine2);

            return col[0];

        }

        private Double DegreesRadiansConversion(Double angle, bool inputIsRadians)
        {
            if (inputIsRadians)
            {
                angle = (180 * (angle / Math.PI));
            }
            else
            {
                angle = (Math.PI * (angle / 180));
            }
            return angle;
        }

        protected override bool Update()
        {
            if (!IsometricSnapIsOn())
            {
                _degAng2Use1 = 0;
                _degAng2Use2 = 90;
            }
            else if (GetIsoPlane() == "Right")
            {
                _degAng2Use1 = 30;
                _degAng2Use2 = 90;
            }
            else if (GetIsoPlane() == "Left")
            {
                if (true)
                {
                    _degAng2Use1 = 330;
                    _degAng2Use2 = 90;
                }
            }
            else
            {
                if (true)
                {
                    _degAng2Use1 = 30;
                    _degAng2Use2 = 330;
                }
            }

            var ang2Use1 = DegreesRadiansConversion(_degAng2Use1, false);
            var conAng2Use1 = ang2Use1 + DegreesRadiansConversion(180, false);

            var ang2Use2 = DegreesRadiansConversion(_degAng2Use2, false);
            var conAng2Use2 = ang2Use2 + DegreesRadiansConversion(180, false);

            P3 = new Point2d(_corner2.X, _corner2.Y);
            //double Y = tan30 * (corner2.X - corner1.X);
            P2 = ImaginaryIntersect(P1, PolarPoint(P1, ang2Use1, 1), P3, PolarPoint(P3, ang2Use2, 1));
            P4 = ImaginaryIntersect(P1, PolarPoint(P1, conAng2Use2, 1), P3, PolarPoint(P3, conAng2Use1, 1));
            Pline.SetPointAt(1, P2);
            Pline.SetPointAt(2, P3);
            Pline.SetPointAt(3, P4);

            return true;
        }

    }
}