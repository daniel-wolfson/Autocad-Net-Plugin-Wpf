using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using ID.Infrastructure.Commands;
using ID.Infrastructure.Enums;
using ID.Infrastructure.Interfaces;
using ID.Infrastructure.Models;
using Intellidesk.AcadNet.Common.Core;
using Intellidesk.AcadNet.Common.Extensions;
using Intellidesk.Data.General;
using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Intellidesk.AcadNet.Common.Models
{
    public class CommandContext : CommandLineBase, IDisposable
    {
        public static Entity CurrentEntity;

        #region <ctor>

        public CommandContext(string globalCommandName, string displayName = null)
            : base(globalCommandName, new TaskArguments() { DisplayName = displayName ?? globalCommandName })
        {
        }

        public CommandContext(string globalCommandName, object commandArgs) : base(globalCommandName, commandArgs)
        {
            RegisterDocEvents();
            StartBackground();
            StartCommand(globalCommandName);
        }

        #endregion <ctor>

        #region <methods>

        public void Start(Action action)
        {
            //CommandLine.SendToExecute(GlobalCommandName);
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Ed.WriteMessage("\nException: {0}\n", ex.Message);
            }
        }

        public void Cancel(string commandName = null, string message = null)
        {
            commandName = commandName ?? CommandName;
            var inProcessList = Doc.GetInProcessList();
            if (inProcessList.ContainsKey(commandName))
            {
                ITaskArguments args = inProcessList[commandName];
                //args.CancellationToken = new CancellationToken(true);
                args.CommandInfo.Add(NotifyStatus.Cancel, message);
                args.IsCanceled = true;
            }
            Cancellation.Cancel();
            Mouse.OverrideCursor = null;
        }

        public void Clean()
        {
            if (Doc.UserData.ContainsKey(CommandName))
                Doc.UserData.Remove(CommandName);
        }

        public void Error(string commandName, string message)
        {
            var inProcessList = Doc.GetInProcessList();
            if (inProcessList.ContainsKey(commandName))
            {
                ITaskArguments commandArgs = inProcessList[commandName];
                //args.CancellationToken = new CancellationToken(true);
                if (!string.IsNullOrEmpty(message))
                    commandArgs.CommandInfo.Add(NotifyStatus.Error, message);
                commandArgs.IsCanceled = true;

            }
            //Close(commandName, new NotifyArgs(NotifyImageStatus.Error, text, toolTip, commandName));
            //Cancellation.Cancel();
            Mouse.OverrideCursor = null;
        }

        /// <summary> OkCommand </summary>
        public void Ok(string commandName, string text = null)
        {
            text = text ?? "";
            Notifications.SendNotifyMessageAsync(new NotifyArgs(NotifyStatus.Ready, text, commandName + ": success", commandName));
        }

        public void Send(string commandName, string commandParameter)
        {
            string tempFileName = System.IO.Path.GetFileNameWithoutExtension(System.IO.Path.GetTempFileName());
            tempFileName = System.IO.Path.GetDirectoryName(commandParameter) + "\\" + tempFileName;

            object ocmd = acadApp.GetSystemVariable("CMDECHO");
            string commandString =
                string.Format("(setvar \"CMDECHO\" 0)(command \"._{0}\" \"\" \"{1}\")(setvar \"CMDECHO\" {2})(princ) ",
                    commandName, tempFileName.Replace('\\', '/'), ocmd);
            //string.Format("(command \"_SAVEAS\" \"{0}\" \"{1}\") ", "", fullPath.Replace('\\', '/'));
            try
            {
                Doc.SendStringToExecute(commandString, false, false, false);
                //dwg.SendStringToExecute("_QSAVE ", true, false, false);
                //Doc.SendStringToExecute("_CLOSE ", true, false, false);

                // open the current file
                //db.ReadDwgFile(file.FullName, System.IO.FileShare.ReadWrite, true, String.Empty)
                //using (var transaction = db.TransactionManager.StartTransaction())
                //{ 
                //    //ToDo 
                //}
                // save as temp file 
                //db.SaveAs(tempFileName, DwgVersion.Current)
                System.IO.File.Copy(tempFileName, commandParameter, true);
                //and erase the temp file
                System.IO.File.Delete(tempFileName);
            }
            catch (Exception)
            {
            }
        }

        public void SaveToCache()
        {
            Cache.TryAdd(CommandName, this);
            //CommandContext.Cache.TryRemove(parentContext.GlobalCommandName, out value2);
        }

        public static bool IsBusy()
        {
            var doc = acadApp.DocumentManager.MdiActiveDocument;
            return doc.UserData
                .Cast<DictionaryEntry>()
                .Any(x => x.Value.GetType().Name.Contains("ITaskArguments"));
        }

        #endregion <methods>

        #region <overrides>

        public CommandContext CommandPrevious(string commandName)
        {
            CommandContext parentContext;
            Cache.TryGetValue(commandName, out parentContext);
            return parentContext;
        }

        public override void OnCommandWillStart(object sender, CommandEventArgs e)
        {
            Ed.WriteMessage($"\n{CommandNames.UserGroup}: {CommandName}...start");

            var commandText = !string.IsNullOrEmpty(CurrentInProcessCommand)
                ? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(CurrentInProcessCommand)
                : CommandName.Replace(PluginSettings.Name.ToUpper(), "").ToLower();

            Notifications.DisplayNotifyMessageAsync(commandText, 100);
        }

        #endregion <overrides>
    }
}
