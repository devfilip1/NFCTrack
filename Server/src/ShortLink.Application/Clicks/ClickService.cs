using ShortLink.Application.Links;
using ShortLink.Domain.Clicks;

namespace ShortLink.Application.Clicks;

public sealed class ClickService(ILinkRepository links, IClickRepository clicks)
{
    public async Task<bool> RecordAsync(string slug, DateTimeOffset occurredAt, string? userAgent, string? referrer, CancellationToken cancellationToken)
    {
        var link = await links.GetBySlugAsync(slug, cancellationToken);

        if (link is null) return false;

        var click = Click.Create(link.Id, occurredAt, userAgent, referrer);

        clicks.Add(click);

        await clicks.SaveChangesAsync(cancellationToken);

        return true;
    }
}
