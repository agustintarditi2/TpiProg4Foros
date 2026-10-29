using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyApp.Domain.Entities;
using MyApp.Domain.ValueObjects;

namespace MyApp.Infrastructure.Persistence.Configurations;

// configura como entity framework guarda la entidad forum en la base de datos
public sealed class ForumConfiguration : IEntityTypeConfiguration<Forum>
{
    // configuracion de forum
    public void Configure(EntityTypeBuilder<Forum> builder)
    {
        // define el nombre de la tabla
        builder.ToTable("Forums");

        // define la clave primaria
        builder.HasKey(f => f.Id);

        // convierte forumid a guid para poder guardarlo en sqlite
        builder.Property(f => f.Id)
            .HasConversion(
                id => id.Value,
                value => new ForumId(value))
            .ValueGeneratedNever();

        // configura el titulo como obligatorio
        builder.Property(f => f.Title)
            .IsRequired();

        // configura la descripcion como obligatoria
        builder.Property(f => f.Description)
            .IsRequired();

        // configura el estado como obligatorio
        builder.Property(f => f.State)
            .IsRequired();
    }
}