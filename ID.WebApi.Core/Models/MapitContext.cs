using Microsoft.EntityFrameworkCore;

namespace ID.Api.Models
{
    public partial class MapitContext : DbContext
    {
        public MapitContext()
        {
        }

        public MapitContext(DbContextOptions<MapitContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Organizations> Organizations { get; set; }
        public virtual DbSet<Permissions> Permissions { get; set; }
        public virtual DbSet<Roles> Roles { get; set; }
        public virtual DbSet<UserClaims> UserClaims { get; set; }
        public virtual DbSet<UserLogins> UserLogins { get; set; }
        public virtual DbSet<UserRoles> UserRoles { get; set; }
        public virtual DbSet<Users> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                this.CustomConfiguring(optionsBuilder);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresExtension("pgcrypto");

            modelBuilder.Entity<Organizations>(entity =>
            {
                entity.HasKey(e => e.OrgId)
                    .HasName("Orgs_Id_pkey");

                entity.ToTable("Organizations", "mapit");

                entity.Property(e => e.OrgId).ValueGeneratedNever();

                entity.Property(e => e.CreateDate)
                    .HasColumnType("date")
                    .HasDefaultValueSql("now()");

                entity.Property(e => e.OrgName)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<Permissions>(entity =>
            {
                entity.HasKey(e => e.PermissionTypeId)
                    .HasName("Permissions_pkey");

                entity.ToTable("Permissions", "mapit");

                entity.Property(e => e.PermissionTypeId).ValueGeneratedNever();

                entity.Property(e => e.PermissionTypeName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.UpdateDate).HasColumnType("date");
            });

            modelBuilder.Entity<Roles>(entity =>
            {
                entity.ToTable("Roles", "mapit");

                entity.HasIndex(e => e.Id)
                    .HasName("PK_Roles");

                entity.HasIndex(e => e.Name)
                    .HasName("RoleNameIndex")
                    .IsUnique();

                entity.Property(e => e.Id).HasMaxLength(128);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(256);
            });

            modelBuilder.Entity<UserClaims>(entity =>
            {
                entity.ToTable("UserClaims", "mapit");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.ClaimType)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.ClaimValue)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserClaims)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Users_Id_fkey");
            });

            modelBuilder.Entity<UserLogins>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.ProviderKey, e.LoginProvider })
                    .HasName("UserLogins_pkey");

                entity.ToTable("UserLogins", "mapit");

                entity.Property(e => e.ProviderKey).HasMaxLength(128);

                entity.Property(e => e.LoginProvider).HasMaxLength(128);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserLogins)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Users_Id");
            });

            modelBuilder.Entity<UserRoles>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.RoleId })
                    .HasName("UserRoles_pkey");

                entity.ToTable("UserRoles", "mapit");

                entity.Property(e => e.RoleId).HasMaxLength(128);

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.UserRoles)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Roles_Id_fkey");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserRoles)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("User_Id_fkey");
            });

            modelBuilder.Entity<Users>(entity =>
            {
                entity.ToTable("Users", "mapit");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.FirstName).HasMaxLength(256);

                entity.Property(e => e.LastName).HasMaxLength(256);

                entity.Property(e => e.PasswordHash)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.PasswordSalt)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.PhoneNumber).HasMaxLength(256);

                entity.Property(e => e.SecurityStamp).HasMaxLength(256);

                entity.Property(e => e.UserName)
                    .IsRequired()
                    .HasMaxLength(256);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
