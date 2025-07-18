using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
namespace Intellidesk.AcadNet.Common.Jig
{
    public class PolylineDrawJig : DrawJig
    {
        public Point3dCollection AllVertexes { get; } = new Point3dCollection();
        public Point3d LastPoint { get; private set; }
        public Document Doc => acadApp.DocumentManager.MdiActiveDocument;
        public Matrix3d Ucs => Doc.Editor.CurrentUserCoordinateSystem;

        protected override bool WorldDraw(WorldDraw draw)
        {
            WorldGeometry geo = draw.Geometry;
            if (geo != null)
            {
                geo.PushModelTransform(Ucs);
                Point3dCollection tempPts = new Point3dCollection();
                foreach (Point3d pt in AllVertexes)
                {
                    tempPts.Add(pt);
                }
                tempPts.Add(LastPoint);

                if (tempPts.Count > 0)
                    geo.Polyline(tempPts, Vector3d.ZAxis, IntPtr.Zero);

                geo.PopModelTransform();
            }
            return true;
        }

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            JigPromptPointOptions prOptions = new JigPromptPointOptions("\nVertex(Enter to finish)")
            {
                UseBasePoint = false,
                UserInputControls = UserInputControls.NullResponseAccepted | UserInputControls.Accept3dCoordinates |
                                    UserInputControls.GovernedByUCSDetect | UserInputControls.GovernedByOrthoMode |
                                    UserInputControls.AcceptMouseUpAsPoint
            };

            PromptPointResult ppr = prompts.AcquirePoint(prOptions);

            if (ppr.Status == PromptStatus.Cancel || ppr.Status == PromptStatus.Error)
                return SamplerStatus.Cancel;

            Point3d tempPt = ppr.Value.TransformBy(Ucs.Inverse());
            LastPoint = tempPt;

            return SamplerStatus.OK;
        }
    }
}