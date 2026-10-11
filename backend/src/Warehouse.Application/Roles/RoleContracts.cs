using Warehouse.Application.Common;

namespace Warehouse.Application.Roles;

public sealed record RoleResponse(Guid Id, string Name, string? Description);

public sealed record UpsertRoleRequest(string Name, string? Description);

public interface IRoleService
{
    Task<PagedResult<RoleResponse>> SearchAsync(string? search, int page, int pageSize, CancellationToken cancellationToken);

    Task<RoleResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<RoleResponse> CreateAsync(UpsertRoleRequest request, CancellationToken cancellationToken);

    Task<RoleResponse?> UpdateAsync(Guid id, UpsertRoleRequest request, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
