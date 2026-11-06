using ShortLink.Domain.Links;

namespace ShortLink.Application.Links;

public sealed class LinkService(ILinkRepository links, TimeProvider timeProvider, ILinkCache cache)
{
    private const int MaxSlugGenerationAttempts = 5;

    public async Task<LinkResponse> CreateAsync(CreateLinkRequest request, CancellationToken cancellationToken)
    {
        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? await GenerateUniqueSlugAsync(cancellationToken)
            : await EnsureSlugAvailableAsync(Slug.Normalize(request.Slug), cancellationToken);

        var link = Link.Create(slug, request.DestinationUrl, timeProvider.GetUtcNow());

        links.Add(link);
        await links.SaveChangesAsync(cancellationToken);

        return LinkResponse.From(link);
    }

    public async Task<LinkResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var link = await links.GetByIdAsync(id, cancellationToken);
        return link is null ? null : LinkResponse.From(link);
    }

    public async Task<string?> ResolveDestinationAsync(string slug, CancellationToken cancellationToken)
    {
        var normalizedSlug = Slug.Normalize(slug);
        if (!Slug.IsValid(normalizedSlug))
            return null;

        var cachedDestination = await cache.GetDestinationAsync(normalizedSlug, cancellationToken);

        if (cachedDestination != null) return cachedDestination;


        var link = await links.GetBySlugAsync(normalizedSlug, cancellationToken);

        if (link is null) return null;

        await cache.SetDestinationAsync(normalizedSlug, link.DestinationUrl, cancellationToken);

        return link.DestinationUrl;
    }

    public async Task<IReadOnlyList<LinkResponse>> ListAsync(CancellationToken cancellationToken)
    {
        var allLinks = await links.ListAsync(cancellationToken);
        return allLinks.Select(LinkResponse.From).ToList();
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var link = await links.GetByIdAsync(id, cancellationToken);

        if (link is null)
            return false;

        links.Remove(link);

        await links.SaveChangesAsync(cancellationToken);

        await cache.RemoveAsync(link.Slug, cancellationToken);

        return true;
    }

    public async Task<LinkResponse?> UpdateDestinationAsync(Guid id, UpdateLinkRequest request, CancellationToken cancellationToken)
    {
        var link = await links.GetByIdAsync(id, cancellationToken);

        if (link is null)
            return null;

        link.ChangeDestination(request.DestinationUrl, timeProvider.GetUtcNow());

        await links.SaveChangesAsync(cancellationToken);

        await cache.RemoveAsync(link.Slug, cancellationToken);

        return LinkResponse.From(link);
    }

    private async Task<string> EnsureSlugAvailableAsync(string slug, CancellationToken cancellationToken)
    {
        if (await links.SlugExistsAsync(slug, cancellationToken))
            throw new SlugAlreadyInUseException(slug);

        return slug;
    }

    private async Task<string> GenerateUniqueSlugAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < MaxSlugGenerationAttempts; attempt++)
        {
            var candidate = Slug.Generate();
            if (!await links.SlugExistsAsync(candidate, cancellationToken))
                return candidate;

        }

        throw new InvalidOperationException("Could not generate a unique slug.");
    }
}
