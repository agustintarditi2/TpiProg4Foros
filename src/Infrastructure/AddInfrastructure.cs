namespace MyApp.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyApp.Application.Abstractions;
using MyApp.Application.Interfaces;
using MyApp.Infrastructure.Persistence;
using MyApp.Infrastructure.Persistence.Repositories;
using MyApp.Infrastructure.Security;
using MyApp.Infrastructure.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("Default")));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IForumRepository, ForumRepository>();
        services.AddScoped<IPostRepository, PostRepository>();

        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<IBanRepository, BanRepository>();
        services.AddScoped<ISanitizer, SanitizationService>();
        services.AddScoped<IJwtServices, JwtServices>();

        string timeApiBaseUrl = configuration["TimeApi:BaseUrl"]
    ?? throw new InvalidOperationException("Missing 'TimeApi:BaseUrl' configuration.");
        int timeApiTimeoutSeconds =
            int.TryParse(configuration["TimeApi:TimeoutSeconds"], out int seconds) ? seconds : 5;

        services.AddHttpClient<IWorldClock, TimeApiClient>(client =>            //AddHttpClient es la alternativa recomendada al new HttpClient()  Crear un cliente nuevo en cada llamada puede agotar las conexiones del sistema con el tiempo.
        {                                                                       //Con AddHttpClient, .NET administra y recicla esas conexiones por vos,
            client.BaseAddress = new Uri(timeApiBaseUrl);                       //y el cliente llega a la clase ya configurado (base y timeout), igual que el resto de tus servicios.
            client.Timeout = TimeSpan.FromSeconds(timeApiTimeoutSeconds);
        });

        return services;
    }
}