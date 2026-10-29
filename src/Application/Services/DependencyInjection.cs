namespace MyApp.Application;

using Microsoft.Extensions.DependencyInjection;
using MyApp.Application.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<UserServices>();
        services.AddScoped<PostServices>();
        services.AddScoped<AuthServices>();
        services.AddScoped<BanServices>();
        services.AddScoped<ForumServices>();
        services.AddScoped<AuthServices>();
        // register other handlers here as you add them

        return services;
    }
}