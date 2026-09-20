using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Entities;
namespace MyApp.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public DbSet<User> Users {get; private set;}
    public DbSet<Post> Posts {get; private set;}
    public DbSet<Comment> Comments {get; private set;}
    public string DbPath {get;}
    public ApplicationDbContext()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = System.IO.Path.Join(path, "blogging.db");
    }
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");
}
