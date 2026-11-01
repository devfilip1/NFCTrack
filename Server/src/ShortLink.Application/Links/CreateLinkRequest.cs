namespace ShortLink.Application.Links;

public sealed record CreateLinkRequest(string DestinationUrl, string? Slug);
