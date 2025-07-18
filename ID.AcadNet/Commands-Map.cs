using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using ID.Infrastructure;
using ID.Infrastructure.Commands;
using ID.Infrastructure.Extensions;
using Intellidesk.AcadNet;
using Intellidesk.AcadNet.Common.Core;
using Intellidesk.AcadNet.Common.Enums;
using Intellidesk.AcadNet.Common.Extensions;
using Intellidesk.AcadNet.Common.Interfaces;
using Intellidesk.AcadNet.Common.Internal;
using Intellidesk.AcadNet.Common.Models;
using Intellidesk.AcadNet.Common.Utils;
using Intellidesk.AcadNet.WebBrowser;
using System;
using System.Linq;
using System.Net.Http;
using acApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Exception = System.Exception;

// This line is not mandatory, but improves loading performances
[assembly: CommandClass(typeof(CommandMap))]
namespace Intellidesk.AcadNet
{
    public class CommandMap : CommandLineBase
    {
        [CommandMethod(CommandNames.UserGroup, CommandNames.Map, CommandFlags.Session)]
        public void MapIt()
        {
            var commandArgs = new CommandArgs(null, CommandNames.Map, new Point3d(32.10778, 34.96604, 0));
            ToolsManager.LoadPallete(PaletteNames.MapView, commandArgs);
        }

        [CommandMethod(CommandNames.UserGroup, CommandNames.UcsChange, CommandFlags.Session)]
        public void UcsChange()
        {
            if (!Utils.IsModelSpace()) return;

            using (var context = new CommandContext(CommandNames.UcsChange, "UcsChange"))
            {
                using (Transaction acTrans = Db.TransactionManager.StartTransaction())
                {
                    Point3d basePt;
                    Point3d moveToPt = new Point3d();
                    var promptBasePointsResult = context.Ed.GetPoint(context.CommandLine.Current() + "Enter base point from:");
                    if (promptBasePointsResult.Status == PromptStatus.OK)
                    {
                        basePt = promptBasePointsResult.Value;
                    }
                    else
                    {
                        context.Ed.WriteMessage("\nCancel");
                        return;
                    }

                    PromptSelectionResult promptSelectionResult;
                    PromptKeywordOptions pKeyOpts = new PromptKeywordOptions("\nChoice system coordinate [OLD/NEW]: ", "OLD NEW");
                    //pKeyOpts.Keywords.Default = "ITM";
                    PromptResult keysPromptResult = context.Ed.GetKeywords(pKeyOpts);

                    if (keysPromptResult.Status == PromptStatus.OK)
                    {
                        if (keysPromptResult.StringResult == "OLD")
                            moveToPt = new Point3d(basePt.X - 50000, basePt.Y - 500000, 0);
                        else if (keysPromptResult.StringResult == "NEW")
                            moveToPt = new Point3d(basePt.X + 50000, basePt.Y + 500000, 0);

                        promptSelectionResult = context.Ed.SelectAll();
                        var ids = promptSelectionResult.Value.GetObjectIds();

                        foreach (var id in ids)
                        {
                            Entity ent = (Entity)acTrans.GetObject(id, OpenMode.ForWrite);
                            ent.TransformBy(Matrix3d.Displacement(basePt.GetVectorTo(moveToPt)));
                        }

                        acTrans.Commit();
                        context.CommandLine.ZoomView(Db.Extmin, Db.Extmax, moveToPt);
                    }
                }

                context.Clean();
            }
        }

        [CommandMethod(CommandNames.UserGroup, CommandNames.FindTextOnMap, CommandFlags.Session)]
        public void FindTextOnMap()
        {
            if (!Utils.IsModelSpace()) return;

            WebBrowserInitializer.IsHostExistAlert();

            using (var context = new CommandContext(CommandNames.FindTextOnMap, "FindTextOnMap"))
            {
                Entity ent = Selects.GetEntity("", null, EntityTypes.TEXT);
                if (ent == null)
                    context.Cancel();

                if (!context.Cancellation.IsCancellationRequested && ent is DBText)
                {
                    CommandContext.CurrentEntity = ent;
                    var textString = ((DBText)ent).TextString;
                    var requestUrl = string.Format(PluginSettings.MapitApiFindUrl, textString, PluginSettings.UserId);
                    //SendToMapHttpClient("FindTextOnMap", requestUrl, context.Cancellation);
                    AcadNetSignalRClientHost.SendToClient("FindtextOnMap",
                        new
                        {
                            text = textString,
                            FileName = System.IO.Path.GetFileNameWithoutExtension(acApp.DocumentManager.MdiActiveDocument.Name)
                        });
                }

                context.Clean();
            }
        }

        [CommandMethod(CommandNames.UserGroup, CommandNames.PointOnMap, CommandFlags.Session)]
        public void SendPointToMap()
        {
            if (!Utils.IsModelSpace()) return;

            WebBrowserInitializer.IsHostExistAlert();

            using (var context = new CommandContext(CommandNames.PointOnMap, "PointOnMap"))
            {
                var ent = SendPointToMap(context);
                if (!context.Cancellation.IsCancellationRequested)
                {
                    var pnt = ((BlockReference)ent).Position;

                    AcadNetSignalRClientHost.SendToClient("PointOnMap",
                        new
                        {
                            Latitude = pnt.Y > pnt.X ? pnt.X : pnt.Y,
                            Longitude = pnt.Y > pnt.X ? pnt.Y : pnt.X,
                            FileName = System.IO.Path.GetFileNameWithoutExtension(acApp.DocumentManager.MdiActiveDocument.Name)
                        });
                }

                context.Clean();
            }
        }

        private Entity SendPointToMap(CommandContext context)
        {
            Entity ent = null;

            if (!Utils.IsModelSpace()) return ent;

            Point3d pointResult = Point3d.Origin;

            var ids = Ed.GetSelectImplied();
            if (ids.Count == 0)
            {
                var blockObjectId = Db.InsertBlock("gis_point", Point3d.Origin,
                    new BlockOptions { Scale = 1, JigPrompt = eJigPrompt.PromptInsert });

                if (blockObjectId != ObjectId.Null)
                {
                    //context.Cancellation.Cancel();
                    //return null;
                    ent = blockObjectId.XCast<BlockReference>();
                    if (ent != null)
                        pointResult = ((BlockReference)ent).Position;
                }
                else
                {
                    ids = Ed.GetSelectionPrompt(1, "Select the point object to send: ");
                    if (ids != null && ids.Count != 0)
                    {
                        ent = ids.FirstOrDefault().XCast<Entity>();
                        pointResult = ent.XGetBasePoint();
                    }
                    else
                    {
                        PromptPointsResult promptPointsResult =
                            Ed.GetPoints(context.CommandLine.Current() + "Enter the point to send:", 1);

                        if (promptPointsResult.Status == PromptStatus.OK)
                            pointResult = promptPointsResult.Value.FirstOrDefault();
                        else
                            context.Cancel();
                    }
                }
            }
            else
            {
                ent = ids.FirstOrDefault().XCast<Entity>();
                pointResult = ent.XGetBasePoint();
            }

            //if (!context.Cancellation.IsCancellationRequested)
            //{
            //    var requestUrl = string.Format(Settings.Default.MapInfoApiPointUrlFormat,
            //        pointResult.X, pointResult.Y, PluginSettings.UserName);

            //    SendToMapHttpClient(CommandNames.PointOnMap, requestUrl, context.Cancellation);

            //    if (!context.Cancellation.IsCancellationRequested)
            //    {
            //        if (ent != null)
            //            context.CommandLine.Zoom(ent.ObjectId);
            //        else if (pointResult != Point3d.Origin)
            //            context.CommandLine.ZoomWindow(pointResult, pointResult);
            //    }
            //}

            return ent;
        }

        private bool SendToMapHttpClient(string commnadName, string requestUrl, CommandCancellationTokenSource cts)
        {
            bool actionResult = true;
            try
            {
                Ed.WriteMessage(PluginSettings.Prompt + commnadName + " get " + requestUrl + "...");
                HttpClient client = new HttpClient();
                HttpResponseMessage wcfResponse = client.GetAsync(requestUrl).GetAwaiter().GetResult();

                if ((int)wcfResponse.StatusCode >= 500)
                {
                    cts.OperationCanceledException = new OperationCanceledException(
                        $"Command {commnadName} response.StatusCode: {wcfResponse.StatusCode}");
                    cts.Cancel(false);
                    Ed.WriteMessage(PluginSettings.Prompt + cts.OperationCanceledException.Message);
                    actionResult = false;
                }

                if (!cts.IsCancellationRequested)
                {
                    HttpContent stream = wcfResponse.Content;
                    var data = stream.ReadAsStringAsync();
                    Ed.WriteMessage(PluginSettings.Prompt + data.Result);
                    cts.Cancel(false);
                }
            }
            catch (Exception ex)
            {
                ICommandLine commandLine = Plugin.GetService<ICommandLine>();
                commandLine.Error(commnadName, ex.ToStringMessage());
                cts.Cancel();
                actionResult = false;
            }
            return actionResult;
        }
    }
}

//string url = "http://vmmapinfo.partnergsm.co.il/AcadNetGis/AcadNetStateService/StateService.svc";
//var webbinding = new WebHttpBinding();
//using (var proxy = new WebChannelFactory<IStateService>(webbinding, new Uri(url)))
//{
//    var webchannel = proxy.CreateChannel();
//    var data = webchannel.GetData().FirstOrDefault();
//    if (data != null)
//    {
//        var filename = data.Shem_autoc;
//        _commandLineService.WriteMessage(filename);
//        _commandLineService.ExecuteOpenDwg(filename, @"\GIS\as-made\");
//    }
//}

//var brs = Db.ReadObjectsDynamic<BlockReference>("")
//.Where(br => br.Position == new Point3d(5.0, 5.0, 0.0))
//.ToList();

//private static DMapInfo CreateMapinfoInstance()
//{
//    //comMapIType = System.Type.GetTypeFromProgID("Mapinfo.Application?­");
//    //comMapIObject = System.Activator.CreateInstance(comMapIType);
//    var mapinfotype = Type.GetTypeFromProgID("Mapinfo.Application");
//    var mapinfoinstance = (DMapInfo)Activator.CreateInstance(mapinfotype);
//    mapinfoinstance.Visible = true;
//    return mapinfoinstance;
//}

//private static MapInfoApplication FindMapinfoInstance()
//{
//    var mapinfoinstance = (MapInfoApplication)Marshal.GetActiveObject("Mapinfo.Application");
//    return mapinfoinstance;
//}
