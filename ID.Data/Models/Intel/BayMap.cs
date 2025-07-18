using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using Intellidesk.Data.Models.Cad;

namespace Intellidesk.Data.Models.Mapping
{
    public class BayMap : EntityTypeConfiguration<Bay>
    {
        public BayMap()
        {
            // Primary Key
            this.HasKey(t => new { t.LayoutID, t.BayId });

            // Properties
            this.Property(t => t.LayoutID)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.BayName)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.BaySide)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.BayPart)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.BayId)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            // Table & Column Mappings
            this.ToTable("Bays");
            this.Property(t => t.LayoutID).HasColumnName("LayoutId");
            this.Property(t => t.BayName).HasColumnName("BayName");
            this.Property(t => t.BaySide).HasColumnName("BaySide");
            this.Property(t => t.BayPart).HasColumnName("BayPart");
            this.Property(t => t.Xmin).HasColumnName("Xmin");
            this.Property(t => t.Ymin).HasColumnName("Ymin");
            this.Property(t => t.Xmax).HasColumnName("Xmax");
            this.Property(t => t.Ymax).HasColumnName("Ymax");
            this.Property(t => t.BayId).HasColumnName("BayId");
            this.Property(t => t.Layout_LayoutID).HasColumnName("LayoutLayoutId");

            // Relationships
            this.HasRequired(t => t.Layout)
                .WithMany(t => t.Bays)
                .HasForeignKey(d => d.Layout_LayoutID);

        }
    }
}
