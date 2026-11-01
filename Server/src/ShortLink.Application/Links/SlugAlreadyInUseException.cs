namespace ShortLink.Application.Links;

public sealed class SlugAlreadyInUseException(string slug)
    : Exception($"Slug '{slug}' is already in use.");
