using UserManagementAPI.Common.Dtos;
using UserManagementAPI.Modules.Roles.DTOs;
using UserManagementAPI.Modules.Roles.Entity;

namespace UserManagementAPI.Modules.Roles.Repositories;

public interface IRoleRepository
{
    Task<PagedResult<RoleResponse>> SearchAsync(string? search, int page, int pageSize, CancellationToken cancellationToken);

    Task<RoleResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Role?> FindAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> NameExistsAsync(string name, Guid? exceptId, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);

    void Add(Role role);

    void Remove(Role role);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
