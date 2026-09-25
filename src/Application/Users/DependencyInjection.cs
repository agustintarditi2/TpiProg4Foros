namespace MyApp.Application;

using Microsoft.Extensions.DependencyInjection;
using MyApp.Application.Users;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<RegisterUserHandler>();
        // register other handlers here as you add them

        return services;
    }
}