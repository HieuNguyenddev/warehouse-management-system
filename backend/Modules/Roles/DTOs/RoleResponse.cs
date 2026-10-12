namespace UserManagementAPI.Modules.Roles.DTOs;

public class RoleResponse
{
    public required Guid Id { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }
}
