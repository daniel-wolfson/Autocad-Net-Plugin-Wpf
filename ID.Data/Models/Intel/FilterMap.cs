using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using Intellidesk.Data.Models.Cad;

namespace Intellidesk.Data.Models.Mapping
{
    public class FilterMap : EntityTypeConfiguration<Filter>
    {
        public FilterMap()
        {
            // Primary Key
            this.HasKey(t => t.FilterId);

            // Properties
            this.Property(t => t.LayoutID)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            // Properties
            this.Property(t => t.AccessType).IsRequired();
            this.Property(t => t.FilterName).IsRequired();
            this.Property(t => t.LayoutName).IsRequired();
            this.Property(t => t.LayoutType).IsRequired();
            this.Property(t => t.LayoutContents).IsRequired();
            this.Property(t => t.LayoutVersion).IsRequired();
            this.Property(t => t.Comment).IsRequired();
            this.Property(t => t.SiteName).IsRequired();
            this.Property(t => t.BuildingLevels).IsRequired();
            this.Property(t => t.CADFileName).IsRequired();
            this.Property(t => t.CreatedBy).IsRequired();
            this.Property(t => t.ModifiedBy).IsRequired();

            // Table & Column Mappings
            this.ToTable("Filters");
            this.Property(t => t.FilterId).HasColumnName("FilterId");
            this.Property(t => t.Active).HasColumnName("Active");
            this.Property(t => t.AccessType).HasColumnName("AccessType");
            this.Property(t => t.FilterName).HasColumnName("FilterName");
            this.Property(t => t.FSA).HasColumnName("FSA");
            this.Property(t => t.LayoutName).HasColumnName("LayoutName");
            this.Property(t => t.LayoutType).HasColumnName("LayoutType");
            this.Property(t => t.LayoutContents).HasColumnName("LayoutContents");
            this.Property(t => t.LayoutVersion).HasColumnName("LayoutVersion");
            this.Property(t => t.Comment).HasColumnName("Comment");
            this.Property(t => t.SiteName).HasColumnName("SiteName");
            this.Property(t => t.BuildingLevels).HasColumnName("BuildingLevels");
            this.Property(t => t.CADFileName).HasColumnName("CADFileName");
            this.Property(t => t.CreatedBy).HasColumnName("CreatedBy");
            this.Property(t => t.DateCreated).HasColumnName("DateCreated");
            this.Property(t => t.ModifiedBy).HasColumnName("ModifiedBy");
            this.Property(t => t.DateModified).HasColumnName("DateModified");
            this.Property(t => t.LayoutState).HasColumnName("LayoutState");
            this.Property(t => t.LayoutID).HasColumnName("LayoutId");
            this.Property(t => t.Layout_LayoutID).HasColumnName("LayoutLayoutId");

            // Relationships
            this.HasRequired(t => t.Layout)
                .WithMany(t => t.Filters)
                .HasForeignKey(d => d.Layout_LayoutID);

        }
    }
}
