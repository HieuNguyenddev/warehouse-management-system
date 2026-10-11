using Warehouse.Application.Common;

namespace Warehouse.Application.Users;

public sealed record UserResponse(
    Guid Id,
    string FullName,
    string Email,
    string? Phone,
    Guid? RoleId,
    string? RoleName,
    bool IsActive);

public sealed record UpsertUserRequest(
    string FullName,
    string Email,
    string? Phone,
    Guid? RoleId,
    bool IsActive,
    string? Password);

public interface IUserService
{
    Task<PagedResult<UserResponse>> SearchAsync(string? search, int page, int pageSize, CancellationToken cancellationToken);

    Task<UserResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<UserResponse> CreateAsync(UpsertUserRequest request, CancellationToken cancellationToken);

    Task<UserResponse?> UpdateAsync(Guid id, UpsertUserRequest request, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
