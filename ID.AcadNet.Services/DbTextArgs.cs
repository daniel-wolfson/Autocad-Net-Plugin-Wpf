using Autodesk.AutoCAD.DatabaseServices;
using Intellidesk.AcadNet.Common.Enums;
using Intellidesk.Data.Models.EntityMetaData;

namespace Intellidesk.AcadNet.Services
{
    public class DbTextArgs : DBText
    {
        public AlignmentOptions Alignment = AlignmentOptions.ByCenterHorizontalMode;
        public Point3D otherPointRotate = null;
    }
}