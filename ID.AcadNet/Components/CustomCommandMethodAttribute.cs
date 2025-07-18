using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using ID.Infrastructure.Commands;
using Intellidesk.AcadNet.Common.Core;
using Intellidesk.AcadNet.Common.Extensions;
using Intellidesk.AcadNet.Common.Models;
using System;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;


namespace Intellidesk.AcadNet.Components
{
    public class CustomCommandMethodAttribute : Attribute, ICommandLineCallable, IDisposable
    {
        private CommandEventHandler actionCommandWillStart;
        private CommandEventHandler actionCommandEnded;
        private CommandEventHandler actionCommandCancelled;
        private DocumentCollectionEventHandler actionDocumentActivated;
        private DocumentCollectionEventHandler actionDocumentToBeDestroyed;

        public CommandCancellationTokenSource Cancellation { get; set; }

        public Document Doc => acadApp.DocumentManager.MdiActiveDocument;
        public Editor Ed => Doc.Editor;
        public Database Db => Doc.Database; //Db = HostApplicationServices.WorkingDatabase; Doc = Application.DocumentManager.GetDocument(Db); 


        public CustomCommandMethodAttribute(string globalName) { GroupName = globalName; }
        public CustomCommandMethodAttribute(string globalName, CommandFlags flags)
        {
            GlobalName = globalName;
            Flags = flags;

            actionCommandWillStart = CommandWillStart;
            actionCommandEnded = CommandEnded;
            actionCommandCancelled = CommandCancelled;
            actionDocumentActivated = OnDocumentActivated;
            actionDocumentToBeDestroyed = OnDocumentToBeDestroyed;

            acadApp.DocumentManager.DocumentActivated += actionDocumentActivated;
            acadApp.DocumentManager.DocumentToBeDestroyed += actionDocumentToBeDestroyed;

            acadApp.DocumentManager.MdiActiveDocument.CommandWillStart += actionCommandWillStart;
            acadApp.DocumentManager.MdiActiveDocument.CommandEnded += actionCommandEnded;
            acadApp.DocumentManager.MdiActiveDocument.CommandCancelled += actionCommandCancelled;

            Cancellation = new CommandCancellationTokenSource();
        }
        public CustomCommandMethodAttribute(string groupName, string globalName, CommandFlags flags)
            : this(globalName, flags)
        {
            GroupName = groupName;
        }
        public CustomCommandMethodAttribute(string groupName, string globalName, string localizedNameId, CommandFlags flags) { }
        public CustomCommandMethodAttribute(string groupName, string globalName, string localizedNameId, CommandFlags flags, Type contextMenuExtensionType) { }
        public CustomCommandMethodAttribute(string groupName, string globalName, string localizedNameId, CommandFlags flags, string helpTopic) { }
        public CustomCommandMethodAttribute(string groupName, string globalName, string localizedNameId, CommandFlags flags, Type contextMenuExtensionType, string helpFileName, string helpTopic) { }

        public string HelpTopic { get; }
        public string HelpFileName { get; }
        public Type ContextMenuExtensionType { get; }
        public CommandFlags Flags { get; }
        public string GroupName { get; }
        public string LocalizedNameId { get; }
        public string GlobalName { get; }

        public CommandArgs CommandArgs { get; private set; }

        public void CommandWillStart(object sender, CommandEventArgs e)
        {
            var commandName = e?.GlobalCommandName ?? GlobalName ?? "";
            CommandArgs = Doc.GetInProcessCommand<CommandArgs>(commandName);
            var groupName = GroupName == CommandNames.MainGroup ? GroupName : CommandNames.UserGroup;

            Ed.WriteMessage($"\n{CommandNames.UserGroup}: {commandName} working...");
            Notifications.DisplayNotifyMessageAsync("Working");
        }

        public void CommandEnded(object sender, CommandEventArgs e)
        {
            Ed.WriteMessage($"\n{CommandNames.UserGroup}: {e.GlobalCommandName}...ended");
            Notifications.DisplayNotifyMessageAsync("Ready");
        }

        public void CommandCancelled(object sender, CommandEventArgs e) { }
        public void OnDocumentActivated(object sender, DocumentCollectionEventArgs e) { }
        public void OnDocumentToBeDestroyed(object sender, DocumentCollectionEventArgs e)
        {
            Doc.CommandWillStart -= actionCommandWillStart;
            if (actionCommandEnded != null)
                Doc.CommandEnded -= actionCommandEnded;
            if (actionCommandCancelled != null)
                Doc.CommandCancelled -= actionCommandCancelled;
            if (actionDocumentActivated != null)
                acadApp.DocumentManager.DocumentActivated -= actionDocumentActivated;
            if (actionDocumentToBeDestroyed != null)
                acadApp.DocumentManager.DocumentToBeDestroyed -= actionDocumentToBeDestroyed;
        }

        public void Dispose()
        {
            acadApp.DocumentManager.DocumentActivated -= actionDocumentActivated;
            acadApp.DocumentManager.DocumentToBeDestroyed -= actionDocumentToBeDestroyed;
            acadApp.DocumentManager.MdiActiveDocument.CommandWillStart -= actionCommandWillStart;
            acadApp.DocumentManager.MdiActiveDocument.CommandEnded -= actionCommandEnded;
            acadApp.DocumentManager.MdiActiveDocument.CommandCancelled -= actionCommandCancelled;
        }
    }
}
