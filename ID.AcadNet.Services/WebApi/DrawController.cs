using System;
using System.Configuration;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Unity;

using Intellidesk.AcadNet.Data.Models.Dto;
using Intellidesk.Infrastructure;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.Windows;

using App = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Intellidesk.AcadNet.Resources.Properties;
using Intellidesk.AcadNet.Services.Core;
using Intellidesk.AcadNet.Services.Extentions;
using Intellidesk.AcadNet.Services.Interfaces;
using Intellidesk.Infrastructure.Core;
using Intellidesk.Infrastructure.Tasks;

namespace Intellidesk.AcadNet.Services.WebApi
{
    /// <summary> DrawController </summary>
    public class DrawController : AcadActionController
    {
        private IEventAggregator _aggregator;
        private ICommandLine _commandLine;
        private ILayerService _layerService;
        public class NotifyMessageStringEvent : PubSubEvent<string> { }

        public DrawController()
        {
            _aggregator = PluginBootstrapper.PluginContainer.Resolve<IEventAggregator>();
            _aggregator.GetEvent<NotifyMessageStringEvent>().Subscribe(TabNotifyUpdateMessage);
            _commandLine = PluginBootstrapper.PluginContainer.Resolve<ICommandLine>();
            _layerService = PluginBootstrapper.PluginContainer.Resolve<ILayerService>();
        }

        [HttpPost]
        public HttpResponseMessage Point(PointArgs args)
        {
            ActionMessage.Length = 0;
            HttpStatusCode code = HttpStatusCode.Created;

            if (args == null)
            {
                code = HttpStatusCode.ExpectationFailed;
                ActionMessage.Append("Circleargs argument is not supplied.");
            }
            else
            {
                if (!DrawPoint(args))
                {
                    code = HttpStatusCode.ExpectationFailed;
                }
            }

            if (code != HttpStatusCode.Created)
                return Request.CreateErrorResponse(code, ActionMessage.ToString());

            return Request.CreateResponse(code, ActionMessage.ToString());
        }

        [HttpGet]
        public HttpResponseMessage Point(double x, double y)
        {
            HttpStatusCode code = HttpStatusCode.Accepted;
            ActionMessage.Length = 0;

            if (DrawPoint(new PointArgs(x, y)))
            {
                ActionMessage.Append("Point was been created successfully.");
                code = HttpStatusCode.Created;
                return Request.CreateResponse(code, ActionMessage.ToString());
            }

            ActionMessage.Append("Point was't been created.");
            code = HttpStatusCode.ExpectationFailed;
            return Request.CreateErrorResponse(code, ActionMessage.ToString());
        }

        public HttpResponseMessage Put(CircleArgs circleArgs)
        {
            HttpStatusCode code = HttpStatusCode.Accepted;
            ActionMessage.Length = 0;

            if (DrawCircle(circleArgs))
            {
                ActionMessage.Append("Point was been created successfully.");
                code = HttpStatusCode.Created;
                return Request.CreateResponse(code, ActionMessage.ToString());
            }

            ActionMessage.Append("Circleargs argument is not supplied.");
            code = HttpStatusCode.ExpectationFailed;
            return Request.CreateErrorResponse(code, ActionMessage.ToString());
        }

        #region private methods

        private bool DrawCircle(CircleArgs args)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Document doc = App.DocumentManager.GetDocument(db);
            Mouse.OverrideCursor = Cursors.Wait;
            var vtenable = App.GetSystemVariable("VTENABLE");
            App.SetSystemVariable("VTENABLE", 7);

            using (DocumentLock l = doc.LockDocument())
            {
                try
                {
                    using (Transaction tran = db.TransactionManager.StartTransaction())
                    {
                        BlockTableRecord model = tran.GetObject(
                            SymbolUtilityServices.GetBlockModelSpaceId(db), OpenMode.ForWrite) as BlockTableRecord;

                        Circle c = new Circle { Center = new Point3d(args.X, args.Y, args.Z), Radius = args.Radius };
                        c.SetDatabaseDefaults(db);

                        if (model != null) model.AppendEntity(c);
                        tran.AddNewlyCreatedDBObject(c, true);

                        tran.Commit();
                    }

                    ActionMessage.Append("Cicle has been added into drawing successfully.");
                }
                catch (Exception ex)
                {
                    ActionMessage.Append("Drawing circle failed:\n" + ex.Message);
                    return false;
                }
            }

            App.SetSystemVariable("VTENABLE", vtenable);
            Mouse.OverrideCursor = null;
            return true;
        }

        private bool DrawPoint(PointArgs args)
        {
            bool result = false;
            _aggregator.GetEvent<NotifyMessageStringEvent>().Publish("Drawing...");
            //var task = Task<bool>.Factory.StartNew(() =>{});
            //ctrl.UpdateSystemVariable(new AcadSysVar("VTENABLE", "7"));

            ComponentManager.Ribbon.Dispatcher.Invoke(() =>
            {
                Mouse.OverrideCursor = Cursors.Wait;
                ComponentManager.Ribbon.Focus();

                ObjectId blockRefId = ObjectId.Null;
                try
                {
                    using (Doc.LockDocument())
                    {
                        if (!_layerService.GetLayers().Select(x=> x.Name).Contains("GIS"))
                        {
                            _layerService.Add("GIS");
                        }

                        blockRefId = Db.InsertBlock("gis_point", new Point3d(args.X, args.Y, 0), 2.0, "GIS");
                        if (blockRefId != ObjectId.Null)
                        {
                            using (new SysVarOverride("VTENABLE", 7))
                            {
                                _commandLine.Zoom(blockRefId, 3);
                                Ed.Regen();
                            }
                        }
                    }
                    ActionMessage.Append("gis_point has been added into drawing successfully.");
                    result = true;
                }
                catch (Exception ex)
                {
                    ActionMessage.Append("Drawing donut failed:\n" + ex.Message);
                    result = false;
                }
                finally
                {
                    _aggregator.GetEvent<NotifyMessageStringEvent>().Publish("Ready!");
                    Mouse.OverrideCursor = null;
                }
            });
            return result;
        }

        private static void TabNotifyUpdateMessage(string notifyMessage = null)
        {
            ComponentManager.Ribbon.Dispatcher.Invoke(() =>
            {
                ComponentManager.Ribbon.Focus();

                var args = PluginBootstrapper.PluginContainer.Resolve<ITaskArguments>();
                var ribbonControl = ComponentManager.Ribbon;
                var resourceClass = ConfigurationManager.AppSettings["resourceClass"];
                Thread.CurrentThread.CurrentUICulture = new CultureInfo("en");
                var resourceManager = new ResourceManager("Intellidesk.AcadNet.Resources.Properties.Images",
                    Assembly.GetAssembly(typeof(Images)));
                var ribbonTab = ribbonControl.Tabs.First(x => x.Id == "ID_Partner");

                var panel = ribbonTab.Panels.FirstOrDefault(x => x.Source.Title == "Notification");
                if (panel != null)
                {
                    var btn = panel.FindItem("NotificationsId"); //.Panels[4].Source.Items[0];
                    btn.Text = notifyMessage ?? "Ready";
                    btn.LargeImage = ImageHelper.GetBitmap((Bitmap)resourceManager.GetObject(
                        notifyMessage != null || args.ErrorInfo.Any() ? "note_ready" : "note_success"), 2, 1);
                }
            });
        }

        #endregion
    }
}
