using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using Intellidesk.Data.Models.Cad;

namespace Intellidesk.Data.Models.Mapping
{
    public class FrameMap : EntityTypeConfiguration<Frame>
    {
        public FrameMap()
        {
            // Primary Key
            this.HasKey(t => new { t.LayoutID, t.BlockIndex, t.FrameIndex });

            // Properties
            this.Property(t => t.LayoutID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            this.Property(t => t.BlockIndex).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            this.Property(t => t.FrameIndex).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            // Table & Column Mappings
            this.ToTable("Frames");
            this.Property(t => t.BlockID).HasColumnName("BlockID");
            this.Property(t => t.FrameID).HasColumnName("FrameID");
            this.Property(t => t.LayoutID).HasColumnName("LayoutId");
            this.Property(t => t.BlockIndex).HasColumnName("BlockIndex");
            this.Property(t => t.FrameIndex).HasColumnName("FrameIndex");
            this.Property(t => t.FrameTypeID).HasColumnName("FrameTypeID");
            this.Property(t => t.Xmin).HasColumnName("Xmin");
            this.Property(t => t.Ymin).HasColumnName("Ymin");
            this.Property(t => t.Xmax).HasColumnName("Xmax");
            this.Property(t => t.Ymax).HasColumnName("Ymax");

            // Relationships
            this.HasRequired(t => t.Block)
                .WithMany(t => t.Frames)
                .HasForeignKey(d => new { d.LayoutID, d.BlockIndex });

        }
    }
}
