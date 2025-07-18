using Autodesk.AutoCAD.DatabaseServices;
using ID.Infrastructure;
using ID.Infrastructure.Enums;
using ID.Infrastructure.General;
using ID.Infrastructure.Interfaces;
using ID.Infrastructure.Models;
using Intellidesk.AcadNet.Common.Core;
using Intellidesk.AcadNet.Common.Enums;
using Intellidesk.AcadNet.Common.Extensions;
using Intellidesk.AcadNet.Common.Internal;
using Intellidesk.AcadNet.Common.Models;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using System.Windows.Input;

namespace Intellidesk.AcadNet.WebApi
{
    //[RoutePrefix("IntelliDeskApi/Find")]
    public class FindController : AcadActionController
    {
        [HttpGet]
        public HttpResponseMessage Text(string v)
        {
            HttpStatusCode code = HttpStatusCode.Accepted;
            ActionMessage.Length = 0;
            NotifyArgs notifyArgs = new ReadyNotifyArgs();

            if (string.IsNullOrEmpty(v))
                return Request.CreateErrorResponse(HttpStatusCode.NoContent, ActionMessage.ToString());

            Notifications.SendNotifyMessageAsync(NotifyStatus.Working);

            InfraManager.RunOnUIThread((Action)(() =>
            {
                try
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                    var tabs = ToolsManager.PaletteTabs;
                    var tabName = PaletteNames.Search.ToString();
                    var paletteTab = tabs[tabName];

                    if (tabs.Count > 0 && tabs.ContainsTab(PaletteNames.Search) && paletteTab.IsActive && paletteTab.TabIndex == tabs.CurrentTabIndex)
                        tabs.Activate((PaletteNames)paletteTab.TabIndex, new CommandArgs(null, "FindText", v));

                    var ent = Db.XReadObjectsDynamic<DBText>().Cast<DBText>().FirstOrDefault(x => x.TextString.Contains(v));
                    if (ent != null)
                    {
                        if (CommandContext.CurrentEntity != null)
                            CommandContext.CurrentEntity.Unhighlight();

                        ent.Highlight();
                        CommandContext.CurrentEntity = ent;

                        code = HttpStatusCode.Found;
                        ActionMessage.Append("Command 'FindFile': Text was been found successfully.");
                        CommandLine.Zoom(ent.ObjectId, AppSettings.ZoomDisplayFactor);
                    }
                    else
                    {
                        code = HttpStatusCode.NotFound;
                        notifyArgs = new ErrorNotifyArgs("Text was't been found.");
                    }
                }
                catch (Exception ex)
                {
                    code = HttpStatusCode.InternalServerError;
                    ActionMessage.Append("Text search failed:" + ex.Message);
                    notifyArgs = new ErrorNotifyArgs(ActionMessage.ToString());
                }
                finally
                {
                    Notifications.SendNotifyMessageAsync((NotifyArgs)notifyArgs);
                    Mouse.OverrideCursor = null;
                }
            }));

            return Request.CreateErrorResponse(code, ActionMessage.ToString());
        }

        //[Route("File/{FullPath}")]
        //[HttpGet]
        //public HttpResponseMessage File(string fullPath)
        //    return File(new FindQuery { FullPath = fullPath });
        //[Route("File/{path}/{FileName}")]

        [HttpGet]
        public HttpResponseMessage File([FromUri] FindQuery query)
        {
            string referrer = query.Referer;

            Notifications.SendNotifyMessageAsync(new NotifyArgs(NotifyStatus.Working));
            InfraManager.RunOnUIThread(() => { Mouse.OverrideCursor = Cursors.Wait; });

            IPluginSettings pluginSettings = Plugin.GetService<IPluginSettings>();
            SimpleActionResult simpleResult = ID.Infrastructure.Files.FindPath(pluginSettings.IncludeFolders.ToArray(), query.FullPath, query.FileName);
            var code = simpleResult.StatusCode;
            NotifyArgs notifyResult = new ReadyNotifyArgs();

            if (code == HttpStatusCode.Found)
            {
                var fullPath = simpleResult.ActionResult.ToString();
                var fileName = query.FileName;
                AppSettings.CurrentFolder = query.FullPath;

                InfraManager.RunOnUIThread((Action)(() =>
                    {
                        try
                        {
                            if (!referrer.ToLower().Contains("map"))
                            {
                                var tabs = ToolsManager.PaletteTabs;
                                var tabName = PaletteNames.ProjectExplorer.ToString();

                                if (tabs.Count > 0 && tabs.ContainsTab(PaletteNames.ProjectExplorer) && tabs[tabName].IsActive)
                                {
                                    var paletteTab = tabs[tabName];
                                    var parameter = string.IsNullOrEmpty(fileName)
                                        ? fullPath
                                        : Path.GetDirectoryName(fullPath);

                                    tabs.Activate((PaletteNames)paletteTab.TabIndex,
                                        new CommandArgs(null, "Find", parameter));
                                }
                            }

                            if (!string.IsNullOrEmpty(fileName))
                                Documents.DocumentAction(fullPath, DocumentOptions.OpenAndActive);

                            code = HttpStatusCode.OK;
                            ActionMessage.Append("Command 'FindFile': File was been opened successfully.");
                        }
                        catch (Exception ex)
                        {
                            code = HttpStatusCode.InternalServerError;
                            ActionMessage.Append($"Command 'FindFile': File {fullPath} was't opened because of {ex}");
                            Notifications.SendNotifyMessageAsync((NotifyArgs)new ErrorNotifyArgs((string)ActionMessage.ToString()));
                        }
                    }));
            }
            else
            {
                notifyResult = new ErrorNotifyArgs($"File '{query.FileName}' in folder '{query.FullPath}' not found");
            }

            Notifications.SendNotifyMessageAsync(notifyResult);
            InfraManager.RunOnUIThread(() => { Mouse.OverrideCursor = null; });
            Thread.Sleep(1000);

            if (Request == null)
            {
                //var controllerContext = new HttpControllerContext();
                //var request = new HttpRequestMessage();
                //request.Headers.Add("X-My-Header", "success");
                //this.ControllerContext = controllerContext;
                return new HttpResponseMessage(code);
            }

            return Request.CreateResponse(code, ActionMessage.ToString());
        }
    }

    public class FindQuery
    {
        public string FullPath { get; set; }
        public string FileName { get; set; }
        public string Referer { get; set; }
    }
}