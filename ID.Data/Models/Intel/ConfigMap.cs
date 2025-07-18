using System.Data.Entity.ModelConfiguration;
using Intellidesk.Data.Models.Cad;

namespace Intellidesk.Data.Models.Mapping
{
    public class ConfigMap : EntityTypeConfiguration<Config>
    {
        public ConfigMap()
        {
            // Primary Key
            this.HasKey(t => new { t.ConfigSetName, t.ParameterName });

            // Properties
            this.Property(t => t.ConfigSetName)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.ParameterName)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.Str1)
                .HasMaxLength(500);

            this.Property(t => t.Str2)
                .HasMaxLength(500);

            this.Property(t => t.Str3)
                .HasMaxLength(500);

            this.Property(t => t.Str4)
                .HasMaxLength(500);

            this.Property(t => t.LongStr)
                .HasMaxLength(1000);

            // Table & Column Mappings
            this.ToTable("Configs");
            this.Property(t => t.ConfigSetName).HasColumnName("ConfigSetName");
            this.Property(t => t.ParameterName).HasColumnName("ParameterName");
            this.Property(t => t.Int1).HasColumnName("Int1");
            this.Property(t => t.Int2).HasColumnName("Int2");
            this.Property(t => t.Int3).HasColumnName("Int3");
            this.Property(t => t.Int4).HasColumnName("Int4");
            this.Property(t => t.Float1).HasColumnName("Float1");
            this.Property(t => t.Float2).HasColumnName("Float2");
            this.Property(t => t.Float3).HasColumnName("Float3");
            this.Property(t => t.Float4).HasColumnName("Float4");
            this.Property(t => t.Date1).HasColumnName("Date1");
            this.Property(t => t.Date2).HasColumnName("Date2");
            this.Property(t => t.Date3).HasColumnName("Date3");
            this.Property(t => t.Date4).HasColumnName("Date4");
            this.Property(t => t.Str1).HasColumnName("Str1");
            this.Property(t => t.Str2).HasColumnName("Str2");
            this.Property(t => t.Str3).HasColumnName("Str3");
            this.Property(t => t.Str4).HasColumnName("Str4");
            this.Property(t => t.LongStr).HasColumnName("LongStr");
        }
    }
}
