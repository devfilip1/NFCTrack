using ShortLink.Application.Links;
using ShortLink.Domain;
using ShortLink.Domain.Clicks;

namespace ShortLink.Application.Clicks;

public sealed class ClickService(ILinkRepository links, IClickRepository clicks, TimeProvider timeProvider)
{
    public const int DefaultStatsDays = 30;
    public const int MaxStatsDays = 365;

    public async Task<bool> RecordAsync(string slug, DateTimeOffset occurredAt, string? userAgent, string? referrer, CancellationToken cancellationToken)
    {
        var link = await links.GetBySlugAsync(slug, cancellationToken);

        if (link is null) return false;

        var click = Click.Create(link.Id, occurredAt, userAgent, referrer);

        clicks.Add(click);

        await clicks.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<LinkStatsResponse?> GetStatsAsync(Guid linkId, int days, CancellationToken cancellationToken)
    {
        if (days < 1 || days > MaxStatsDays)
            throw new DomainException($"Days must be between 1 and {MaxStatsDays}.");

        var link = await links.GetByIdAsync(linkId, cancellationToken);

        if (link is null) return null;

        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        var firstDay = today.AddDays(-(days - 1));
        var since = new DateTimeOffset(firstDay.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);

        var total = await clicks.CountByLinkAsync(linkId, cancellationToken);
        var perDay = await clicks.CountPerDayAsync(linkId, since, cancellationToken);

        var clicksByDate = perDay.ToDictionary(d => d.Date, d => d.Clicks);
        var daily = Enumerable.Range(0, days)
            .Select(i => firstDay.AddDays(i))
            .Select(date => new DailyClicks(date, clicksByDate.GetValueOrDefault(date)))
            .ToList();

        return new LinkStatsResponse(linkId, total, daily);
    }
}
