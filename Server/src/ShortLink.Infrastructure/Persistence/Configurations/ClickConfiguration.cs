using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShortLink.Domain.Clicks;
using ShortLink.Domain.Links;

namespace ShortLink.Infrastructure.Persistence.Configurations;

internal sealed class ClickConfiguration : IEntityTypeConfiguration<Click>
{
    public void Configure(EntityTypeBuilder<Click> builder)
    {
        builder.ToTable("Clicks");

        builder.HasKey(click => click.Id);
        builder.Property(click => click.Id).ValueGeneratedNever();

        builder.Property(click => click.UserAgent)
            .HasMaxLength(Click.UserAgentMaxLength);

        builder.Property(click => click.Referrer)
            .HasMaxLength(Click.ReferrerMaxLength);

        builder.HasOne<Link>().WithMany().HasForeignKey(click => click.LinkId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(click => new { click.LinkId, click.OccurredAt });
    }
}
