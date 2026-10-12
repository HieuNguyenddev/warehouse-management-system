namespace UserManagementAPI.Modules.Users.DTOs;

public class UserResponse
{
    public required Guid Id { get; set; }

    public required string FullName { get; set; }

    public required string Email { get; set; }

    public string? Phone { get; set; }

    public Guid? RoleId { get; set; }

    public string? RoleName { get; set; }

    public required bool IsActive { get; set; }

    public required bool HasPassword { get; set; }
}
