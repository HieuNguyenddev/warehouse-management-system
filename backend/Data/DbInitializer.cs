using Microsoft.EntityFrameworkCore;
using UserManagementAPI.Modules.Auth.Helpers;
using UserManagementAPI.Modules.Roles.Entity;
using UserManagementAPI.Modules.Users.Entity;

namespace UserManagementAPI.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        var configuration = services.GetRequiredService<IConfiguration>();
        var db = services.GetRequiredService<WarehouseDbContext>();

        if (configuration.GetValue<bool>("Database:ApplyMigrations"))
        {
            db.Database.Migrate();
        }

        await SeedAdminAsync(db, configuration, cancellationToken);
    }

    private static async Task SeedAdminAsync(WarehouseDbContext db, IConfiguration configuration, CancellationToken cancellationToken)
    {
        var role = await db.Roles.FirstOrDefaultAsync(item => item.Name == "admin", cancellationToken);
        if (role is null)
        {
            role = new Role
            {
                Id = Guid.NewGuid(),
                Name = "admin",
                Description = "Quản trị hệ thống"
            };
            db.Roles.Add(role);
            await db.SaveChangesAsync(cancellationToken);
        }

        var email = configuration["Auth:SeedEmail"]?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email))
        {
            email = "admin@kho.local";
        }

        var password = configuration["Auth:SeedPassword"];
        if (string.IsNullOrWhiteSpace(password))
        {
            password = "Admin@123";
        }

        var name = configuration["Auth:SeedName"]?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            name = "Quản trị";
        }

        var user = await db.Users.FirstOrDefaultAsync(item => item.Email == email, cancellationToken);
        if (user is null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                FullName = name,
                Email = email,
                IsActive = true,
                RoleId = role.Id
            };
            user.PasswordHash = Passwords.Hash(user, password);
            user.PasswordReveal = PasswordVault.Protect(password, SigningKey(configuration));
            db.Users.Add(user);
            await db.SaveChangesAsync(cancellationToken);
            return;
        }

        var changed = false;
        if (string.IsNullOrEmpty(user.PasswordHash))
        {
            user.PasswordHash = Passwords.Hash(user, password);
            user.PasswordReveal = PasswordVault.Protect(password, SigningKey(configuration));
            user.IsActive = true;
            changed = true;
        }
        else if (string.IsNullOrEmpty(user.PasswordReveal) && Passwords.Verify(user, password))
        {
            user.PasswordReveal = PasswordVault.Protect(password, SigningKey(configuration));
            changed = true;
        }

        if (user.RoleId is null)
        {
            user.RoleId = role.Id;
            changed = true;
        }

        if (changed)
        {
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private static string SigningKey(IConfiguration configuration)
    {
        var key = configuration["Jwt:SigningKey"];
        if (string.IsNullOrWhiteSpace(key) || System.Text.Encoding.UTF8.GetByteCount(key) < 32)
        {
            throw new InvalidOperationException("Jwt:SigningKey phải dài ít nhất 32 byte.");
        }

        return key;
    }
}
