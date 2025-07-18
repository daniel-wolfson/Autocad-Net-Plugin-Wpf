using System;

namespace Intellidesk.AcadNet.Common.Core
{
    public interface IPluginPaletteSet
    {
        Guid Guid { get; set; }
        string PaletteName { get; set; }
        bool Visible { get; set; }
    }
}