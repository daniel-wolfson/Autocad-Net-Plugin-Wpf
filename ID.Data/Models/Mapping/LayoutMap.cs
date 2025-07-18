using System.Data.Entity.ModelConfiguration;
using Intellidesk.Data.Models.Cad;

namespace Intellidesk.Data.Models.Mapping
{
    public class LayoutMap : EntityTypeConfiguration<ILayout>
    {
        public LayoutMap()
        {
            // Primary Key
            this.HasKey(t => t.LayoutID);

            // Properties
            this.Property(t => t.LayoutName).HasMaxLength(500);
            this.Property(t => t.LayoutType).HasMaxLength(25);
            this.Property(t => t.LayoutContents).HasMaxLength(500);
            this.Property(t => t.AccessType).HasMaxLength(25);
            this.Property(t => t.LayoutVersion).HasMaxLength(25);
            this.Property(t => t.Comment).HasMaxLength(500);
            this.Property(t => t.SiteName).HasMaxLength(25);
            this.Property(t => t.BuildingLevels).HasMaxLength(1000);
            this.Property(t => t.ProcessName1).HasMaxLength(25);
            this.Property(t => t.ProcessName2).HasMaxLength(25);
            this.Property(t => t.ProcessName3).HasMaxLength(25);
            this.Property(t => t.ProcessName4).HasMaxLength(25);
            this.Property(t => t.CADFileName).HasMaxLength(500);
            this.Property(t => t.ConfigSetName).HasMaxLength(50);
            this.Property(t => t.Param1).IsFixedLength().HasMaxLength(25);
            this.Property(t => t.Param2).IsFixedLength().HasMaxLength(25);
            this.Property(t => t.Param3).IsFixedLength().HasMaxLength(25);
            this.Property(t => t.Param4).IsFixedLength().HasMaxLength(25);

            // Table & Column Mappings
            this.ToTable("Layouts");
            this.Property(t => t.LayoutID).HasColumnName("LayoutId");
            this.Property(t => t.LayoutName).HasColumnName("LayoutName");
            this.Property(t => t.LayoutType).HasColumnName("LayoutType");
            this.Property(t => t.LayoutContents).HasColumnName("LayoutContents");
            this.Property(t => t.AccessType).HasColumnName("AccessType");
            this.Property(t => t.LayoutVersion).HasColumnName("LayoutVersion");
            this.Property(t => t.Comment).HasColumnName("Comment");
            this.Property(t => t.SiteName).HasColumnName("SiteName");
            this.Property(t => t.BuildingLevels).HasColumnName("BuildingLevels");
            this.Property(t => t.ProcessName1).HasColumnName("ProcessName1");
            this.Property(t => t.ProcessName2).HasColumnName("ProcessName2");
            this.Property(t => t.ProcessName3).HasColumnName("ProcessName3");
            this.Property(t => t.ProcessName4).HasColumnName("ProcessName4");
            this.Property(t => t.CADFileName).HasColumnName("CADFileName");
            this.Property(t => t.CreatedBy).HasColumnName("CreatedBy");
            this.Property(t => t.DateCreated).HasColumnName("DateCreated");
            this.Property(t => t.ModifiedBy).HasColumnName("ModifiedBy");
            this.Property(t => t.DateModified).HasColumnName("DateModified");
            this.Property(t => t.LayoutState).HasColumnName("LayoutState");
            this.Property(t => t.FSA).HasColumnName("FSA");
            this.Property(t => t.ConfigSetName).HasColumnName("ConfigSetName");
            this.Property(t => t.Visible).HasColumnName("Visible");
            this.Property(t => t.TABFileName).HasColumnName("TABFileName");
            this.Property(t => t.Param1).HasColumnName("Param1");
            this.Property(t => t.Param2).HasColumnName("Param2");
            this.Property(t => t.Param3).HasColumnName("Param3");
            this.Property(t => t.Param4).HasColumnName("Param4");
        }
    }
}
