using System;
using System.ComponentModel;
using System.Threading;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using System.Threading.Tasks;
using Intellidesk.Common.Enums;
using Intellidesk.Data.General;
using Intellidesk.AcadNet.Common.Models;

[assembly: CommandClass(typeof(Intellidesk.AcadNet.CommandWorker))]
namespace Intellidesk.AcadNet
{
    public class CommandWorker : CommandLineBase
    {
        /// <summary> Current backgroundWorker </summary>
        public BackgroundWorker Worker;
        public static Object SyncObj = new object();
        public Thread TaskThread;
        public Thread ActionThread;
        private static readonly Mutex Mut = new Mutex();

        /// <summary> Current task arguments </summary>
        public static TaskArguments CurrentTaskArgs;

        /// <summary> Occurs when System.ComponentModel.BackgroundWorker.RunWorkerAsync() is called. </summary>
        public void BgDoWork(object sender, DoWorkEventArgs e)
        {
            var taskArgs = (TaskArguments)e.Argument;
            var worker = (BackgroundWorker)sender;
            taskArgs.DisplayStatus(0);

            if (taskArgs.IsTimerOn)
                taskArgs.Status = StatusOptions.Runnig;

            foreach (var currentAction in taskArgs.ActionPool)
            {
                //Perform a time consuming operation and report progress.
                taskArgs.Title = currentAction.Key;
                taskArgs.Content = currentAction.Key;
                try
                {
                    e.Result = currentAction.Value(taskArgs);
                    if (worker.CancellationPending || !Convert.ToBoolean(e.Result))
                    {
                        taskArgs.Status = StatusOptions.Cancel;
                        e.Cancel = true;
                        break;
                    }
                }
                catch (System.Exception ex)
                {
                    var msg = ex.InnerException ?? ex;
                    taskArgs.ExpandedInfo = msg.Message;
                    taskArgs.Status = StatusOptions.Error;
                    Thread.Sleep(100);
                    return;
                }
            }

            if (taskArgs.Context != null)
                taskArgs.Context.Dispose();

            e.Result = taskArgs;
        }

        /// <summary> Occurs when the background operation has completed, has been canceled, or has raised an exception. </summary>
        private void BgRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                CurrentTaskArgs.Status = StatusOptions.Error;
                CurrentTaskArgs.Content = "Canceled";
            }
            else if (e.Error != null)
            {
                CurrentTaskArgs = (TaskArguments)e.Result;
                CurrentTaskArgs.Status = StatusOptions.Error;
                CurrentTaskArgs.Content = "Error";
                CurrentTaskArgs.ExpandedInfo = e.Error.InnerException.Message;
            }
            else if (e.Result != null)
            {
                // Returning task to global object
                CurrentTaskArgs = (TaskArguments)e.Result;
            }
        }

        /// <summary> Occurs when System.ComponentModel.BackgroundWorker.ReportProgress is called. </summary>
        private void BgProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var taskArgs = (TaskArguments)e.UserState;
            //_taskbarManager.SetProgressValue(e.ProgressPercentage, 100);
            if (taskArgs != null && taskArgs.Dialog != null)
            {
                taskArgs.ProgressPercentage = (int)((float)e.ProgressPercentage / taskArgs.ProgressLimit * 100);
                //taskArgs.Dialog.SetProgressBarPosition(e.ProgressPercentage);

                if (!taskArgs.IsTimerOn) taskArgs.DisplayStatus(0);
            }
        }

        [CommandMethod("IDTESTASYNC")]
        static public void TestCommandAsync()
        {
            MethodAsync();
        }

        static public async void MethodAsync()
        {
            try
            {
                var doc = Application.DocumentManager.MdiActiveDocument;
                var ed = doc.Editor;
                for (int i = 0; i < 20; i++)
                {
                    ed.WriteMessage("Processing {0}...", i);
                    string result = await WaitASynchronously();
                    ed.WriteMessage("Done.\n");
                }
            }
            catch (System.Exception ex)
            {
                Application.ShowAlertDialog("Error: " + ex.Message);
            }
        }

        static public async Task<string> WaitASynchronously()
        {
            await Task.Delay(500);
            return "Finished";
        }
    }
}
