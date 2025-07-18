using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;

namespace Intellidesk.AcadNet.Commands.Jig
{
    public class RectangleJig : DrawJig
    {
        #region Fields

        private Point3d mCorner1;
        private Point3d mCorner2;

        #endregion

        #region Constructors

        public RectangleJig(Point3d basePt)
        {
            mCorner1 = basePt;
        }

        #endregion

        #region Properties

        public Point3d Corner1
        {
            get { return mCorner1; }
            set { mCorner1 = value; }
        }

        public Point3d Corner2
        {
            get { return mCorner2; }
            set { mCorner2 = value; }
        }

        public Matrix3d UCS
        {
            get
            {
                return Application.DocumentManager.MdiActiveDocument.Editor.CurrentUserCoordinateSystem;
            }
        }

        public Point3dCollection Corners
        {
            get
            {
                return new Point3dCollection(
                            new Point3d[]
                            {
                                mCorner1,
                                new Point3d(mCorner1.X, mCorner2.Y, 0),
                                mCorner2,
                                new Point3d(mCorner2.X, mCorner1.Y, 0)
                            }
                        );
            }
        }

        #endregion

        #region Overrides

        protected override bool WorldDraw(Autodesk.AutoCAD.GraphicsInterface.WorldDraw draw)
        {
            WorldGeometry geo = draw.Geometry;
            if (geo != null)
            {
                geo.PushModelTransform(UCS);

                geo.Polygon(Corners);

                geo.PopModelTransform();
            }

            return true;
        }

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            JigPromptPointOptions prOptions2 = new JigPromptPointOptions("\nCorner2:");
            prOptions2.UseBasePoint = false;

            PromptPointResult prResult2 = prompts.AcquirePoint(prOptions2);
            if (prResult2.Status == PromptStatus.Cancel || prResult2.Status == PromptStatus.Error)
                return SamplerStatus.Cancel;

            Point3d tmpPt = prResult2.Value.TransformBy(UCS.Inverse());
            if (!mCorner2.IsEqualTo(tmpPt, new Tolerance(10e-10, 10e-10)))
            {
                mCorner2 = tmpPt;
                return SamplerStatus.OK;
            }
            else
                return SamplerStatus.NoChange;
        }

        #endregion



    }

}
