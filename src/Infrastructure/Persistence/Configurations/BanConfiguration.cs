namespace MyApp.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyApp.Domain.Entities;
using MyApp.Domain.ValueObjects;

public sealed class BanConfiguration : IEntityTypeConfiguration<Ban>
{
    public void Configure(EntityTypeBuilder<Ban> builder)
    {
        builder.ToTable("Bans");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .HasConversion(id => id.Value, v => new BanId(v))
            .ValueGeneratedNever();

        builder.Property(b => b.UserId)
            .HasConversion(id => id.Value, v => new UserId(v))
            .IsRequired();

        builder.Property(b => b.AdminId)
            .HasConversion(id => id.Value, v => new UserId(v))
            .IsRequired();

        builder.Property(b => b.StartDate).IsRequired();
        builder.Property(b => b.EndDate).IsRequired();
        builder.Property(b => b.Duration).IsRequired();

        builder.Property(b => b.Reason)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(b => b.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(b => b.AdminId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(b => b.UserId);
        builder.HasIndex(b => b.AdminId);
    }
}