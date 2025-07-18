using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Intellidesk.AcadNet.Common.Interfaces
{
    public interface IDrawService
    {
        string CurrentLayerName { get; set; }

        /// <summary> AttachRasterImage </summary>
        void AttachRasterImage1(Database db, Point3d pnt);

        /// <summary> AttachRasterImage </summary>
        void AttachRasterImage(Database db, Point3d pnt);

        /// <summary> Set curent layer </summary>
        string SetLayer(string layerName);
    }
}