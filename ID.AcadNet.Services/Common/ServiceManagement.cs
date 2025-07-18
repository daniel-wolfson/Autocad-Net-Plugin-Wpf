using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Intellidesk.AcadNet.Services.Common
{
    /// <summary> Service manager </summary>
    public static class ServiceManagement
    {
        /// <summary>  active document </summary>
        public static Document Doc => acadApp.DocumentManager.MdiActiveDocument;

        /// <summary>  active database </summary>
        public static Database Db => Doc.Database;

        /// <summary> active editor </summary>
        public static Editor Ed => Doc.Editor;
    }
}
