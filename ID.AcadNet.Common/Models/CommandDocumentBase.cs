using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using ID.Infrastructure;
using ID.Infrastructure.Interfaces;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Intellidesk.AcadNet.Common.Models
{
    public class CommandDocumentBase
    {
        public Document Doc => acadApp.DocumentManager.MdiActiveDocument;
        public Editor Ed => Doc.Editor;
        public Database Db => Doc.Database ?? HostApplicationServices.WorkingDatabase;

        public IPluginSettings PluginSettings => Plugin.GetService<IPluginSettings>();
    }
}
