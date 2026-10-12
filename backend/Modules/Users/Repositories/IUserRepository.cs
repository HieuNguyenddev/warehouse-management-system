using UserManagementAPI.Common.Dtos;
using UserManagementAPI.Modules.Users.DTOs;
using UserManagementAPI.Modules.Users.Entity;

namespace UserManagementAPI.Modules.Users.Repositories;

public interface IUserRepository
{
    Task<PagedResult<UserResponse>> SearchAsync(string? search, int page, int pageSize, CancellationToken cancellationToken);

    Task<UserResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<User?> FindAsync(Guid id, CancellationToken cancellationToken);

    Task<User?> FindReadOnlyAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> EmailExistsAsync(string email, Guid? exceptId, CancellationToken cancellationToken);

    void Add(User user);

    void Remove(User user);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
