using Microsoft.EntityFrameworkCore;
using Warehouse.Application.Common;
using Warehouse.Application.Users;
using Warehouse.Domain.Users;
using Warehouse.Infrastructure.Persistence;

namespace Warehouse.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly WarehouseDbContext _db;

    public UserService(WarehouseDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<UserResponse>> SearchAsync(string? search, int page, int pageSize, CancellationToken cancellationToken)
    {
        (page, pageSize) = PageRequest.Normalize(page, pageSize);
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
            .Select(user => new UserResponse(
                user.Id,
                user.FullName,
                user.Email,
                user.Phone,
                user.RoleId,
                user.Role != null ? user.Role.Name : null,
                user.IsActive))
            .ToListAsync(cancellationToken);

        return new PagedResult<UserResponse>(items, page, pageSize, totalCount);
    }

    public async Task<UserResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _db.Users
            .AsNoTracking()
            .Where(user => user.Id == id)
            .Select(user => new UserResponse(
                user.Id,
                user.FullName,
                user.Email,
                user.Phone,
                user.RoleId,
                user.Role != null ? user.Role.Name : null,
                user.IsActive))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<UserResponse> CreateAsync(UpsertUserRequest request, CancellationToken cancellationToken)
    {
        var normalized = Normalize(request);
        await EnsureEmailAvailable(normalized.Email, null, cancellationToken);
        await EnsureRoleExists(normalized.RoleId, cancellationToken);

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = normalized.FullName,
            Email = normalized.Email,
            Phone = normalized.Phone,
            RoleId = normalized.RoleId,
            IsActive = normalized.IsActive
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(user.Id, cancellationToken)
            ?? throw new AppException(500, "Không đọc lại được người dùng vừa tạo.");
    }

    public async Task<UserResponse?> UpdateAsync(Guid id, UpsertUserRequest request, CancellationToken cancellationToken)
    {
        var user = await _db.Users.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (user is null)
        {
            return null;
        }

        var normalized = Normalize(request);
        await EnsureEmailAvailable(normalized.Email, id, cancellationToken);
        await EnsureRoleExists(normalized.RoleId, cancellationToken);

        user.FullName = normalized.FullName;
        user.Email = normalized.Email;
        user.Phone = normalized.Phone;
        user.RoleId = normalized.RoleId;
        user.IsActive = normalized.IsActive;
        await _db.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(user.Id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await _db.Users.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (user is null)
        {
            return false;
        }

        _db.Users.Remove(user);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task EnsureEmailAvailable(string email, Guid? currentId, CancellationToken cancellationToken)
    {
        var exists = await _db.Users.AnyAsync(
            user => user.Email == email && (currentId == null || user.Id != currentId),
            cancellationToken);

        if (exists)
        {
            throw new AppException(409, "Email đã tồn tại.");
        }
    }

    private async Task EnsureRoleExists(Guid? roleId, CancellationToken cancellationToken)
    {
        if (roleId is null)
        {
            return;
        }

        var exists = await _db.Roles.AnyAsync(role => role.Id == roleId, cancellationToken);
        if (!exists)
        {
            throw new AppException(400, "Vai trò không tồn tại.");
        }
    }

    private static NormalizedUser Normalize(UpsertUserRequest request)
    {
        var fullName = request.FullName?.Trim() ?? string.Empty;
        var email = request.Email?.Trim() ?? string.Empty;
        var phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();

        if (fullName.Length is 0 or > 150)
        {
            throw new AppException(400, "Họ tên là bắt buộc và tối đa 150 ký tự.");
        }

        if (email.Length is 0 or > 200 || !email.Contains('@') || !email.Contains('.'))
        {
            throw new AppException(400, "Email không hợp lệ.");
        }

        if (phone?.Length > 20)
        {
            throw new AppException(400, "Số điện thoại tối đa 20 ký tự.");
        }

        return new NormalizedUser(fullName, email, phone, request.RoleId, request.IsActive);
    }

    private sealed record NormalizedUser(string FullName, string Email, string? Phone, Guid? RoleId, bool IsActive);
}
