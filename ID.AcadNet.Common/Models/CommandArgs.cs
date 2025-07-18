using ID.Infrastructure;
using ID.Infrastructure.Enums;
using ID.Infrastructure.Models;
using Intellidesk.AcadNet.Common.Core;
using Intellidesk.AcadNet.Common.Interfaces;
using System;
using System.Threading;
using System.Windows.Input;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Intellidesk.AcadNet.Common.Models
{
    public class CommandArgs : ICommandArgs
    {
        private static CancellationTokenSource _cancelTokenSource { get; set; }
        public static string CurrentCommandName { get; set; }

        #region <props>

        public object Sender { get; set; }
        public string CommandName { get; set; }
        public object CommandParameter { get; set; }
        public string CommandGroup { get; set; }
        public CancellationToken CancelToken { get; }
        public CancellationTokenSource CancelTokenSource
        {
            get { return _cancelTokenSource; }
            set { _cancelTokenSource = value; }
        }
        public NotifyArgs NotifyArgs { get; set; }
        public Action<ICommandArgs> CommandCallBack { get; set; }
        private ICommandLine _commandLine;
        public ICommandLine CommandLine => _commandLine ?? (_commandLine = Plugin.GetService<ICommandLine>());

        #endregion <props>

        #region <ctor>

        public CommandArgs(object sender, string commandName, object commandParameter, bool isGroup = true)
        {
            Sender = sender;
            CommandName = commandName.ToUpper();
            CurrentCommandName = CommandName;
            CommandParameter = commandParameter;
            CommandGroup = isGroup ? CommandName : null;
            _cancelTokenSource = new CommandCancellationTokenSource();
            CancelToken = _cancelTokenSource.Token;
            NotifyArgs = new ReadyNotifyArgs();
        }
        public CommandArgs(object sender, string commandName, params object[] commandParameters)
            : this(sender, commandName, (object)commandParameters)
        {
        }

        #endregion <ctor>

        #region <Static methods>

        public static ICommandArgs Create(object sender, string commandName, object commandParameter, bool isGroup = true)
        {
            return new CommandArgs(sender, commandName, commandParameter, isGroup);
        }

        public static ICommandArgs Get(string commandName)
        {
            Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView();
            Mouse.OverrideCursor = Cursors.Wait;

            ICommandArgs commandArgs = acadApp.DocumentManager.MdiActiveDocument.UserData[commandName] as ICommandArgs;
            if (commandArgs != null)
            {
                commandArgs.NotifyArgs = new NotifyArgs(NotifyStatus.Working);
            }
            else
            {
                commandArgs = new CommandArgs(null, commandName, null);
                commandArgs.Cancel(new NotifyArgs(NotifyStatus.NotFound, "command not exist in context"));
            }

            CurrentCommandName = commandName;
            return commandArgs;
        }

        public static void Cancel(string commandName = null)
        {
            string cmd = commandName ?? CurrentCommandName;
            if (!string.IsNullOrEmpty(cmd))
            {
                ICommandArgs CurrentCommand = Get(cmd);
                CurrentCommand?.Cancel(new CancelNotifyArgs("it was been canceled by manual click"));
            }
        }

        #endregion <Static methods>

        #region <Public methods>

        public void Success(NotifyArgs notifyArgs = null)
        {
            NotifyArgs = notifyArgs ?? new ReadyNotifyArgs();
            CancelTokenSource.Dispose();
        }

        /// Cancel command
        public void Cancel(NotifyArgs notifyArgs = null)
        {
            NotifyArgs = notifyArgs ?? new CancelNotifyArgs();
            CancelTokenSource.Cancel(false);
        }

        /// Add command to execute queue
        public void AddToExecQueue1()
        {
            if (!acadApp.DocumentManager.MdiActiveDocument.UserData.ContainsKey(CommandName))
                acadApp.DocumentManager.MdiActiveDocument.UserData.Add(CommandName, this);
        }

        /// Send to execute now, include call of AddToExecQueue and commandCallBack, 
        /// which will called if IsCancellationRequested = false
        public void SendToExecute(Action<ICommandArgs> commandCallBack = null)
        {
            if (commandCallBack != null)
                CommandCallBack = new Action<ICommandArgs>((cmd) =>
                {
                    if (!this.CancelToken.IsCancellationRequested)
                    {
                        commandCallBack(this);
                    }
                });

            if (!acadApp.DocumentManager.MdiActiveDocument.UserData.ContainsKey(CommandName))
                acadApp.DocumentManager.MdiActiveDocument.UserData.Add(CommandName, this);

            acadApp.DocumentManager.MdiActiveDocument.SendStringToExecute(CommandName + " ", true, false, false);
            Autodesk.AutoCAD.Internal.Utils.PostCommandPrompt();
        }

        public void RunIdle(Action<object> commandCallBack = null)
        {
            CommandCallBack = commandCallBack;

            if (!acadApp.DocumentManager.MdiActiveDocument.UserData.ContainsKey("COMMON_IDLE_" + CommandName))
                acadApp.DocumentManager.MdiActiveDocument.UserData.Add("COMMON_IDLE_" + CommandName, this);

            //acadApp.DocumentManager.MdiActiveDocument.SendStringToExecute(this.CommandName + " ", true, false, false);
            //acadApp.Idle += ToolsManager.Common_Idle;
            //Autodesk.AutoCAD.Internal.Utils.PostCommandPrompt(); });
        }

        public void Clean()
        {
            if (acadApp.DocumentManager.MdiActiveDocument.UserData.ContainsKey(CommandName))
                acadApp.DocumentManager.MdiActiveDocument.UserData.Remove(CommandName);
            CurrentCommandName = "";
        }

        #endregion <Public methods>

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing) // dispose managed state (managed objects).
                {
                    if (CancelToken.IsCancellationRequested)
                        Notifications.SendNotifyMessageAsync(new ErrorNotifyArgs(CommandName + " canceled"));
                    else if (CommandGroup != null)
                        Notifications.SendNotifyMessageAsync(NotifyStatus.Ready);

                    CommandCallBack?.Invoke(this);
                    acadApp.DocumentManager.MdiActiveDocument.UserData.Remove(CommandName);
                }

                CurrentCommandName = "";
                CommandLine.Cancel();
                Mouse.OverrideCursor = null;
                disposedValue = true;
                Autodesk.AutoCAD.Internal.Utils.PostCommandPrompt();
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~CommandArgs() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }

    public class CommandParam
    {
        public Enum ParamTypeId;
        public string ParamType;
        public string ParamValue;
        public string ParamAction;
    }
}