#define INTEL

using Autodesk.AutoCAD.Runtime;
using ID.Infrastructure.Commands;
using ID.Infrastructure.Enums;
using Intellidesk.AcadNet;
using Intellidesk.AcadNet.Common.Core;
using Intellidesk.AcadNet.Common.Enums;
using Intellidesk.AcadNet.Common.Interfaces;
using Intellidesk.AcadNet.Common.Models;
using Serilog;
using System.Threading;
using System.Windows.Input;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

#if (INTEL)
[assembly: CommandClass(typeof(CommandIntel))]
#endif
namespace Intellidesk.AcadNet
{
    public class CommandIntel : CommandLineBase
    {
        [CommandMethod(CommandNames.UserGroup, CommandNames.LayerQueries, CommandFlags.Session)]
        public void LayerQueries()
        {
            ICommandArgs palleteCommandArgs = new CommandArgs(null, CommandNames.LayerQueries);
            ToolsManager.LoadPallete(PaletteNames.LayerQueries, palleteCommandArgs);
        }

        [CommandMethod(CommandNames.UserGroup, CommandNames.BayQueries, CommandFlags.Session)]
        public void BayQueries()
        {
            ICommandArgs palleteCommandArgs = new CommandArgs(null, CommandNames.BayQueries);
            ToolsManager.LoadPallete(PaletteNames.BayQueries, palleteCommandArgs);
        }

        [CommandMethod(CommandNames.UserGroup, CommandNames.ApplyBayQueries,
            CommandFlags.Session | CommandFlags.NoHistory | CommandFlags.NoPaperSpace)]
        public void ExrtactFromBays()
        {
            using (var commandArgs = CommandArgs.Get(CommandNames.ApplyBayQueries))
            {
                if (commandArgs == null || commandArgs.CancelToken.IsCancellationRequested)
                {
                    commandArgs?.Clean();
                    return;
                }

                string CurrentProcessName = commandArgs.CommandParameter.ToString();

                Mouse.OverrideCursor = Cursors.Wait;
                Notifications.SendNotifyMessageAsync(NotifyStatus.Working);

                try
                {
                    using (Doc.LockDocument())
                    {
                        //System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
                        //stopWatch.Start();
                        ProgressMeter pm = new ProgressMeter();

                        pm.Start($"Processing for: {CurrentProcessName}...");
                        pm.SetLimit(2);
                        Thread.Sleep(500);

                        pm.MeterProgress();
                        //Db.BindAllXref();
                        pm.MeterProgress();

                        pm.Stop();
                        pm.Dispose();
                        acadApp.ShowAlertDialog("BindAllXref ened");
                        Mouse.OverrideCursor = null;
                        Notifications.SendNotifyMessageAsync(NotifyStatus.Ready);

                        //stopWatch.Stop();
                        //TimeSpan ts = stopWatch.Elapsed;

                        //// Format and display the TimeSpan value.
                        //string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                        //    ts.Hours, ts.Minutes, ts.Seconds,
                        //    ts.Milliseconds / 10);
                        //acadApp.ShowAlertDialog("RunTime " + elapsedTime);
                    }
                }
                catch (System.Exception ex)
                {
                    Log.Logger.Error(ex.InnerException?.Message ?? ex.Message);
                    //commandArgs.ActionMessage.Append("Drawing point failed:" + ex.Message);
                    //notifyResult = new ErrorNotifyArgs("Drawing point failed: " + ex.Message);
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
                commandArgs.Clean();
            }
        }
    }
}

