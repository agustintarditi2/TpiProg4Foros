namespace MyApp.Application;

using Microsoft.Extensions.DependencyInjection;
using MyApp.Application.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<UserServices>();
        services.AddScoped<BanServices>();
        services.AddScoped<ForumServices>();
        // register other handlers here as you add them

        return services;
    }
}