using Autodesk.AutoCAD.Windows;
using System.Collections.Generic;

namespace Intellidesk.AcadNet.Common.Core
{
    public class CustomPaletteSet : PaletteSet
    {
        public Dictionary<string, object> PalleteActivateArgument { get; set; }

        public CustomPaletteSet(string name) : base(name)
        {
        }
    }
}
