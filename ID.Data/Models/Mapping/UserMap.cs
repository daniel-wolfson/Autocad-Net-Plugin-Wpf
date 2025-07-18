using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using Intellidesk.Data.Models.Cad;

namespace Intellidesk.Data.Models.Mapping
{
    public class UserMap : EntityTypeConfiguration<User>
    {
        public UserMap()
        {
            // Primary Key
            this.HasKey(t => new { t.UserId, t.UserSettingId });

            // Properties
            this.Property(t => t.UserId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            this.Property(t => t.UserSettingId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            this.Property(t => t.Name).IsRequired();
            this.Property(t => t.Login).IsRequired().HasMaxLength(50);
            this.Property(t => t.Email).IsRequired();
            this.Property(t => t.Settings_Data).IsRequired();
            this.Property(t => t.Drive).IsRequired();

            // Table & Column Mappings
            this.ToTable("Users");
            this.Property(t => t.UserId).HasColumnName("UserId");
            this.Property(t => t.UserSettingId).HasColumnName("UserSettingId");
            this.Property(t => t.Name).HasColumnName("Name");
            this.Property(t => t.Login).HasColumnName("Login");
            this.Property(t => t.Email).HasColumnName("Email");
            this.Property(t => t.Settings_Data).HasColumnName("Settings_Data");
            this.Property(t => t.Drive).HasColumnName("Drive");
            this.Property(t => t.IsBlocked).HasColumnName("IsBlocked");
        }
    }
}
