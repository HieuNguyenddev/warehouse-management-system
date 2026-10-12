using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserManagementAPI.Modules.Users.Entity;

namespace UserManagementAPI.Modules.Users.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.FullName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(user => user.Email)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(user => user.PasswordHash)
            .HasMaxLength(500);

        builder.Property(user => user.PasswordReveal)
            .HasMaxLength(500);

        builder.Property(user => user.Phone)
            .HasMaxLength(20);

        builder.HasIndex(user => user.Email)
            .IsUnique();

        builder.HasOne(user => user.Role)
            .WithMany()
            .HasForeignKey(user => user.RoleId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
