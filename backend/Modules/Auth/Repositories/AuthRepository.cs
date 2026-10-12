using Microsoft.EntityFrameworkCore;
using UserManagementAPI.Data;
using UserManagementAPI.Modules.Users.Entity;

namespace UserManagementAPI.Modules.Auth.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly WarehouseDbContext _db;

    public AuthRepository(WarehouseDbContext db)
    {
        _db = db;
    }

    public Task<User?> FindByEmailWithRoleAsync(string email, CancellationToken cancellationToken)
    {
        return _db.Users
            .Include(item => item.Role)
            .FirstOrDefaultAsync(item => item.Email == email, cancellationToken);
    }

    public Task<User?> FindActiveByIdWithRoleAsync(Guid id, CancellationToken cancellationToken)
    {
        return _db.Users
            .AsNoTracking()
            .Include(item => item.Role)
            .FirstOrDefaultAsync(item => item.Id == id && item.IsActive, cancellationToken);
    }
}
