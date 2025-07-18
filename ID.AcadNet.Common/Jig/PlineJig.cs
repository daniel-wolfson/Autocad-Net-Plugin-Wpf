using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

namespace Intellidesk.AcadNet.Common.Jig
{
    public class PlineJig : EntityJig
    {
        // Maintain a list of vertices...
        // Not strictly necessary, as these will be stored in the
        // polyline, but will not adversely impact performance
        private Point3dCollection _pts;
        // Use a separate variable for the most recent point...
        // Again, not strictly necessary, but easier to reference

        public List<Point3d> AllVertexes = new List<Point3d>();
        public Point3d LastPoint { get; private set; }
        public Matrix3d Ucs { get; private set; }

        private Plane _plane;
        public PlineJig(Matrix3d ucs) : base(new Polyline())
        {
            Ucs = ucs;
            // Create a point collection to store our vertices
            _pts = new Point3dCollection();
            // Create a temporary plane, to help with calcs
            var origin = new Point3d(0, 0, 0);
            var normal = new Vector3d(0, 0, 1);
            normal = normal.TransformBy(ucs);

            _plane = new Plane(origin, normal);
            
            // Create polyline, set defaults, add dummy vertex
            var pline = (Polyline)Entity;
            pline.SetDatabaseDefaults();
            pline.Normal = normal;
            pline.AddVertexAt(0, new Point2d(0, 0), 0, 0, 0);
            AllVertexes.Add(new Point3d(0, 0, 0));
        }

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            var jigOpts = new JigPromptPointOptions
            {
                UserInputControls =
                    (UserInputControls.Accept3dCoordinates | UserInputControls.NullResponseAccepted |
                     UserInputControls.NoNegativeResponseAccepted)
            };
            if (_pts.Count == 0)
            {
                // For the first vertex, just ask for the point
                jigOpts.Message = "\nStart point of polyline: ";
            }
            else if (_pts.Count > 0)
            {
                // For subsequent vertices, use a base point
                jigOpts.BasePoint = _pts[_pts.Count - 1];
                jigOpts.UseBasePoint = true;
                jigOpts.Message = "\nPolyline vertex: ";
            }
            else
            {
                // should never happen
                return SamplerStatus.Cancel;
            }
            // Get the point itself
            var res = prompts.AcquirePoint(jigOpts);
            // Check if it has changed or not
            // (reduces flicker)
            if (LastPoint == res.Value)
            {
                return SamplerStatus.NoChange;
            }
            if (res.Status == PromptStatus.OK)
            {
                LastPoint = res.Value;
                AllVertexes.Add(LastPoint);
                return SamplerStatus.OK;
            }
            return SamplerStatus.Cancel;
        }

        protected override bool Update()
        {
            // Update the dummy vertex to be our
            // 3D point projected onto our plane
            var pline = (Polyline)Entity;
            pline.SetPointAt(pline.NumberOfVertices - 1, LastPoint.Convert2d(_plane));
            return true;
        }

        public Entity GetEntity()
        {
            return Entity;
        }

        public void AddLatestVertex()
        {
            // Add the latest selected point to our internal list...
            // This point will already be in the most recently added pline vertex
            _pts.Add(LastPoint);
            var pline = (Polyline)Entity;
            // Create a new dummy vertex... can have any initial value
            pline.AddVertexAt(pline.NumberOfVertices, new Point2d(0, 0), 0, 0, 0);
        }

        public void RemoveLastVertex()
        {
            // Let's remove our dummy vertex
            var pline = (Polyline)Entity;
            pline.RemoveVertexAt(_pts.Count);
        }
    }
}