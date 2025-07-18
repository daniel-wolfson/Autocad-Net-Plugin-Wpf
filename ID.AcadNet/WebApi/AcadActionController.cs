using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using ID.Infrastructure;
using ID.Infrastructure.Interfaces;
using Intellidesk.AcadNet.Common.Interfaces;
using System.Text;
using System.Web.Http;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Intellidesk.AcadNet.WebApi
{
    //[EnableCors(origins: "http://localhost:19680,http://localhost:43210,http://mapinfo", headers: "*", methods: "*")]
    public class AcadActionController : ApiController
    {
        public Document Doc => Application.DocumentManager.MdiActiveDocument;
        public Editor Ed => Doc.Editor;
        public Database Db => Doc.Database;
        public IPluginSettings AppSettings => Plugin.GetService<IPluginSettings>();
        public static StringBuilder ActionMessage { get; } = new StringBuilder();

        //protected IEventAggregator Aggregator;
        protected ICommandLine CommandLine;
        protected ILayerService LayerService;
        //public class NotifyMessageStringEvent : PubSubEvent<string> { }

        public AcadActionController()
        {
            //Aggregator = Plugin.GetService<IEventAggregator>();
            //Aggregator.GetEvent<NotifyMessageStringEvent>().Subscribe(Notifications.DisplayNotifyMessage);
            CommandLine = Plugin.GetService<ICommandLine>();
            LayerService = Plugin.GetService<ILayerService>();
        }
    }
}
