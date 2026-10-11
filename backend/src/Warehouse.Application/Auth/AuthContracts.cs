namespace Warehouse.Application.Auth;

public sealed record LoginRequest(string Email, string Password);

public sealed record LoginResponse(string Token, DateTime ExpiresAt, UserSession User);

public sealed record UserSession(Guid Id, string FullName, string Email, string? RoleName);

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);

    Task<UserSession?> CurrentAsync(Guid userId, CancellationToken cancellationToken);

    Task EnsureSeedAdminAsync(CancellationToken cancellationToken);
}
