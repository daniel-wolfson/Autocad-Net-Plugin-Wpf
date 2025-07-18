using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Intellidesk.AcadNet.Common.Internal;

namespace Intellidesk.AcadNet.Common.Jig
{
    internal class LineJig2 : EntityJig
    {
        private readonly Point3d _startPoint;
        private readonly string _message;

        public Point3d EndPoint { get; private set; }

        public LineJig2(Point3d startPoint, string message)
            : base(new Line(startPoint, Point3d.Origin))
        {
            _startPoint = startPoint;
            _message = message;
        }

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            var jppo = new JigPromptPointOptions(_message)
            {
                Cursor = CursorType.RubberBand,
                BasePoint = _startPoint,
                UseBasePoint = true
            };
            jppo.Keywords.Add(""); // mod 20140527
            var endPoint = prompts.AcquirePoint(jppo).Value;
            if (endPoint.IsNull())
            {
                return SamplerStatus.Cancel;
            }
            else if (endPoint != EndPoint)
            {
                EndPoint = endPoint;
                return SamplerStatus.OK;
            }

            return SamplerStatus.NoChange;
        }

        protected override bool Update()
        {
            try
            {
                (Entity as Line).EndPoint = EndPoint;
            }
            catch
            {
            }
            return true;
        }
    }
}
