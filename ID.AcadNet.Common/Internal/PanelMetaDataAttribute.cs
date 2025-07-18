using Intellidesk.AcadNet.Common.Enums;
using System;

namespace Intellidesk.AcadNet.Common.Internal
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)
]
    public class PanelMetaDataAttribute : Attribute
    {
        public readonly PaletteNames Name;
        public readonly Type ElemntType;
        public readonly string LayerName;

        public PanelMetaDataAttribute(PaletteNames panelName, Type elemntType, string layerName)
        {
            Name = panelName;
            ElemntType = elemntType;
            LayerName = layerName;
        }
    }
}
