using UserManagementAPI.Modules.Roles.Entity;

namespace UserManagementAPI.Modules.Users.Entity;

public class User
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? PasswordHash { get; set; }

    public string? PasswordReveal { get; set; }

    public string? Phone { get; set; }

    public Guid? RoleId { get; set; }

    public Role? Role { get; set; }

    public bool IsActive { get; set; } = true;
}
