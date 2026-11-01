using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShortLink.Domain.Links;

namespace ShortLink.Infrastructure.Persistence.Configurations;

internal sealed class LinkConfiguration : IEntityTypeConfiguration<Link>
{
    public void Configure(EntityTypeBuilder<Link> builder)
    {
        builder.ToTable("Links");

        builder.HasKey(link => link.Id);
        builder.Property(link => link.Id).ValueGeneratedNever();

        builder.Property(link => link.Slug)
            .HasMaxLength(Slug.MaxLength)
            .IsUnicode(false)
            .IsRequired();

        builder.HasIndex(link => link.Slug).IsUnique();

        builder.Property(link => link.DestinationUrl)
            .HasMaxLength(Link.DestinationUrlMaxLength)
            .IsRequired();

    }
}
