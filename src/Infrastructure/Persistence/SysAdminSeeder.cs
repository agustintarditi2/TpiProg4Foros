using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyApp.Application.Abstractions;
using MyApp.Domain.Entities;
using MyApp.Domain.Enums;

namespace MyApp.Infrastructure.Persistence;

public static class SysAdminSeeder
{
    public static async Task SeedAsync(
        IServiceProvider services,
        IConfiguration configuration)
    {
        // Solo se ejecuta si está habilitado.
        if (!configuration.GetValue<bool>("SysAdminSeed:Enabled"))
            return;

        using var scope = services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        // Evita crear administradores duplicados
        bool exists = await db.Users
            .AnyAsync(u => u.Role == UserRole.SysAdmin);

        if (exists)
            return;

        // Obtiene las credenciales desde la configuración
        string? email = configuration["SysAdminSeed:Email"];
        string? password = configuration["SysAdminSeed:Password"];
        string? name = configuration["SysAdminSeed:Name"];
        string? dateText = configuration["SysAdminSeed:DateOfBirth"];

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(name) ||
            !DateOnly.TryParseExact(
                dateText,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var dateOfBirth))
        {
            throw new InvalidOperationException(
                "SysAdminSeed configuration is incomplete or invalid.");
        }

        // usa el mismo sistema de hash que el resto de usuarios
        var hasher = scope.ServiceProvider
            .GetRequiredService<IPasswordHasher>();

        var passwordHash = hasher.Hash(password);

        // Crea el usuario inicial
        var admin = new User(
            name,
            dateOfBirth,
            email,
            passwordHash,
            DateTimeOffset.UtcNow
        );

        admin.SetRole(UserRole.SysAdmin);

        db.Users.Add(admin);

        await db.SaveChangesAsync();
    }
}