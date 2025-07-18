//extern alias Newtonsoft10;
//extern alias Newtonsoft6;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Intellidesk.AcadNet.Common.Extensions;
using System.Collections.Generic;

namespace Intellidesk.AcadNet.ViewModels
{
    public class Bay
    {
        public string Name { get; set; }
        public ObjectId ObjectId { get; set; }
        public Dictionary<string, string> Attrs { get; set; }
        public Point3dCollection Vertices { get; set; }
        public Extents2d Extends => Vertices.XGetPointsExtents2d();

        public Bay(string name, ObjectId objectId, Dictionary<string, string> attrs, Point3dCollection vertices)
        {
            Name = name;
            ObjectId = objectId;
            Attrs = attrs;
            Vertices = vertices;
        }
    }
}

//_machineNames = machineMatchesLayers.Select(x => x.Substring(0, x.IndexOf(x.Split('-')[3])-1)).Distinct().ToList();
//_machineScopes = machineMatchesLayers.Select(x => x.Split('-')[3]).Distinct().ToList();
//_machineRamps = machineMatchesLayers.Select(x => x.Split('-')[5]).Distinct()
//    .ToDictionary(k => k, v =>
//    {
//        int destValue;
//        bool success = int.TryParse(v, out destValue);
//        return success ? destValue : 0;
//    })
//    .OrderBy(item => item.Value)
//    .Select(k => k.Key).ToList();