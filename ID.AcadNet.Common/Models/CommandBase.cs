using Autodesk.AutoCAD.ApplicationServices;
using ID.Infrastructure;
using ID.Infrastructure.Commands;
using ID.Infrastructure.Enums;
using ID.Infrastructure.Extensions;
using ID.Infrastructure.Interfaces;
using ID.Infrastructure.Models;
using Intellidesk.AcadNet.Common.Core;
using Intellidesk.AcadNet.Common.Extensions;
using Prism.Events;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using System.Windows.Threading;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Intellidesk.AcadNet.Common.Models
{

    public class CommandBase : CommandDocumentBase, IDisposable
    {
        public static readonly ConcurrentDictionary<string, CommandContext> Cache = new ConcurrentDictionary<string, CommandContext>();

        #region <props>

        private ILogger _logger;
        private IEventAggregator _eventAggregator;
        private IPluginSettings _pluginSettings;
        private CommandEventHandler _actionCommandWillStartHandler;
        private CommandEventHandler _actionCommandEndedHandler;
        private CommandEventHandler _actionCommandCancelledHandler;
        private DocumentCollectionEventHandler _actionDocumentActivatedHandler;
        private DocumentCollectionEventHandler _actionDocumentToBeDestroyedHandler;

        public CommandCancellationTokenSource Cancellation { get; set; } = new CommandCancellationTokenSource();
        public readonly Dictionary<string, object> Parameters = new Dictionary<string, object>();


        public Dispatcher UIDispatcher => Dispatcher.CurrentDispatcher;
        public Dispatcher BackgroundDispatcher;

        public string CommandName { get; set; }
        public CommandArgs CommandArgs { get; set; }

        public ILogger Logger => _logger
            ?? (_logger = Plugin.GetService<ILogger>());

        public IEventAggregator EventAggregator => _eventAggregator
            ?? (_eventAggregator = Plugin.GetService<IEventAggregator>());

        public string CurrentInProcessCommand => Doc.GetInProcessList().Any()
            ? Doc.GetInProcessList().Last().Value.DisplayName : "";

        public string PromptMessage => $"\n{CommandNames.UserGroup}. {CommandName}";

        #endregion <props>

        #region <ctor>

        public CommandBase(string globalCommandName, object commandArgs)
        {
            //acadApp.Idle -= CommandManager.OnApplicationIdle
            Cancellation = new CommandCancellationTokenSource();
            CommandName = globalCommandName ?? "";
            CommandArgs = commandArgs as CommandArgs;

            if (globalCommandName != null)
            {
                if (Doc.UserData.ContainsKey(globalCommandName))
                    Doc.UserData.Remove(globalCommandName);
                Doc.UserData.Add(globalCommandName, commandArgs);
            }
        }

        #endregion <ctor>

        #region <virtual doc methods>

        public virtual void StartCommand(string globalCommandName)
        {
            OnCommandWillStart(globalCommandName, new EventArgs() as CommandEventArgs);
        }
        public virtual void OnCommandWillStart(object sender, CommandEventArgs e) { }
        public virtual void OnCommandEnded(object sender, CommandEventArgs e) { }
        public virtual void OnCommandCancelled(object sender, CommandEventArgs e) { }
        public virtual void OnDocumentActivated(object sender, DocumentCollectionEventArgs e) { }
        public virtual void OnDocumentToBeDestroyed(object sender, DocumentCollectionEventArgs e) { }

        #endregion <virtual doc methods>

        #region <public methods>

        public void StartBackground()
        {
            new Thread(() =>
            {
                BackgroundDispatcher = Dispatcher.CurrentDispatcher;
                Dispatcher.Run();
            }).Start();
        }

        public void RegisterDocEvents()
        {
            _actionCommandWillStartHandler = OnCommandWillStart;
            _actionCommandEndedHandler = OnCommandEnded;
            _actionCommandCancelledHandler = OnCommandCancelled;
            _actionDocumentActivatedHandler = OnDocumentActivated;
            _actionDocumentToBeDestroyedHandler = OnDocumentToBeDestroyed;

            acadApp.DocumentManager.DocumentActivated += _actionDocumentActivatedHandler;

            acadApp.DocumentManager.DocumentToBeDestroyed += (object sender, DocumentCollectionEventArgs e) =>
            {
                UnRegisterDocEvents();
                _actionDocumentToBeDestroyedHandler(sender, e);
            };

            Doc.CommandWillStart += (object sender, CommandEventArgs e) =>
            {
                if ((e?.GlobalCommandName ?? sender.ToString()) != CommandName
                        || Cancellation.IsCancellationRequested
                        || !CommandName.Contains(PluginSettings.Name.ToUpper())) return;
                try
                {
                    //Mouse.OverrideCursor = Cursors.Wait;

                    _actionCommandWillStartHandler(sender, e);

                    var inProcessList = Doc.GetInProcessList();
                    if (inProcessList.ContainsKey(CommandName))
                    {
                        var commandArgs = inProcessList[CommandName];
                        if (commandArgs != null)
                        {
                            commandArgs.CancellationToken = Cancellation.Token;
                            commandArgs.CancellationToken.Register(() =>
                                TokenCancelRegistred(CommandName, commandArgs));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Plugin.Logger.ErrorEx(ex);
                    //Mouse.OverrideCursor = null;
                }
            };

            Doc.CommandEnded += (object sender, CommandEventArgs e) =>
            {
                if ((e?.GlobalCommandName ?? sender.ToString()) != CommandName) return;
                try
                {
                    NotifyArgs notifyArgs = new NotifyArgs(NotifyStatus.Ready, "Ready");

                    if (Cancellation.IsCancellationRequested)
                    {
                        var inProcessList = Doc.GetInProcessList();

                        if (Cancellation.OperationCanceledException != null)
                        {
                            notifyArgs = new ErrorNotifyArgs(Cancellation.OperationCanceledException.Message);
                        }
                        else if (inProcessList.ContainsKey(CommandName))
                        {
                            ITaskArguments commandArgs = inProcessList[CommandName];
                            if (commandArgs?.CommandInfo != null && commandArgs.CommandInfo.Any(x => x.Key == NotifyStatus.Error))
                            {
                                notifyArgs = new NotifyArgs(NotifyStatus.Error, "Error",
                                    string.Join("\n", commandArgs.CommandInfo.Values.ToArray()));
                            }
                        }
                    }
                    else
                    {
                        _actionCommandEndedHandler(sender, e);
                    }

                    if (Doc.UserData.ContainsKey(CommandName))
                        Doc.UserData.Remove(CommandName);

                    Mouse.OverrideCursor = null;
                    Ed.WriteMessage($"\n{CommandNames.UserGroup} : {CommandName}...end");
                    Notifications.SendNotifyMessageAsync(notifyArgs);
                }
                catch (Exception ex)
                {
                    Plugin.Logger.ErrorEx(ex);
                    //Mouse.OverrideCursor = null;
                }
            };

            Doc.CommandCancelled += _actionCommandCancelledHandler;

            //CommandEventHandler handlerEnd = null;
            //handlerEnd = (sender, e) =>
            //{
            //    mdi.CommandWillStart -= handlerStart;
            //    mdi.CommandCancelled -= handlerEnd;
            //    mdi.CommandEnded -= handlerEnd;
            //    manualResetEvent.Set();
            //};

            //LispWillStartEventHandler handlerStart = (sender, e) => SendToExecute(cmd, cmdArg);
            //EventHandler handlerEnd = null;
            //handlerEnd = (sender, e) =>
            //{
            //    mdi.LispWillStart -= handlerStart;
            //    mdi.LispCancelled -= handlerEnd;
            //    mdi.LispEnded -= handlerEnd;
            //    manualResetEvent.Set();
            //};
            //mdi.LispWillStart += handlerStart;
            //mdi.LispEnded += handlerEnd;
            //mdi.LispCancelled += handlerEnd;
        }

        public void UnRegisterDocEvents()
        {
            if (_actionDocumentActivatedHandler != null) acadApp.DocumentManager.DocumentActivated -= _actionDocumentActivatedHandler;
            if (_actionDocumentToBeDestroyedHandler != null) acadApp.DocumentManager.DocumentToBeDestroyed -= _actionDocumentToBeDestroyedHandler;
            if (_actionCommandWillStartHandler != null) Doc.CommandWillStart -= _actionCommandWillStartHandler;
            if (_actionCommandEndedHandler != null) Doc.CommandEnded -= _actionCommandEndedHandler;
            if (_actionCommandCancelledHandler != null) Doc.CommandCancelled -= _actionCommandCancelledHandler;
        }

        #endregion <public methods>

        #region <private methods>

        private void TokenCancelRegistred(string commandName, ITaskArguments commandArgs)
        {
            var inProcessList = Doc.GetInProcessList();
            if (inProcessList.ContainsKey(commandName))
            {
                if (commandArgs.CancellationToken.IsCancellationRequested)
                {
                    var notifyArgs = new NotifyArgs(NotifyStatus.Error, "Cancel",
                        string.Join("\n", commandArgs.CommandInfo.ToArray()));

                    Notifications.SendNotifyMessageAsync(notifyArgs);
                }
            }
        }

        #endregion <private methods>


        #region <dispose>
        public void Dispose()
        {
            UnRegisterDocEvents();
        }
        #endregion <dispose>
    }
}
