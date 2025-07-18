using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Microsoft.AspNet.SignalR.Client;

[assembly: CommandClass(typeof(AcadSignalRClient.MyCommands))]

namespace AcadSignalRClient
{
    public class MyCommands
    {
        private static bool _trackCommand = false;
        private static HubConnection _hubConnection = null;
        private static IHubProxy _hubProxy = null;
        private const string HOST_URL = "http://localhost:8080";

        [CommandMethod("TrackCmd", CommandFlags.Session)]
        public static void RunMyCommand()
        {
            Document dwg = Application.DocumentManager.MdiActiveDocument;
            Editor ed = dwg.Editor;

            if (!_trackCommand)
            {
                try
                {
                    ed.WriteMessage(
                        "\nConnecting to SignalR server...");

                    //Attach event handler to DocumentCollection/Document
                    AttachDocumentEventHandlers();

                    //Setup SignalR client
                    bool connected = CreateHubConnection();
                    if (connected)
                    {
                        ed.WriteMessage(
                            "\nConnection to SignalR server established!");
                        _trackCommand = true;
                    }
                    else
                    {
                        _trackCommand = false;
                        throw new InvalidOperationException(
                            "cannot connect to SignalR server at " + HOST_URL);
                    } 
    
                    if (_trackCommand)
                    {
                        ed.WriteMessage("\nCommand tracking is on.");
                    }
                }
                catch (System.Exception ex)
                {
                    ed.WriteMessage("\nError: {0}", ex.Message);

                    _trackCommand = false;
                    _hubConnection = null;
                    _hubProxy = null;
                }
            }
            else
            {
                _hubConnection.Stop();
                _hubConnection.Dispose();
                _hubConnection = null;
                _hubProxy = null;

                ed.WriteMessage("\nCommand tracking is off.");
            }

            Autodesk.AutoCAD.Internal.Utils.PostCommandPrompt();
        }

        #region private methods

        private static bool CreateHubConnection()
        {
            _hubConnection = new HubConnection(HOST_URL);
            _hubProxy = _hubConnection.CreateHubProxy("MyHub");

            bool connected = true;

            _hubConnection.Start().ContinueWith(task =>
            {
                if (!task.IsCompleted)
                {
                    connected = false;
                }
            }).Wait();

            return connected;
        }

        private static void AttachDocumentEventHandlers()
        {
            Document dwg = Application.DocumentManager.MdiActiveDocument;
            dwg.CommandWillStart += dwg_CommandWillStart;

            Application.DocumentManager.DocumentCreated += 
                DocumentManager_DocumentCreated;
            Application.DocumentManager.DocumentToBeDestroyed += 
                DocumentManager_DocumentToBeDestroyed;
        }

        private static void DocumentManager_DocumentToBeDestroyed(
            object sender, DocumentCollectionEventArgs e)
        {
            e.Document.CommandWillStart -= dwg_CommandWillStart;
        }

        private static void DocumentManager_DocumentCreated(
            object sender, DocumentCollectionEventArgs e)
        {
            e.Document.CommandWillStart += dwg_CommandWillStart;
        }

        private static void dwg_CommandWillStart(
            object sender, CommandEventArgs e)
        {
            if (!_trackCommand) return;

            Document dwg = Application.DocumentManager.MdiActiveDocument;
            Editor ed = dwg.Editor;
            ed.WriteMessage("\nCommand {0} is about to start...", 
                e.GlobalCommandName.ToUpper());

            string dwgFileName = System.IO.Path.GetFileName(dwg.Name);

            SendMessageToSignalRServer(e.GlobalCommandName, dwgFileName);
        }

        private static void SendMessageToSignalRServer(
            string cmdName, string dwgFileName)
        {
            if (_hubConnection == null) return;

            string user = Environment.UserName;
            string computer = Environment.MachineName;

            SignalRCadData.AcadCommandTrack atrck = 
                new SignalRCadData.AcadCommandTrack()
                {
                    UserName = user,
                    ComputerName = computer,
                    CommandName = cmdName,
                    CmdExecTime = DateTime.Now,
                    DwgFileName=dwgFileName
                };

            //Call the hub method "RelayAcadMessage()" on 
            //SignalR server side
            _hubProxy.Invoke<SignalRCadData.AcadCommandTrack>(
                "RelayAcadMessage", atrck);
        }

        #endregion
    }
}
