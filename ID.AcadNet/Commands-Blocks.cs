using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using ID.Infrastructure.Commands;
using Intellidesk.AcadNet;
using Intellidesk.AcadNet.Common.Enums;
using Intellidesk.AcadNet.Common.Extensions;
using Intellidesk.AcadNet.Common.Internal;
using Intellidesk.AcadNet.Common.Models;
using Intellidesk.AcadNet.Common.Utils;
using System;
using System.IO;
using System.Linq;
using System.Windows.Input;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Exception = Autodesk.AutoCAD.Runtime.Exception;
using Files = ID.Infrastructure.Files;

[assembly: CommandClass(typeof(CommandBlocks))]
namespace Intellidesk.AcadNet
{
    /// <summary>
    /// This class is instantiated by AutoCAD for each document when
    /// a command is called by the user the first time in the context
    /// of a given document. In other words, non static data in this class
    /// is implicitly per-document!
    /// </summary>
    public class CommandBlocks : CommandLineBase
    {
        [CommandMethod(CommandNames.UserGroup, CommandNames.CopyAsBlock,
            CommandFlags.Transparent | CommandFlags.Redraw | CommandFlags.UsePickSet)]
        public void CopyAsBlock()
        {
            if (!Utils.IsModelSpace()) return;

            using (CommandContext context = new CommandContext(CommandNames.CopyAsBlock, "CopyAsBlock"))
            {
                Mouse.OverrideCursor = null;
                var psr = context.Ed.GetSelection(new PromptSelectionOptions
                { MessageForAdding = context.CommandLine.Current() + "Select objects: " });

                if (psr.Status == PromptStatus.OK)
                {
                    var ss = psr.Value;
                    if (ss != null)
                    {
                        PromptResult promptYesNo = Ed.XPromptYesNo("Select base point? : ", false);
                        if (promptYesNo.Status != PromptStatus.OK)
                        {
                            context.Cancel();
                            context.Clean();
                            context.CommandLine.Cancel();
                            return;
                        }

                        string fileName;
                        Point3d promptBasePoint;

                        if (promptYesNo.StringResult == "Yes")
                        {
                            PromptPointResult pointPromptResult =
                                Ed.XPromptForPoint("Base point: (press Esc for leftdown corner)");
                            promptBasePoint = pointPromptResult.Status == PromptStatus.OK
                                ? pointPromptResult.Value
                                : ss.GetObjectIds().GetExtents().MinPoint; //Geoms.ScreenExtents().MinPoint;
                            fileName = $"{context.PluginSettings.TempPath}tmp_blk_prompt_{Path.GetFileNameWithoutExtension(Doc.Name)}";
                        }
                        else
                        {
                            promptBasePoint = ss.GetObjectIds().GetExtents().MinPoint;
                            fileName = $"{context.PluginSettings.TempPath}tmp_blk_basepoint_{Path.GetFileNameWithoutExtension(Doc.Name)}";
                        }

                        if (File.Exists(fileName))
                            File.Delete(fileName);

                        using (context.Doc.LockDocument())
                        using (Database newDb = new Database(true, false))
                        {
                            ObjectIdCollection objIds = new ObjectIdCollection(ss.GetObjectIds());
                            context.Ed.WriteMessage("\nSelection is succcessful and has {0} entities.", objIds.Count);
                            context.Db.Wblock(newDb, objIds, promptBasePoint, DuplicateRecordCloning.Ignore);
                            context.Parameters.Add("BasePoint", promptBasePoint);
                            context.Parameters.Add("BlockName", fileName);
                            context.Parameters.Add("UCS", Doc.Editor.CurrentUserCoordinateSystem);

                            newDb.SaveAs(fileName, DwgVersion.Newest);
                            PluginSettings.BlockFilePath = fileName;
                            PluginSettings.BlockBasePoint = promptBasePoint.ToString().Replace("(", "").Replace(")", "");
                            PluginSettings.Save();
                        }

                        Ed.WriteMessage("\n{0} entities selected.\n", psr.Value.Count);
                        Ed.SetImpliedSelection(psr.Value.GetObjectIds());

                        Doc.SendStringToExecute("_.COPYCLIP ", true, false, true);
                        context.CommandLine.Cancel();
                    }
                    else
                        context.Ed.WriteMessage("\nSelection not valid!");
                    //context.Send("SAVEAS", context.BlockName);
                }

                context.Clean();
            }
        }

        private static DateTime _pasteAsBlockLastDateTime;

        [CommandMethod(CommandNames.UserGroup, CommandNames.PasteAsBlock, CommandFlags.Session)]
        public void PasteAsBlock()
        {
            if (!Utils.IsModelSpace()) return;

            //short tilemode = (short)acadApp.GetSystemVariable("TILEMODE");
            //if (tilemode == 0)
            //{
            //    Ed.WriteMessage(CommandNames.PasteAsBlock + " command for modelspace only!");
            //    Doc.SendStringToExecute("_.PASTECLIP ", true, false, true);
            //    return;
            //}

            using (var context = new CommandContext(CommandNames.PasteAsBlock, "Paste"))
            {
                try
                {
                    if (DateTime.Now.Subtract(_pasteAsBlockLastDateTime).TotalSeconds > PluginSettings.SaveAsAutoSaveTime)
                    {
                        acadApp.DocumentManager.MdiActiveDocument.Database
                            .SaveAs(context.Db.OriginalFileName.Replace("." + Files.GetExt(Db.OriginalFileName), ".bak"), DwgVersion.Current);
                        _pasteAsBlockLastDateTime = DateTime.Now;
                    }

                    BlockOptions blockOptions =
                        new BlockOptions
                        {
                            LayerName = "Temp",
                            Scale = 1,
                            JigPrompt = PluginSettings.BlockFilePath.Contains("_prompt")
                                ? eJigPrompt.PromptInsert : eJigPrompt.NoPrompt,
                            IsRegen = false,
                            IsBlockExistCreateNew = true
                        };

                    Point3d promptBasePoint = Point3d.Origin;
                    if (PluginSettings.BlockFilePath.Contains("_basepoint"))
                    {
                        var basePoint = PluginSettings.BlockBasePoint;
                        promptBasePoint = new Point3d(basePoint.Split(',').ToList().Select(Convert.ToDouble).ToArray());
                    }

                    var blockRefId = context.Db.InsertBlock(PluginSettings.BlockFilePath, promptBasePoint, blockOptions);

                    Entity ent = blockRefId.XCast<BlockReference>();
                    if (ent != null)
                    {
                        ent.Highlight();
                        context.Ed.WriteMessage(PluginSettings.Prompt + "block inserted successfully");
                    }
                }
                catch (Exception ex)
                {
                    if (string.IsNullOrEmpty(ex.Message))
                        Ed.WriteMessage(ex.Message);
                }

                context.Clean();
            }
        }

    }
}