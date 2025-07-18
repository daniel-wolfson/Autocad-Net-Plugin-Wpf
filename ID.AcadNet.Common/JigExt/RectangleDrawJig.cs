using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;

namespace Intellidesk.AcadNet.Common.Jig
{
    public class RectangleDrawJig : DrawJig
    {
        #region Fields

        private Point3d _corner1;
        private Point3d _corner2;
        private string _promptPointMesssage;

        #endregion

        #region Constructors

        public RectangleDrawJig(Point3d basePt, string promptPointText = "\nSecond corner:")
        {
            _corner1 = basePt;
            _promptPointMesssage = promptPointText;
        }

        #endregion

        #region Properties

        public Point3d Corner1
        {
            get { return _corner1; }
            set { _corner1 = value; }
        }

        public Point3d Corner2
        {
            get { return _corner2; }
            set { _corner2 = value; }
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
                                _corner1,
                                new Point3d(_corner1.X, _corner2.Y, 0),
                                _corner2,
                                new Point3d(_corner2.X, _corner1.Y, 0)
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
            JigPromptPointOptions prOptions2 = new JigPromptPointOptions(_promptPointMesssage);
            prOptions2.UseBasePoint = false;

            PromptPointResult prResult2 = prompts.AcquirePoint(prOptions2);
            if (prResult2.Status == PromptStatus.Cancel || prResult2.Status == PromptStatus.Error)
                return SamplerStatus.Cancel;

            Point3d tmpPt = prResult2.Value.TransformBy(UCS.Inverse());
            if (!_corner2.IsEqualTo(tmpPt, new Tolerance(10e-10, 10e-10)))
            {
                _corner2 = tmpPt;
                return SamplerStatus.OK;
            }
            else
                return SamplerStatus.NoChange;
        }

        #endregion



    }

}
