using ShortLink.Domain.Links;

namespace ShortLink.Application.Links;

public sealed record LinkResponse(
    Guid Id,
    string Slug,
    string DestinationUrl,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt)
{
    public static LinkResponse From(Link link) =>
        new(link.Id, link.Slug, link.DestinationUrl, link.CreatedAt, link.UpdatedAt);
}
