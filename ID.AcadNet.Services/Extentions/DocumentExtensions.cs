using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using ID.Infrastructure;
using ID.Infrastructure.Enums;
using ID.Infrastructure.General;
using ID.Infrastructure.Interfaces;
using ID.Infrastructure.Models;
using Intellidesk.AcadNet.Common.Core;
using Intellidesk.AcadNet.Common.Models;
using System;
using System.Collections;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Input;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Intellidesk.AcadNet.Services.Extentions
{
    public static class DocumentExtensions
    {
        private static Editor Ed => Doc.Editor;
        public static Database Db => HostApplicationServices.WorkingDatabase; //Doc.Database;
        private static Document Doc => acadApp.DocumentManager.MdiActiveDocument;
        private static IPluginSettings PluginSettings => Plugin.GetService<IPluginSettings>();

        public static void Open(this DocumentCollection docs, string fullPath, string fileName)
        {
            StringBuilder actionMessage = new StringBuilder();
            Notifications.SendNotifyMessageAsync(new NotifyArgs(NotifyStatus.Working));
            InfraManager.RunOnUIThread(() => { Mouse.OverrideCursor = Cursors.Wait; });

            SimpleActionResult simpleResult = Files.FindPath(PluginSettings.IncludeFolders.ToArray(), fullPath, fileName);
            var code = simpleResult.StatusCode;
            NotifyArgs notifyResult = new ReadyNotifyArgs();

            if (code == HttpStatusCode.Found)
            {
                fullPath = simpleResult.ActionResult.ToString();
                Document activeDocument = acadApp.DocumentManager.GetDocument(fullPath);
                if (activeDocument != null)
                {
                    acadApp.DocumentManager.MdiActiveDocument = activeDocument;
                    return;
                }

                var pluginSettings = Plugin.GetService<IPluginSettings>();
                pluginSettings.CurrentFolder = fullPath;

                InfraManager.RunOnUIThread((Action)(() =>
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(fullPath))
                            docs.Open(fullPath, false);

                        //acadApp.DocumentManager.ExecuteInApplicationContext(data =>
                        //{
                        //    Editor ed = acadApp.DocumentManager.MdiActiveDocument.Editor;
                        //    ed.WriteMessage($"{PluginSettings.Prompt} switch to Modelspace: {CommandNames.XSwitchToModelspace}");
                        //    acadApp.DocumentManager.CurrentDocument.UserData.XAdd(CommandNames.XDisplayPoint, data);
                        //    ed.CommandAsync("." + CommandNames.XSwitchToModelspace);
                        //}, null);

                        code = HttpStatusCode.OK;
                        actionMessage.Append("Command 'FindFile': File was been opened successfully.");
                    }
                    catch (Exception ex)
                    {
                        code = HttpStatusCode.InternalServerError;
                        actionMessage.Append($"Command 'FindFile': File {fullPath} was't opened because of {ex}");
                        Notifications.SendNotifyMessageAsync((NotifyArgs)new ErrorNotifyArgs((string)actionMessage.ToString()));
                    }
                }));
            }
            else
            {
                notifyResult = new ErrorNotifyArgs($"File '{fileName}' in folder '{fullPath}' not found");
            }

            Notifications.SendNotifyMessageAsync(notifyResult);
            InfraManager.RunOnUIThread(() => { Mouse.OverrideCursor = null; });
        }

        public static Document GetDocument(this DocumentCollection docs, string fileName)
        {
            return acadApp.DocumentManager.Cast<Document>().FirstOrDefault(x => x.Name.ToLower() == fileName.ToLower());
        }

        public static RasterImage AddRaster(this CommandContext context, string RasterFile, Point3d InsPoint, double Scale, bool ImageTransp, string ImageLayer)
        {
            Editor ed = context.Ed;
            RasterImage functionReturnValue = default(RasterImage);
            RasterImage RasterEnt = default(RasterImage);
            RasterImageDef RasterDef = default(RasterImageDef);
            ObjectId ImageDicID = default(ObjectId);
            ObjectId ImageDefID = default(ObjectId);
            DBDictionary ImageDic = default(DBDictionary);
            Matrix3d Matrix = default(Matrix3d);
            BlockTableRecord btr = default(BlockTableRecord);
            string RasterName = "";

            RasterName = RasterFile.Substring(RasterFile.LastIndexOf("\\") + 1);
            RasterName = RasterName.Substring(0, RasterName.IndexOf("."));

            functionReturnValue = null;
            if (context.Doc == null)
                return functionReturnValue;


            try
            {
                using (Transaction tr = context.Doc.TransactionManager.StartTransaction())
                {

                    RasterEnt = new RasterImage();
                    RasterEnt.Dispose();
                    // force loading of RasterImage.dbx module (needed for 2009 and earlier)

                    RasterDef = new RasterImageDef();
                    RasterDef.SourceFileName = RasterFile;
                    RasterDef.ActiveFileName = RasterFile;
                    RasterDef.Load();

                    ImageDicID = RasterImageDef.GetImageDictionary(context.Doc.Database);
                    if (ImageDicID.IsNull)
                    {
                        RasterImageDef.CreateImageDictionary(context.Doc.Database);
                        ImageDicID = RasterImageDef.GetImageDictionary(context.Doc.Database);
                    }
                    if (ImageDicID.IsNull)
                    {
                        context.Logger.Fatal("Could not create image dictionary");
                        //Exit Try
                    }
                    ImageDic = tr.GetObject(ImageDicID, OpenMode.ForWrite) as DBDictionary;
                    if (ImageDic == null)
                    {
                        context.Logger.Fatal("Could not open image dictionary");
                    }
                    else
                    {
                        if (ImageDic.Contains(RasterName))
                        {
                            context.Logger.Fatal("That image name is already in use");
                        }
                        else
                        {
                            ImageDic.UpgradeOpen();
                            ImageDefID = ImageDic.SetAt(RasterName, RasterDef);
                            tr.AddNewlyCreatedDBObject(RasterDef, true);
                            RasterEnt = new RasterImage();
                            RasterEnt.SetDatabaseDefaults(context.Doc.Database);
                            Matrix = new Matrix3d();
                            Matrix = Matrix3d.Scaling(Scale / RasterDef.Size.X, new Point3d(0, 0, 0));
                            RasterEnt.TransformBy(Matrix);
                            Matrix = Matrix3d.Displacement(new Vector3d(InsPoint.X, InsPoint.Y, InsPoint.Z));
                            RasterEnt.TransformBy(Matrix);
                            RasterEnt.ImageDefId = ImageDefID;
                            if (ImageTransp == true)
                            {
                                RasterEnt.ImageTransparency = true;
                            }
                            RasterEnt.Layer = ImageLayer;
                            btr = tr.GetObject(context.Doc.Database.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;
                            btr.AppendEntity(RasterEnt);
                            tr.AddNewlyCreatedDBObject(RasterEnt, true);
                            RasterEnt.AssociateRasterDef(RasterDef);
                            tr.Commit();
                        }
                    }

                }

                functionReturnValue = RasterEnt;


            }
            catch (Exception ex)
            {
                context.Logger.Error("Error inserting image " + RasterName, ex);

            }
            return functionReturnValue;

        }

        public static void ActiveWindowSetFocus()
        {
            Document activeDoc = acadApp.DocumentManager.MdiActiveDocument;
            dynamic acadDocObj = activeDoc.GetAcadDocument();
            activeDoc.Window.Focus();
            //acadDocObj.SendCommand("-Insert\nAutodesk\n");
        }

        public static void SendCommandSynchronously(this Document doc, string command)
        {
            var acadDoc = doc.GetAcadDocument();
            acadDoc.GetType().InvokeMember("SendCommand", System.Reflection.BindingFlags.InvokeMethod, null, acadDoc, new object[] { command + "\n" });
        }

        public static DocumentCollection.ExecutionResult ExecuteCommandAsync(this DocumentCollection documentManager, CommandArgs commandParameter)
        {
            return documentManager.ExecuteCommandAsync(commandParameter.CommandName, commandParameter);
        }

        public static DocumentCollection.ExecutionResult ExecuteCommandAsync(this DocumentCollection documentManager, string commandName, object commandParameter)
        {
            object commandArgs = null;
            if (commandParameter != null && commandParameter.GetType() != typeof(CommandArgs) && commandParameter.GetType() != typeof(CommandArgs[]))
                commandArgs = new CommandArgs(null, commandName, commandParameter);
            else
                commandArgs = commandParameter;

            return documentManager.ExecuteInCommandContextAsync(async data =>
            {
                if (acadApp.DocumentManager.CurrentDocument != null)
                {
                    //var x = (string[])data;
                    Editor ed = acadApp.DocumentManager.CurrentDocument.Editor;
                    acadApp.DocumentManager.CurrentDocument.UserData.XAdd(commandName.ToUpper(), data);
                    await ed.CommandAsync("." + commandName.ToUpper());
                }
            }, commandArgs);
        }

        public static void XAdd(this Hashtable userData, CommandArgs commandArgs)
        {
            //acadApp.DocumentManager.CurrentDocument.UserData
            if (userData.Contains(commandArgs.CommandName))
                userData.Remove(commandArgs.CommandName);
            userData.Add(commandArgs.CommandName, commandArgs);
        }

        public static void XAdd(this Hashtable userData, string commandName, object data)
        {
            if (userData.Contains(commandName))
                userData.Remove(commandName);

            if (data != null && (data.GetType() == typeof(CommandArgs) || data.GetType() == typeof(CommandArgs[])))
                userData.Add(commandName, data);
            else
                userData.Add(commandName, new CommandArgs(null, commandName, data));
        }
    }
}