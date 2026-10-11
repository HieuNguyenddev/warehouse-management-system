using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Common;
using Warehouse.Application.Roles;
using Warehouse.Domain.Roles;
using Warehouse.Infrastructure.Persistence;

namespace Warehouse.Infrastructure.Services;

public class RoleService : IRoleService
{
    private readonly WarehouseDbContext _db;

    public RoleService(WarehouseDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<RoleResponse>> SearchAsync(string? search, int page, int pageSize, CancellationToken cancellationToken)
    {
        (page, pageSize) = PageRequest.Normalize(page, pageSize);
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
            .Select(role => new RoleResponse(role.Id, role.Name, role.Description))
            .ToListAsync(cancellationToken);

        return new PagedResult<RoleResponse>(items, page, pageSize, totalCount);
    }

    public async Task<RoleResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _db.Roles
            .AsNoTracking()
            .Where(role => role.Id == id)
            .Select(role => new RoleResponse(role.Id, role.Name, role.Description))
            .FirstOrDefaultAsync(cancellationToken);
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

        _db.Roles.Add(role);
        await _db.SaveChangesAsync(cancellationToken);
        return new RoleResponse(role.Id, role.Name, role.Description);
    }

    public async Task<RoleResponse?> UpdateAsync(Guid id, UpsertRoleRequest request, CancellationToken cancellationToken)
    {
        var role = await _db.Roles.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (role is null)
        {
            return null;
        }

        var (name, description) = Normalize(request);
        await EnsureNameAvailable(name, id, cancellationToken);

        role.Name = name;
        role.Description = description;
        await _db.SaveChangesAsync(cancellationToken);
        return new RoleResponse(role.Id, role.Name, role.Description);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var role = await _db.Roles.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (role is null)
        {
            return false;
        }

        _db.Roles.Remove(role);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task EnsureNameAvailable(string name, Guid? currentId, CancellationToken cancellationToken)
    {
        var exists = await _db.Roles.AnyAsync(
            role => role.Name == name && (currentId == null || role.Id != currentId),
            cancellationToken);

        if (exists)
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
