namespace ShortLink.Domain.Links;

public sealed class Link
{
    public const int DestinationUrlMaxLength = 2048;

    public Guid Id { get; private set; }
    public Guid OwnerId { get; private set; }
    public string Slug { get; private set; }
    public string DestinationUrl { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    private Link(Guid id, Guid ownerId, string slug, string destinationUrl, DateTimeOffset createdAt)
    {
        Id = id;
        OwnerId = ownerId;
        Slug = slug;
        DestinationUrl = destinationUrl;
        CreatedAt = createdAt;
    }

    public static Link Create(Guid ownerId, string slug, string destinationUrl, DateTimeOffset now)
    {
        if (ownerId == Guid.Empty)
            throw new DomainException("Link owner is required.");

        var normalizedSlug = Links.Slug.Normalize(slug);

        if (!Links.Slug.IsValid(normalizedSlug))
        {
            throw new DomainException(
                $"Slug must be {Links.Slug.MinLength}-{Links.Slug.MaxLength} characters long, " +
                "contain only lowercase letters, digits and single hyphens, and not be reserved.");
        }

        return new Link(Guid.CreateVersion7(), ownerId, normalizedSlug, EnsureValidDestination(destinationUrl), now);
    }

    public bool IsOwnedBy(Guid userId) => OwnerId == userId;

    public void ChangeDestination(string destinationUrl, DateTimeOffset now)
    {
        DestinationUrl = EnsureValidDestination(destinationUrl);
        UpdatedAt = now;
    }

    private static string EnsureValidDestination(string destinationUrl)
    {
        var trimmed = destinationUrl.Trim();

        if (trimmed.Length > DestinationUrlMaxLength
            || !Uri.TryCreate(trimmed, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new DomainException("Destination URL must be an absolute http or https URL.");
        }

        return trimmed;
    }
}
