using UserManagementAPI.Modules.Users.DTOs;

namespace UserManagementAPI.Modules.Users.Helpers;

public static class UserMapper
{
    public static UserResponse ToDto(Guid id, string fullName, string email, string? phone, Guid? roleId, string? roleName, bool isActive, bool hasPassword)
    {
        return new UserResponse
        {
            Id = id,
            FullName = fullName,
            Email = email,
            Phone = phone,
            RoleId = roleId,
            RoleName = roleName,
            IsActive = isActive,
            HasPassword = hasPassword
        };
    }
}
