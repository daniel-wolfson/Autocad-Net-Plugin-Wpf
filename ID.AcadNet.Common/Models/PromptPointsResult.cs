using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

namespace Intellidesk.AcadNet.Common.Models
{
    public class PromptPointsResult
    {
        private List<Point3d> _value = new List<Point3d>();

        public List<Point3d> Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public PromptStatus Status { get; set; } = PromptStatus.None;

        public PromptPointsResult()
        {
            Status = PromptStatus.None;
            _value = new List<Point3d>();
        }
        public PromptPointsResult(PromptStatus status, List<Point3d> points)
        {
            Status = status;
            _value = points;
        }

        public override string ToString()
        {
            object[] objArray = _value.Cast<object>().ToArray();
            return string.Format("({0},{1})", objArray);
        }
    }
}