using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using Intellidesk.Data.Models.Entities;

namespace Intellidesk.Data.Models.Mapping
{
    public class ClosureMap : EntityTypeConfiguration<Closure>
    {
        public ClosureMap()
        {
            // Primary Key
            this.HasKey(t => new { t.ClosureId });

            // Properties
            this.Property(t => t.ClosureId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            this.Property(t => t.Title).HasMaxLength(500);
            this.Property(t => t.Handle);

            // Table & Column Mappings
            this.ToTable("Closures");
            this.Property(t => t.ClosureId).HasColumnName("ClosureId");
            this.Property(t => t.Title).HasColumnName("Name");
            this.Property(t => t.Handle).HasColumnName("Handle");

            // Relationships
            this.HasRequired(t => t.Layout)
                .WithMany(t => t.Closures)
                .HasForeignKey(d => d.ClosureId);

        }
    }
}
