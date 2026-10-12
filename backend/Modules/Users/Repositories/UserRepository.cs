using Microsoft.EntityFrameworkCore;
using UserManagementAPI.Common.Dtos;
using UserManagementAPI.Data;
using UserManagementAPI.Modules.Users.DTOs;
using UserManagementAPI.Modules.Users.Entity;

namespace UserManagementAPI.Modules.Users.Repositories;

public class UserRepository : IUserRepository
{
    private readonly WarehouseDbContext _db;

    public UserRepository(WarehouseDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<UserResponse>> SearchAsync(string? search, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = _db.Users.AsNoTracking();
        var pattern = TextSearch.ToContainsPattern(search);

        if (pattern is not null)
        {
            query = query.Where(user =>
                EF.Functions.Like(EF.Functions.Collate(user.FullName, TextSearch.AccentInsensitive), pattern) ||
                EF.Functions.Like(EF.Functions.Collate(user.Email, TextSearch.AccentInsensitive), pattern) ||
                EF.Functions.Like(EF.Functions.Collate(user.Phone, TextSearch.AccentInsensitive), pattern) ||
                (user.Role != null && EF.Functions.Like(EF.Functions.Collate(user.Role.Name, TextSearch.AccentInsensitive), pattern)));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(user => user.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(user => new UserResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                RoleId = user.RoleId,
                RoleName = user.Role != null ? user.Role.Name : null,
                IsActive = user.IsActive,
                HasPassword = user.PasswordHash != null && user.PasswordHash != ""
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<UserResponse>(items, page, pageSize, totalCount);
    }

    public async Task<UserResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _db.Users
            .AsNoTracking()
            .Where(user => user.Id == id)
            .Select(user => new UserResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                RoleId = user.RoleId,
                RoleName = user.Role != null ? user.Role.Name : null,
                IsActive = user.IsActive,
                HasPassword = user.PasswordHash != null && user.PasswordHash != ""
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<User?> FindAsync(Guid id, CancellationToken cancellationToken)
    {
        return _db.Users.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
    }

    public Task<User?> FindReadOnlyAsync(Guid id, CancellationToken cancellationToken)
    {
        return _db.Users.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
    }

    public Task<bool> EmailExistsAsync(string email, Guid? exceptId, CancellationToken cancellationToken)
    {
        return _db.Users.AnyAsync(
            user => user.Email == email && (exceptId == null || user.Id != exceptId),
            cancellationToken);
    }

    public void Add(User user)
    {
        _db.Users.Add(user);
    }

    public void Remove(User user)
    {
        _db.Users.Remove(user);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _db.SaveChangesAsync(cancellationToken);
    }
}
