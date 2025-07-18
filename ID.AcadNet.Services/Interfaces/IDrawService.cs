using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Intellidesk.AcadNet.Services.Interfaces
{
    public interface IDrawService
    {
        Entity Circle(Transaction tr, double tPntX, double tPntY, double tRadius, short tColor = Colors.ByLayer);
        Entity Donut(Transaction tr, double x, double y, double rd, short color = -1);

        Entity Line(Transaction tr, double tPntX, double tPntY, double tPntX2, double tPntY2, int tTypeLine = 0,
            short tColor = Colors.ByLayer, double tRotation = 0, Point2d? tPntRotate = null);

        Entity Polyline(Transaction tr, Extents3d tExtents3D, string layer = "0", int colorIndex = Colors.ByLayer,
            double width = 0.0, byte close = 1, Point3d? basePointTransform = null, double tRotation = 0.0, double scaleFactor = 0.0, Vector3d? displacement = null);

        Entity Polyline(Transaction tr, Point3d tPointStart, Point3d tPointEnd, string layer = "0", int colorIndex = Colors.ByLayer,
            double tWidth = 0.0, byte close = 1, Point3d? basePointTransform = null, double rotation = 0.0, double scaleFactor = 0.0, Vector3d? displacement = null);

        Entity Polyline(Transaction tr, Point3dCollection points, string layer = "0", int color = Colors.ByLayer,
            double width = 0.0, byte close = 1, Point3d? basePointTransform = null,
            double rotation = 0.0, double scaleFactor = 0.0,
            Vector3d? tDisplacement = null);

        Entity Pline(Transaction tr, double[] x, double[] y, short tColor = Colors.ByLayer, double tWidth = 0.0,
            byte tClose = 0, double tPntXRotate = Double.MinValue, double tPntYRotate = Double.MinValue, double tRotation = 0.0);

        object Solid(Transaction tr, double[] x, double[] y, short tColor = Colors.ByLayer);

        Entity Point(Transaction tr, double tPntX, double tPntY, short tColor = Colors.ByLayer,
            int tFmod = 0, int tPntMod = 34, double tPntSize = 5.0);

        Entity Text(Transaction tr, double tPntX, double tPntY, string tStr, OptionsAlignment tFlgAlignment = OptionsAlignment.ByCenterHorizontalMode,
            short tColor = Colors.ByLayer, double tRotation = 0.0,
            double? tPntXRotate = null, double? tPntYRotate = null,
            double tHeight = 8.0);

        Entity RecHat(Transaction tr, double[] x, double[] y, short tColor = Colors.ByLayer);
        Entity Solid3D(Transaction tr, object contourColl, double height, int colorIndex);
        Entity Solid3D(Transaction tr, double[] x, double[] y, double[] z, double height, int colorIndex);
        Entity Solid3D(Transaction tr, IEnumerable<Entity> entities, BooleanOperationType booleanOperationType, string layer = "0", int colorIndex = Colors.ByLayer);
    }
}
