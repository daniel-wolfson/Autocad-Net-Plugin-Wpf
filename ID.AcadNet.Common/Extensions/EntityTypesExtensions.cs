using System;
using Intellidesk.AcadNet.Common.Enums;

namespace Intellidesk.AcadNet.Common.Extensions
{
    public static class EntityTypesExtensions
    {
        public static string XToString(this EntityTypes entityType)
        {
            string filter;
            if (entityType == EntityTypes.POLYLINE2D) filter = "2DPOLYLINE";
            else if (entityType == EntityTypes.POLYLINE3D) filter = "3DPOLYLINE";
            else if (entityType == EntityTypes.POLYLINE) filter = "*POLYLINE";
            else filter = Convert.ToString(entityType);
            return filter;
        }
    }
}