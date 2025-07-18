#define PARTNER

namespace ID.Infrastructure.Commands
{
    public static class CommandNames
    {
#if PARTNER
        public const string UserGroup = "PARTNER";
#elif MLBS
        public const string UserGroup = "MLBS";
#elif INTEL
        public const string UserGroup = "INTEL";
#else
#endif

        public const string MainGroup = "Intellidesk";

        public const string WorkFilesDisplay = UserGroup + "WORKFILESDISPLAY";
        public const string WorkFilesOpenAll = UserGroup + "WORKFILESOPENALL";
        public const string Apply = UserGroup + "APPLY";
        public const string UpLoad = UserGroup + "UPLOAD";
        public const string Export = UserGroup + "EXPORT";

        //Main
        public const string Search = UserGroup + "SEARCH";
        public const string Explorer = UserGroup + "EXPLORER";
        public const string SearchPanelRemove = UserGroup + "SEARCHPANELREMOVE";
        public const string ExplorerPanelRemove = UserGroup + "EXPLORERPANELREMOVE";
        public const string LayerQueriesPanelRemove = UserGroup + "LAYERQUERIESPANELREMOVE";
        public const string BayQueriesPanelRemove = UserGroup + "BAYQUERIESPANELREMOVE";
        public const string Ribbon = UserGroup + "RIBBON";
        public const string Regen = UserGroup + "REGEN";
        public const string Refresh = UserGroup + "REFRESH";
        public const string LoadAfter = UserGroup + "LOADAFTER";
        public const string Purge = UserGroup + "PURGE";
        public const string Undo = UserGroup + "UNDO";
        public const string Ruler = UserGroup + "RULER";

        //Fiber
        public const string Cable = UserGroup + "CABLE";
        public const string AddCable = UserGroup + "ADDCABLE";
        public const string AddTitleCable = UserGroup + "ADDTITLECABLE";
        public const string UpdateCable = UserGroup + "UPDATECABLE";
        public const string CablePanelRemove = UserGroup + "CABLEPANELREMOVE";

        public const string Closure = UserGroup + "CLOSURE";
        public const string AddClosure = UserGroup + "ADDCLOSURE";
        public const string AddTitleClosure = UserGroup + "ADDTITLECLOSURE";
        public const string UpdateClosure = UserGroup + "UPDATECLOSURE";
        public const string ClosurePanelRemove = UserGroup + "CLOSUREPANELREMOVE";

        public const string ClosureConnect = UserGroup + "CLOSURECONNECT";
        public const string AddClosureConnect = UserGroup + "ADDCLOSURECONNECT";
        public const string AddTitleClosureConnect = UserGroup + "ADDTITLECLOSURECONNECT";
        public const string UpdateClosureConnect = UserGroup + "UPDATECLOSURECONNECT";
        public const string AddMarkerClosureConnect = UserGroup + "ADDMARKERCLOSURECONNECT";
        public const string ClosureConnectPanelRemove = UserGroup + "CLOSURECONNECTPANELREMOVE";

        public const string Cabinet = UserGroup + "CABINET";
        public const string AddCabinet = UserGroup + "ADDCABINET";
        public const string AddTitleCabinet = UserGroup + "ADDTITLECABINET";
        public const string UpdateCabinet = UserGroup + "UPDATECABINET";
        public const string CabinetPanelRmove = UserGroup + "CABINETPANELREMOVE";
        public const string EditElement = UserGroup + "EDITELEMENT";

        //Map
        public const string Map = UserGroup + "MAP";
        public const string MapViewPanelRemove = UserGroup + "MAPVIEWPANELREMOVE";
        public const string UcsChange = UserGroup + "UCSCHANGE";
        public const string FindTextOnMap = UserGroup + "FINDTEXTONMAP";
        public const string PointOnMap = UserGroup + "POINTONMAP";

        //Block
        public const string CopyAsBlock = UserGroup + "COPYASBLOCK";
        public const string PasteAsBlock = UserGroup + "PASTEASBLOCK";

        //Plot
        public const string Snapshot = UserGroup + "SNAPSHOT";
        public const string PlotWindow = UserGroup + "PLOTWINDOW";
        public const string PlotExtents = UserGroup + "PLOTEXTENTS";

        //Convert
        public const string ConvertToMarkers = UserGroup + "CONVERTTOMARKERS";

        //Intel
        public const string LayerQueries = UserGroup + "LAYERQUERIES";
        public const string BayQueries = UserGroup + "BAYQUERIES";
        public const string ApplyLayerQueries = UserGroup + "APPLYLAYERQUERIES";
        public const string ApplyBayQueries = UserGroup + "APPLYBAYQUERIES";

        public const string ExportToExcelFile = "EXPORTTOEXCELFILE";

        //System
        public const string XFileOpen = "XXFILEOPEN";
        public const string XFileTempSave = "XXFILETEMPSAVE";
        public const string XCheckPalettesetClose = "XXCHECKPALETTESETCLOSE";
        public const string XDisplayPoint = "XXDISPLAYPOINT";
        public const string XWriteMessage = "XXWRITEMESSAGE";
        public const string XSwitchToModelspace = "XXSWITCHTOMODELSPACE";
        public const string XIdleOnHubDisconnected = "XXIDLEONHUBDISCONNECTED";
        public const string XIdleOnHubMessage = "XXIDLEONHUBMESSAGE";
        public const string XLayersLoadData = "XXLAYERSLOADDATA";
        public const string SearchTextRunCommand = "SEARCHTEXTRUNCOMMAND";

        public const string Reg = "IDREG";
        public const string UnReg = "IDUNREG";
        public const string PluginPreLoad = "IDPLUGINPRELOAD";
        public const string PluginLoad = "IDPLUGINLOAD";
        public const string SaveGeoData = "IDSAVEGEODATA";

        //Signalr
        public const string SendFromAcad = "SendFromAcad";
    }
}