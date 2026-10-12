using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using UserManagementAPI.Common.Dtos;
using UserManagementAPI.Modules.Auth.DTOs;
using UserManagementAPI.Modules.Auth.Helpers;
using UserManagementAPI.Modules.Auth.Repositories;
using UserManagementAPI.Modules.Users.Entity;

namespace UserManagementAPI.Modules.Auth.Services;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _auth;
    private readonly IConfiguration _configuration;

    public AuthService(IAuthRepository auth, IConfiguration configuration)
    {
        _auth = auth;
        _configuration = configuration;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email?.Trim().ToLowerInvariant() ?? string.Empty;
        var password = request.Password ?? string.Empty;
        var user = await _auth.FindByEmailWithRoleAsync(email, cancellationToken);

        if (user is null || !user.IsActive || !Passwords.Verify(user, password))
        {
            throw new AppException(401, "Email hoặc mật khẩu không đúng.");
        }

        var expiresMinutes = _configuration.GetValue("Jwt:ExpiresMinutes", 480);
        if (expiresMinutes < 5)
        {
            expiresMinutes = 5;
        }

        var expiresAt = DateTime.UtcNow.AddMinutes(expiresMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Name, user.FullName)
        };

        if (!string.IsNullOrWhiteSpace(user.Role?.Name))
        {
            claims.Add(new Claim(ClaimTypes.Role, user.Role.Name));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey()));
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new LoginResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAt = expiresAt,
            User = ToSession(user)
        };
    }

    public async Task<UserSession?> CurrentAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _auth.FindActiveByIdWithRoleAsync(userId, cancellationToken);
        return user is null ? null : ToSession(user);
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

    private static UserSession ToSession(User user)
    {
        return new UserSession
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            RoleName = user.Role?.Name
        };
    }
}
