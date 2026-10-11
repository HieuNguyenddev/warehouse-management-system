using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Warehouse.Application.Roles;
using Warehouse.Infrastructure.Persistence;
using Warehouse.Infrastructure.Services;

namespace Warehouse.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<WarehouseDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IRoleService, RoleService>();

        return services;
    }
}
