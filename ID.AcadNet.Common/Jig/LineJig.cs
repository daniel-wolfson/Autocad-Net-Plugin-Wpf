using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

namespace Intellidesk.AcadNet.Common.Jig
{
    public class LineJig : EntityJig
    {
        private Point3dCollection _pts;
        // Use a separate variable for the most recent point...
        // Again, not strictly necessary, but easier to reference
        private Point3d _tempPoint;

        private Plane _plane;

        public LineJig(Matrix3d ucs, Point3d? pt1 = null, Point3d? pt2 = null)
            : base(new Line())
        {
            // Create a point collection to store our vertices
            _pts = new Point3dCollection();
            if (pt1 != null)
                _pts.Add((Point3d)pt1);
            if (pt2 != null)
                _pts.Add((Point3d)pt2);
            // Create a temporary plane, to help with calcs
            var origin = new Point3d(0, 0, 0);
            var normal = new Vector3d(0, 0, 1);
            normal = normal.TransformBy(ucs);
            _plane = new Plane(origin, normal);
            // Create line, set defaults, add dummy vertex
            var line = (Line)Entity;
            line.SetDatabaseDefaults();
            line.Normal = normal;
            if (pt1 != null)
                line.StartPoint = (Point3d)pt1;
            //line.EndPoint = pt2
            //MsgBox("1:" + line.Angle.ToString)
        }

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            var jigOpts = new JigPromptPointOptions
            {
                UserInputControls =
                    (UserInputControls.Accept3dCoordinates |
                     UserInputControls.NullResponseAccepted |
                     UserInputControls.NoNegativeResponseAccepted)
            };

            switch (_pts.Count)
            {
                case 0:
                    jigOpts.Message = "\nStart point of Construction: ";
                    break;
                case 1:
                    jigOpts.BasePoint = _pts[_pts.Count - 1];
                    jigOpts.UseBasePoint = true;
                    jigOpts.Message = "\nSecond point of Construction: ";
                    break;
                default:
                    return (SamplerStatus.Cancel);
            }

            // Get the point itself
            var res = prompts.AcquirePoint(jigOpts);
            // Check if it has changed or not
            // (reduces flicker)

            if (_tempPoint == res.Value)
            {
                return SamplerStatus.NoChange;
            }

            if (res.Status == PromptStatus.OK)
            {
                _tempPoint = res.Value;
                //If m_pts.Count < 1 Then
                return SamplerStatus.OK;
                //End If
            }

            return SamplerStatus.Cancel;
        }

        protected override bool Update()
        {
            var line = (Line)Entity;
            if (_pts.Count == 1)
                line.StartPoint = _pts[0];
            if (_pts.Count == 2)
                line.EndPoint = new Point3d(_pts[0].X, _tempPoint.Convert2d(_plane).X, 0);
            return true;
        }

        public double GetAngle()
        {
            _pts.Add(_tempPoint);
            var line = (Line)Entity;
            line.StartPoint = _pts[0];
            line.EndPoint = _pts[1];
            Application.ShowAlertDialog("GetAngle:" + Convert.ToString(line.Angle));
            return line.Angle;
        }
    }
}