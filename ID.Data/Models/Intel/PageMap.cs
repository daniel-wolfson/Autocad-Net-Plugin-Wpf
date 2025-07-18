using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Intellidesk.Data.Models.Mapping
{
    public class PageMap : EntityTypeConfiguration<Page>
    {
        public PageMap()
        {
            // Primary Key
            this.HasKey(t => new { t.ID_Page, t.Title, t.FullName, t.URL, t.isActive, t.isPrinted, t.LevelAgent, t.ID_System });

            // Properties
            this.Property(t => t.ID_Page)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.Title)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.FullName)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.URL)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.AttributeURL)
                .HasMaxLength(50);

            this.Property(t => t.Tooltip)
                .HasMaxLength(50);

            this.Property(t => t.LevelAgent)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.ID_System)
                .IsRequired()
                .HasMaxLength(2);

            // Table & Column Mappings
            this.ToTable("Pages");
            this.Property(t => t.ID_Page).HasColumnName("ID_Page");
            this.Property(t => t.Title).HasColumnName("Title");
            this.Property(t => t.FullName).HasColumnName("FullName");
            this.Property(t => t.URL).HasColumnName("URL");
            this.Property(t => t.AttributeURL).HasColumnName("AttributeURL");
            this.Property(t => t.Tooltip).HasColumnName("Tooltip");
            this.Property(t => t.isActive).HasColumnName("isActive");
            this.Property(t => t.isPrinted).HasColumnName("isPrinted");
            this.Property(t => t.Ordered).HasColumnName("Ordered");
            this.Property(t => t.LevelAgent).HasColumnName("LevelAgent");
            this.Property(t => t.ID_System).HasColumnName("ID_System");
        }
    }
}
