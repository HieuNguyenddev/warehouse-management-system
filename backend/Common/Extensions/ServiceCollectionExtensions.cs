using Microsoft.EntityFrameworkCore;
using UserManagementAPI.Data;
using UserManagementAPI.Modules.Auth.Repositories;
using UserManagementAPI.Modules.Auth.Services;
using UserManagementAPI.Modules.Roles.Repositories;
using UserManagementAPI.Modules.Roles.Services;
using UserManagementAPI.Modules.Users.Repositories;
using UserManagementAPI.Modules.Users.Services;

namespace UserManagementAPI.Common.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWarehouseServices(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<WarehouseDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
