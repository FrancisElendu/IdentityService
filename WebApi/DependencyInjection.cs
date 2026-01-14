using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Infrastructure.Contexts;
using Microsoft.AspNetCore.Authorization;
using AuthLibrary.Permissions;
using Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Application;
using Microsoft.Extensions.Configuration;

namespace WebApi
{
    public static class DependencyInjection
    {
        internal static async Task<IApplicationBuilder> SeedDatabase(this IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                var seeder = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbSeeder>();
                await seeder.SeedIdentityDatabaseAsync();
            }
            return app;
        }

        internal static IServiceCollection AddIdentitySettings(this IServiceCollection services)
        {
            services
                .AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>()
                .AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>()
                .AddIdentity<ApplicationUser, ApplicationRole>(options =>
                {
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequiredLength = 6;
                    options.User.RequireUniqueEmail = true;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
            return services;
        }

        internal static TokenSettings GetTokenSettings(this IServiceCollection services, IConfiguration config)
        {
            var tokenSettingsConfig = config.GetSection(nameof(TokenSettings));
            services.Configure<TokenSettings>(tokenSettingsConfig);
            return tokenSettingsConfig.Get<TokenSettings>();
        }
    }
}
