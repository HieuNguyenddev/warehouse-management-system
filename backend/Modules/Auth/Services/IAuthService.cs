using UserManagementAPI.Modules.Auth.DTOs;

namespace UserManagementAPI.Modules.Auth.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);

    Task<UserSession?> CurrentAsync(Guid userId, CancellationToken cancellationToken);
}
