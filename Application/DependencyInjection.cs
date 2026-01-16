using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Register application services here
            var assembly = Assembly.GetExecutingAssembly();
            return services
                .AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(assembly));
        }
    }
}
