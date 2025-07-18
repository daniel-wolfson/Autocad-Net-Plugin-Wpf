using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using Intellidesk.Data.Models.Cad;

namespace Intellidesk.Data.Models.Mapping
{
    public class BlockItemAttributeDefinitionMap : EntityTypeConfiguration<BlockItemAttributeDef>
    {
        public BlockItemAttributeDefinitionMap()
        {
            // Primary Key
            this.HasKey(t => new { t.LayoutId, t.BlockItemAttributeDefId });

            // Properties
            this.Property(t => t.LayoutId)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.TabIndex)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.Title)
                .IsRequired()
                .HasMaxLength(100);

            this.Property(t => t.Value)
                .HasMaxLength(1000);

            // Table & Column Mappings
            this.ToTable("BlockItemAttributes");
            this.Property(t => t.BlockItemAttributeDefId).HasColumnName("ItemID");
            this.Property(t => t.LayoutId).HasColumnName("LayoutId");
            this.Property(t => t.TabIndex).HasColumnName("ItemIndex");
            this.Property(t => t.Title).HasColumnName("ItemAttributeName");
            this.Property(t => t.Value).HasColumnName("ItemAttributeValue");

            // Relationships
            //this.HasRequired(t => t.Instance)
            //    .WithMany(t => t.Attributes)
            //    .HasForeignKey(d => new { d.Item_LayoutID });

        }
    }
}
