//Tasks: Intel
using Autodesk.AutoCAD.DatabaseServices;
using ID.Infrastructure.Interfaces;
using Intellidesk.AcadNet.Common.Models;
using Intellidesk.AcadNet.Services;
using Intellidesk.Common.Enums;
using Intellidesk.Data.General;
using System.Collections.Generic;

//using Layout = Intellidesk.AcadNet.Data.Models.Intel.Layout;
//using Rule = Intellidesk.AcadNet.Data.Models.Intel.Rule;

namespace Intellidesk.AcadNet
{
    public class CommandIntelOld : CommandLineBase
    {
        #region "Task: FSA APPLY"

        /// <summary> Run Fsa Parser </summary>
        //[CommandMethod("Partner", "PARTNERFSAAPPLY", CommandFlags.UsePickSet)]
        public void ParserStart()
        {
            //if (MainViewModel == null) MainViewModel = ComposeViewModel();

            //ConfigManager.ChainDistance = MainViewModel.CurrentUserSetting.ChainDistance;

            //var taskArgs = new TaskArguments
            //{
            //    IsTimerOn = true,
            //    DataSource = SelectManager.GetImplied()
            //                    .XGetObjects(new ActionArguments
            //                    {
            //                        FilterTypesOn = Rule.LsdsTypeFilterOn, //FilterAttributesOn = Rule.LsdsAttributePatternOn,
            //                    })
            //};
            //if (ToolsManager.Doc.Name.ToLower() == MainViewModel.CurrentLayout.CADFileName.ToLower()
            //     && MainViewModel.CurrentLayout.FSA && ((List<ObjectId>)taskArgs.DataSource).Count >= 100)
            //{
            //    var config = new TaskDialogOptions
            //    {
            //        Title = ProjectManager.Name + " new task: Apply FSA",
            //        MainInstruction = "The FSA has already been applyed!",
            //        MainIcon = VistaTaskDialogIcon.Warning,
            //        Content = "Do you want to apply again?",
            //        CustomButtons = new[] { "&Apply", "&Close" },
            //    };
            //    var result = TaskDialog.Show(config);
            //    if (result.CustomButtonResult == 1)
            //        return;
            //}
            //TaskDialogParserShow(taskArgs);
        }

        public void ParserDynamicStart()
        {
            //if (MainViewModel == null) MainViewModel = ComposeViewModel();

            //ConfigManager.ChainDistance = MainViewModel.CurrentUserSetting.ChainDistance;
            //var taskArgs = new TaskArguments
            //{
            //    IsTimerOn = true,
            //    DataSource = SelectManager.GetImplied()
            //                    .Cast<dynamic>().ToList()
            //                    .DynamicXGetObjects(new ActionArguments
            //                    {
            //                        FilterTypesOn = Rule.LsdsTypeFilterOn,
            //                        FilterAttributesOn = Rule.LsdsAttributePatternOn
            //                    })
            //};

            //if (ToolsManager.Doc.Name.ToLower() == MainViewModel.CurrentLayout.CADFileName.ToLower()
            //     && MainViewModel.CurrentLayout.FSA && ((List<ObjectId>)taskArgs.DataSource).Count >= 100)
            //{
            //    var config = new TaskDialogOptions
            //    {
            //        Title = ProjectManager.Name + " new task: Apply FSA",
            //        MainInstruction = "The FSA has already been applyed!",
            //        MainIcon = VistaTaskDialogIcon.Warning,
            //        Content = "Do you want to apply again?",
            //        CustomButtons = new[] { "&Apply", "&Close" },
            //    };
            //    var result = TaskDialog.Show(config);
            //    if (result.CustomButtonResult == 1)
            //        return;
            //}

            //TaskDialogParserShow(taskArgs);
        }

        /// <summary> Show Parser task dialog </summary>
        public void TaskDialogParserShow(TaskArguments taskArgs)
        {
            //Rule.LsdsParserMode = parserMode;
            //var item = MainViewModel.LayoutItems.FirstOrDefault(x => x.FindLayoutFullPath() == Application.DocumentManager.MdiActiveDocument.Name.ToLower());
            //var titleInstruction = String.Format("Layout: {0}\n",
            //    (item == null) ? Application.DocumentManager.MdiActiveDocument.Name.ToLower() : item.LayoutName);

            ////if (item != null)
            ////{
            ////    var grd = (DataGrid)((LayoutView)UIManager.PaletteViewCurrent).FindName("DataGrid");
            ////    if (grd != null) 
            ////    { 
            ////        grd.SelectedItem = item;
            ////        grd.Focus();
            ////    }
            ////}

            //var config = new TaskDialogOptions
            //{
            //    //AllowDialogCancellation = true,
            //    Callback = TaskDialogParserCallBack,
            //    CallbackData = taskArgs,
            //    CanBeMinimized = false,
            //    Content = "Message: ...",
            //    CustomButtons = new[] { "&Apply", "&Close" },
            //    EnableCallbackTimer = true,
            //    ExpandedInfo = "Rules detail..."
            //        + String.Join("; ", MainViewModel.RuleItems.Select(x => String.Format("\n{0} : {1}", x.Layout.LayoutName, String.Join(",", x.LayerPatternOn))))
            //        + String.Format("\n\nUserSettings... Chain Distance = {0}", MainViewModel.CurrentUserSetting.ChainDistance),
            //    FooterText = "for additional information: <a href=\"http://www.intel.com/lsds\">Lsds help</a> ," +
            //                 "<a href=\"" + ProjectManager.UserSettingsPath + "Errors.xml" + "\"" + ">Lsds errors</a>",
            //    FooterIcon = VistaTaskDialogIcon.Information,
            //    Handle = Application.MainWindow.Handle,
            //    MainIcon = VistaTaskDialogIcon.Shield,
            //    MainInstruction = String.Format("{0}Task: Apply FSA to {1} items.", titleInstruction, taskArgs.ProgressLimit),
            //    ShowProgressBar = true,
            //    Title = ProjectManager.Name + " new task:",
            //    Width = 200,
            //    VerificationText = "Use Color ByLayer/ByDefault",
            //    VerificationByDefault = MainViewModel.CurrentUserSetting.IsColorMode
            //};

            //// Mute mode (supress all messages from autocad command line) C:\\Program Files\\Autodesk\\ApplicationPlugins\\LsdsPlugin2013.bundle\\Contents\\Win64\\
            //using (new SysVarOverride("NOMUTT", 1))
            //{
            //    TaskDialog.Show(config);
            //}
        }

        /// <summary> The signature of the callback that recieves notificaitons from a Task Dialog. </summary>
        //private bool TaskDialogParserCallBack(IActiveTaskDialog dialog, VistaTaskDialogNotificationArgs args, object callbackArgs)
        //{
        //    var taskArgs = (TaskArguments)callbackArgs;

        //    // Regulate a dialog closing
        //    var result = false;

        //    // Task dialog event notification arguments
        //    //switch (args.Notification)
        //    //{
        //    //    case VistaTaskDialogNotification.Created:

        //    //        //set up dialog process limits
        //    //        dialog.SetProgressBarRange(0, (short)taskArgs.ProgressLimit);

        //    //        // Task process argument initilizing
        //    //        taskArgs.ActionPool = new Dictionary<string, Func<ITaskArgs, bool>> { { "Applying ... ", OnParserAction } };
        //    //        taskArgs.ActionCompleted = OnParserCompleted;
        //    //        taskArgs.Context = null;
        //    //        taskArgs.Content = "Applying ... ";
        //    //        taskArgs.Dialog = dialog;
        //    //        taskArgs.ExpandedInfo = "Actions: ...";
        //    //        taskArgs.ProgressStep = 1;
        //    //        taskArgs.Status = StatusOptions.None;

        //    //        Worker = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };
        //    //        Worker.DoWork += BgDoWork;
        //    //        Worker.ProgressChanged += BgProgressChanged;
        //    //        Worker.RunWorkerCompleted += BgRunWorkerCompleted;
        //    //        result = true;
        //    //        break;

        //    //    case VistaTaskDialogNotification.VerificationClicked:
        //    //        MainViewModel.CurrentUserSetting.IsColorMode = args.VerificationFlagChecked;
        //    //        //MainViewModel.CurrentUserSetting.Save();
        //    //        taskArgs.IsVerifyCheckBox = args.VerificationFlagChecked;
        //    //        result = true;
        //    //        break;

        //    //    case VistaTaskDialogNotification.ButtonClicked:

        //    //        switch (args.ButtonId)
        //    //        {
        //    //            case 500: //Button the "Parse"
        //    //                MemoryClear();
        //    //                taskArgs.Status = StatusOptions.Start;

        //    //                // Start the asynchronous operation.
        //    //                if (Worker != null && !Worker.IsBusy)
        //    //                    Worker.RunWorkerAsync(taskArgs);

        //    //                result = true;
        //    //                CurrentTaskArgs = taskArgs;
        //    //                break;

        //    //            // Cancel the asynchronous operation.
        //    //            case 501: //Button the "Close"

        //    //                if (Worker != null && Worker.IsBusy)
        //    //                {
        //    //                    Worker.CancelAsync();
        //    //                    Lsds2Context.Clear();
        //    //                    MemoryClear();

        //    //                    if (TaskThread != null)
        //    //                    {
        //    //                        TaskThread.Abort();
        //    //                        // Use the Join method to block the current thread 
        //    //                        // until the object's thread terminates.
        //    //                        TaskThread.Join();
        //    //                    }
        //    //                }

        //    //                if (taskArgs.Status == StatusOptions.Cancel || taskArgs.Status == StatusOptions.None)
        //    //                {
        //    //                    // Request that the worker thread stop itself 
        //    //                    if (TaskThread != null)
        //    //                    {
        //    //                        TaskThread.Abort();
        //    //                        // Use the Join method to block the current thread 
        //    //                        // until the object's thread terminates.
        //    //                        TaskThread.Join();
        //    //                    }

        //    //                    // Autocad commnd for drawing regeneration and ribbon refresh
        //    //                    CmdManager.SendToExecute("PARTNERREGEN ");
        //    //                }
        //    //                //else
        //    //                //{
        //    //                //    //TRUE: reset tick count (args.TimerTickCount) and prevent dialog from closing
        //    //                //    result = true;
        //    //                //    // Clear all temporary accumulate tables
        //    //                //    Lsds2Context.Clear();
        //    //                //}

        //    //                taskArgs.Status = StatusOptions.Cancel;
        //    //                break;
        //    //        }
        //    //        break;

        //    //    // Processing of timer
        //    //    case VistaTaskDialogNotification.Timer:
        //    //        if (taskArgs.Status != StatusOptions.None && taskArgs.Status != StatusOptions.Start)
        //    //            result = taskArgs.DisplayStatus(args.TimerTickCount);
        //    //        else
        //    //            result = true;
        //    //        break;

        //    //    // Processing of event the Hyperlink click
        //    //    case VistaTaskDialogNotification.HyperlinkClicked:
        //    //        Process.Start(args.Hyperlink);
        //    //        break;
        //    //}
        //    return result;
        //}

        /// <summary> Main parse action </summary>
        public bool OnParserAction(ITaskArgs taskArgs)
        {
            var result = true;
            try
            {
                //// Initialize
                //Lsds2Context.Clear(); // Clear to all temporary accumulate tables
                //LayerManager.Init(MainViewModel.RuleItems.Cast<IRule>().ToList()); // Initialize layers by rules
                //Rule.RuleIndexesReset(); // Reset for all common options of rules

                //var listItems = new ObservableCollection<IRule>();
                //var ruleItems = MainViewModel.RuleItems.Cast<IRule>().ToList();
                //ruleItems.ForEach(listItems.Add);
                //// Read the autocad entity objects with ActionArguments.

                //var readArgs = new ActionArguments { Rules = listItems };

                //try
                //{
                //    foreach (var rule in readArgs.Rules.Reverse())
                //    {
                //        var db = HostApplicationServices.WorkingDatabase;
                //        List<ObjectId> ids;
                //        using (var tr = db.TransactionManager.StartTransaction())
                //        {
                //            ids = ((List<ObjectId>)taskArgs.DataSource).Where(id => id.XGetLayer(tr).XIsMatchFor(rule.LayerPatternOn)).ToList();
                //        }

                //        foreach (var objectId in ids)
                //        {
                //            taskArgs.ActionResult = OnParseBlockItem(objectId, readArgs, taskArgs, rule.LayerTypeId);
                //            if (Worker.CancellationPending)
                //            {
                //                result = false;
                //                break;
                //            }
                //            Worker.ReportProgress(++taskArgs.ProgressIndex, taskArgs);
                //        }
                //        //Parallel.ForEach((List<dynamic>)taskArgs.DataSource, (objectId, state) =>
                //        //    {
                //        //        taskArgs.ActionResult = OnDynamicParseBlock(objectId, readArgs, taskArgs);
                //        //        if (Worker.CancellationPending)
                //        //        {
                //        //            result = false;
                //        //            state.Stop();
                //        //        }
                //        //        Worker.ReportProgress(++taskArgs.ProgressIndex, taskArgs);
                //        //    });
                //    }
                //    taskArgs.ProgressIndex = ((List<ObjectId>)taskArgs.DataSource).Count();
                //}
                //catch (System.Exception ex)
                //{
                //    result = false;
                //    taskArgs.ExpandedInfo = ex.Message;
                //    Lsds2Context.Clear();
                //}
                //finally
                //{
                //    Worker.ReportProgress(taskArgs.ProgressIndex, taskArgs);
                //}
            }
            catch (System.Exception ex)
            {
                // add error message into dialog
                taskArgs.ExpandedInfo = ex.InnerException.Message;

                // dialog status is error
                taskArgs.Status = StatusOptions.Error;
            }
            return result;
        }

        /// <summary> Implementation for event of function "OnParseBlock" when blocks are read </summary>
        public bool OnParseBlockItem(ObjectId sender, ActionArguments readArgs, ITaskArgs taskArgs, short frameTypeId)
        {
            var result = true;

            var db = HostApplicationServices.WorkingDatabase;
            //using (readArgs.Trans = db.TransactionManager.StartTransaction())
            //{
            //    // Get block reference from current object
            //    var br = (BlockReference)readArgs.Trans.GetObject(sender, OpenMode.ForRead);

            //    if (!br.Visible)
            //        taskArgs.ErrorInfo.Add(String.Format("\nItem: {0}, not visible", br.Name));

            //    //-var pointPosition = br.Position; if (basepoint != null) pointPosition = pointPosition + ((Point3d)basepoint).GetAsVector();
            //    readArgs.Position = br.Position;

            //    // groups of objects for selecting together
            //    var groupObjectIds = new List<ObjectId>();

            //    var frameVertices = new List<Point3dCollection>();

            //    foreach (var rule in readArgs.Rules)
            //    {
            //        //current draw colorIndex
            //        var ruleColorIndex = 0;

            //        readArgs.LayerPatternOn = rule.LayerPatternOn;
            //        readArgs.FilterTypesOn = rule.TypeFilterOn;
            //        readArgs.IsParentFilterTypes = rule.isTypeFilterParent;

            //        var objectIds = br.XGetObjects(readArgs);
            //        //var objectIds = br.XGetObjects(readArgs.Trans, rule.TypeFilterOn, rule.isTypeFilterParent, rule.LayerPatternOn);

            //        //entities without area
            //        var noAreaObjects = new List<Entity>();

            //        //iteration by all entities
            //        foreach (var id in objectIds)
            //        {
            //            var ent = (Entity)readArgs.Trans.GetObject(id, OpenMode.ForRead);
            //            if (ent.Color.ColorMethod != ColorMethod.ByLayer || ent.Bounds == null) continue;

            //            //set colorIndex by Layer
            //            if (MainViewModel.CurrentUserSetting.IsColorMode)
            //                ruleColorIndex = ((LayerTableRecord)
            //                                  readArgs.Trans.GetObject((ent.Layer == "0")
            //                                                   ? br.LayerId
            //                                                   : ent.LayerId, OpenMode.ForRead)).Color.ColorIndex;
            //            else
            //                ruleColorIndex = MainViewModel.CurrentUserSetting.ColorIndex;

            //            //Determines whether an entity is in the type entity list.
            //            if (ent.XIsMacthFor(typeof(Line), typeof(Arc), typeof(Ellipse)) |
            //                (ent.GetType() == typeof(Polyline) && ((Polyline)ent).Area <= 0))
            //                noAreaObjects.Add(ent);
            //            else
            //                //<Polyline>, <Circle>, ... converting to points-vertices
            //                if (ent.Bounds.HasValue)
            //                    frameVertices.Add(ent.XGetPointsOrthogonal());
            //        }

            //        //Makes from noarea objects chain's elements, and each of Multiple chains troop to one frame, 
            //        //and others Single chains troops to group and only then into frame 
            //        if (noAreaObjects.Count > 0)
            //        {
            //            var chains = new List<Extents3d>();
            //            var singleChainMembers = new Point3dCollection();

            //            //Convert (double-ended) entity members to points, and Removing(Distinct) the repeated points 
            //            var tempChainMembers = noAreaObjects.XParseToChainsPoints().Distinct().ToList();

            //            //Sort tempChainMembers for improve next cycle processing
            //            tempChainMembers.Sort((points1, points2) => (points2.Count).CompareTo(points1.Count));
            //            foreach (var chain in tempChainMembers)
            //            {
            //                if (chain.Count > 2)
            //                    //Multiple chain is it with more than two vertices of chain
            //                    chains.Add(chain.XGetPointsExtents());
            //                else
            //                    //Simple chain is it with less or equal than to two vertices of chain
            //                    chain.OfType<Point3d>().ToList().ForEach(x => singleChainMembers.Add(x));
            //            }

            //            if (singleChainMembers.Count != 0)
            //                //join all single, i.e. define Extents for chain members and add it to Chains
            //                chains.Add(singleChainMembers.XGetPointsExtents());

            //            if (chains.Count != 0)
            //                //add vertices
            //                chains.ForEach(ext3D => frameVertices.Add(ext3D.XGetPoints()));
            //        }

            //        if (frameVertices.Count == 0) continue;

            //        //PolylinesAction(tr, br, groupObjectIds, frameVertices, readArgs, rule, ruleColorIndex);
            //        frameVertices.XSortByArea();
            //        frameVertices.XForEachEraseNested();

            //        var frames = new List<Entity>();

            //        //creating and drawing polylines by vertices
            //        frameVertices.ForEach
            //            (points => frames.Add(
            //                new Polyline
            //                {
            //                    Layer = rule.LayerDestination,
            //                    ColorIndex = ruleColorIndex,
            //                    LinetypeScale = rule.LineTypeScale,
            //                    Linetype = rule.LineType,
            //                    Closed = true
            //                }
            //                .XInit(points).XAddNew())
            //            );

            //        //if (readArgs.Rules.IndexOf(rule) == 0)
            //        DbSetsPreSave(br, frames, rule, frameTypeId);

            //        // LsdsParserMode: FSA(default), Detail, Outline
            //        switch (Rule.LsdsParserMode)
            //        {
            //            case LsdsParserMode.Default:
            //                groupObjectIds.AddRange(frames.Select(x => x.ObjectId).ToArray());
            //                break;
            //            case LsdsParserMode.Detail:
            //                groupObjectIds.AddRange(frames.Select(x => x.ObjectId).ToArray());
            //                break;

            //            case LsdsParserMode.Outline:
            //                var outline = (new Solid3d
            //                {
            //                    Layer = rule.LayerDestination,
            //                    ColorIndex = ruleColorIndex,
            //                    LinetypeScale = rule.LineTypeScale,
            //                    Linetype = rule.LineType,
            //                })
            //                .XInit(frames, BooleanOperationType.BoolUnite).XAddNew();
            //                groupObjectIds.Add(outline.ObjectId);
            //                break;
            //        }
            //        frames.Clear();

            //        if (groupObjectIds.Count > 0 && !Worker.CancellationPending)
            //        {
            //            groupObjectIds.XMoveTo(readArgs.Trans, readArgs.Position);
            //            groupObjectIds.XScale(readArgs.Trans, br.ScaleFactors.X, readArgs.Position);
            //            groupObjectIds.XRotate(readArgs.Trans, br.Rotation, readArgs.Position);

            //            var grp =
            //                new Group(
            //                    String.Format("Group{0}_{1}", Rule.LsdsGroupIndexCurrent++, br.Name.Replace("*", "_")), true);
            //            grp.XAddGroup(readArgs.Trans, new ObjectIdCollection(groupObjectIds.ToArray()));
            //        }
            //        groupObjectIds.Clear();

            //        if (Worker.CancellationPending)
            //        {
            //            result = false;
            //            break;
            //        }
            //        frameVertices.Clear();
            //    }
            //    readArgs.Trans.Commit();
            //}
            return result;
        }

        /// <summary>  Prepare DbSets for following saving to DB </summary>
        public static void DbSetsPreSave(BlockReference br, List<Entity> frames, IRule rule, short frameTypeId, Transaction tr = null)
        {
            //// define name of block
            //var brName = br.IsDynamicBlock ? br.XGetTrueName(tr) : br.Name;

            //// all attributes of block
            //var brAttrs = (tr != null) ? br.XGetAttributes(tr).ToList() : br.DynamicXGetAttributes().ToList();

            //// Attributes for create the 'double records items' by conditions: ENTITY_CODE" || "ENTITY_CODES"
            //var brItemDoubleAttrs = brAttrs.Where(x => x.Tag == "ENTITY_CODE" || x.Tag == "ENTITY_CODES").ToList();

            //// Attributes for create the 'double records items' by table Lsds_Config
            //var brItemConfigAttrs = brAttrs
            //    .Where(x => Rule.LsdsAttributePatternOn.Contains(x.Tag) && x.Tag != "ENTITY_CODE" && x.Tag != "ENTITY_CODES").ToList();

            //if (brItemDoubleAttrs.Any())
            //    for (byte i = 0; i < brItemDoubleAttrs.Count(); i++)
            //    {
            //        DbSetsPreSave(br, brName, brAttrs, brItemDoubleAttrs, frames, rule, rule.LayerTypeId, i);
            //    }
            //else if (brItemConfigAttrs.Any())

            //    for (byte i = 0; i < brItemConfigAttrs.Count(); i++)
            //    {
            //        DbSetsPreSave(br, brName, brAttrs, brItemConfigAttrs, frames, rule, rule.LayerTypeId, i);
            //    }
            //else
            //{
            //    DbSetsPreSave(br, brName, brAttrs, null, frames, rule, frameTypeId, 0);
            //}
        }

        /// <summary>  Prepare DbSets for following saving to DB </summary>
        private static void DbSetsPreSave(BlockReference br, string brName, List<AttributeReference> brAttrs,
                                          List<AttributeReference> configAttrs, List<Entity> frames, IRule rule, short frameTypeId, byte i)
        {
            //bool isItemFound; AttributeReference attr;
            //if (configAttrs != null)
            //{
            //    attr = configAttrs[i];
            //    isItemFound = Lsds2Context.LoItems.Any(x => x.ItemHandle == br.Handle.ToString() && x.ItemName == attr.TextString);
            //}
            //else
            //{
            //    attr = null;
            //    isItemFound = Lsds2Context.LoItems.Any(x => x.ItemHandle == br.Handle.ToString());
            //}

            //if (!isItemFound)
            //{
            //    if (!Lsds2Context.LoBlocks.Any(x => x.BlockHandle == br.Handle.ToString() && x.BlockName == brName))
            //    {
            //        Lsds2Context.LoBlocks.Add(new LO_Block
            //        {
            //            BlockIndex = ++Rule.LsdsBlockIndexCurrent,
            //            BlockHandle = br.Handle.ToString(),
            //            BlockName = brName,
            //            LayoutID = MainViewModel.CurrentLayout.LayoutID,
            //            BlockXrefName = br.Layer.Contains("$0$") ? br.Layer.Substring(0, br.Layer.LastIndexOf("$0$", StringComparison.Ordinal)) : "NULL"
            //        });

            //        Rule.LsdsBlockAttributeIndexCurrent = 0;
            //        foreach (var at in brAttrs)
            //        {
            //            Lsds2Context.LoBlockAttributes.Add(new LO_BlockAttribute
            //            {
            //                BlockAttributeIndex = ++Rule.LsdsBlockAttributeIndexCurrent,
            //                BlockIndex = Rule.LsdsBlockIndexCurrent,
            //                BlockAttributeName = at.Tag,
            //                BlockAttributeValue = at.TextString,
            //                LayoutID = MainViewModel.CurrentLayout.LayoutID
            //            });
            //        }
            //    }

            //    int itemIndex; string itemName, itemAttributeName; List<AttributeReference> itemAttrs;

            //    if (configAttrs != null)
            //    {
            //        Rule.LsdsItemAttributeIndexCurrent = (byte)(i + 1);
            //        itemIndex = (i == 0) ? ++Rule.LsdsItemIndexCurrent : Rule.LsdsItemIndexCurrent;
            //        itemName = attr.TextString;
            //        itemAttributeName = attr.Tag;
            //        itemAttrs = brAttrs.Where(x => !configAttrs.Select(t => t.Tag).Contains(x.Tag)).ToList();
            //        //frameTypeId = rule.LayerTypeId;
            //    }
            //    else
            //    {
            //        Rule.LsdsItemAttributeIndexCurrent = 0;
            //        itemIndex = ++Rule.LsdsItemIndexCurrent;
            //        itemName = brName;
            //        itemAttributeName = null;
            //        itemAttrs = brAttrs;
            //    }

            //    var item = new LO_Item
            //    {
            //        BlockIndex = Rule.LsdsBlockIndexCurrent,
            //        BlockName = brName,
            //        ItemHandle = br.Handle.ToString(),
            //        ItemIndex = itemIndex,
            //        ItemName = itemName,
            //        ItemAttributeName = itemAttributeName,
            //        ItemAttributeIndex = Rule.LsdsItemAttributeIndexCurrent,
            //        LayerName = br.Layer,
            //        LayoutID = MainViewModel.CurrentLayout.LayoutID,
            //        Rotation = GeomManager.RadianToDegree(br.Rotation),
            //        Xpos = br.Position.X,
            //        Ypos = br.Position.Y,
            //        Zpos = br.Position.Z,
            //        Xscale = Math.Round(br.ScaleFactors.X, 4),
            //        Yscale = Math.Round(br.ScaleFactors.Y, 4),
            //        Zscale = Math.Round(br.ScaleFactors.Z, 4),
            //        XrefName = br.Layer.Contains("$0$") ? br.Layer.Substring(0, br.Layer.LastIndexOf("$0$", StringComparison.Ordinal)) : "NULL"
            //    };
            //    Lsds2Context.LoItems.Add(item);

            //    // add special attribute as first
            //    if (configAttrs != null)
            //        Lsds2Context.LoItemAttributes.Add(new LO_ItemAttribute
            //        {
            //            ItemIndex = Rule.LsdsItemIndexCurrent,
            //            ItemAttributeIndex = Rule.LsdsItemAttributeIndexCurrent,
            //            ItemAttributeName = attr.Tag,
            //            ItemAttributeValue = attr.TextString,
            //            LayoutID = MainViewModel.CurrentLayout.LayoutID,
            //        });

            //    // remaining attributes from (list attributes) brAttrs
            //    if ((configAttrs != null && i == configAttrs.Count() - 1) || (configAttrs == null))
            //        foreach (var at in itemAttrs)
            //        {
            //            var itemAttr = new LO_ItemAttribute
            //            {
            //                ItemIndex = Rule.LsdsItemIndexCurrent,
            //                ItemAttributeIndex = ++Rule.LsdsItemAttributeIndexCurrent,
            //                ItemAttributeName = at.Tag,
            //                ItemAttributeValue = at.TextString,
            //                LayoutID = MainViewModel.CurrentLayout.LayoutID,
            //            };
            //            Lsds2Context.LoItemAttributes.Add(itemAttr);
            //        }
            //}

            //if (!Lsds2Context.LoFrames.Any(x => x != null && (x.BlockIndex == Rule.LsdsBlockIndexCurrent && x.FrameTypeID == frameTypeId))) //rule.LayerTypeId
            //{
            //    frames.ForEach(f =>
            //    {
            //        var frame = new LO_Frame
            //        {
            //            BlockIndex = Rule.LsdsBlockIndexCurrent,
            //            FrameIndex = ++Rule.LsdsFrameIndexCurrent,
            //            FrameTypeID = frameTypeId,
            //            LayoutID = MainViewModel.CurrentLayout.LayoutID,
            //            Xmin = f.GeometricExtents.MinPoint.X,
            //            Ymin = f.GeometricExtents.MinPoint.Y,
            //            Xmax = f.GeometricExtents.MaxPoint.X,
            //            Ymax = f.GeometricExtents.MaxPoint.Y
            //        };
            //        Lsds2Context.LoFrames.Add(frame);
            //    });
            //}
        }

        //old
        private static void DbSetsPreSaveByDefault(BlockReference br, string brName, List<AttributeReference> brAttrs,
                                                   AttributeReference attr, List<Entity> frames, IRule rule, short frameTypeId)
        {
            //if (Lsds2Context.LoItems.All(x => x.ItemHandle != br.Handle.ToString()))
            //{
            //    if (!Lsds2Context.LoBlocks.Any(x => x.BlockHandle == br.Handle.ToString() && x.BlockName == brName))
            //    {
            //        Lsds2Context.LoBlocks.Add(new LO_Block
            //        {
            //            BlockIndex = ++Rule.LsdsBlockIndexCurrent,
            //            BlockHandle = br.Handle.ToString(),
            //            BlockName = brName,
            //            LayoutID = MainViewModel.CurrentLayout.LayoutID,
            //            BlockXrefName = br.Layer.Contains("$0$") ? br.Layer.Substring(0, br.Layer.LastIndexOf("$0$", StringComparison.Ordinal)) : "NULL"
            //        });

            //        Rule.LsdsBlockAttributeIndexCurrent = 0;
            //        foreach (var at in brAttrs)
            //        {
            //            Lsds2Context.LoBlockAttributes.Add(new LO_BlockAttribute
            //            {
            //                BlockAttributeIndex = ++Rule.LsdsBlockAttributeIndexCurrent,
            //                BlockIndex = Rule.LsdsBlockIndexCurrent,
            //                BlockAttributeName = at.Tag,
            //                BlockAttributeValue = at.TextString,
            //                LayoutID = MainViewModel.CurrentLayout.LayoutID
            //            });
            //        }
            //    }

            //    Rule.LsdsItemAttributeIndexCurrent = 1;

            //    var item = new LO_Item
            //    {
            //        BlockIndex = Rule.LsdsBlockIndexCurrent,
            //        BlockName = brName,
            //        ItemHandle = br.Handle.ToString(),
            //        ItemIndex = ++Rule.LsdsItemIndexCurrent,
            //        ItemName = brName,
            //        ItemAttributeName = null,
            //        ItemAttributeIndex = 0,
            //        LayerName = br.Layer,
            //        LayoutID = MainViewModel.CurrentLayout.LayoutID,
            //        Rotation = GeomManager.RadianToDegree(br.Rotation),
            //        Xpos = br.Position.X,
            //        Ypos = br.Position.Y,
            //        Zpos = br.Position.Z,
            //        Xscale = Math.Round(br.ScaleFactors.X, 4),
            //        Yscale = Math.Round(br.ScaleFactors.Y, 4),
            //        Zscale = Math.Round(br.ScaleFactors.Z, 4),
            //        XrefName = br.Layer.Contains("$0$") ? br.Layer.Substring(0, br.Layer.LastIndexOf("$0$", StringComparison.Ordinal)) : "NULL"
            //    };
            //    Lsds2Context.LoItems.Add(item);

            //    foreach (var at in brAttrs)
            //    {
            //        var itemAttr = new LO_ItemAttribute
            //        {
            //            ItemIndex = Rule.LsdsItemIndexCurrent,
            //            ItemAttributeIndex = Rule.LsdsItemAttributeIndexCurrent++,
            //            ItemAttributeName = at.Tag,
            //            ItemAttributeValue = at.TextString,
            //            LayoutID = MainViewModel.CurrentLayout.LayoutID,
            //        };
            //        Lsds2Context.LoItemAttributes.Add(itemAttr);
            //    }
            //}
            //if (!Lsds2Context.LoFrames.Any(x => x != null && (x.BlockIndex == Rule.LsdsBlockIndexCurrent && x.FrameTypeID == frameTypeId)))
            //{
            //    frames.ForEach(f =>
            //    {
            //        var frame = new LO_Frame
            //        {
            //            BlockIndex = Rule.LsdsBlockIndexCurrent,
            //            FrameIndex = ++Rule.LsdsFrameIndexCurrent,
            //            FrameTypeID = frameTypeId,
            //            LayoutID = MainViewModel.CurrentLayout.LayoutID,
            //            Xmin = f.GeometricExtents.MinPoint.X,
            //            Ymin = f.GeometricExtents.MinPoint.Y,
            //            Xmax = f.GeometricExtents.MaxPoint.X,
            //            Ymax = f.GeometricExtents.MaxPoint.Y
            //        };
            //        Lsds2Context.LoFrames.Add(frame);
            //    });
            //}
        }

        /// <summary>  Update PropertyGrid </summary>
        private void OnParserCompleted(object result)
        {
            //var docName = Application.DocumentManager.GetDocument(ToolsManager.Db).Name.ToLower(); //.Substring(2)
            //var layout = MainViewModel.LayoutItems.
            //    FirstOrDefault(x => x.LayoutID == MainViewModel.CurrentLayout.LayoutID && docName.Contains(x.CADFileName.ToLower()));
            //if (layout != null) // && !layout.FSA)
            //{
            //    var idx = MainViewModel.LayoutItems.IndexOf(layout);
            //    var item = ((Layout)layout).CloneTo<Layout>();
            //    item.FSA = true;
            //    MainViewModel.LayoutItems[idx] = item;
            //    MainViewModel.CurrentLayout = MainViewModel.LayoutItems[idx];
            //}
        }

        /// <summary> Implementation for event of function "OnParseBlock" when blocks are read </summary>
        public bool OnDynamicParseBlock(ObjectId sender, ActionArguments readArgs, ITaskArgs taskArgs)
        {
            //var result = true;
            //Mut.WaitOne();
            //var br = (BlockReference)sender.GetObject(OpenMode.ForRead);
            //readArgs.Position = br.Position;
            //Mut.ReleaseMutex();

            //// groups of objects for selecting together
            //var groupObjectIds = new List<ObjectId>();
            //var frameVertices = new List<Point3dCollection>();

            //foreach (var rule in readArgs.Rules)
            //{
            //    //current draw colorIndex
            //    var ruleColorIndex = 0;

            //    //block table record that contains all entities of block
            //    Mut.WaitOne();
            //    var objectIds = br.DynamicXGetObjects(rule.TypeFilterOn, rule.isTypeFilterParent, rule.LayerPatternOn);
            //    Mut.ReleaseMutex();

            //    //entities without area
            //    var noAreaObjects = new List<Entity>();

            //    //iteration by all entities
            //    foreach (var id in objectIds)
            //    {
            //        Mut.WaitOne();
            //        var ent = (Entity)id.GetObject(OpenMode.ForRead);

            //        if (ent.Color.ColorMethod != ColorMethod.ByLayer || ent.Bounds == null) continue;

            //        //set colorIndex by Layer
            //        if (MainViewModel.CurrentUserSetting.IsColorMode)
            //            ruleColorIndex = ((LayerTableRecord)
            //                              ((ent.Layer == "0") ? br.LayerId : ent.LayerId).GetObject(OpenMode.ForRead)).Color.ColorIndex;
            //        else
            //            ruleColorIndex = MainViewModel.CurrentUserSetting.ColorIndex;
            //        Mut.ReleaseMutex();

            //        //Determines whether an entity is in the type entity list.
            //        if (ent.XIsMacthFor(typeof(Line), typeof(Arc), typeof(Ellipse)) |
            //            (ent.GetType() == typeof(Polyline) && ((Polyline)ent).Area <= 0))
            //            noAreaObjects.Add(ent);
            //        else
            //            //<Polyline>, <Circle>, ... converting to points-vertices
            //            if (ent.Bounds.HasValue)
            //                frameVertices.Add(ent.XGetPointsOrthogonal());
            //    }

            //    //Makes from noarea objects chain's elements, and each of Multiple chains troop to one frame, 
            //    //and others Single chains troops to group and only then into frame 
            //    if (noAreaObjects.Count > 0)
            //    {
            //        var chains = new List<Extents3d>();
            //        var singleChainMembers = new Point3dCollection();

            //        //Convert (double-ended) entity members to points, and Removing(Distinct) the repeated points 
            //        var tempChainMembers = noAreaObjects.XParseToChainsPoints().Distinct().ToList();

            //        //Sort tempChainMembers for improve next cycle processing
            //        tempChainMembers.Sort((points1, points2) => (points2.Count).CompareTo(points1.Count));
            //        foreach (var chain in tempChainMembers)
            //        {
            //            if (chain.Count > 2)
            //                //Multiple chain is it with more than two vertices of chain
            //                chains.Add(chain.XGetPointsExtents());
            //            else
            //                //Simple chain is it with less or equal than to two vertices of chain
            //                chain.OfType<Point3d>().ToList().ForEach(x => singleChainMembers.Add(x));
            //        }

            //        if (singleChainMembers.Count != 0)
            //            //join all single, i.e. define Extents for chain members and add it to Chains
            //            chains.Add(singleChainMembers.XGetPointsExtents());

            //        if (chains.Count != 0)
            //            //add vertices
            //            chains.ForEach(ext3D => frameVertices.Add(ext3D.XGetPoints()));
            //    }

            //    if (frameVertices.Count == 0) continue;

            //    Mut.WaitOne();

            //    //FrameOptimization(null, br, groupObjectIds, frameVertices, readArgs, rule, ruleColorIndex);
            //    frameVertices.XSortByArea();
            //    frameVertices.XForEachEraseNested();

            //    var frames = new List<Entity>();

            //    //creating and drawing polylines by vertices
            //    frameVertices.ForEach
            //        (points => frames.Add((new Polyline
            //        {
            //            Layer = rule.LayerDestination,
            //            ColorIndex = ruleColorIndex,
            //            LinetypeScale = rule.LineTypeScale,
            //            Linetype = rule.LineType,
            //            Closed = true
            //        }).XInit(points).XAddNew()));

            //    // LsdsParserMode: FSA(default), Detail, Outline
            //    switch (Rule.LsdsParserMode)
            //    {
            //        case LsdsParserMode.Default:
            //            groupObjectIds.AddRange(frames.Select(x => x.ObjectId).ToArray());
            //            break;
            //        case LsdsParserMode.Detail:
            //            groupObjectIds.AddRange(frames.Select(x => x.ObjectId).ToArray());
            //            break;

            //        case LsdsParserMode.Outline:
            //            var outline = (new Solid3d
            //            {
            //                Layer = rule.LayerDestination,
            //                ColorIndex = ruleColorIndex,
            //                LinetypeScale = rule.LineTypeScale,
            //                Linetype = rule.LineType,
            //            })
            //            .XInit(frames, BooleanOperationType.BoolUnite).XAddNew();
            //            groupObjectIds.Add(outline.ObjectId);
            //            break;
            //    }
            //    frames.Clear();

            //    Mut.ReleaseMutex();

            //    if (Worker.CancellationPending)
            //    {
            //        result = false;
            //        break;
            //    }
            //    frameVertices.Clear();
            //}

            //Mut.WaitOne();
            ////Group's operations
            //if (groupObjectIds.Count > 0 && !Worker.CancellationPending)
            //{
            //    groupObjectIds.DynamicXMoveTo(readArgs.Position);
            //    groupObjectIds.DynamicXScale(br.ScaleFactors.X, readArgs.Position);
            //    groupObjectIds.DynamicXRotate(br.Rotation, readArgs.Position);

            //    var grp =
            //        new Group(
            //            String.Format("Group{0}_{1}", Rule.LsdsGroupIndexCurrent++, br.Name.Replace("*", "_")), true);
            //    grp.DynamicXAddGroup(new ObjectIdCollection(groupObjectIds.ToArray()));
            //}
            //Mut.ReleaseMutex();

            //return result;
            return true;
        }

        /// <summary> Run parser in detail mode </summary>
        public void ParserDetailStart()
        {
            //TaskParseStart(LsdsParserMode.Detail);
        }

        /// <summary> Run parser in outline mode </summary>
        public void ParserOutlineStart()
        {
            //TaskParseStart(LsdsParserMode.Outline);
        }

        #endregion

        #region "Task: FSA UPLOAD with actions: ConnectionTest, LayoutClean, SaveChanges"

        /// <summary> Create new task </summary>
        //[CommandMethod("Partner", "PARTNERFSAUPLOAD", CommandFlags.Session)]
        public void TaskUploadStart()
        {
            //if (MainViewModel == null) MainViewModel = ComposeViewModel();

            //if (Lsds2Context.LoFrames.Count == 0)
            //{
            //    SystemSounds.Asterisk.Play();
            //    var vtd = new VistaTaskDialog
            //    {
            //        Content = "Before EXPORT you need start Upload FSA and try task again ...",
            //        CommonButtons = VistaTaskDialogCommonButtons.Close,
            //        MainInstruction = "No frame objects.",
            //        MainIcon = VistaTaskDialogIcon.Warning,
            //        WindowTitle = ProjectManager.Name + " new task: Upload",
            //    };
            //    vtd.Show(Application.MainWindow.Handle);
            //    return;
            //}

            //var taskArgs = CurrentTaskArgs;
            //taskArgs.Reset();

            //if ((taskArgs != null) && (TaskThread == null || !TaskThread.IsAlive))
            //{
            //    TaskThread = new Thread(x => TaskDialogUploadShow(taskArgs)) { IsBackground = true };
            //    TaskThread.Start();
            //}
        }

        /// <summary> Show Transfer task dialog </summary>
        public void TaskDialogUploadShow(ITaskArgs taskArgs)
        {
            //var titleInstruction = "";
            //if (Lsds2Context.LoFrames != null && Lsds2Context.LoFrames.Count != 0)
            //    titleInstruction = "Layout: " + MainViewModel.LayoutItems.First(x => x.LayoutID == Lsds2Context.LoFrames[0].LayoutID).LayoutName + "\n";

            //var config = new TaskDialogOptions
            //{
            //    AllowDialogCancellation = true,
            //    Callback = TaskDialogUploadCallBack,
            //    CallbackData = taskArgs,
            //    CanBeMinimized = true,
            //    CustomButtons = new[] { "&Upload", "&Close" },
            //    Content = "Message: ...",
            //    EnableCallbackTimer = true,
            //    ExpandedInfo = "Detail...",
            //    FooterText = "for additional information: <a href=\"http://www.intel.com/lsds\">Lsds help</a> ," +
            //       "<a href=\"" + ProjectManager.UserSettingsPath + "Errors.xml" + "\"" + ">Lsds errors</a>",
            //    FooterIcon = VistaTaskDialogIcon.Information,
            //    Handle = Application.MainWindow.Handle,
            //    MainInstruction = String.Format("{0}Task: Upload for {1} items to DB.", titleInstruction, ((List<ObjectId>)taskArgs.DataSource).Count),
            //    MainIcon = VistaTaskDialogIcon.Shield,
            //    ShowProgressBar = true,
            //    Title = ProjectManager.Name + " new task:",
            //    VerificationText = "Background mode",
            //    Width = 200,
            //};

            ////td.Buttons.Add(new TaskDialogCoomonButton(0, "Run the operation on the selected objects."));
            ////td.Buttons.Add(new TaskDialogButton(1, "Cancel")); //Do nothing and cancel the command
            //using (new SysVarOverride("NOMUTT", 1))
            //{
            //    TaskDialog.Show(config);
            //}
        }

        /// <summary> The signature of the callback that recieves notificaitons from a Task Dialog. </summary>
        //private bool TaskDialogUploadCallBack(IActiveTaskDialog dialog, VistaTaskDialogNotificationArgs dialogArgs, object callbackArgs)
        //{
        //    //var taskArgs = (TaskArguments)callbackArgs;
        //    //var result = false;
        //    //switch (dialogArgs.Notification)
        //    //{
        //    //    case VistaTaskDialogNotification.Created:
        //    //        taskArgs.Status = StatusOptions.None;
        //    //        taskArgs.ExpandedInfo = "...";
        //    //        taskArgs.Context = DataManager.CreateContext<Lsds2Context>();
        //    //        taskArgs.ActionPool =
        //    //            new Dictionary<string, Func<ITaskArgs, bool>>
        //    //                { { "Connection test ... ", OnConnectionTestAction }, 
        //    //                  { "Layout cleaning ... ", OnLayoutCleanAction }, 
        //    //                  { "Save changes ... ", OnParserSaveChangesAction } };

        //    //        taskArgs.Dialog = dialog;
        //    //        taskArgs.ProgressLimit =
        //    //            (new List<object> 
        //    //                { Lsds2Context.LoBlocks, Lsds2Context.LoFrames, 
        //    //                  Lsds2Context.LoItems, Lsds2Context.LoBlockAttributes,
        //    //                  Lsds2Context.LoItemAttributes }).Select(x => ((IEnumerable<object>)x).Count()).Sum();

        //    //        //set up dialog process limits
        //    //        dialog.SetProgressBarRange(0, (short)taskArgs.ProgressLimit);

        //    //        Worker = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };
        //    //        Worker.DoWork += BgDoWork;
        //    //        Worker.ProgressChanged += BgProgressChanged;
        //    //        Worker.RunWorkerCompleted += BgRunWorkerCompleted;
        //    //        result = true;
        //    //        break;

        //    //    case VistaTaskDialogNotification.ButtonClicked:
        //    //        taskArgs.Dialog = dialog;
        //    //        switch (dialogArgs.ButtonId)
        //    //        {
        //    //            case 500: // Clicked Run button

        //    //                taskArgs.Status = StatusOptions.Start;

        //    //                // Start the asynchronous operation.
        //    //                if (Worker != null && !Worker.IsBusy)
        //    //                    Worker.RunWorkerAsync(taskArgs);

        //    //                // true is prevent dialog from closing
        //    //                result = true;
        //    //                break;

        //    //            case 501: // Clicked Cancel button

        //    //                // Cancel the asynchronous operation.
        //    //                if (Worker != null && Worker.IsBusy)
        //    //                    Worker.CancelAsync();

        //    //                Thread.Sleep(100);

        //    //                if (taskArgs.Status != StatusOptions.Cancel && taskArgs.Status != StatusOptions.None)
        //    //                {
        //    //                    //TRUE: reset tick count (args.TimerTickCount) and prevent dialog from closing
        //    //                    result = true;
        //    //                }
        //    //                else
        //    //                {
        //    //                    // Request that the worker thread stop itself 
        //    //                    if (TaskThread != null)
        //    //                    {
        //    //                        TaskThread.Abort();

        //    //                        // Use the Join method to block the current thread until the object's thread terminates.
        //    //                        TaskThread.Join();
        //    //                    }
        //    //                }

        //    //                taskArgs.Status = StatusOptions.Cancel;
        //    //                break;
        //    //        }
        //    //        break;

        //    //    case VistaTaskDialogNotification.Timer:
        //    //        if (taskArgs.Status != StatusOptions.None && taskArgs.Status != StatusOptions.Start)
        //    //            result = taskArgs.DisplayStatus(dialogArgs.TimerTickCount);
        //    //        else
        //    //            result = true;
        //    //        break;

        //    //    case VistaTaskDialogNotification.HyperlinkClicked:
        //    //        Process.Start(dialogArgs.Hyperlink);
        //    //        break;
        //    //}
        //    //return result;
        //    return true;
        //}

        /// <summary> Task: Connection Test </summary>
        public bool OnConnectionTestAction(ITaskArgs taskArgs)
        {
            //var lsdsContext = taskArgs.Context as Lsds2Context;
            //if (lsdsContext != null)
            //{
            //    var oldTimeOut = lsdsContext.XGetObjectContext().CommandTimeout;
            //    try
            //    {
            //        lsdsContext.XGetObjectContext().CommandTimeout = 1;
            //        lsdsContext.Database.Connection.Open();   // check the database connection
            //        if (lsdsContext.Database.Connection.State == ConnectionState.Open)
            //        {
            //            Worker.ReportProgress(taskArgs.ProgressIndex, taskArgs);
            //            Thread.Sleep(1000);
            //            return true;
            //        }
            //    }
            //    catch
            //    {
            //        taskArgs.ExpandedInfo = "no connection";
            //        taskArgs.Status = StatusOptions.Error;
            //        return false;
            //    }
            //    finally
            //    {
            //        lsdsContext.XGetObjectContext().CommandTimeout = oldTimeOut;
            //    }
            //}
            return true;
        }

        /// <summary> Task: Layout Cleaning </summary>
        public bool OnLayoutCleanAction(ITaskArgs taskArgs)
        {
            var retValue = false;
            //var lsdsContext = taskArgs.Context as Lsds2Context;
            //if (lsdsContext != null)
            //{
            //    try
            //    {
            //        var result = lsdsContext.LayoutManager("REMOVE_FSA", (int?)MainViewModel.CurrentLayout.LayoutID, Environment.UserName, DateTime.Now);
            //        if (result.Any(x => x.RETURN_CODE == 1))
            //        {
            //            Worker.ReportProgress(taskArgs.ProgressIndex, taskArgs);
            //            Thread.Sleep(1000);
            //            retValue = true;
            //        }
            //        else
            //        {
            //            var msg = "Return code: " + result.Select(x => Convert.ToString(x.RETURN_CODE) + " message: " + x.RETURN_MESSAGE);
            //            taskArgs.ExpandedInfo = msg;
            //            taskArgs.Status = StatusOptions.Error;
            //        }
            //        Thread.Sleep(1000);
            //    }
            //    catch (System.Exception ex)
            //    {
            //        taskArgs.ExpandedInfo = ex.InnerException.Message;
            //        taskArgs.Status = StatusOptions.Error;
            //    }
            //}
            return retValue;
        }

        /// <summary> Task: Save Changes </summary>
        private bool OnParserSaveChangesAction(ITaskArgs taskArgs)
        {
            var result = true;
            //var lsdsContext = taskArgs.Context as Lsds2Context;
            //if (lsdsContext != null)
            //{
            //    //DialogArgs.Context.Configuration.AutoDetectChangesEnabled = false;
            //    // command pattern for building queries
            //    const string insertCommandPattern = "insert into dbo.{0} ({1}) VALUES ({2}) ";
            //    var updateCommandPattern = "update dbo.{0} SET {1} = '{2}' WHERE LayoutID = " + (Convert.ToString(MainViewModel.CurrentLayout.LayoutID)).Trim();

            //    //var data = new List<Tuple<object, string[], string[]>> { new Tuple<object, string[], string[]>(Lsds2Context.LoBlocks, null, null) };

            //    var dataSets = new Dictionary<object, string> 
            //        {
            //            {Lsds2Context.LoBlocks, insertCommandPattern},
            //            {Lsds2Context.LoFrames, insertCommandPattern},
            //            {Lsds2Context.LoItems, insertCommandPattern},
            //            {Lsds2Context.LoBlockAttributes, insertCommandPattern},
            //            {Lsds2Context.LoItemAttributes, insertCommandPattern}
            //        };
            //    try
            //    {
            //        // limit/max number for bar into dialog window
            //        //taskArgs.ProgressLimit = dataSets.Select(x => ((IEnumerable<object>)x.Key).Count()).Sum();

            //        // loop by tables
            //        foreach (var data in dataSets)
            //        {
            //            var dataType = data.Key.GetType().GetGenericArguments()[0];

            //            taskArgs.Content = taskArgs.Title + dataType.Name;

            //            // size batch for composite queries to transfer to server
            //            //const int batchSize = 100;

            //            // the function "MakeBatchSqlCommand" to build of queries 
            //            var method = typeof(DataManager).GetMethod("MakeBatchSqlCommand").MakeGenericMethod(new[] { dataType });

            //            // Invoke MakeBatchSqlCommand
            //            var methodResult = method.Invoke(null, new[] { taskArgs, insertCommandPattern, data.Key });

            //            // Check a worker for cancel events, that canceling process
            //            if (Worker.CancellationPending) return false;

            //            //// add info about current action into dialog window 
            //            //taskArgs.ExpandedInfo += String.Format("{0} ({1});", dataType.Name, methodResult);

            //            if (!taskArgs.IsTimerOn) taskArgs.Dialog.SetContent(taskArgs.Content);

            //            var dataCount = ((IEnumerable<object>)data.Key).Count();
            //            taskArgs.ProgressIndex += dataCount;
            //            Worker.ReportProgress(taskArgs.ProgressIndex, taskArgs);

            //            // delay for swctching threads (mandatory - yes/not ?)
            //            Thread.Sleep(100);
            //        }
            //        MainViewModel.SaveChanges((Layout)MainViewModel.CurrentLayout.Clone(), "FSA", true);
            //    }
            //    catch (Exception ex)
            //    {
            //        taskArgs.ExpandedInfo = ex.InnerException.Message;
            //        taskArgs.Status = StatusOptions.Error;
            //        Thread.Sleep(100);
            //        result = false;
            //    }
            //}
            return result;
        }

        #endregion

        #region "Tasks: CREATE, DELETE, UNDELETE, PURGE"

        /// <summary> Task layout actions : Delete, UnDelete, Purge </summary>
        //[CommandMethod("Partner", "PARTNERLAYOUTACTIONS", CommandFlags.Session)]
        public void TaskLayoutActionStart()
        {
            //IList<string> notValidMessages = new string[] { };
            //if (MainViewModel.CurrentLayout == null) notValidMessages.Add("No current object");
            ////if (!MainViewModel.CurrentLayout.IsPropertiesValid()) notValidMessages.Add("It is not valid!");

            //if (notValidMessages.Count > 0)
            //{
            //    SystemSounds.Asterisk.Play();
            //    var vtd = new VistaTaskDialog
            //    {
            //        WindowTitle = ProjectManager.Name + " new task: " + CurrentTaskArgs.Command,
            //        MainInstruction = String.Join(",", notValidMessages),
            //        MainIcon = VistaTaskDialogIcon.Warning,
            //        Content = "Try check the layout and run task again ...",
            //        CommonButtons = VistaTaskDialogCommonButtons.Close,
            //    };
            //    vtd.Show(Application.MainWindow.Handle);
            //    return;
            //}

            //var layout = (Layout)CurrentTaskArgs.CommandArguments[0];
            //CurrentTaskArgs.Content = (String.IsNullOrEmpty(CurrentTaskArgs.Content) ? "" : CurrentTaskArgs.Content) +
            //                             "Layout name... " + layout.LayoutName +
            //                             "\nLayout id... " + (layout.LayoutID < 0 ? "(number from server)" : layout.LayoutID.ToString()) +
            //                             "\nCreated by... " + layout.CreatedBy +
            //                             "\nDate created... " + layout.DateCreated +
            //                             (CurrentTaskArgs.Command == "CREATE" ? "" : "\nModified by... " + layout.ModifiedBy) +
            //                             (CurrentTaskArgs.Command == "CREATE" ? "" : "\nDate last modified... " + layout.DateModified);
            //TaskLayoutActionShow(CurrentTaskArgs);
        }

        /// <summary> Show Transfer task dialog </summary>
        public void TaskLayoutActionShow(ITaskArgs taskArgs)
        {
            //var config = new TaskDialogOptions
            //{
            //    AllowDialogCancellation = true,
            //    Callback = TaskLayoutActionCallBack,
            //    CallbackData = taskArgs,
            //    CanBeMinimized = false,
            //    Content = taskArgs.Content,
            //    CustomButtons = new[] { "&" + taskArgs.Command, "&Close" },
            //    EnableCallbackTimer = true,
            //    ExpandedInfo = taskArgs.ExpandedInfo,
            //    FooterText = "for additional information: <a href=\"http://www.intel.com/lsds\">Lsds help</a> ," +
            //        "<a href=\"" + ProjectManager.UserSettingsPath + "Errors.xml" + "\"" + ">Lsds errors</a>",
            //    FooterIcon = VistaTaskDialogIcon.Information,
            //    Handle = Application.MainWindow.Handle,
            //    MainInstruction = String.Format("Task: {0}.", taskArgs.Title),
            //    MainIcon = VistaTaskDialogIcon.Shield,
            //    Title = ProjectManager.Name + " new task:",
            //    ShowProgressBar = true,
            //    Width = 200,
            //};

            ////using (new SysVarOverride("NOMUTT", 1))
            ////{
            //TaskDialog.Show(config);
            ////}
        }

        /// <summary> The signature of the callback that recieves notificaitons from a Task Dialog. </summary>
        //private bool TaskLayoutActionCallBack(IActiveTaskDialog dialog, VistaTaskDialogNotificationArgs dialogArgs, object callbackArgs)
        //{
        //    //var taskArgs = (TaskArguments)callbackArgs;
        //    //// Regulate a dialog closing
        //    var result = false;

        //    //// Task dialog event notification arguments
        //    //switch (dialogArgs.Notification)
        //    //{
        //    //    case VistaTaskDialogNotification.Created:

        //    //        //set up dialog process limits
        //    //        dialog.SetProgressBarRange(0, 100);

        //    //        // Task process argument initilizing
        //    //        taskArgs.Status = StatusOptions.None;
        //    //        //taskArgs.Percent = 0;
        //    //        taskArgs.Dialog = dialog;
        //    //        taskArgs.Context = DataManager.CreateContext<Lsds2Context>();
        //    //        taskArgs.ActionPool = new Dictionary<string, Func<ITaskArgs, bool>> { { taskArgs.Command, OnLayoutActions } };
        //    //        Worker = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };
        //    //        Worker.DoWork += BgDoWork;
        //    //        Worker.ProgressChanged += BgProgressChanged;
        //    //        Worker.RunWorkerCompleted += BgRunWorkerCompleted;
        //    //        result = true;
        //    //        break;

        //    //    case VistaTaskDialogNotification.ButtonClicked:

        //    //        taskArgs.Dialog = dialog;
        //    //        switch (dialogArgs.ButtonId)
        //    //        {
        //    //            case 500: //Button the "Parse"
        //    //                taskArgs.Status = StatusOptions.Start;

        //    //                // Start the asynchronous operation.
        //    //                if (Worker != null && !Worker.IsBusy)
        //    //                    Worker.RunWorkerAsync(taskArgs);

        //    //                result = true;
        //    //                break;

        //    //            // Cancel the asynchronous operation.
        //    //            case 501: //Button the "Close"

        //    //                if (Worker != null && Worker.IsBusy)
        //    //                    Worker.CancelAsync();

        //    //                //improving a parallel work with others processes (it is not mandatory !?)
        //    //                Thread.Sleep(100);

        //    //                if (taskArgs.Status == StatusOptions.Cancel || taskArgs.Status == StatusOptions.None)
        //    //                {
        //    //                    // Autocad commnd for drawing regeneration and ribbon refresh
        //    //                    CmdManager.SendToExecute("PARTNERREGEN ");
        //    //                }

        //    //                taskArgs.Status = StatusOptions.Cancel;
        //    //                break;
        //    //        }
        //    //        break;

        //    //    // Processing of timer
        //    //    case VistaTaskDialogNotification.Timer:

        //    //        if (taskArgs.Status != StatusOptions.None && taskArgs.Status != StatusOptions.Start)
        //    //        {
        //    //            result = taskArgs.DisplayStatus(dialogArgs.TimerTickCount);
        //    //        }
        //    //        else
        //    //        {
        //    //            result = true;
        //    //        }
        //    //        break;

        //    //    // Processing of event the Hyperlink click
        //    //    case VistaTaskDialogNotification.HyperlinkClicked:
        //    //        Process.Start(dialogArgs.Hyperlink);
        //    //        break;
        //    //}
        //    return result;
        //}

        /// <summary> Task: Layout Db Create/Read/Update/Delete actions </summary>
        public bool OnLayoutActions(ITaskArgs taskArgs)
        {
            var retValue = false;
            //var lsdsContext = taskArgs.Context as Lsds2Context;
            //if (lsdsContext != null)
            //{
            //    try
            //    {
            //        var layout = (Layout)taskArgs.CommandArguments[0];

            //        //LayoutManager_Result result;
            //        //if (taskArgs.Command == "NEW")
            //        //{
            //        //    var layoutIDmax = lsdsContext.LO_Layouts.Select(x => x.LayoutID).Max();
            //        //    LO_Layout newLayout = MainViewModel.CurrentLayout != null
            //        //        ? (LO_Layout)MainViewModel.CurrentLayout.CloneToLO_Layout() : new LO_Layout();
            //        //    newLayout.CreatedBy = Environment.UserName;
            //        //    newLayout.DateCreated = DateTime.Now;
            //        //    newLayout.DateModified = DateTime.Now;
            //        //    newLayout.FSA = false;
            //        //    newLayout.LayoutID = ++layoutIDmax;
            //        //    newLayout.LayoutName = "New layout N" + layoutIDmax.ToString();
            //        //    newLayout.LayoutState = null;
            //        //    newLayout.ModifiedBy = Environment.UserName;
            //        //    newLayout.LayoutVersion = "...";
            //        //    lsdsContext.LO_Layouts.Add(newLayout);
            //        //    try
            //        //    {
            //        //        lsdsContext.SaveChanges();
            //        //        result = new LayoutManager_Result();
            //        //        result.RETURN_CODE = 0;
            //        //    }
            //        //    catch (Exception)
            //        //    {
            //        //        result = new LayoutManager_Result();
            //        //        result.RETURN_CODE = -1;
            //        //        result.RETURN_MESSAGE = "Error on create new layout!";
            //        //    }
            //        //}
            //        //else
            //        //{

            //        //var result = lsdsContext.LayoutManager
            //        //      (taskArgs.Command, (int)layout.LayoutID, Environment.UserName, DateTime.Now).FetchResult();
            //        ////}

            //        //taskArgs.TaskResult = result;
            //        //if (result.RETURN_CODE >= 0)
            //        //{
            //        //    taskArgs.ProgressIndex = 100;
            //        //    Worker.ReportProgress(taskArgs.ProgressIndex, taskArgs);
            //        //    //if (Worker.GetType() == typeof(BackgroundWorker))
            //        //    //if (taskArgs.ProgressBar != null) ((ProgressMeterBar)taskArgs.ProgressBar).MeterProgress();
            //        //    //Thread.Sleep(100);
            //        //    retValue = true;
            //        //}
            //        //else
            //        //{
            //        //    var msg = "Return code: " + result.RETURN_CODE + " message: " + result.RETURN_MESSAGE;
            //        //    taskArgs.ExpandedInfo = msg;
            //        //    taskArgs.Status = StatusOptions.Error;
            //        //}
            //        //Thread.Sleep(100);
            //    }
            //    catch (Exception ex)
            //    {
            //        taskArgs.ExpandedInfo = ex.InnerException.Message;
            //        taskArgs.Status = StatusOptions.Error;
            //    }
            //}
            return retValue;
        }

        #endregion

    }
}

