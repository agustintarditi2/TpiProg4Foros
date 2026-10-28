namespace MyApp.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public sealed class ApplicationDbContextFactory
    : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // busca la carpeta raiz del repositorio
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (directory != null &&
               !File.Exists(Path.Combine(
                   directory.FullName,
                   "src",
                   "Infrastructure",
                   "Infrastructure.csproj")))
        {
            directory = directory.Parent;
        }

        if (directory == null)
        {
            throw new InvalidOperationException(
                "No se encontro la raiz del proyecto.");
        }

        // obtiene la ruta absoluta de la base sqlite
        var dbPath = Path.Combine(
            directory.FullName,
            "src",
            "Infrastructure",
            "ForosTup.db"
        );

        // configura entity framework con la base correcta
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite($"Data Source={dbPath}")
            .Options;

        return new ApplicationDbContext(options);
    }
}