using ShortLink.Domain.Clicks;

namespace ShortLink.Application.Clicks;

public interface IClickRepository
{
    void Add(Click click);
    Task SaveChangesAsync(CancellationToken cancellationToken);
    Task<int> CountByLinkAsync(Guid linkId, CancellationToken cancellationToken);
    Task<IReadOnlyList<DailyClicks>> CountPerDayAsync(Guid linkId, DateTimeOffset since, CancellationToken cancellationToken);
}
