using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Intellidesk.Data.Models.Cad
{
    public class BlockItemMap : EntityTypeConfiguration<BlockRef>
    {
        public BlockItemMap()
        {
            // Primary Key
            this.HasKey(t => new { t.LayoutId, t.TabIndex });

            // Properties
            this.Property(t => t.Title).HasMaxLength(100);
            this.Property(t => t.LayoutId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            this.Property(t => t.TabIndex).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            this.Property(t => t.XrefName).HasMaxLength(500);
            this.Property(t => t.LayerId);
            this.Property(t => t.Handle);

            // Table & Column Mappings
            this.ToTable("BlockItems");
            this.Property(t => t.BlockRefId).HasColumnName("BlockItemId");
            this.Property(t => t.BlockId).HasColumnName("BlockName");
            this.Property(t => t.Title).HasColumnName("Name");
            this.Property(t => t.LayoutId).HasColumnName("LayoutId");
            this.Property(t => t.TabIndex).HasColumnName("TabIndex");
            this.Property(t => t.XrefName).HasColumnName("XrefName");
            this.Property(t => t.LayerId).HasColumnName("LayerId");
            this.Property(t => t.Scale).HasColumnName("Scale");
            this.Property(t => t.Rotation).HasColumnName("Rotation");
            this.Property(t => t.Handle).HasColumnName("Handle");

            // Relationships
            this.HasRequired(t => t.Layout)
                .WithMany(t => t.BlockItems)
                .HasForeignKey(d => d.LayoutId);

        }
    }
}
