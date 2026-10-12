using UserManagementAPI.Common.Dtos;
using UserManagementAPI.Modules.Users.DTOs;

namespace UserManagementAPI.Modules.Users.Services;

public interface IUserService
{
    Task<PagedResult<UserResponse>> SearchAsync(string? search, int page, int pageSize, CancellationToken cancellationToken);

    Task<UserResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<UserPasswordResponse?> GetPasswordAsync(Guid id, CancellationToken cancellationToken);

    Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken);

    Task<UserResponse?> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
