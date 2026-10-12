using UserManagementAPI.Common.Dtos;
using UserManagementAPI.Modules.Roles.DTOs;
using UserManagementAPI.Modules.Roles.Entity;
using UserManagementAPI.Modules.Roles.Helpers;
using UserManagementAPI.Modules.Roles.Repositories;

namespace UserManagementAPI.Modules.Roles.Services;

public class RoleService : IRoleService
{
    private readonly IRoleRepository _roles;

    public RoleService(IRoleRepository roles)
    {
        _roles = roles;
    }

    public Task<PagedResult<RoleResponse>> SearchAsync(string? search, int page, int pageSize, CancellationToken cancellationToken)
    {
        (page, pageSize) = PageRequest.Normalize(page, pageSize);
        return _roles.SearchAsync(search, page, pageSize, cancellationToken);
    }

    public Task<RoleResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _roles.GetByIdAsync(id, cancellationToken);
    }

    public async Task<RoleResponse> CreateAsync(UpsertRoleRequest request, CancellationToken cancellationToken)
    {
        var (name, description) = Normalize(request);
        await EnsureNameAvailable(name, null, cancellationToken);

        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description
        };

        _roles.Add(role);
        await _roles.SaveChangesAsync(cancellationToken);
        return RoleMapper.ToDto(role.Id, role.Name, role.Description);
    }

    public async Task<RoleResponse?> UpdateAsync(Guid id, UpsertRoleRequest request, CancellationToken cancellationToken)
    {
        var role = await _roles.FindAsync(id, cancellationToken);
        if (role is null)
        {
            return null;
        }

        var (name, description) = Normalize(request);
        await EnsureNameAvailable(name, id, cancellationToken);

        role.Name = name;
        role.Description = description;
        await _roles.SaveChangesAsync(cancellationToken);
        return RoleMapper.ToDto(role.Id, role.Name, role.Description);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var role = await _roles.FindAsync(id, cancellationToken);
        if (role is null)
        {
            return false;
        }

        _roles.Remove(role);
        await _roles.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task EnsureNameAvailable(string name, Guid? currentId, CancellationToken cancellationToken)
    {
        if (await _roles.NameExistsAsync(name, currentId, cancellationToken))
        {
            throw new AppException(409, "Tên vai trò đã tồn tại.");
        }
    }

    private static (string Name, string? Description) Normalize(UpsertRoleRequest request)
    {
        var name = request.Name?.Trim() ?? string.Empty;
        var description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();

        if (name.Length is 0 or > 150)
        {
            throw new AppException(400, "Tên vai trò là bắt buộc và tối đa 150 ký tự.");
        }

        if (description?.Length > 500)
        {
            throw new AppException(400, "Mô tả tối đa 500 ký tự.");
        }

        return (name, description);
    }
}
