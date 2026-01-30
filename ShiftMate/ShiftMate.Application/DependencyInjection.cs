using Microsoft.Extensions.DependencyInjection;

namespace ShiftMate.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Här registrerar vi MediatR och säger: 
            // "Leta igenom hela detta projekt (Assembly) efter Queries och Commands"
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

            return services;
        }
    }
}