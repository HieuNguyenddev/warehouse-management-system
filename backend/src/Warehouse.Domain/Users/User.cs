using Warehouse.Domain.Roles;

namespace Warehouse.Domain.Users;

public class User
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? PasswordHash { get; set; }

    public string? Phone { get; set; }

    public Guid? RoleId { get; set; }

    public Role? Role { get; set; }

    public bool IsActive { get; set; } = true;
}
