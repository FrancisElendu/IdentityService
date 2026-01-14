using Application.Features.Token;
using Application.Interfaces.Roles;
using Application.Interfaces.Users;
using Infrastructure.Contexts;
using Infrastructure.Services.Roles;
using Infrastructure.Services.Token;
using Infrastructure.Services.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            // Register DbContext
            return services.AddDbContext<ApplicationDbContext>(options => options
                .UseSqlServer
                (
                    configuration.GetConnectionString("DefaultConnection"),
                    builder =>
                    {
                        builder.MigrationsHistoryTable("Migrations", "EFCore");
                        builder.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                        builder.EnableRetryOnFailure
                        (
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: [1]
                         );
                    }
                ))
            .AddTransient<ApplicationDbSeeder>();
        }

        public static IServiceCollection AddIdentityService(this IServiceCollection services)
        {
            return services
                .AddTransient<IUserService, UserService>()
                .AddTransient<IRoleService, RoleService>()
                .AddTransient<ITokenService, TokenService>();
        }
    }
}
