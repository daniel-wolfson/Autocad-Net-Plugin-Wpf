using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using ID.Infrastructure;
using ID.Infrastructure.Interfaces;
using Intellidesk.AcadNet.Common.Models;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Intellidesk.AcadNet.Common.Extensions
{
    public static class DocumentExtensions
    {
        public static Document Doc => acadApp.DocumentManager.MdiActiveDocument;
        public static Editor Ed => Doc.Editor;
        public static Database Db => Doc.Database;

        public static string GetCurrentSpace(this DocumentCollection docs)
        {
            short tilemode = (short)acadApp.GetSystemVariable("TILEMODE");
            return tilemode == 1 ? BlockTableRecord.ModelSpace : BlockTableRecord.PaperSpace;
        }

        public static ObjectId GetCurrentSpaceId(this DocumentCollection docs, Database db)
        {
            //var db = docs.MdiActiveDocument.Database;
            short tilemode = (short)acadApp.GetSystemVariable("TILEMODE");
            return tilemode == 1
                ? SymbolUtilityServices.GetBlockModelSpaceId(db)
                : SymbolUtilityServices.GetBlockPaperSpaceId(db);
        }

        public static void Save(this Document doc)
        {
            object acadDoc = doc.GetAcadDocument();            // change to .GetAcadDocument() for 2013
            acadDoc.GetType().InvokeMember("Save",
                BindingFlags.InvokeMethod, null, acadDoc, new object[0]);
        }

        /// <summary> Sent command to async execution </summary>
        public static void SendCommandToExecute(this Document doc, object sender, string command, object commandParameter = null)
        {
            var commandArgs = new CommandArgs(sender, command, commandParameter);

            if (!doc.UserData.ContainsKey(command))
                doc.UserData.Add(command, commandArgs);

            doc.SendStringToExecute(command + " ", true, false, false);
        }

        public static IDictionary<string, ITaskArguments> GetInProcessList(this Document doc)
        {
            return doc?.UserData?
                .Cast<DictionaryEntry>()
                .Where(x => x.Value is ITaskArguments)
                .ToDictionary(k => (string)k.Key, v => (ITaskArguments)v.Value)
                ?? new Dictionary<string, ITaskArguments>();
        }

        public static T GetInProcessCommand<T>(this Document doc, string command) where T : CommandArgs
        {
            return doc?.UserData?
                .Cast<DictionaryEntry>()
                .FirstOrDefault(x => x.Value is T && x.Key.ToString() == command).Value as T;
        }


        /// <summary> AddRegAppTableRecord </summary>
        public static void AddRegAppTableRecord(this Document doc, string regAppName)
        {
            var db = doc.Database ?? HostApplicationServices.WorkingDatabase;
            using (var tr = db.TransactionManager.StartTransaction())
            {
                using (doc.LockDocument())
                {
                    RegAppTable acRegAppTbl = (RegAppTable)tr.GetObject(db.RegAppTableId, OpenMode.ForRead);
                    if (!acRegAppTbl.Has(regAppName))
                    {
                        var acRegAppTblRec = new RegAppTableRecord { Name = regAppName };

                        acRegAppTbl.UpgradeOpen();
                        acRegAppTbl.Add(acRegAppTblRec);
                        tr.AddNewlyCreatedDBObject(acRegAppTblRec, true);
                        acRegAppTbl.DowngradeOpen();

                        IPluginSettings pluginSettings = Plugin.GetService<IPluginSettings>();
                        pluginSettings.IsRegAppTable = true;
                    }
                }
                tr.Commit();
            }
        }

        public static bool IsRegAppTableRecord(this Database db, string regAppName)
        {
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var rat = (RegAppTable)tr.GetObject(db.RegAppTableId, OpenMode.ForRead, false);
                return rat != null && rat.Has(regAppName);
            }
            //ObjectId id = SymbolUtilityServices.GetRegAppAcadId(db);
            //dynamic _db = HostApplicationServices.WorkingDatabase;
        }
    }
}