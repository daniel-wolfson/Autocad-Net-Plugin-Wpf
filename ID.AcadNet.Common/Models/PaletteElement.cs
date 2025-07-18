using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Intellidesk.AcadNet.Common.Interfaces;
using Intellidesk.Data.Models.Entities;

namespace Intellidesk.AcadNet.Common.Models
{
    public class PaletteElement1 : PaletteElement, IAcadElementDefinition
    {
        public Point3d BasePoint { get; set; }

        public TypedValue[] GetTypedValues()
        {
            throw new System.NotImplementedException();
        }
    }
}