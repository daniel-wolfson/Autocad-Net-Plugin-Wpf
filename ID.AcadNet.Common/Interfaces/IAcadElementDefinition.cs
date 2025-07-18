using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Intellidesk.Data.Models.Entities;

namespace Intellidesk.AcadNet.Common.Interfaces
{
    public interface IAcadElementDefinition : IPaletteElement
    {
        Point3d BasePoint { get; set; }
        TypedValue[] GetTypedValues();
    }
}