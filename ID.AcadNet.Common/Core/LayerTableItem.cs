using System;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Intellidesk.Data.Repositories.Infrastructure;

namespace Intellidesk.AcadNet.Common.Core
{
    public class LayerTableItem : IObjectState
    {
        public string Name;
        public ObjectId LayerId;
        public Color Color;
        public bool IsFrozen;

        public ObjectState ObjectState { get; set; } = ObjectState.Unchanged;
    }
}