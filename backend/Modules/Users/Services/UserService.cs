using System.Text;
using UserManagementAPI.Common.Dtos;
using UserManagementAPI.Modules.Auth.Helpers;
using UserManagementAPI.Modules.Roles.Repositories;
using UserManagementAPI.Modules.Users.DTOs;
using UserManagementAPI.Modules.Users.Entity;
using UserManagementAPI.Modules.Users.Repositories;

namespace UserManagementAPI.Modules.Users.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _users;
    private readonly IRoleRepository _roles;
    private readonly IConfiguration _configuration;

    public UserService(IUserRepository users, IRoleRepository roles, IConfiguration configuration)
    {
        _users = users;
        _roles = roles;
        _configuration = configuration;
    }

    public Task<PagedResult<UserResponse>> SearchAsync(string? search, int page, int pageSize, CancellationToken cancellationToken)
    {
        (page, pageSize) = PageRequest.Normalize(page, pageSize);
        return _users.SearchAsync(search, page, pageSize, cancellationToken);
    }

    public Task<UserPasswordResponse?> GetPasswordAsync(Guid id, CancellationToken cancellationToken)
    {
        return ReadPasswordAsync(id, cancellationToken);
    }

    public Task<UserResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _users.GetByIdAsync(id, cancellationToken);
    }

    public async Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var normalized = Normalize(request, request.Password, requirePassword: true);
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
        StorePassword(user, normalized.Password!);

        _users.Add(user);
        await _users.SaveChangesAsync(cancellationToken);
        return await _users.GetByIdAsync(user.Id, cancellationToken)
            ?? throw new AppException(500, "Không đọc lại được người dùng vừa tạo.");
    }

    public async Task<UserResponse?> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var user = await _users.FindAsync(id, cancellationToken);
        if (user is null)
        {
            return null;
        }

        var normalized = Normalize(request, request.Password, requirePassword: false);
        await EnsureEmailAvailable(normalized.Email, id, cancellationToken);
        await EnsureRoleExists(normalized.RoleId, cancellationToken);

        user.FullName = normalized.FullName;
        user.Email = normalized.Email;
        user.Phone = normalized.Phone;
        user.RoleId = normalized.RoleId;
        user.IsActive = normalized.IsActive;
        if (normalized.Password is not null)
        {
            StorePassword(user, normalized.Password);
        }

        await _users.SaveChangesAsync(cancellationToken);
        return await _users.GetByIdAsync(user.Id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await _users.FindAsync(id, cancellationToken);
        if (user is null)
        {
            return false;
        }

        _users.Remove(user);
        await _users.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<UserPasswordResponse?> ReadPasswordAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await _users.FindReadOnlyAsync(id, cancellationToken);
        if (user is null)
        {
            return null;
        }

        return new UserPasswordResponse
        {
            Password = PasswordVault.Open(user.PasswordReveal, SigningKey())
        };
    }

    private void StorePassword(User user, string password)
    {
        user.PasswordHash = Passwords.Hash(user, password);
        user.PasswordReveal = PasswordVault.Protect(password, SigningKey());
    }

    private string SigningKey()
    {
        var key = _configuration["Jwt:SigningKey"];
        if (string.IsNullOrWhiteSpace(key) || Encoding.UTF8.GetByteCount(key) < 32)
        {
            throw new AppException(500, "Khóa ký JWT chưa được cấu hình hoặc ngắn hơn 32 byte.");
        }

        return key;
    }

    private async Task EnsureEmailAvailable(string email, Guid? currentId, CancellationToken cancellationToken)
    {
        if (await _users.EmailExistsAsync(email, currentId, cancellationToken))
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

        if (!await _roles.ExistsAsync(roleId.Value, cancellationToken))
        {
            throw new AppException(400, "Vai trò không tồn tại.");
        }
    }

    private static NormalizedUser Normalize(UserWriteRequest request, string? rawPassword, bool requirePassword)
    {
        var fullName = request.FullName?.Trim() ?? string.Empty;
        var email = request.Email?.Trim().ToLowerInvariant() ?? string.Empty;
        var phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();
        var password = string.IsNullOrWhiteSpace(rawPassword) ? null : rawPassword;

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

        if (requirePassword && password is null)
        {
            throw new AppException(400, "Mật khẩu là bắt buộc.");
        }

        if (password is not null && password.Length is < 8 or > 100)
        {
            throw new AppException(400, "Mật khẩu từ 8 đến 100 ký tự.");
        }

        return new NormalizedUser(fullName, email, phone, request.RoleId, request.IsActive, password);
    }

    private sealed record NormalizedUser(string FullName, string Email, string? Phone, Guid? RoleId, bool IsActive, string? Password);
}
