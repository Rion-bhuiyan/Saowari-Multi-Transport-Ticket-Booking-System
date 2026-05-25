using Microsoft.Extensions.DependencyInjection;
using Saowari.Interfaces;
using Saowari.Services;
using System.Linq;
using System.Reflection;

namespace Saowari.Extensions
{
    public static class ServiceExtensions
    {
        public static void AddApplicationServices(this IServiceCollection services)
        {
            // Register Generic Repository
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            // Register JwtService
            services.AddScoped<IJwtService, JwtService>();

            // Auto-register all Services matching I*Service
            var assembly = Assembly.GetExecutingAssembly();
            
            var serviceTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Service") && 
                            (t.Namespace == "Saowari.Services" || t.Namespace == "Saowari.Services.BusinessServices"));

            foreach (var type in serviceTypes)
            {
                var interfaceType = type.GetInterfaces().FirstOrDefault(i => i.Name == $"I{type.Name}");
                if (interfaceType != null)
                {
                    services.AddScoped(interfaceType, type);
                }
            }
        }
    }
}
