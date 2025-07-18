using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Intellidesk.AcadNet.Common.Extensions
{
    /// <summary> DrawExtensions </summary>
    public static class DrawExtensions
    {
        /// <summary> Polyline extension method </summary>
        public static Polyline XCreateVertices(this Polyline obj, Point3dCollection points)
        {
            for (var i = 0; i <= points.Count - 1; i++)
            {
                obj.AddVertexAt(i, new Point2d(points[i].X, points[i].Y), 0, 0, 0);
            }
            //if (width <= 0.0001) continue;
            //obj.SetStartWidthAt(i, width);
            //obj.SetEndWidthAt(i, width);
            return obj;
        }

        public static Entity XTransform(this Polyline obj, Point3dCollection points,
            double width = 0.0, object linetypeId = null, Point3d? basePointTransform = null,
            double rotation = 0.0, double scaleFactor = 0.0,
            Vector3d? tDisplacement = null)
        {
            for (var i = 0; i < points.Count; i++)
            {
                obj.AddVertexAt(i, new Point2d(points[i].X, points[i].Y), 0, 0, 0);
                if (width <= 0.0001) continue;
                obj.SetStartWidthAt(i, width);
                obj.SetEndWidthAt(i, width);
            }

            if (linetypeId != null)
                obj.LinetypeId = (ObjectId)linetypeId;

            if ((scaleFactor > 0.0 | tDisplacement != null | rotation > 0.0) && basePointTransform == null)
            {
                var p3D = new Polyline3d(Poly3dType.SimplePoly, points, true);
                if (p3D.Bounds.HasValue) basePointTransform = p3D.Bounds.Value.MinPoint;
            }

            if (scaleFactor > 0.0 && basePointTransform != null) obj.XScale(scaleFactor, (Point3d)basePointTransform);

            if (tDisplacement != null) obj.TransformBy(Matrix3d.Displacement((Vector3d)tDisplacement));

            if (rotation > 0.0 && basePointTransform != null) obj.XRotate(rotation, (Point3d)basePointTransform);

            return obj;
        }

        public static Entity XAddVertex(this Polyline obj, Point2d addPoint)
        {
            obj.AddVertexAt(3, addPoint, 0, 0, 0);
            return obj;
        }

        /// <summary> Polyline extension method </summary>
        public static Solid3d XInit(this Solid3d solid3D, IEnumerable<Entity> entities,
            BooleanOperationType booleanOperationType)
        {
            var dbObjects = new DBObjectCollection();
            DBObjectCollection objects = dbObjects;
            entities.ToList().ForEach(x =>
            {
                objects.Add(x);
                x.XErase();
            });

            dbObjects = Region.CreateFromCurves(dbObjects);
            if (dbObjects.Count > 1)
            {
                for (int i = 1; i <= dbObjects.Count - 1; i++)
                {
                    ((Region)dbObjects[0]).BooleanOperation(booleanOperationType, (Region)dbObjects[i]);
                    dbObjects[i].Dispose();
                }
            }
            var acRegion = (Region)dbObjects[0];
            solid3D.Extrude(acRegion, 3, 0);
            return solid3D;
        }
    }
}