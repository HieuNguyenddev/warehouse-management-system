using Microsoft.EntityFrameworkCore;
using UserManagementAPI.Common.Dtos;
using UserManagementAPI.Data;
using UserManagementAPI.Modules.Roles.DTOs;
using UserManagementAPI.Modules.Roles.Entity;

namespace UserManagementAPI.Modules.Roles.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly WarehouseDbContext _db;

    public RoleRepository(WarehouseDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<RoleResponse>> SearchAsync(string? search, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = _db.Roles.AsNoTracking();
        var pattern = TextSearch.ToContainsPattern(search);

        if (pattern is not null)
        {
            query = query.Where(role =>
                EF.Functions.Like(EF.Functions.Collate(role.Name, TextSearch.AccentInsensitive), pattern) ||
                EF.Functions.Like(EF.Functions.Collate(role.Description, TextSearch.AccentInsensitive), pattern));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(role => role.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(role => new RoleResponse
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<RoleResponse>(items, page, pageSize, totalCount);
    }

    public async Task<RoleResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _db.Roles
            .AsNoTracking()
            .Where(role => role.Id == id)
            .Select(role => new RoleResponse
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<Role?> FindAsync(Guid id, CancellationToken cancellationToken)
    {
        return _db.Roles.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
    }

    public Task<bool> NameExistsAsync(string name, Guid? exceptId, CancellationToken cancellationToken)
    {
        return _db.Roles.AnyAsync(
            role => role.Name == name && (exceptId == null || role.Id != exceptId),
            cancellationToken);
    }

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
    {
        return _db.Roles.AnyAsync(role => role.Id == id, cancellationToken);
    }

    public void Add(Role role)
    {
        _db.Roles.Add(role);
    }

    public void Remove(Role role)
    {
        _db.Roles.Remove(role);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _db.SaveChangesAsync(cancellationToken);
    }
}
