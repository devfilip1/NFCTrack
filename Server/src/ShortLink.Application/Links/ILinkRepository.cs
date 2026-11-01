using ShortLink.Domain.Links;

namespace ShortLink.Application.Links;

public interface ILinkRepository
{
    Task<Link?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Link?> GetBySlugAsync(string slug, CancellationToken cancellationToken);
    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken);
    Task<IReadOnlyList<Link>> ListAsync(CancellationToken cancellationToken);
    void Add(Link link);
    void Remove(Link link);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
