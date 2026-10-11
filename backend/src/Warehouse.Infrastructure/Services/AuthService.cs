using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Warehouse.Application.Auth;
using Warehouse.Application.Common;
using Warehouse.Domain.Roles;
using Warehouse.Domain.Users;
using Warehouse.Infrastructure.Persistence;

namespace Warehouse.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly WarehouseDbContext _db;
    private readonly IConfiguration _configuration;

    public AuthService(WarehouseDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email?.Trim().ToLowerInvariant() ?? string.Empty;
        var password = request.Password ?? string.Empty;
        var user = await _db.Users
            .Include(item => item.Role)
            .FirstOrDefaultAsync(item => item.Email == email, cancellationToken);

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

        return new LoginResponse(new JwtSecurityTokenHandler().WriteToken(token), expiresAt, ToSession(user));
    }

    public async Task<UserSession?> CurrentAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _db.Users
            .AsNoTracking()
            .Include(item => item.Role)
            .FirstOrDefaultAsync(item => item.Id == userId && item.IsActive, cancellationToken);

        return user is null ? null : ToSession(user);
    }

    public async Task EnsureSeedAdminAsync(CancellationToken cancellationToken)
    {
        var role = await _db.Roles.FirstOrDefaultAsync(item => item.Name == "admin", cancellationToken);
        if (role is null)
        {
            role = new Role
            {
                Id = Guid.NewGuid(),
                Name = "admin",
                Description = "Quản trị hệ thống"
            };
            _db.Roles.Add(role);
            await _db.SaveChangesAsync(cancellationToken);
        }

        var email = _configuration["Auth:SeedEmail"]?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email))
        {
            email = "admin@kho.local";
        }

        var password = _configuration["Auth:SeedPassword"];
        if (string.IsNullOrWhiteSpace(password))
        {
            password = "Admin@123";
        }

        var name = _configuration["Auth:SeedName"]?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            name = "Quản trị";
        }

        var user = await _db.Users.FirstOrDefaultAsync(item => item.Email == email, cancellationToken);
        if (user is null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                FullName = name,
                Email = email,
                IsActive = true,
                RoleId = role.Id
            };
            user.PasswordHash = Passwords.Hash(user, password);
            _db.Users.Add(user);
            await _db.SaveChangesAsync(cancellationToken);
            return;
        }

        var changed = false;
        if (string.IsNullOrEmpty(user.PasswordHash))
        {
            user.PasswordHash = Passwords.Hash(user, password);
            user.IsActive = true;
            changed = true;
        }

        if (user.RoleId is null)
        {
            user.RoleId = role.Id;
            changed = true;
        }

        if (changed)
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
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
        return new UserSession(user.Id, user.FullName, user.Email, user.Role?.Name);
    }
}
