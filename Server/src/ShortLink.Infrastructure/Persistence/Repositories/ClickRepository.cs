using Microsoft.EntityFrameworkCore;
using ShortLink.Application.Clicks;
using ShortLink.Domain.Clicks;

namespace ShortLink.Infrastructure.Persistence.Repositories;

internal sealed class ClickRepository(AppDbContext dbContext) : IClickRepository
{
    public void Add(Click click)
    {
        dbContext.Clicks.Add(click);
    }

    public Task<int> CountByLinkAsync(Guid linkId, CancellationToken cancellationToken)
    {
        return dbContext.Clicks.CountAsync(c => c.LinkId == linkId, cancellationToken);
    }

    public async Task<IReadOnlyList<DailyClicks>> CountPerDayAsync(Guid linkId, DateTimeOffset since, CancellationToken cancellationToken)
    {
        var rows = await dbContext.Clicks
            .Where(c => c.LinkId == linkId && c.OccurredAt >= since)
            .GroupBy(c => c.OccurredAt.Date)
            .Select(g => new { Date = g.Key, Clicks = g.Count() })
            .OrderBy(r => r.Date)
            .ToListAsync(cancellationToken);

        return rows
            .Select(r => new DailyClicks(DateOnly.FromDateTime(r.Date), r.Clicks))
            .ToList();
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
