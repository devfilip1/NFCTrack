namespace ShortLink.Application.Clicks;

public interface IClickTracker
{
    Task TrackAsync(string slug, DateTimeOffset occurredAt, string? userAgent, string? referrer, CancellationToken cancellationToken);
}
