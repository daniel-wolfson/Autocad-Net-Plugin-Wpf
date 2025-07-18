using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using ID.Infrastructure;
using ID.Infrastructure.Commands;
using ID.Infrastructure.Interfaces;
using Serilog;
using System;
using System.Linq;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Intellidesk.AcadNet.Services.Commands
{
    public static class CommandManager1
    {
        private static Editor Ed => Doc.Editor;
        public static Database Db => HostApplicationServices.WorkingDatabase; //Doc.Database;
        private static Document Doc => acadApp.DocumentManager.MdiActiveDocument;
        private static IPluginSettings PluginSettings => Plugin.GetService<IPluginSettings>();

        public static void OnApplicationIdle(object sender, EventArgs e)
        {
            var doc = acadApp.DocumentManager.MdiActiveDocument;

            // Remove the event handler as it is no longer needed
            if (sender != null || doc == null) return;

            if (doc.UserData.Keys.Cast<string>().Any(x => x.StartsWith("IDLE")))
            {
                if (doc.UserData.ContainsKey(CommandNames.XFileTempSave))
                {
                    dynamic dynamicCommandArgs = acadApp.DocumentManager.MdiActiveDocument.UserData[CommandNames.XFileTempSave];
                    try
                    {
                        //acadApp.DocumentManager.MdiActiveDocument.SendCommandSynchronously(CommanName.Refresh + " ");//SendStringToExecute
                        //HttpResponseMessage httpResponseMessage = new FindController().File(
                        //    new FindQuery
                        //    {
                        //        FileName = dynamicCommandArgs.FileName,
                        //        FullPath = dynamicCommandArgs.FolderName,
                        //        Referer = "Mapit"
                        //    });
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Error(ex.InnerException?.Message ?? ex.Message);
                        //Ignore
                    }
                    finally
                    {
                        acadApp.DocumentManager.MdiActiveDocument.UserData.Remove(CommandNames.XFileTempSave);
                    }
                }
                else if (doc.UserData.ContainsKey(CommandNames.XIdleOnHubMessage))
                {
                    dynamic message = acadApp.DocumentManager.MdiActiveDocument.UserData[CommandNames.XIdleOnHubMessage];
                    Ed.WriteMessage(PluginSettings.Prompt + message.ToString());
                    acadApp.DocumentManager.MdiActiveDocument.UserData.Remove(CommandNames.XIdleOnHubMessage);
                    Autodesk.AutoCAD.Internal.Utils.PostCommandPrompt();
                }
                else if (doc.UserData.ContainsKey(CommandNames.XIdleOnHubDisconnected))
                {
                    doc.UserData.Remove(CommandNames.XIdleOnHubDisconnected);
                    doc.SendStringToExecute(CommandNames.XIdleOnHubDisconnected + " ", true, false, false);
                }

                if (!doc.UserData.Keys.Cast<string>().Any(x => x.StartsWith("IDLE")))
                    acadApp.Idle -= OnApplicationIdle;
            }
        }

        //object[] commandArgs = commandName.TrimEnd().Split(' ').Cast<object>().ToArray();
        //doc.Editor.CommandAsync("_."+ commandName.TrimEnd(), Editor.PauseToken).OnCompleted(() =>
        //{
        //    Notifications.DisplayNotifyMessage(new NotifyArgs(NotifyStatus.Ready, commandName + "End"));
        //});
    }
}
