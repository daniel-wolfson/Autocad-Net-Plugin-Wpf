using System.Data.Entity.ModelConfiguration;
using Intellidesk.Data.Models.Cad;

namespace Intellidesk.Data.Models.Mapping
{
    public class UserSettingMap : EntityTypeConfiguration<UserSetting>
    {
        public UserSettingMap()
        {
            // Primary Key
            this.HasKey(t => t.UserSettingId);

            // Properties
            this.Property(t => t.ConfigSetName).IsRequired();
            this.Property(t => t.ProjectStatus).IsRequired();
            this.Property(t => t.Drive).IsRequired();

            // Table & Column Mappings
            this.ToTable("UserSettings");
            this.Property(t => t.UserSettingId).HasColumnName("UserSettingId");
            this.Property(t => t.UserId).HasColumnName("UserId");
            this.Property(t => t.ConfigSetName).HasColumnName("ConfigSetName");
            this.Property(t => t.ChainDistance).HasColumnName("ChainDistance");
            this.Property(t => t.DateStarted).HasColumnName("DateStarted");
            this.Property(t => t.IsActive).HasColumnName("IsActive");
            this.Property(t => t.IsColorMode).HasColumnName("IsColorMode");
            this.Property(t => t.ProjectExplorerRowSplitterPosition).HasColumnName("ProjectExplorerRowSplitterPosition");
            this.Property(t => t.ProjectExplorerPGridColumnSplitterPosition).HasColumnName("ProjectExplorerPGridColumnSplitterPosition");
            this.Property(t => t.Percent).HasColumnName("Percent");
            this.Property(t => t.ProjectStatus).HasColumnName("ProjectStatus");
            this.Property(t => t.ToggleLayoutDataTemplateSelector).HasColumnName("ToggleLayoutDataTemplateSelector");
            this.Property(t => t.MinWidth).HasColumnName("MinWidth");
            this.Property(t => t.LayoutId).HasColumnName("LayoutId");
            this.Property(t => t.Drive).HasColumnName("Drive");
            this.Property(t => t.ColorIndex).HasColumnName("ColorIndex");
            this.Property(t => t.Pos_X).HasColumnName("Pos_X");
            this.Property(t => t.Pos_Y).HasColumnName("Pos_Y");
            this.Property(t => t.Pos_Z).HasColumnName("Pos_Z");
            this.Property(t => t.GeoPos).HasColumnName("GeoPos");
            this.Property(t => t.UserSettingUser_UserSetting_UserId).HasColumnName("UserSettingUser_UserSetting_UserId");
            this.Property(t => t.UserSettingUser_UserSetting_UserSettingId).HasColumnName("UserSettingUser_UserSetting_UserSettingId");
            this.Property(t => t.Layout_LayoutID).HasColumnName("LayoutLayoutId");

            // Relationships
            this.HasRequired(t => t.Layout)
                .WithMany(t => t.UserSettings)
                .HasForeignKey(d => d.Layout_LayoutID);
            this.HasRequired(t => t.User)
                .WithMany(t => t.UserSettings)
                .HasForeignKey(d => new { d.UserSettingUser_UserSetting_UserId, d.UserSettingUser_UserSetting_UserSettingId });

        }
    }
}
