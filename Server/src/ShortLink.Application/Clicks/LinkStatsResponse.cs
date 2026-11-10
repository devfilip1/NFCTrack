namespace ShortLink.Application.Clicks;

public sealed record LinkStatsResponse(Guid LinkId, int TotalClicks, IReadOnlyList<DailyClicks> Daily);
