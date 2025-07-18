using System.Data.Entity;
using Intellidesk.Data.Models.FiberOptics;
using Intellidesk.Data.Services;

namespace Intellidesk.Data.Models.Cad
{
    public partial class AppDbContext : Repositories.EF6.DataContext.DataContext
    {
        public AppDbContext()
            : base(ConfigurationManager<AppDbContext>.ConnectionString)
        //: base("name=AppDbContext")
        {
        }

        public virtual DbSet<Bay> Bays { get; set; }
        public virtual DbSet<BlockAttribute> BlockAttributes { get; set; }
        public virtual DbSet<Block> Blocks { get; set; }
        public virtual DbSet<Config> Configs { get; set; }
        public virtual DbSet<Filter> Filters { get; set; }
        public virtual DbSet<Frame> Frames { get; set; }
        public virtual DbSet<ItemAttribute> ItemAttributes { get; set; }
        public virtual DbSet<Item> Items { get; set; }
        public virtual DbSet<Layout> Layouts { get; set; }
        public virtual DbSet<Page> Pages { get; set; }
        public virtual DbSet<Rule> Rules { get; set; }
        public virtual DbSet<State> States { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserSetting> UserSettings { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Bay>()
                .Property(e => e.LayoutID)
                .HasPrecision(6, 0);

            modelBuilder.Entity<Bay>()
                .Property(e => e.BayName)
                .IsUnicode(false);

            modelBuilder.Entity<Bay>()
                .Property(e => e.BaySide)
                .IsUnicode(false);

            modelBuilder.Entity<Bay>()
                .Property(e => e.BayPart)
                .IsUnicode(false);

            modelBuilder.Entity<Bay>()
                .Property(e => e.Layout_LayoutID)
                .HasPrecision(6, 0);

            modelBuilder.Entity<BlockAttribute>()
                .Property(e => e.BlockID)
                .HasPrecision(15, 0);

            modelBuilder.Entity<BlockAttribute>()
                .Property(e => e.BlockAttributeID)
                .HasPrecision(22, 0);

            modelBuilder.Entity<BlockAttribute>()
                .Property(e => e.LayoutID)
                .HasPrecision(6, 0);

            modelBuilder.Entity<BlockAttribute>()
                .Property(e => e.BlockIndex)
                .HasPrecision(6, 0);

            modelBuilder.Entity<BlockAttribute>()
                .Property(e => e.BlockAttributeIndex)
                .HasPrecision(6, 0);

            modelBuilder.Entity<BlockAttribute>()
                .Property(e => e.BlockAttributeName)
                .IsUnicode(false);

            modelBuilder.Entity<BlockAttribute>()
                .Property(e => e.BlockAttributeValue)
                .IsUnicode(false);

            modelBuilder.Entity<Block>()
                .Property(e => e.BlockID)
                .HasPrecision(15, 0);

            modelBuilder.Entity<Block>()
                .Property(e => e.LayoutID)
                .HasPrecision(6, 0);

            modelBuilder.Entity<Block>()
                .Property(e => e.BlockIndex)
                .HasPrecision(6, 0);

            modelBuilder.Entity<Block>()
                .Property(e => e.BlockName)
                .IsUnicode(false);

            modelBuilder.Entity<Block>()
                .Property(e => e.BlockXrefName)
                .IsUnicode(false);

            modelBuilder.Entity<Block>()
                .Property(e => e.BlockHandle)
                .IsUnicode(false);

            modelBuilder.Entity<Block>()
                .Property(e => e.Layout_LayoutID)
                .HasPrecision(6, 0);

            modelBuilder.Entity<Block>().HasMany(e => e.BlockAttributes)
                .WithRequired(e => e.Block)
                .HasForeignKey(e => new { e.LayoutID, e.BlockIndex })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Block>().HasMany(e => e.Frames).WithRequired(e => e.Block)
                .HasForeignKey(e => new { e.LayoutID, e.BlockIndex }).WillCascadeOnDelete(false);
            modelBuilder.Entity<Config>().Property(e => e.ConfigSetName).IsUnicode(false);
            modelBuilder.Entity<Config>().Property(e => e.ParameterName).IsUnicode(false);
            modelBuilder.Entity<Config>().Property(e => e.Str1).IsUnicode(false);
            modelBuilder.Entity<Config>().Property(e => e.Str2).IsUnicode(false);
            modelBuilder.Entity<Config>().Property(e => e.Str3).IsUnicode(false);
            modelBuilder.Entity<Config>().Property(e => e.Str4).IsUnicode(false);
            modelBuilder.Entity<Config>().Property(e => e.LongStr).IsUnicode(false);
            
            modelBuilder.Entity<Filter>().Property(e => e.LayoutID).HasPrecision(6, 0);
            modelBuilder.Entity<Filter>().Property(e => e.AccessType).IsUnicode(false);
            modelBuilder.Entity<Filter>().Property(e => e.Active);
            modelBuilder.Entity<Filter>().Property(e => e.CADFileName).IsUnicode(false);
            modelBuilder.Entity<Filter>().Property(e => e.DateModified);
            modelBuilder.Entity<Filter>().Property(e => e.FSA);
            modelBuilder.Entity<Filter>().Property(e => e.LayoutName).IsUnicode(false);
            modelBuilder.Entity<Filter>().Property(e => e.Layout_LayoutID).HasPrecision(6, 0);

            modelBuilder.Entity<Frame>()
                .Property(e => e.BlockID)
                .HasPrecision(15, 0);

            modelBuilder.Entity<Frame>()
                .Property(e => e.FrameID)
                .HasPrecision(22, 0);

            modelBuilder.Entity<Frame>()
                .Property(e => e.LayoutID)
                .HasPrecision(6, 0);

            modelBuilder.Entity<Frame>()
                .Property(e => e.BlockIndex)
                .HasPrecision(6, 0);

            modelBuilder.Entity<Frame>()
                .Property(e => e.FrameIndex)
                .HasPrecision(6, 0);

            modelBuilder.Entity<ItemAttribute>()
                .Property(e => e.ItemID)
                .HasPrecision(15, 0);

            modelBuilder.Entity<ItemAttribute>()
                .Property(e => e.ItemAttributeID)
                .HasPrecision(22, 0);

            modelBuilder.Entity<ItemAttribute>()
                .Property(e => e.LayoutID)
                .HasPrecision(6, 0);

            modelBuilder.Entity<ItemAttribute>()
                .Property(e => e.ItemIndex)
                .HasPrecision(6, 0);

            modelBuilder.Entity<ItemAttribute>()
                .Property(e => e.ItemAttributeIndex)
                .HasPrecision(6, 0);

            modelBuilder.Entity<ItemAttribute>()
                .Property(e => e.ItemAttributeName)
                .IsUnicode(false);

            modelBuilder.Entity<ItemAttribute>()
                .Property(e => e.ItemAttributeValue)
                .IsUnicode(false);

            modelBuilder.Entity<ItemAttribute>()
                .Property(e => e.Item_LayoutID)
                .HasPrecision(6, 0);

            modelBuilder.Entity<ItemAttribute>()
                .Property(e => e.Item_ItemIndex)
                .HasPrecision(6, 0);

            modelBuilder.Entity<ItemAttribute>()
                .Property(e => e.Item_ItemAttributeIndex)
                .HasPrecision(6, 0);

            modelBuilder.Entity<Item>()
                .Property(e => e.ItemID)
                .HasPrecision(15, 0);

            modelBuilder.Entity<Item>()
                .Property(e => e.ItemName)
                .IsUnicode(false);

            modelBuilder.Entity<Item>()
                .Property(e => e.LayoutID)
                .HasPrecision(6, 0);

            modelBuilder.Entity<Item>()
                .Property(e => e.ItemIndex)
                .HasPrecision(6, 0);

            modelBuilder.Entity<Item>()
                .Property(e => e.ItemAttributeIndex)
                .HasPrecision(6, 0);

            modelBuilder.Entity<Item>()
                .Property(e => e.ItemAttributeName)
                .IsUnicode(false);

            modelBuilder.Entity<Item>()
                .Property(e => e.ItemAttributeID)
                .HasPrecision(22, 0);

            modelBuilder.Entity<Item>()
                .Property(e => e.BlockID)
                .HasPrecision(15, 0);

            modelBuilder.Entity<Item>()
                .Property(e => e.BlockIndex)
                .HasPrecision(6, 0);

            modelBuilder.Entity<Item>()
                .Property(e => e.BlockName)
                .IsUnicode(false);

            modelBuilder.Entity<Item>()
                .Property(e => e.XrefName)
                .IsUnicode(false);

            modelBuilder.Entity<Item>()
                .Property(e => e.LayerName)
                .IsUnicode(false);

            modelBuilder.Entity<Item>()
                .Property(e => e.ItemHandle)
                .IsUnicode(false);

            modelBuilder.Entity<Item>()
                .Property(e => e.Layout_LayoutID)
                .HasPrecision(6, 0);

            modelBuilder.Entity<Item>()
                .HasMany(e => e.ItemAttributes)
                .WithRequired(e => e.Item)
                .HasForeignKey(e => new { e.Item_LayoutID, e.Item_ItemIndex, e.Item_ItemAttributeIndex })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Layout>()
                .Property(e => e.LayoutID)
                .HasPrecision(6, 0);

            modelBuilder.Entity<Layout>()
                .Property(e => e.LayoutName)
                .IsUnicode(false);

            modelBuilder.Entity<Layout>()
                .Property(e => e.LayoutType)
                .IsUnicode(false);

            modelBuilder.Entity<Layout>()
                .Property(e => e.LayoutContents)
                .IsUnicode(false);

            modelBuilder.Entity<Layout>()
                .Property(e => e.AccessType)
                .IsUnicode(false);

            modelBuilder.Entity<Layout>()
                .Property(e => e.LayoutVersion)
                .IsUnicode(false);

            modelBuilder.Entity<Layout>()
                .Property(e => e.Comment)
                .IsUnicode(false);

            modelBuilder.Entity<Layout>()
                .Property(e => e.SiteName)
                .IsUnicode(false);

            modelBuilder.Entity<Layout>()
                .Property(e => e.BuildingLevels)
                .IsUnicode(false);

            modelBuilder.Entity<Layout>()
                .Property(e => e.ProcessName1)
                .IsUnicode(false);

            modelBuilder.Entity<Layout>()
                .Property(e => e.ProcessName2)
                .IsUnicode(false);

            modelBuilder.Entity<Layout>()
                .Property(e => e.ProcessName3)
                .IsUnicode(false);

            modelBuilder.Entity<Layout>()
                .Property(e => e.ProcessName4)
                .IsUnicode(false);

            modelBuilder.Entity<Layout>()
                .Property(e => e.CADFileName)
                .IsUnicode(false);

            modelBuilder.Entity<Layout>()
                .Property(e => e.ConfigSetName)
                .IsUnicode(false);

            modelBuilder.Entity<Layout>()
                .Property(e => e.Param1)
                .IsFixedLength();

            modelBuilder.Entity<Layout>()
                .Property(e => e.Param2)
                .IsFixedLength();

            modelBuilder.Entity<Layout>()
                .Property(e => e.Param3)
                .IsFixedLength();

            modelBuilder.Entity<Layout>()
                .Property(e => e.Param4)
                .IsFixedLength();

            modelBuilder.Entity<Layout>()
                .HasMany(e => e.Bays)
                .WithRequired(e => e.Layout)
                .HasForeignKey(e => e.Layout_LayoutID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Layout>()
                .HasMany(e => e.Blocks)
                .WithRequired(e => e.Layout)
                .HasForeignKey(e => e.Layout_LayoutID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Layout>()
                .HasMany(e => e.Filters)
                .WithRequired(e => e.Layout)
                .HasForeignKey(e => e.Layout_LayoutID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Layout>()
                .HasMany(e => e.Items)
                .WithRequired(e => e.Layout)
                .HasForeignKey(e => e.Layout_LayoutID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Layout>()
                .HasMany(e => e.Rules)
                .WithRequired(e => e.Layout)
                .HasForeignKey(e => e.Layout_LayoutID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Layout>()
                .HasMany(e => e.States)
                .WithRequired(e => e.Layout)
                .HasForeignKey(e => e.Layout_LayoutID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Layout>()
                .HasMany(e => e.UserSettings)
                .WithRequired(e => e.Layout)
                .HasForeignKey(e => e.Layout_LayoutID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Page>()
                .Property(e => e.URL)
                .IsUnicode(false);

            modelBuilder.Entity<Page>()
                .Property(e => e.AttributeURL)
                .IsUnicode(false);

            modelBuilder.Entity<Page>()
                .Property(e => e.ID_System)
                .IsUnicode(false);

            modelBuilder.Entity<Rule>()
                .Property(e => e.LayoutId)
                .HasPrecision(6, 0);

            modelBuilder.Entity<Rule>()
                .Property(e => e.Layout_LayoutID)
                .HasPrecision(6, 0);

            modelBuilder.Entity<State>()
                .Property(e => e.Layout_LayoutID)
                .HasPrecision(6, 0);

            modelBuilder.Entity<User>()
                .HasMany(e => e.UserSettings)
                .WithRequired(e => e.User)
                .HasForeignKey(e => new { e.UserSettingUser_UserSetting_UserId, e.UserSettingUser_UserSetting_UserSettingId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<UserSetting>()
                .Property(e => e.LayoutId)
                .HasPrecision(6, 0);

            modelBuilder.Entity<UserSetting>()
                .Property(e => e.Layout_LayoutID)
                .HasPrecision(6, 0);
        }

    }
}
