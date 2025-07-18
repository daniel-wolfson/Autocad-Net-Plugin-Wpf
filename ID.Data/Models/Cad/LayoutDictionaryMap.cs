using System.Data.Entity.ModelConfiguration;

namespace Intellidesk.Data.Models.Cad
{
    public class LayoutDictionaryMap : EntityTypeConfiguration<LayoutDictionary>
    {
        public LayoutDictionaryMap()
        {
            // Primary Key
            this.HasKey(t => t.LayoutDicId);

            // Properties
            this.Property(t => t.ConfigSetName).IsRequired();
            this.Property(t => t.ParameterName).IsRequired();
            this.Property(t => t.Key).IsRequired();
            this.Property(t => t.Value).IsRequired();

            // Table & Column Mappings
            this.ToTable("LayoutDictionaries");
            this.Property(t => t.LayoutDicId).HasColumnName("LayoutDicId");
            this.Property(t => t.ParameterName).HasColumnName("ParameterName");
            this.Property(t => t.ConfigSetName).HasColumnName("ConfigSetName");
            this.Property(t => t.Key).HasColumnName("Key");
            this.Property(t => t.Value).HasColumnName("Value");
            //this.Property(t => t.Config_ConfigID).HasColumnName("Config_ConfigID");

            //// Relationships
            //this.HasRequired(t => t.Config)
            //    .WithMany(t => t.LayoutOptions)
            //    .HasForeignKey(d => d.Config_ConfigID);

        }
    }
}