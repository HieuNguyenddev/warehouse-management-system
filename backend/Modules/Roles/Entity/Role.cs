namespace UserManagementAPI.Modules.Roles.Entity;

public class Role
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}
