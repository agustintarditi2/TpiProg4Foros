namespace MyApp.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyApp.Domain.Entities;
using MyApp.Domain.ValueObjects;

public sealed class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("Comments");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasConversion(id => id.Value, v => new CommentId(v))
            .ValueGeneratedNever();

        builder.Property(c => c.ParentPost)
            .HasConversion(id => id.Value, v => new PostId(v))
            .IsRequired();

        builder.Property(c => c.RepliedComment)
            .HasConversion(
                id => id!.Value.Value,
                v => new CommentId(v));

        builder.Property(c => c.Poster)
            .HasConversion(id => id.Value, v => new UserId(v))
            .IsRequired();

        builder.Property(c => c.Body)
            .HasConversion(b => b.Value, v => BodyText.FromPersistence(v))
            .IsRequired();

        builder.Property(c => c.DateCreated).IsRequired();
        builder.Property(c => c.DateEdited);

        builder.HasOne<Post>()
            .WithMany()
            .HasForeignKey(c => c.ParentPost)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Comment>()
            .WithMany()
            .HasForeignKey(c => c.RepliedComment)
            .OnDelete(DeleteBehavior.Restrict);
    }
}