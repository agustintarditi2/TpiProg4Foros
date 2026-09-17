using Microsoft.EntityFrameworkCore;
using Domain.Entities;

namespace Infrastructure.Data
{
    public class ApplicationContext : DbContext
    {
        // para que trabaje con objetos Forum y pueda persistirlos en la base de datos
        public DbSet<Forum> Forums { get; set; }

        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
        }

    }
}