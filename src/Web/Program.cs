using MyApp.Application;
using MyApp.Web.Extensions;
using MyApp.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using MyApp.Web.Middlewares.ErrorHandling;
using Microsoft.EntityFrameworkCore;
using MyApp.Infrastructure.Persistence;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Esto llama a la configuración de Jwt que está en Extensions
builder.Services.AddJwtAuthentication(builder.Configuration);


// Acá se generan los controllers
builder.Services.AddControllers();

builder.Services.AddOpenApi(options =>
{
    // Declaramos el esquema de autenticación JWT para OpenAPI.
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new OpenApiComponents();

        document.Components.SecuritySchemes ??=
            new Dictionary<string, IOpenApiSecurityScheme>();

        document.Components.SecuritySchemes["Bearer"] =
            new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description =
                    "Autenticación mediante JWT. " +
                    "Pegue el token de acceso obtenido al iniciar sesión."
            };

        return Task.CompletedTask;
    });

    // Marcamos únicamente las operaciones protegidas con [Authorize].
    options.AddOperationTransformer((operation, context, cancellationToken) =>
    {
        var metadata =
            context.Description.ActionDescriptor.EndpointMetadata;

        bool allowsAnonymous =
            metadata.OfType<IAllowAnonymous>().Any();

        bool requiresAuthorization =
            metadata.OfType<IAuthorizeData>().Any();

        // No exige token en endpoints públicos.
        if (allowsAnonymous || !requiresAuthorization)
        {
            return Task.CompletedTask;
        }

        // Asociamos la operación al esquema Bearer declarado anteriormente.
        operation.Security ??= [];

        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [
                new OpenApiSecuritySchemeReference(
                    "Bearer",
                    context.Document)
            ] = []
        });

        return Task.CompletedTask;
    });
});

// Acá se hace la inyección de dependencias
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

// Acá se agregan el middleware de excepciones
builder.Services.AddTransient<GlobalExceptionHandlingMiddleware>();

// Se genera la API
var app = builder.Build();


app.UseMiddleware<GlobalExceptionHandlingMiddleware>();


// Habilita Swagger en desarrollo o cuando se configure explícitamente.
if (app.Environment.IsDevelopment() ||
    app.Configuration.GetValue<bool>("Swagger:Enabled"))
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/openapi/v1.json",
            "Foros API V1"
        );
    });
}

// Aplica migraciones automáticamente cuando la configuración lo habilita.
if (app.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
{
    using var scope = app.Services.CreateScope();

    var db = scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();

    db.Database.Migrate();
}

// Inicializa el SysAdmin si está habilitado.
await SysAdminSeeder.SeedAsync(
    app.Services,
    app.Configuration
);

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


// Se levanta la API
app.Run();
