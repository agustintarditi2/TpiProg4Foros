namespace MyApp.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyApp.Domain.Entities;
using MyApp.Domain.ValueObjects;

public sealed class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.ToTable("Posts");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasConversion(id => id.Value, v => new PostId(v))
            .ValueGeneratedNever();

        builder.Property(p => p.Poster)
            .HasConversion(id => id.Value, v => new UserId(v))
            .IsRequired();

        builder.Property(p => p.Body)
            .HasConversion(b => b.Value, v => BodyText.FromPersistence(v))
            .IsRequired();

        builder.Property(p => p.DateCreated).IsRequired();
        builder.Property(p => p.DateEdited);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(p => p.Poster)
            .OnDelete(DeleteBehavior.Restrict);
    }
}