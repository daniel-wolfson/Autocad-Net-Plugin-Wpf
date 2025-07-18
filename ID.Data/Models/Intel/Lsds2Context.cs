using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Xml.Serialization;

using Intellidesk.AcadNet.Data.Common.Editors;
using Intellidesk.AcadNet.Data.Models.Entities;
using Intellidesk.AcadNet.Data.Repositories;
using Intellidesk.AcadNet.Data.Repositories.EF6;
using Intellidesk.AcadNet.Data.Repositories.EF6.DataContext;
using Intellidesk.AcadNet.Data.Repositories.Infrastructure;
using Intellidesk.NetTools;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace Intellidesk.AcadNet.Data.Models.Intel
{
    public enum LsdsParserMode
    {
        /// <summary> Lsds Parser mode FSA </summary>
        Default,
        /// <summary> Lsds Parser mode Detail </summary>
        Detail,
        /// <summary> Lsds Parser mode Outline </summary>
        Outline
    }

    public partial class Lsds2Context : IDataContextAsync 
    {
        public Lsds2Context(string connectionString) : base(connectionString) { }

        public static List<LO_Bay> LoBays = new List<LO_Bay>();
        public static List<LO_BlockAttribute> LoBlockAttributes = new List<LO_BlockAttribute>();
        public static List<LO_Block> LoBlocks = new List<LO_Block>();
        public static List<LO_Frame> LoFrames = new List<LO_Frame>();
        public static List<LO_ItemAttribute> LoItemAttributes = new List<LO_ItemAttribute>();
        public static List<LO_Item> LoItems = new List<LO_Item>();
        public static List<LO_Layout> LoLayouts = new List<LO_Layout>();
        public static List<LSDS_Config> LoConfigs = new List<LSDS_Config>();

        #region "Query samples"

        //public override int SaveChanges()
        //{
        //    var changeSet = ChangeTracker.Entries<LO_Layout>();

        //    if (changeSet != null)
        //    {
        //        foreach (var entry in changeSet.Where(c => c.State != EntityState.Unchanged))
        //        {
        //            //entry.Entity. = DateProvider.GetCurrentDate();
        //            //entry.Entity.ModifiedBy = UserName;
        //        }
        //    }
        //    return base.SaveChanges();
        //}

        // Sample query: @0=28,@1=1,@2='*U27',@3='GBIE3-TF',@4='D5403'
        //public string BlockFields = "LayoutID,BlockIndex,BlockName,BlockXrefName,BlockHandle";

        //@0=28,@1=1,@2=3,@3=1,@4=-5.8210339357358016,@5=16.672783598604752,@6=-4.1960339357358016,@7=18.297783598604752
        //public string FrameFields = "LayoutID,BlockIndex,FrameIndex,FrameTypeID,Xmin,Ymin,Xmax,Ymax";

        //@0='XDK109',@1=28,@2=1,@3=1,@4='ENTITY_CODE',@5=1,@6='*U27',@7='GBIE3-TF',@8='GBIE3-TF$0$IE-EQPM-XDK109',@9=7805.0234213550866,@10=3445.1232246177169,@11=0,@12=25.399999999999999,@13=25.399999999999999,@14=25.399999999999999,@15=90,@16='D5403'
        //public string ItemFields =
        //    "ItemName,LayoutID,ItemIndex,ItemAttributeIndex,ItemAttributeName,BlockIndex,BlockName,XrefName,LayerName,Xpos,Ypos,Zpos,Xscale,Yscale,Zscale,Rotation,ItemHandle";

        //@0=28,@1=1,@2=7,@3='DRAFT_ID',@4=''
        //public string BlockAttributesFields =
        //   "LayoutID,BlockIndex,BlockAttributeIndex,BlockAttributeName,BlockAttributeValue";

        //@0=28,@1=1,@2=11,@3='ATT1_ID',@4=''
        //public string ItemAttributesFields = "LayoutID,ItemIndex,ItemAttributeIndex,ItemAttributeName,ItemAttributeValue";

        //#region "FullQueries"

        //public string BlockQuery = "exec sp_executesql N'insert [dbo].[LO_Blocks]([LayoutID], [BlockIndex], [BlockName], [BlockXrefName], [BlockHandle]) " +
        //"values (@0, @1, @2, @3, @4) select [BlockID] from [dbo].[LO_Blocks] where @@ROWCOUNT > 0 and [LayoutID] = @0 and [BlockIndex] = @1',N'" +
        //"@0 decimal(6,0),@1 decimal(6,0),@2 varchar(500),@3 varchar(500),@4 varchar(25)'," +
        //"@0={0},@1={1},@2='{2}',@3='{3}',@4='{4}'";

        //public string FrameQuery = "exec sp_executesql N'insert [dbo].[LO_Frames]([LayoutID], [BlockIndex], [FrameIndex], [FrameTypeID], [Xmin], [Ymin], [Xmax], [Ymax]) " +
        //    "values (@0, @1, @2, @3, @4, @5, @6, @7) select [BlockID], [FrameID] from [dbo].[LO_Frames] " +
        //    "where @@ROWCOUNT > 0 and [LayoutID] = @0 and [BlockIndex] = @1 and [FrameIndex] = @2',N'" +
        //    "@0 decimal(6,0),@1 decimal(6,0),@2 decimal(6,0),@3 smallint,@4 float,@5 float,@6 float,@7 float'," +
        //    "@0={0},@1={1},@2={2},@3={3},@4={4},@5={5},@6={6},@7={7}";

        //public string ItemQuery =
        //    "exec sp_executesql N'insert [dbo].[LO_Items]([ItemName], [LayoutID], [ItemIndex], [ItemAttributeIndex], [ItemAttributeName], [BlockIndex], [BlockName], [XrefName], [LayerName], [Xpos], [Ypos], [Zpos], [Xscale], [Yscale], [Zscale], [Rotation], [ItemHandle]) " +
        //    "values (@{0}, @{1, @2, @3, @4, @5, @6, @7, @8, @9, @10, @11, @12, @13, @14, @15, @16) select [ItemID], [ItemAttributeID], [BlockID] from [dbo].[LO_Items] " +
        //    "where @@ROWCOUNT > 0 and [LayoutID] = @1 and [ItemIndex] = @2 and [ItemAttributeIndex] = @3',N'" +
        //    "@0 varchar(100),@1 decimal(6,0),@2 decimal(6,0),@3 decimal(6,0),@4 varchar(100),@5 decimal(6,0),@6 varchar(500),@7 varchar(500),@8 varchar(500),@9 float,@10 float,@11 float,@12 float,@13 float,@14 float,@15 float,@16 varchar(25)'," +
        //    "@0='{0}',@1={1},@2={2},@3={3},@4='{4}',@5={5},@6='{6}',@7='{7}',@8='{8}',@9={9},@10={10},@11={11},@12={12},@13={13},@14={14},@15={15},@16='{16}'";

        //public string BlockAttributesQuery =
        //    "exec sp_executesql N'insert [dbo].[LO_BlockAttributes]([LayoutID], [BlockIndex], [BlockAttributeIndex], [BlockAttributeName], [BlockAttributeValue]) " +
        //    "values (@0, @1, @2, @3, @4) select [BlockID], [BlockAttributeID] from [dbo].[LO_BlockAttributes] " +
        //    "where @@ROWCOUNT > 0 and [LayoutID] = @0 and [BlockIndex] = @1 and [BlockAttributeIndex] = @2',N'@0 decimal(6,0),@1 decimal(6,0),@2 decimal(6,0),@3 varchar(100),@4 varchar(1000)'," +
        //    "@0={0},@1={1},@2={2},@3='{3}',@4='{}'";

        //public string ItemAttributesQuery =
        //    "exec sp_executesql N'insert [dbo].[LO_ItemAttributes]([LayoutID], [ItemIndex], [ItemAttributeIndex], [ItemAttributeName], [ItemAttributeValue]) " +
        //    "values (@0, @1, @2, @3, @4) select [ItemID], [ItemAttributeID] from [dbo].[LO_ItemAttributes] " +
        //    "where @@ROWCOUNT > 0 and [LayoutID] = @0 and [ItemIndex] = @1 and [ItemAttributeIndex] = @2',N'@0 decimal(6,0),@1 decimal(6,0),@2 decimal(6,0),@3 varchar(100),@4 varchar(1000)'," +
        //    "@0={0},@1={1},@2={2},@3='{3}',@4='{4}'";
        //#endregion

        #endregion

        public static void Clear()
        {
            LoItems.Clear(); LoItems = null;
            LoItems = new List<LO_Item>();

            LoItemAttributes.Clear(); LoItemAttributes = null;
            LoItemAttributes = new List<LO_ItemAttribute>();

            LoBlocks.Clear(); LoBlocks = null;
            LoBlocks = new List<LO_Block>();

            LoBlockAttributes.Clear(); LoBlockAttributes = null;
            LoBlockAttributes = new List<LO_BlockAttribute>();

            LoFrames.Clear(); LoFrames = null;
            LoFrames = new List<LO_Frame>();

            System.Threading.Thread.Sleep(1000);
        }

        public void SyncObjectState<TEntity>(TEntity entity) where TEntity : class, IObjectState
        {
            throw new NotImplementedException();
        }

        public void SyncObjectsStatePostCommit()
        {
            throw new NotImplementedException();
        }
    }

    [MetadataType(typeof(Layout))]
    public partial class LO_Layout { }

    [MetadataType(typeof(Config))]
    public partial class LSDS_Config { }

    public class Layout : BaseLayout, ILayout
    {
        private string _processName1;
        [Category("Processes and Ramp"), DisplayName("Process #1 name")]
        [ItemsSource(typeof(ProcessesItemsSource))]
        public string ProcessName1
        {
            get { return _processName1; }
            set
            {
                _processName1 = value ?? "none";
                OnPropertyChanged();
            }
        }

        private string _wspw1;
        [Category("Processes and Ramp"), DisplayName("Process #1 WSPW")]
        public string Wspw1
        {
            get { return _wspw1; }
            set
            {
                _wspw1 = value;
                OnPropertyChanged();
            }
        }

        private string _processName2;
        [Category("Processes and Ramp"), DisplayName("Process #2 name")]
        public string ProcessName2
        {
            get { return _processName2; }
            set
            {
                _processName2 = value;
                OnPropertyChanged();
            }
        }

        private string _wspw2;
        [Category("Processes and Ramp"), DisplayName("Process #2 WSPW")]
        public string Wspw2
        {
            get { return _wspw2; }
            set
            {
                _wspw2 = value;
                OnPropertyChanged();
            }
        }

        private string _processName3;
        [Category("Processes and Ramp"), DisplayName("Process #3 name")]
        public string ProcessName3
        {
            get { return _processName3; }
            set
            {
                _processName3 = value;
                OnPropertyChanged();
            }
        }

        private string _wspw3;
        [Category("Processes and Ramp"), DisplayName("Process #3 WSPW")]
        public string Wspw3
        {
            get { return _wspw3; }
            set
            {
                _wspw3 = value;
                OnPropertyChanged();
            }
        }

        private string _processName4;
        [Category("Processes and Ramp"), DisplayName("Process #4 name")]
        public string ProcessName4
        {
            get { return _processName4; }
            set
            {
                _processName4 = value;
                OnPropertyChanged();
            }
        }

        private string _wspw4;
        [Category("Processes and Ramp"), DisplayName("Process #4 WSPW")]
        public string Wspw4
        {
            get { return _wspw4; }
            set
            {
                _wspw4 = value;
                OnPropertyChanged();
            }
        }

    }

    [Serializable(), DefaultProperty("Id")]
    public class LayoutFilter : BaseEntity
    {
        private string _processName1;
        [XmlElement(ElementName = "ProcessName1")]
        [Category("Processes and Ramp"), DisplayName("Process #1 name")]
        [ItemsSource(typeof(ProcessesItemsSource))]
        public string ProcessName1
        {
            get { return _processName1; }
            set
            {
                _processName1 = value;
                OnPropertyChanged();
            }
        }

        private string _wspw1;
        [XmlElement(ElementName = "WSPW1")]
        [Category("Processes and Ramp"), DisplayName("Process #1 WSPW")]
        public string Wspw1
        {
            get { return _wspw1; }
            set
            {
                _wspw1 = value;
                OnPropertyChanged();
            }
        }

        private string _processName2;
        [XmlElement(ElementName = "ProcessName2")]
        [Category("Processes and Ramp"), DisplayName("Process #2 name")]
        public string ProcessName2
        {
            get { return _processName2; }
            set
            {
                _processName2 = value;
                OnPropertyChanged();
            }
        }

        private string _wspw2;
        [XmlElement(ElementName = "WSPW2")]
        [Category("Processes and Ramp"), DisplayName("Process #2 WSPW")]
        public string Wspw2
        {
            get { return _wspw2; }
            set
            {
                _wspw2 = value;
                OnPropertyChanged();
            }
        }

        private string _processName3;
        [XmlElement(ElementName = "ProcessName3")]
        [Category("Processes and Ramp"), DisplayName("Process #3 name")]
        public string ProcessName3
        {
            get { return _processName3; }
            set
            {
                _processName3 = value;
                OnPropertyChanged();
            }
        }

        private string _wspw3;
        [XmlElement(ElementName = "WSPW3")]
        [Category("Processes and Ramp"), DisplayName("Process #3 WSPW")]
        public string WSPW3
        {
            get { return _wspw3; }
            set
            {
                _wspw3 = value;
                OnPropertyChanged();
            }
        }

        private string _processName4;
        [XmlElement(ElementName = "ProcessName4")]
        [Category("Processes and Ramp"), DisplayName("Process #4 name")]
        public string ProcessName4
        {
            get { return _processName4; }
            set
            {
                _processName4 = value;
                OnPropertyChanged();
            }
        }

        private string _wspw4;
        [XmlElement(ElementName = "WSPW4")]
        [Category("Processes and Ramp"), DisplayName("Process #4 WSPW")]
        public string Wspw4
        {
            get { return _wspw4; }
            set
            {
                _wspw4 = value;
                OnPropertyChanged();
            }
        }

        public static LayoutFilter Load(string filename = "")
        {
            LayoutFilter ret;

            if (filename != "")
            {
                ProjectManager.LayoutFiltersFileName = filename;
            }

            try
            {
                var xs = new XmlSerializer(typeof(LayoutFilter));
                var sr = new StreamReader(ProjectManager.RootPath + ProjectManager.LayoutFiltersFileName);
                ret = (LayoutFilter)xs.Deserialize(sr);
                sr.Close();
            }
            catch (Exception)
            {
                // File not found: create default settings
                return new LayoutFilter(); //{ FilterName = "Default", Active = false };
            }
            return ret;
        }

        public void Save()
        {
            try
            {
                var xs = new XmlSerializer(typeof(LayoutFilter));
                var sw = new StreamWriter(ProjectManager.RootPath + ProjectManager.LayoutFiltersFileName, false);
                xs.Serialize(sw, this);
                sw.Close();
            }
            catch (Exception)
            {
                //var ed = Application.DocumentManager.MdiActiveDocument.Editor;
                //ed.WriteMessage("\nUnable to save the application filters: {0}", ex);
            }
        }
    }

    /// <summary> Rules </summary>
    public class Rule : BaseEntity
    {
        //Lsds properties
        public static decimal LsdsLayoutIdCurrent = 0;
        public static long LsdsBlockIndexCurrent = 0; // Incremental current block index
        public static byte LsdsBlockAttributeIndexCurrent = 0; // Incremental current blockAttribute index
        public static int LsdsGroupIndexCurrent = 1; // Incremental current blockAttribute index
        public static int LsdsItemIndexCurrent = 0;
        public static int LsdsFrameIndexCurrent = 0;
        public static byte LsdsItemAttributeIndexCurrent = 0;
        public static int LsdsFrameColor = -1;
        public static LsdsParserMode LsdsParserMode = LsdsParserMode.Default;
        public static List<Tuple<string, string[]>> LsdsLayoutCatalogOptions = new List<Tuple<string, string[]>>();
        public static List<Tuple<string, string[]>> LsdsLayoutCatalogSites = new List<Tuple<string, string[]>>();
        public static string[] LsdsTooNameAttributes { get; set; }
        public static bool LsdsEnableCallbackTimer { get; set; }
        public static Type[] LsdsTypeFilterOn { get; set; }
        public static string[] LsdsAttributePatternOn { get; set; }

        public static Rule RuleSample()
        {
            var newLayout = new Rule()
            {
                LayoutCatalogOptions = new[] { "" },
                AttributePatternOn = new[] { "ENTITY_CODE", "ENTITY_CODES", "TOOL_ID" },
                ColorIndex = 191,
                ConfigDictionary = new List<Tuple<string, int?, string>>(),
                FilterBlockAttributesOn = new string[] { },
                isTypeFilterParent = true,
                IncludeNested = true,
                LayerDestination = ".LSDS_DEFAULT_FYSICAL",
                LayerPatternOn = new[] { "0" },
                TypeFilterOn = new[] { Type.GetType("Autodesk.AutoCAD.DatabaseServices.Entity.BlockReference") },
                LineType = "DASHED"
            };
            return newLayout;
        }

        //IRule properties
        public string[] AttributePatternOn { get; set; }
        public string Comment { get; set; }

        [DefaultValue("")]
        public string DxfName { get; set; }
        public List<Tuple<string, int?, string>> ConfigDictionary { get; set; }

        [DefaultValue(null)]
        public string[] FilterBlockAttributesOn { get; set; }
        public long Id { get; set; }

        //public int ID { get; set; }
        public bool isTypeFilterParent { get; set; }
        public bool IncludeNested { get; set; }
        //public bool IncludeTransform { get; set; }

        public string[] LayoutCatalogSite { get; set; }
        public string[] LayoutCatalogOptions { get; set; }
        public Point3D MainPosition { get; set; }
        public string Name { get; set; }
        public int ColorIndex { get; set; }

        public string LayerDestination { get; set; }
        public short LayerTypeId { get; set; }
        public string[] LayerPatternOn { get; set; }
        public string[] LayerPatternOff { get; set; }
        public string LineType { get; set; }
        public int LineTypeScale { get; set; }
        public long LineTypeId { get; set; }

        public string ObjectPattern { get; set; }
        public Point3D Position { get; set; }
        public string ParameterList { get; set; }
        public long StartBlockIndex { get; set; }
        public string[] TooNameAttributes { get; set; }
        public Type[] TypeFilterOn { get; set; }

        /// <summary> Reset indexes </summary>
        public static void RuleIndexesReset()
        {
            LsdsEnableCallbackTimer = false;
            LsdsBlockIndexCurrent = 0;
            LsdsBlockAttributeIndexCurrent = 0;
            LsdsGroupIndexCurrent = 1;
            LsdsItemIndexCurrent = 0;
            LsdsFrameIndexCurrent = 0;
            LsdsItemAttributeIndexCurrent = 0;
            LsdsLayoutCatalogOptions = new List<Tuple<string, string[]>>();
            LsdsLayoutCatalogSites = new List<Tuple<string, string[]>>();
        }

    }


    /// <summary>
    /// This class is instantiated by AutoCAD once and kept alive for the 
    /// duration of the session. If you don't do any one time initialization 
    /// then you should remove this class.
    /// </summary>

    /// <summary> Define properties for tasks </summary>
    //public class TaskLsds : ITask
    //{
    //    public string ProcessName { get; set; }
    //    [DefaultValue(false)]
    //    public bool IsStarted { get; set; }
    //    [DefaultValue(false)]
    //    public bool IsSuccessed { get; set; }
    //    [DefaultValue(0)]
    //    public byte RunOrder { get; set; }
    //    public bool IsExecuted { get; set; }
    //    public Func<bool> Action { get; set; }
    //}

    /// <summary> Layout extensions methods </summary>
    //public static class LayoutExtensionsOld
    //{
    //    public static bool IsLayoutReadOnly(this Layout layout)
    //    {
    //        string[] logicalDrives = System.IO.Directory.GetLogicalDrives();

    //        return logicalDrives.ToList()
    //                    .Any(drive => Application.DocumentManager.Cast<Document>()
    //                        .Where(doc => doc.IsReadOnly)
    //                        .Select(doc => doc.Name.ToLower())
    //                        .Contains((layout.CADFileName.Contains(":") ? layout.CADFileName : drive.Substring(0, 2) + layout.CADFileName).ToLower()));
    //    }

    //    /// <summary> defines if layout contained in open documents collection </summary>
    //    public static bool IsLayoutLoaded(this Layout layout)
    //    {
    //        if (layout == null || String.IsNullOrEmpty(layout.CADFileName)) return false;

    //        var logicalDrives = Directory.GetLogicalDrives();

    //        return logicalDrives.ToList()
    //                    .Any(drive => Application.DocumentManager.Cast<Document>()
    //                        .Select(doc => doc.Name.ToLower())
    //                        .Contains((layout.CADFileName.Contains(":") ? layout.CADFileName : drive.Substring(0, 2) + layout.CADFileName).ToLower()));
    //    }
    //    public static bool IsLinkedMapLoaded(this Layout layout)
    //    {
    //        if (layout == null || String.IsNullOrEmpty(layout.TABFileName)) return false;

    //        var logicalDrives = Directory.GetLogicalDrives();

    //        return logicalDrives.ToList()
    //                    .Any(drive => Application.DocumentManager.Cast<Document>()
    //                        .Select(doc => doc.Name.ToLower())
    //                        .Contains((layout.TABFileName.Contains(":") ? layout.TABFileName : drive.Substring(0, 2) + layout.TABFileName).ToLower()));
    //    }

    //    /// <summary> defines if layout contained in open documents collection </summary>
    //    public static string FindLayoutFullPath(this Layout layout)
    //    {
    //        var result = "";
    //        try
    //        {
    //            var logicalDrives = Directory.GetLogicalDrives();
    //            result = logicalDrives
    //                         .Select(drive => (layout.CADFileName.Contains(":") ? layout.CADFileName : drive.Substring(0, 2) + layout.CADFileName).ToLower())
    //                         .FirstOrDefault(File.Exists);
    //            if (result == null) result = "";
    //        }
    //        catch { }
    //        return result;
    //    }
    //    public static string FindMapFullPath(this Layout layout)
    //    {
    //        var result = "";
    //        try
    //        {
    //            var logicalDrives = Directory.GetLogicalDrives();
    //            result = logicalDrives
    //                         .Select(drive => (layout.TABFileName.Contains(":") ? layout.TABFileName : drive.Substring(0, 2) + layout.TABFileName))
    //                         .FirstOrDefault(File.Exists);
    //            if (result == null) result = "";
    //        }
    //        catch { }
    //        return result;
    //    }

    //    /// <summary> defines if layout contained in open documents collection </summary>
    //    public static bool IsPropertiesValid(this Layout layout)
    //    {
    //        return layout.InvalidProperties.Count == 0;
    //    }

    //    /// <summary> defines if layout contained in open documents collection </summary>
    //    public static bool IsPropertiesChanged(this Layout layout)
    //    {
    //        return layout.ChangedProperties.Count > 0;
    //    }

    //    /// <summary> Test </summary>
    //    public static ObservableCollection<Layout> GetList(this ObservableCollection<Layout> query, Func<Layout, bool> expression)
    //    {
    //        return new ObservableCollection<Layout>(query.Where(expression));
    //    }

    //    /// <summary> Is Lsds Compatible </summary>
    //    public static bool IsLsdsCompatible(this Layout layout)
    //    {
    //        if (layout == null) return false;

    //        //var doc = Application.DocumentManager.MdiActiveDocument;
    //        //var db = HostApplicationServices.WorkingDatabase; //doc.Database;
    //        //var ed = doc.Editor;

    //        //dynamic bt = db.BlockTableId;
    //        //ed.WriteMessage("\n*** BlockTableRecords in this DWG ***");
    //        //foreach (dynamic btr in bt)
    //        //    ed.WriteMessage("\n" + btr.Name);

    //        dynamic ms = SymbolUtilityServices.GetBlockModelSpaceId(HostApplicationServices.WorkingDatabase);

    //        var currentObjectIds =
    //            ((IEnumerable<dynamic>)ms)
    //                .Where(ent => ent.IsKindOf(typeof(BlockReference)))
    //                .Select(x => x.ObjectId)
    //                .Cast<ObjectId>()
    //                .ToList();

    //        currentObjectIds.XGetObjects(null, Rule.LsdsAttributePatternOn);

    //        return currentObjectIds.Count != 0;

    //        //return btr.Cast<ObjectId>()
    //        //        .Select(n => n.GetObject(OpenMode.ForRead) as Entity)
    //        //        .Where(n => n != null && !(n is AttributeDefinition))
    //        //        .Max(n => n.GeometricExtents.MaxPoint.Y);

    //        //var lineStartPoints =
    //        //  from ent in (IEnumerable<dynamic>)ms
    //        //  where ent.IsKindOf(typeof(Line))
    //        //  select ent.StartPoint;

    //        //foreach (Point3d start in lineStartPoints)
    //        //    ed.WriteMessage("\n" + start.ToString());

    //        //// LINQ query - all entities on layer '0'
    //        //ed.WriteMessage("\n\n*** Entities on Layer 0 ***");

    //        //var entsOnLayer0 =
    //        //  from ent in (IEnumerable<dynamic>)ms
    //        //  where ent.Layer == "0"
    //        //  select ent;
            
    //        //foreach (dynamic e in entsOnLayer0)
    //        //    ed.WriteMessage("\nHandle=" + e.Handle.ToString() + ", ObjectId=" +
    //        //        ((ObjectId)e).ToString() + ", Class=" + e.ToString());

    //        //ed.WriteMessage("\n\n");
    //        //// Using LINQ with selection sets

    //        //PromptSelectionResult res = ed.GetSelection();
    //        //if (res.Status != PromptStatus.OK)
    //        //    return;

    //        //// Select all entities in selection set that have an object
    //        //// called "MyDict" in their extension dictionary

    //        //var extDicts =
    //        //  from ent in res.Value.GetObjectIds().Cast<dynamic>()
    //        //  where ent.ExtensionDictionary != ObjectId.Null &&
    //        //    ent.ExtensionDictionary.Contains("MyDict")
    //        //  select ent.ExtensionDictionary.Item("MyDict");

    //        //// Erase our dictionary
    //        //foreach (dynamic myDict in extDicts)
    //        //    myDict.Erase();
    //        //return true;
    //    }
    //}
}
