using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;

namespace Intellidesk.AcadNet.Services.Core
{
    /// <summary> Test </summary>
    public class ObjectIdDistinctComparer : IEqualityComparer<ObjectId>
    {
        public int Compare(ObjectId aX, ObjectId bX)
        {
            if (aX.Equals(bX)) return 0; // ==
            if (aX < bX) return -1; // <
            return 1; // >
        }

        public bool Equals(ObjectId x, ObjectId y)
        {
            return (x.Equals(y));
        }

        public int GetHashCode(ObjectId obj)
        {
            throw new NotImplementedException();
        }
    }
}
