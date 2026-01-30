using FluentValidation; // <--- Se till att denna finns
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace ShiftMate.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Registrera MediatR (som du redan har)
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            // --- NYTT: Registrera alla Validators automatiskt ---
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            return services;
        }
    }
}