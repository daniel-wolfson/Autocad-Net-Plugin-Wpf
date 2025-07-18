using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using ID.Infrastructure;
using ID.Infrastructure.Enums;
using ID.Infrastructure.Models;
using Intellidesk.AcadNet.Common.Core;
using Intellidesk.AcadNet.Common.Extensions;
using Intellidesk.AcadNet.Common.Models;
using Intellidesk.AcadNet.Services.Core;
using Intellidesk.Data.Models.Dto;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using System.Windows.Input;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Intellidesk.AcadNet.WebApi
{
    /// <summary> DrawController </summary>
    public class DrawController : AcadActionController
    {
        /// <summary> Web Api: Draw point by X,Y </summary>
        [HttpGet]
        public HttpResponseMessage Point(double x, double y)
        {
            return Point(new PointArgs(x, y));
        }

        /// <summary> Web Api: Draw point by pointArgs</summary>
        [HttpPost]
        public HttpResponseMessage Point(PointArgs args)
        {
            NotifyArgs notifyResult = new ReadyNotifyArgs();
            ActionMessage.Length = 0;
            HttpStatusCode code = HttpStatusCode.Created;

            if (args == null)
            {
                code = HttpStatusCode.ExpectationFailed;
                notifyResult = new ErrorNotifyArgs("PointArgs argument is not supplied.");
            }
            else
            {
                Notifications.SendNotifyMessageAsync(new NotifyArgs(NotifyStatus.Working));
                Thread.Sleep(1000);

                //var task = Task<bool>.Factory.StartNew(() =>{});
                //ctrl.UpdateSystemVariable(new AcadSysVar("VTENABLE", "7"));

                InfraManager.RunOnUIThread(() =>
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                    ObjectId blockRefId = ObjectId.Null;
                    try
                    {
                        using (Doc.LockDocument())
                        {
                            blockRefId = Db.InsertBlock("gis_point", new Point3d(args.X, args.Y, 0), new BlockOptions { Scale = 2.0 });
                            if (blockRefId != ObjectId.Null)
                            {
                                using (new SysVarOverride("VTENABLE", 7))
                                {
                                    CommandLine.Zoom(blockRefId, 3);
                                    Ed.Regen();
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ActionMessage.Append("Drawing point failed:" + ex.Message);
                        notifyResult = new ErrorNotifyArgs("Drawing point failed: " + ex.Message);
                    }
                    finally
                    {
                        Mouse.OverrideCursor = null;
                    }
                });
            }

            Notifications.SendNotifyMessageAsync(notifyResult);
            Thread.Sleep(1000);

            return code != HttpStatusCode.Created
                ? Request.CreateErrorResponse(code, ActionMessage.ToString())
                : Request.CreateResponse(code, ActionMessage.ToString());
        }

        /// <summary>  Web Api: temp Put </summary>
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

        // ====================== private methods ====================== //

        private bool DrawCircle(CircleArgs args)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Document doc = acadApp.DocumentManager.GetDocument(db);
            Mouse.OverrideCursor = Cursors.Wait;
            var vtenable = acadApp.GetSystemVariable("VTENABLE");
            acadApp.SetSystemVariable("VTENABLE", 7);

            using (DocumentLock l = doc.LockDocument())
            {
                try
                {
                    using (Transaction tran = db.TransactionManager.StartTransaction())
                    {
                        BlockTableRecord model = tran.GetObject(
                            acadApp.DocumentManager.GetCurrentSpaceId(db), OpenMode.ForWrite) as BlockTableRecord;

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

            acadApp.SetSystemVariable("VTENABLE", vtenable);
            Mouse.OverrideCursor = null;
            return true;
        }

        private bool DrawPoint(PointArgs args)
        {
            bool result = false;
            NotifyArgs notifyResult = new ReadyNotifyArgs();
            Notifications.SendNotifyMessageAsync(new NotifyArgs(NotifyStatus.Working));
            Thread.Sleep(1000);

            //var task = Task<bool>.Factory.StartNew(() =>{});
            //ctrl.UpdateSystemVariable(new AcadSysVar("VTENABLE", "7"));

            InfraManager.RunOnUIThread(() =>
            {
                Mouse.OverrideCursor = Cursors.Wait;
                ObjectId blockRefId = ObjectId.Null;
                try
                {
                    using (Doc.LockDocument())
                    {
                        blockRefId = Db.InsertBlock("gis_point", new Point3d(args.X, args.Y, 0), new BlockOptions { Scale = 2.0 });
                        if (blockRefId != ObjectId.Null)
                        {
                            using (new SysVarOverride("VTENABLE", 7))
                            {
                                CommandLine.Zoom(blockRefId, 3);
                                Ed.Regen();
                            }
                        }
                    }
                    result = true;
                }
                catch (Exception ex)
                {
                    ActionMessage.Append("Drawing point failed:" + ex.Message);
                    notifyResult = new ErrorNotifyArgs("Drawing point failed: " + ex.Message);
                    result = false;
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            });

            Notifications.SendNotifyMessageAsync(notifyResult);
            Thread.Sleep(1000);

            return result;
        }
    }
}
