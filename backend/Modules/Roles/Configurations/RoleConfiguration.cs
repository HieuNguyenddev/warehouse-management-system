using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserManagementAPI.Modules.Roles.Entity;

namespace UserManagementAPI.Modules.Roles.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(role => role.Id);

        builder.Property(role => role.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(role => role.Description)
            .HasMaxLength(500);

        builder.HasIndex(role => role.Name)
            .IsUnique();
    }
}
