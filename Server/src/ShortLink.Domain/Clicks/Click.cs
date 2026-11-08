namespace ShortLink.Domain.Clicks;

public sealed class Click
{
    public const int UserAgentMaxLength = 512;
    public const int ReferrerMaxLength = 2048;

    public Guid Id { get; private set; }
    public Guid LinkId { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }
    public string? UserAgent { get; private set; }
    public string? Referrer { get; private set; }

    private Click(Guid id, Guid linkId, DateTimeOffset occurredAt, string? userAgent, string? referrer)
    {
        Id = id;
        LinkId = linkId;
        OccurredAt = occurredAt;
        UserAgent = userAgent;
        Referrer = referrer;
    }

    public static Click Create(Guid linkId, DateTimeOffset occurredAt, string? userAgent, string? referrer)
    {
        return new Click(
            Guid.CreateVersion7(),
            linkId,
            occurredAt,
            Truncate(userAgent, UserAgentMaxLength),
            Truncate(referrer, ReferrerMaxLength));
    }

    private static string? Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();

        return trimmed.Length > maxLength ? trimmed[..maxLength] : trimmed;
    }
}
