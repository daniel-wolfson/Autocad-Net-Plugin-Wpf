using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using Intellidesk.Data.Models.Cad;

namespace Intellidesk.Data.Models.Mapping
{
    public class RuleMap : EntityTypeConfiguration<Rule>
    {
        public RuleMap()
        {
            // Primary Key
            this.HasKey(t => new { t.RuleId, t.LayoutId });

            // Properties
            this.Property(t => t.RuleId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            this.Property(t => t.LayerDestination).IsRequired();
            this.Property(t => t.LineType).IsRequired();
            this.Property(t => t.Comment).IsRequired();
            this.Property(t => t.LayoutId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            this.Property(t => t.Name).HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("Rules");
            this.Property(t => t.RuleId).HasColumnName("RuleId");
            this.Property(t => t.AttributePatternOn_Data).HasColumnName("AttributePatternOn_Data");
            this.Property(t => t.ColorIndex).HasColumnName("ColorIndex");
            this.Property(t => t.FilterBlockAttributesOn_Data).HasColumnName("FilterBlockAttributesOn_Data");
            this.Property(t => t.IncludeNested).HasColumnName("IncludeNested");
            this.Property(t => t.isTypeFilterParent).HasColumnName("isTypeFilterParent");
            this.Property(t => t.LayerDestination).HasColumnName("LayerDestination");
            this.Property(t => t.LayerPatternOn_Data).HasColumnName("LayerPatternOn_Data");
            this.Property(t => t.TypeFilterOn_Data).HasColumnName("TypeFilterOn_Data");
            this.Property(t => t.LineType).HasColumnName("LineType");
            this.Property(t => t.Comment).HasColumnName("Comment");
            this.Property(t => t.LayerPatternOff_Data).HasColumnName("LayerPatternOff_Data");
            this.Property(t => t.LayoutId).HasColumnName("LayoutId");
            this.Property(t => t.Name).HasColumnName("Name");
            this.Property(t => t.Position_X).HasColumnName("Position_X");
            this.Property(t => t.Position_Y).HasColumnName("Position_Y");
            this.Property(t => t.Position_Z).HasColumnName("Position_Z");
            this.Property(t => t.LayoutCatalogSite_Data).HasColumnName("LayoutCatalogSite_Data");
            this.Property(t => t.LayoutCatalogOptions_Data).HasColumnName("LayoutCatalogOptions_Data");
            this.Property(t => t.TooNameAttributes_Data).HasColumnName("TooNameAttributes_Data");
            this.Property(t => t.Layout_LayoutID).HasColumnName("LayoutLayoutId");

            // Relationships
            this.HasRequired(t => t.Layout)
                .WithMany(t => t.Rules)
                .HasForeignKey(d => d.Layout_LayoutID);
        }
    }
}
