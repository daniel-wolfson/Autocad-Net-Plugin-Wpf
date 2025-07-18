using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using Intellidesk.Data.Models.Cad;

namespace Intellidesk.Data.Models.Mapping
{
    public class BlockAttributeMap : EntityTypeConfiguration<BlockAttributeDefinition>
    {
        public BlockAttributeMap()
        {
            // Primary Key
            this.HasKey(t => new { t.LayoutId, t.BlockIndex, t.BlockAttributeIndex });

            // Properties
            this.Property(t => t.LayoutId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            this.Property(t => t.BlockIndex).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            this.Property(t => t.BlockAttributeIndex).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            this.Property(t => t.BlockAttributeName).HasMaxLength(100);
            this.Property(t => t.BlockAttributeValue).HasMaxLength(1000);

            // Table & Column Mappings
            this.ToTable("BlockAttributes");
            this.Property(t => t.BlockId).HasColumnName("BlockID");
            this.Property(t => t.BlockAttributeId).HasColumnName("BlockAttributeId");
            this.Property(t => t.LayoutId).HasColumnName("LayoutId");
            this.Property(t => t.BlockIndex).HasColumnName("BlockIndex");
            this.Property(t => t.BlockAttributeIndex).HasColumnName("BlockAttributeIndex");
            this.Property(t => t.BlockAttributeName).HasColumnName("BlockAttributeName");
            this.Property(t => t.BlockAttributeValue).HasColumnName("BlockAttributeValue");

            // Relationships
            this.HasRequired(t => t.Block)
                .WithMany(t => t.BlockAttributes)
                .HasForeignKey(d => new { d.LayoutId, d.BlockIndex });

        }
    }
}
