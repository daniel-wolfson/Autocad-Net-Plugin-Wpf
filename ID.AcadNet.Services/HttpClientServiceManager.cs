using Autodesk.AutoCAD.DatabaseServices;
using ID.Infrastructure.Commands;
using Intellidesk.AcadNet.Common.Enums;
using Intellidesk.AcadNet.Common.Internal;
using Intellidesk.AcadNet.Common.Models;
using Serilog;
using System;
using System.Net.Http;

namespace Intellidesk.AcadNet.Services
{
    public class HttpClientServiceManager : BaseService
    {
        public bool SendToMap(string commnadName, string requestUrl, CommandCancellationTokenSource cts)
        {
            bool actionResult = true;
            try
            {
                Doc.Editor.WriteMessage(PluginSettings.Prompt + commnadName + " get " + requestUrl + "...");
                HttpClient client = new HttpClient();
                HttpResponseMessage wcfResponse = client.GetAsync(requestUrl).GetAwaiter().GetResult();

                if ((int)wcfResponse.StatusCode >= 500)
                {
                    cts.OperationCanceledException = new OperationCanceledException(
                        $"Command {commnadName} response.StatusCode: {wcfResponse.StatusCode}");
                    cts.Cancel(false);
                    Doc.Editor.WriteMessage(PluginSettings.Prompt + cts.OperationCanceledException.Message);
                    actionResult = false;
                }

                if (!cts.IsCancellationRequested)
                {
                    HttpContent stream = wcfResponse.Content;
                    var data = stream.ReadAsStringAsync();
                    Doc.Editor.WriteMessage(PluginSettings.Prompt + data.Result);
                    cts.Cancel(false);
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex.InnerException?.Message ?? ex.Message);
                //CommandLine.Error(commnadName, ex.ToStringMessage());
                cts.Cancel();
                actionResult = false;
            }
            return actionResult;
        }

        public void FindTextOnMap()
        {
            using (var context = new CommandContext(CommandNames.FindTextOnMap, "FindTextOnMap"))
            {
                var cts = context.Cancellation;
                Entity ent = Selects.GetEntity("", null, EntityTypes.TEXT);
                if (ent == null)
                    context.Cancel();

                if (!cts.IsCancellationRequested && ent is DBText)
                {
                    CommandContext.CurrentEntity = ent;
                    var textString = ((DBText)ent).TextString;
                    var requestUrl = string.Format(PluginSettings.MapitApiFindUrl, textString, PluginSettings.UserId);

                    SendToMap(CommandNames.FindTextOnMap, requestUrl, cts);
                }

                context.Clean();
            }
        }

        //acadApp.DocumentManager.ExecuteInApplicationContext(data =>
        //{
        //    Commands.RunOnUIThread(() =>
        //    {
        //        //acadApp.DocumentManager.ExecuteInCommandContextAsync(o =>
        //        //{
        //        //    acadApp.DocumentManager.CurrentDocument.Editor.CommandAsync(CommandNames.Refresh + " ");
        //        //    return null;
        //        //}, data);

        //        //var activeDocument = acadApp.DocumentManager.Cast<Document>().FirstOrDefault(x => x.Name.ToLower() == data.ToString().ToLower());
        //        //if (activeDocument == null)
        //        //{
        //        //    acadApp.DocumentManager.Open(data.ToString(), false);
        //        //}
        //    });
        //}, fullPath);

    }
}