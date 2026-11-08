using ShortLink.Domain.Clicks;

namespace ShortLink.Application.Clicks;

public interface IClickRepository
{
    void Add(Click click);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
