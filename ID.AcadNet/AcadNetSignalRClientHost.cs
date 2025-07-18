using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using ID.Infrastructure;
using ID.Infrastructure.Commands;
using ID.Infrastructure.General;
using Intellidesk.AcadNet.Common.Models;
using Intellidesk.AcadNet.Services.Extentions;
using Intellidesk.Data.Models.Cad;
using Intellidesk.Data.Models.Map;
using Microsoft.AspNet.SignalR.Client;
using Serilog;
using System;
using System.Collections;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Exception = System.Exception;
using SynchronizationContext = System.Threading.SynchronizationContext;

namespace Intellidesk.AcadNet
{
    public class AcadNetSignalRClientHost : CommandLineBase
    {
        private static HubConnection _hubConnection;
        private static IHubProxy _hubProxy;
        private readonly SynchronizationContext _syncContext;
        private readonly object _accesslock = new Object();
        private int _reconnectAttemptCount;

        public bool IsConnected => _hubConnection != null;

        public AcadNetSignalRClientHost()
        {
            _syncContext = SynchronizationContext.Current;
        }

        #region <Static methods>

        static void OnDisconnectedRetry()
        {
            Console.WriteLine("connection closed");
            var t = _hubConnection.Start();

            bool result = false;
            t.ContinueWith(task =>
            {
                if (!task.IsFaulted)
                {
                    result = true;
                }
            }).Wait();

            if (!result)
            {
                OnDisconnectedRetry();
            }
        }

        private static void dwg_CommandWillStart(object sender, CommandEventArgs e)
        {
            if (_hubConnection != null && _hubConnection.State != ConnectionState.Connected) return;

            Document dwg = acadApp.DocumentManager.MdiActiveDocument;
            Editor ed = dwg.Editor;
            ed.WriteMessage("\nCommand {0} is about to start...", e.GlobalCommandName);

            string dwgFileName = Path.GetFileName(dwg.Name);
            SendToClient(e.GlobalCommandName, dwgFileName);
        }

        public static async void SendToClient(string commandName, object commandArgs)
        {
            bool passEnabled = true;

            if (_hubConnection == null || _hubConnection.State != ConnectionState.Connected)
                passEnabled = await AcadNetManager.HttpServerHostInitializer.ResetSignalRAsync();

            if (passEnabled)
            {
                AcadCommandArgs pass = new AcadCommandArgs
                {
                    ClientName = Environment.UserName,
                    ClientGroup = "acad",
                    ComputerName = Environment.MachineName,
                    CommandName = commandName,
                    CommandArgs = commandArgs,
                    ExecTime = DateTime.Now
                };

                _hubProxy?.Invoke<AcadCommandArgs>("SendFromAcad", pass);
            }
        }

        public static void UpdateDrawingAndClose(dynamic data)
        {
            string drawingPath = data as string;
            Document docToWorkOn = acadApp.DocumentManager.Open(drawingPath, false);

            while (acadApp.IsQuiescent == false)
            {
            }

            //using (docToWorkOn.LockDocument())
            //{
            //    docToWorkOn.Database.SaveAs(drawingPath, DwgVersion.Current);
            //}

            //docToWorkOn.CloseAndDiscard();
        }

        #endregion

        #region <Private methods>

        private void OnStateChanged(StateChange obj)
        {
            var doc = acadApp.DocumentManager.CurrentDocument;
            if (doc == null) return;

            if (obj.NewState == ConnectionState.Connected)
            {
                _reconnectAttemptCount = 0;
                Hashtable userData = doc.UserData;
                if (!userData.ContainsKey(CommandNames.XWriteMessage))
                    userData.Add(CommandNames.XWriteMessage, $"HubConnection StateChanged {obj.NewState}");
                acadApp.Idle -= AcadNetPlugin.OnApplicationIdle;
                acadApp.Idle += AcadNetPlugin.OnApplicationIdle;
                //doc.Editor.WriteMessageAsync($"HubConnection StateChanged {obj.NewState}({_reconnectAttemptCount})");
            }
            else if (obj.NewState == ConnectionState.Connecting)
            {
                _reconnectAttemptCount++;
                if (_reconnectAttemptCount == 5)
                    _hubConnection.StateChanged -= OnStateChanged;
            }
            else if (obj.NewState == ConnectionState.Disconnected || obj.NewState == ConnectionState.Reconnecting)
            {
                _hubConnection = null;
                lock (_accesslock)
                {
                    Hashtable userData = doc.UserData;
                    if (!userData.ContainsKey(CommandNames.XIdleOnHubDisconnected))
                        userData.Add(CommandNames.XIdleOnHubDisconnected, $"HubConnection StateChanged {obj.NewState}");
                    acadApp.Idle -= AcadNetPlugin.OnApplicationIdle;
                    acadApp.Idle += AcadNetPlugin.OnApplicationIdle;
                }
            }
        }

        public bool IsWorking(string commandName)
        {
            return acadApp.DocumentManager.MdiActiveDocument.UserData.ContainsKey(commandName.ToUpper());
        }

        private void OnHostMessage(string message)
        {
            if (!string.IsNullOrEmpty(message))
                Ed.WriteMessageAsync(message);
        }

        private void OnDisconnected()
        {
            // Small delay before retrying connection
            Thread.Sleep(1000);

            if (_reconnectAttemptCount <= 5)
                // Need to recreate connection
                InitConnectionAsync(t =>
                {
                    Ed?.WriteMessage($"{PluginSettings.Prompt} SignalR hub recreated, connected");
                }).ConfigureAwait(false);
        }

        #endregion

        #region <Public methods>

        public async Task<string> LoadHostAsync()
        {
            Ed.WriteMessage("Executing file ID.SignalRSelfHost.exe ");
            try
            {
                return await ProcessManager.RunAsync("ID.SignalRSelfHost", null,
                message =>
                {
                    //Ed.WriteMessageAsync(message);
                    Log.Logger.Error($"SignalRSelfhost message: {message}");
                },
                async complete =>
                {
                    var isInitConnection = await InitConnectionAsync(t =>
                       {
                           //if (!task.IsCompleted || _hubConnection.State != ConnectionState.Connected)
                           //{ _hubProxy.Invoke("AddConnection", "acad").Wait(); connected = false; }
                           Ed.WriteMessageAsync($"{PluginSettings.Prompt} SignalR hub connection state: {_hubConnection.State.ToString()}");
                       });

                    if (!isInitConnection)
                        Log.Logger.Error($"SignalR hub connection state: {_hubConnection.State.ToString()}");
                });
            }
            catch (Exception ex)
            {
                var message = ex?.InnerException?.Message ?? ex.Message;
                Ed.WriteMessageAsync(message);
                return message;
            }

            //.ContinueWith(task => {
            //    if (task.IsCompleted)
            //        InitConnectionAsync(t =>
            //        {
            //            //if (!task.IsCompleted || _hubConnection.State != ConnectionState.Connected)
            //            //{ _hubProxy.Invoke("AddConnection", "acad").Wait(); connected = false; }
            //            Ed.WriteMessage($"{PluginSettings.Prompt} SignalR hub connection state: {_hubConnection.State.ToString()}");
            //        });
            //    return task;
            //}).ContinueWith(task =>
            //{
            //    Ed.WriteMessage(task.IsCompleted ? " complete\n" : $"{PluginSettings.Prompt}Error: {task.Result}");
            //    Utils.PostCommandPrompt();
            //    IsBusy = false;
            //    ProcessManager.ResetTaskCompletionSource();
            //});
        }

        public async Task<bool> InitConnectionAsync(Action<Task> act)
        {
            if (_hubConnection != null)
                _hubConnection.Closed -= OnDisconnected; // Clean up previous connection

            try
            {
                Ed.WriteMessage($"{PluginSettings.Prompt}Connecting to SignalR server...\n");

                AppDomain.CurrentDomain.AssemblyResolve += AppAssemblyResolver.OnCurrentDomainOnAssemblyResolve;
                _hubConnection = new HubConnection("http://localhost:8080/", "intellidesk-clienttype=acad");
                AppDomain.CurrentDomain.AssemblyResolve -= AppAssemblyResolver.OnCurrentDomainOnAssemblyResolve;

                _hubProxy = _hubConnection.CreateHubProxy("AcadMapHub");

                // from client
                _hubProxy.On<string>("onMessage", message =>
                {
                    var doc = acadApp.DocumentManager.CurrentDocument;
                    if (doc == null) return;

                    if (!doc.UserData.ContainsKey(CommandNames.XIdleOnHubMessage))
                        doc.UserData.Add(CommandNames.XIdleOnHubMessage, message);

                    acadApp.Idle -= AcadNetPlugin.OnApplicationIdle;
                    acadApp.Idle += AcadNetPlugin.OnApplicationIdle;
                });

                // from client
                _hubProxy.On<MapMarkerElement>("onFileOpen", async args =>
                {
                    //_syncContext.Post(o => o, null);
                    await acadApp.DocumentManager.ExecuteInCommandContextAsync(async data =>
                        {
                            var x = (string[])data;
                            var doc = acadApp.DocumentManager.CurrentDocument;
                            Editor ed = doc.Editor;
                            ed.WriteMessage($"{PluginSettings.Prompt}send to AcadMapHub command 'SendMapFileOpen': {x[0]}, {x[1]}");

                            doc.UserData.XAdd(CommandNames.XFileOpen, data);
                            await ed.CommandAsync("." + CommandNames.XFileOpen);

                        }, new[] { args.FolderName, args.FileName });

                    //new[] { JsonConvert.DeserializeObject(args)}new string[] { args.FolderName, args.FileName }
                    //Documents.Open(args.FolderName, args.FileName);
                    //acadApp.DocumentManager.Open(fullPath, false);
                    // Application.DocumentManager.IsApplicationContext
                    //acadApp.Idle += AcadNetPlugin.Application_Idle;
                });

                // from client
                _hubProxy.On<MapMarkerElement>("onDisplayPoint", async args =>
                {
                    if (args != null)
                    {
                        var doc = acadApp.DocumentManager.CurrentDocument;
                        if (LayoutManager.Current.CurrentLayout != "Model")
                            doc.SendStringToExecute("tilemode 1 ", true, false, false);

                        doc.SendStringToExecute("_mspace ", true, false, false);

                        await acadApp.DocumentManager.ExecuteInCommandContextAsync(async data =>
                        {
                            var x = (string[])data;
                            Editor ed = doc.Editor;
                            ed.WriteMessage(
                                $"{PluginSettings.Prompt}sent from AcadMapHub the command 'onDisplayPoint': {x[0]}, {x[1]}");

                            doc.UserData.XAdd(CommandNames.XDisplayPoint, data);
                            await ed.CommandAsync("." + CommandNames.XDisplayPoint);

                        }, new[] { args.Latitude.ToString(), args.Longitude.ToString() });
                    }
                });

                _hubConnection.Closed += OnDisconnected;

                _hubConnection.StateChanged += OnStateChanged;

                _hubConnection.Start().ContinueWith(act).Wait();
            }
            catch (Exception ex)
            {
                _hubConnection = null;
                _hubProxy = null;
                Log.Logger.Error(ex?.InnerException?.Message ?? ex.Message);
            }
            finally
            {
                if (_hubConnection == null)
                {
                    Hashtable userData = acadApp.DocumentManager.CurrentDocument.UserData;
                    if (!userData.ContainsKey(CommandNames.XIdleOnHubDisconnected))
                        userData.Add(CommandNames.XIdleOnHubDisconnected, "HubConnection StateChanged failed");
                }
            }

            return await Task.FromResult(_hubConnection == null);
        }

        public void Terminate()
        {
            _hubConnection.Closed -= OnDisconnected;
            _hubConnection?.Stop();
            _hubConnection?.Dispose();
            _hubConnection = null;
            _hubProxy = null;
        }

        #endregion
    }
}
