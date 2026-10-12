using UserManagementAPI.Modules.Roles.DTOs;

namespace UserManagementAPI.Modules.Roles.Helpers;

public static class RoleMapper
{
    public static RoleResponse ToDto(Guid id, string name, string? description)
    {
        return new RoleResponse
        {
            Id = id,
            Name = name,
            Description = description
        };
    }
}
