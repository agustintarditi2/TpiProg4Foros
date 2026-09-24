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

        builder.Property(u => u.Password)
            .HasConversion(p => p.Value, v => PasswordHash.Create(v));
    }
}