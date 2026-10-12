using UserManagementAPI.Modules.Users.Entity;

namespace UserManagementAPI.Modules.Auth.Repositories;

public interface IAuthRepository
{
    Task<User?> FindByEmailWithRoleAsync(string email, CancellationToken cancellationToken);

    Task<User?> FindActiveByIdWithRoleAsync(Guid id, CancellationToken cancellationToken);
}
