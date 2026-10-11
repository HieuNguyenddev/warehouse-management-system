using Microsoft.EntityFrameworkCore;
using Warehouse.Domain.Roles;
using Warehouse.Domain.Users;

namespace Warehouse.Infrastructure.Persistence;

public class WarehouseDbContext : DbContext
{
    public WarehouseDbContext(DbContextOptions<WarehouseDbContext> options)
        : base(options)
    {
    }

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WarehouseDbContext).Assembly);
    }
}
