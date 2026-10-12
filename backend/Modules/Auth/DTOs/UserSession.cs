namespace UserManagementAPI.Modules.Auth.DTOs;

public class UserSession
{
    public required Guid Id { get; set; }

    public required string FullName { get; set; }

    public required string Email { get; set; }

    public string? RoleName { get; set; }
}
