using System.ComponentModel;

namespace Intellidesk.AcadNet.Common.Enums
{
    public enum PaletteNames
    {
        [Description("Project explorer")] ProjectExplorer = 0,
        [Description("Search Text")] Search = 1,
        [Description("MapView")] MapView = 2,
        [Description("CableView")] Cable = 3,
        [Description("ClosureView")] Closure = 4,
        [Description("CabinetView")] Cabinet = 5,
        [Description("Layer Queries")] LayerQueries = 6,
        [Description("Bay Queries")] BayQueries = 7,
        [Description("ClosureConnectView")] ClosureConnect = 8,
        [Description("PartnerTab")] PartnerTab = 9,
        [Description("IntelTab")] IntelTab = 10
    }
}