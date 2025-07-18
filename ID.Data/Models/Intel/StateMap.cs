using System.Data.Entity.ModelConfiguration;
using Intellidesk.Data.Models.FiberOptics;

namespace Intellidesk.Data.Models.Mapping
{
    public class StateMap : EntityTypeConfiguration<State>
    {
        public StateMap()
        {
            // Primary Key
            this.HasKey(t => new { t.As_made, t.StateId });

            // Properties
            this.Property(t => t.As_made).IsRequired().HasMaxLength(20);
            this.Property(t => t.FileName).HasMaxLength(50);
            this.Property(t => t.LayoutId).IsRequired();
            this.Property(t => t.Supplier).IsRequired();
            this.Property(t => t.Data_Search).IsRequired();
            this.Property(t => t.Status).IsRequired();

            // Table & Column Mappings
            this.ToTable("States");
            this.Property(t => t.As_made).HasColumnName("As_made");
            this.Property(t => t.Latitude).HasColumnName("Latitude");
            this.Property(t => t.Longitude).HasColumnName("Longitude");
            this.Property(t => t.FileName).HasColumnName("FileName");
            this.Property(t => t.DateCreated).HasColumnName("DateCreated");
            this.Property(t => t.LayoutId).HasColumnName("LayoutId");
            this.Property(t => t.StateId).HasColumnName("StateId");
            this.Property(t => t.Supplier).HasColumnName("Supplier");
            this.Property(t => t.CoordX).HasColumnName("CoordX");
            this.Property(t => t.CoordY).HasColumnName("CoordY");
            this.Property(t => t.Data_Search).HasColumnName("Data_Search");
            this.Property(t => t.Status).HasColumnName("Status");
            this.Property(t => t.Layout_LayoutID).HasColumnName("LayoutLayoutId");

            // Relationships
            this.HasRequired(t => t.Layout)
                .WithMany(t => t.States)
                .HasForeignKey(d => d.Layout_LayoutID);
        }
    }
}
