using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;

namespace Intellidesk.AcadNet.Common.Models
{
    public class ObjectIdItemArgs : ObjectIdItem
    {
        public ObjectIdItemArgs()
        {
            ObjectId = ObjectId.Null;
            DisplayName = "none";
        }

        public ObjectIdItemArgs(ObjectId objectId, string displayName) : base(objectId, displayName)
        {
            ObjectId = objectId;
            DisplayName = displayName;
        }

        public Point3d BasePoint { get; set; }
        public Dictionary<string, string> Attrs { get; set; }
    }
}
