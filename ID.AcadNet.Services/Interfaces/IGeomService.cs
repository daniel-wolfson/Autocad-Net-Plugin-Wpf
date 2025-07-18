using Autodesk.AutoCAD.Geometry;

namespace Intellidesk.AcadNet.Services.Interfaces
{
    public interface IGeomService
    {
        /// <summary> Degree to Radian </summary>
        double DegreeToRadian(int angle);

        /// <summary> Radian to Degree </summary>
        double RadianToDegree(double radian);

        void GetIntersect();
        bool IsOrthModeOn();
        Matrix3d UcsToWcs();

        Matrix3d WorldToPlane(Vector3d vec);

        Point3d Point3DUcsToWcs(Point3d ucsPoint);
    }
}