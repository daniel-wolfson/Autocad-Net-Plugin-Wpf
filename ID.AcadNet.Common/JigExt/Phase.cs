using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;

namespace Intellidesk.AcadNet.Common.Jig
{
    public abstract class Phase

    {
        // Our member data (could add real properties, as needed)

        public string Message;
        public object Value;

        public Phase(string msg)
        {
            Message = msg;
        }
    }

    // And another for geometric classes...
    public abstract class GeometryPhase : Phase
    {
        // Geometric classes can also have an offset for the basepoint
        public Func<List<Phase>, Point3d, Vector3d> Offset;

        public GeometryPhase(string msg, Func<List<Phase>, Point3d, Vector3d> offset = null
        ) : base(msg)
        {
            Offset = offset;
        }
    }

    // A phase for distance input
    public class DistancePhase : GeometryPhase
    {
        public DistancePhase(
          string msg,
          object defval = null,
          Func<List<Phase>, Point3d, Vector3d> offset = null
        ) : base(msg, offset)
        {
            Value = (defval == null ? 0 : defval);
        }
    }

    // A phase for distance input related to Autodesk Shape Manager
    // (whose internal tolerance if 1e-06, so we need a larger
    // default value to allow Solid3d creation to succeed)
    public class SolidDistancePhase : DistancePhase
    {
        public SolidDistancePhase(
          string msg,
          object defval = null,
          Func<List<Phase>, Point3d, Vector3d> offset = null
        ) : base(msg, defval, offset)
        {
            Value = (defval == null ? 1e-05 : defval);
        }
    }

    // A phase for point input
    public class PointPhase : GeometryPhase
    {
        public PointPhase(
          string msg,
          object defval = null,
          Func<List<Phase>, Point3d, Vector3d> offset = null
        ) : base(msg, offset)
        {
            Value =
              (defval == null ? Point3d.Origin : defval);
        }
    }

    // A phase for angle input
    public class AnglePhase : GeometryPhase
    {
        public AnglePhase(
          string msg,
          object defval = null,
          Func<List<Phase>, Point3d, Vector3d> offset = null
        ) : base(msg, offset)
        {
            Value = (defval == null ? 0 : defval);
        }
    }

    // And a non-geometric phase for string input
    public class StringPhase : Phase
    {
        public StringPhase(
          string msg,
          string defval = null
        ) : base(msg)
        {
            Value = (defval == null ? "" : defval);
        }
    }
}
