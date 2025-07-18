using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using Intellidesk.Data.Models.Cad;

namespace Intellidesk.Data.Models.Mapping
{
    public class BlockMap : EntityTypeConfiguration<BlockDefinition>
    {
        public BlockMap()
        {
            // Primary Key
            this.HasKey(t => new { LayoutID = t.LayoutId, t.BlockIndex });

            // Properties
            this.Property(t => t.LayoutId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            this.Property(t => t.BlockIndex).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            this.Property(t => t.BlockName).HasMaxLength(500);
            this.Property(t => t.BlockXrefName).HasMaxLength(500);
            this.Property(t => t.BlockHandle).HasMaxLength(25);

            // Table & Column Mappings
            this.ToTable("Blocks");
            this.Property(t => t.BlockId).HasColumnName("BlockID");
            this.Property(t => t.LayoutId).HasColumnName("LayoutId");
            this.Property(t => t.BlockIndex).HasColumnName("BlockIndex");
            this.Property(t => t.BlockName).HasColumnName("BlockName");
            this.Property(t => t.BlockXrefName).HasColumnName("BlockXrefName");
            this.Property(t => t.BlockHandle).HasColumnName("BlockHandle");

            // Relationships
            this.HasRequired(t => t.Layout)
                .WithMany(t => t.Blocks)
                .HasForeignKey(d => d.LayoutId);

        }
    }
}
