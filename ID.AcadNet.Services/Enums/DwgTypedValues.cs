using System.ComponentModel;
using Autodesk.AutoCAD.Geometry;

namespace Intellidesk.AcadNet.Services.Enums
{
    public enum CoordSystems
    {
        [Description("Israeli Transverse Mercator")]
        ITM,
        [Description("Israeli Cassini-Soldner")]
        ICS,
        [Description("Universal Transverse Mercator")]
        UTM
    }

    public class ProjectInfo
    {
        public string CoordSystem { get; set; }
        public int Percentage { get; set; }
        public Point2d LastXMin { get; set; }
        public Point2d LastYMin { get; set; }
    }
}
