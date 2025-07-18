using Intellidesk.AcadNet.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using IPanelTabView = Intellidesk.AcadNet.Common.Interfaces.IPanelTabView;

namespace Intellidesk.AcadNet.Enums
{
    public static class PaletteTabNames1
    {
        [Description("Project explorer")]
        public static Dictionary<string, Type> ProjectExplorer { get; set; }
        [Description("Search Text")]
        public static Dictionary<string, Type> SearchText { get; set; }

        static PaletteTabNames1()
        {
            ProjectExplorer = new Dictionary<string, Type> { { "ProjectExplorer", typeof(ITabProjectExplorerView) } };
            SearchText = new Dictionary<string, Type> { { "SearchText", typeof(IPanelTabView) } };
        }
    }
}