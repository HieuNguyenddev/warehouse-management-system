using UserManagementAPI.Common.Dtos;
using UserManagementAPI.Modules.Roles.DTOs;

namespace UserManagementAPI.Modules.Roles.Services;

public interface IRoleService
{
    Task<PagedResult<RoleResponse>> SearchAsync(string? search, int page, int pageSize, CancellationToken cancellationToken);

    Task<RoleResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<RoleResponse> CreateAsync(UpsertRoleRequest request, CancellationToken cancellationToken);

    Task<RoleResponse?> UpdateAsync(Guid id, UpsertRoleRequest request, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
