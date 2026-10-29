namespace MyApp.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyApp.Domain.Entities;
using MyApp.Domain.ValueObjects;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasConversion(id => id.Value, v => new UserId(v))
            .ValueGeneratedNever();

        builder.Property(u => u.Name)
            .HasMaxLength(127)
            .IsRequired();

        builder.Property(u => u.Email)
            .HasConversion(e => e.Value, v => EmailAddress.Create(v))
            .HasMaxLength(254)
            .IsRequired();

        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.Role)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(u => u.DateOfBirth).IsRequired();
        builder.Property(u => u.DateCreated).IsRequired();

        builder.Property(u => u.Password)
            .HasConversion(p => p.Value, v => PasswordHash.Create(v))
            .IsRequired();

        builder.OwnsMany(u => u.Reactions, rb =>
        {
            rb.ToTable("Reactions");

            rb.Property(r => r.User)
                .HasConversion(id => id.Value, v => new UserId(v))
                .IsRequired();

            rb.Property(r => r.Post)
                .HasConversion(
                    id => id.HasValue ? id.Value.Value : (Guid?)null,
                    v => v.HasValue ? new PostId(v.Value) : (PostId?)null);

            rb.Property(r => r.Comment)
                .HasConversion(
                    id => id.HasValue ? id.Value.Value : (Guid?)null,
                    v => v.HasValue ? new CommentId(v.Value) : (CommentId?)null);

            rb.Property(r => r.Type).IsRequired();

            rb.HasIndex(r => new { r.User, r.Post, r.Comment });
        });
    }
}